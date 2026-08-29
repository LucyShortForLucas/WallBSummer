#pragma once

#define SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR 1
#include "Grid2d_template.h"
#undef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR

#include "GridHelpers.h"
#include "Chunk2d.h"

#include <functional>
#include <bit>

namespace grid {

template<ValidGridData T>
inline T Grid2d<T>::get_tile(GridTileCoord2d coord) {
    auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

    load_chunk_asleep(chunkCoord);

    return m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)];
}

template<ValidGridData T>
void Grid2d<T>::set_tile(GridTileCoord2d coord, T value) {
    auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

    load_chunk_asleep(chunkCoord);

    m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)] = value;
}

template<ValidGridData T>
std::vector<T> Grid2d<T>::get_tile_rect(GridTileRect rect) {
    std::vector<T> result;
    for (auto& chunkRect : grid_to_chunk_rect(rect)) {
        load_chunk_asleep(chunkRect.first);
        auto& chunk{ m_Chunks[chunkRect.first] };
        utils::fetch_rect_from_square_array<T, CHUNK_DATA_WIDTH>(chunk->current_data_buffer(),
            chunkRect.second.coord.value.x+1, chunkRect.second.coord.value.y+1,
            chunkRect.second.width, chunkRect.second.height,
            result);
    }
    return result;
}

template<ValidGridData T>
void Grid2d<T>::fill_tile_rect(GridTileRect rect, T value) {
    for (auto& chunkRect : grid_to_chunk_rect(rect)) {
        load_chunk_asleep(chunkRect.first);
        auto& chunk{m_Chunks[chunkRect.first]};
        utils::fill_rect_in_square_array<T, CHUNK_DATA_WIDTH>(chunk->current_data_buffer(),
            chunkRect.second.coord.value.x+1, chunkRect.second.coord.value.y+1,
            chunkRect.second.width, chunkRect.second.height,
            value);
        mark_chunk_dirty(chunk->coord, 255);
    }
}


template<ValidGridData T>
void Grid2d<T>::load_chunk_asleep(ChunkCoord2d chunkCoord) {
    if (!m_Chunks.contains(chunkCoord)) {
        m_Chunks[chunkCoord] = std::make_unique<Chunk2d<T>>(chunkCoord, m_ChunkFactory->generate(chunkCoord));
        m_LoadedChunks.emplace_back(m_Chunks[chunkCoord].get());
    }
}

template<ValidGridData T>
void Grid2d<T>::wake_chunk(ChunkCoord2d coord) {
    load_chunk_asleep(coord);
    auto it{ std::ranges::find(m_LoadedChunks, m_Chunks[coord].get()) };
    if (std::distance(m_LoadedChunks.begin(), it) < m_AwakeChunkCount) return;

    std::swap(*it, m_LoadedChunks[m_AwakeChunkCount]);
    ++m_AwakeChunkCount;
}

template<ValidGridData T>
void Grid2d<T>::sleep_chunk(ChunkCoord2d coord) {
    load_chunk_asleep(coord);
    auto it{ std::ranges::find(m_LoadedChunks, m_Chunks[coord].get()) };
    if (std::distance(m_LoadedChunks.begin(), it) >= m_AwakeChunkCount) return;

    std::swap(*it, m_LoadedChunks[m_AwakeChunkCount-1]);
    --m_AwakeChunkCount;
}

template<ValidGridData T>
void Grid2d<T>::load_chunks_asleep(ChunkRect rect) {
    for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
        for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
            load_chunk_asleep({x,y});
        }
    }
}

template<ValidGridData T>
void Grid2d<T>::wake_chunks(ChunkRect rect) {
    for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
        for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
            wake_chunk({ x,y });
        }
    }
}

template<ValidGridData T>
void Grid2d<T>::sleep_chunks(ChunkRect rect) {
    for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
        for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
            sleep_chunk({ x,y });
        }
    }
}

template<ValidGridData T>
int Grid2d<T>::loaded_chunk_count() {
    return m_LoadedChunks.size();
}

template<ValidGridData T>
int Grid2d<T>::awake_chunk_count() {
    return m_AwakeChunkCount;
}

template<ValidGridData T>
int Grid2d<T>::sleeping_chunk_count() {
    return m_LoadedChunks.size() - m_AwakeChunkCount;
}

template <ValidGridData T>
template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
void Grid2d<T>::run_on_chunk(ChunkCoord2d chunkCoord, F&& func, Args&&... args) {
    load_chunk_asleep(chunkCoord);
    m_pAlgoRunner->Run(std::bind(func, m_Chunks[chunkCoord].get(), this, args...));
}

template <ValidGridData T>
template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
void Grid2d<T>::run_on_awake_chunks(F&& func, Args&&... args) {
    std::for_each_n(m_LoadedChunks.begin(), m_AwakeChunkCount, [&](Chunk2d<T>* pChunk){
        m_pAlgoRunner->Run(std::bind(func, pChunk, this, args...));
    });
}

template <ValidGridData T>
template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
void Grid2d<T>::run_on_loaded_chunks(F&& func, Args&&... args) {
    for (auto pChunk : m_LoadedChunks) {
        m_pAlgoRunner->Run(std::bind(func, pChunk, this, args...));
    }
}

namespace detail {
template <typename T>
struct Sync {
    Sync() = delete; // this is not a real type, just a way to nicely instantiate all templated functions herein

    static void sync_N(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        auto pSrcBuffer{ target.current_data_buffer().data()};       // We fetch pointers to the start of our buffers
        auto pDstBuffer{ neighbour.current_data_buffer().data()};   // so we can efficiently bulk copy from them 
                                                                   //  later with std::copy_n
        auto src{ pSrcBuffer + CHUNK_SW_INDEX };
        auto dst{ pDstBuffer + HALO_NW_INDEX + 1};

        std::copy_n(src, CHUNK_WIDTH, dst);
    }

    static void sync_E(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        auto& srcBuffer{ target.current_data_buffer() };
        auto& dstBuffer{ neighbour.current_data_buffer() };

        for (int dsti{ HALO_NW_INDEX + CHUNK_DATA_WIDTH }; dsti < HALO_SW_INDEX; dsti += CHUNK_DATA_WIDTH) {
            int srci{ dsti + CHUNK_WIDTH };
            dstBuffer[dsti] = srcBuffer[srci];
        }
    }

    static void sync_S(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        auto pSrcBuffer{ target.current_data_buffer().data() };        // We fetch pointers to the start of our buffers
        auto pDstBuffer{ neighbour.current_data_buffer().data() };    // so we can efficiently bulk copy from them 
                                                                     //  later with std::copy_n
        auto src{ pSrcBuffer + CHUNK_NW_INDEX };
        auto dst{ pDstBuffer + HALO_SW_INDEX + 1 };

        std::copy_n(src, CHUNK_WIDTH, dst);
    }

    static void sync_W(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        auto& srcBuffer{ target.current_data_buffer() };
        auto& dstBuffer{ neighbour.current_data_buffer() };

        for (int dsti{ HALO_NE_INDEX + CHUNK_DATA_WIDTH }; dsti < HALO_SE_INDEX; dsti += CHUNK_DATA_WIDTH) {
            int srci{ dsti - CHUNK_WIDTH};
            dstBuffer[dsti] = srcBuffer[srci];
        }
    }

    static void sync_NE(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        neighbour.current_data_buffer()[HALO_SW_INDEX] = target.current_data_buffer()[CHUNK_NE_INDEX];
    }
    static void sync_SE(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        neighbour.current_data_buffer()[HALO_NW_INDEX] = target.current_data_buffer()[CHUNK_SE_INDEX];
    }
    static void sync_SW(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        neighbour.current_data_buffer()[HALO_NE_INDEX] = target.current_data_buffer()[CHUNK_SW_INDEX];
    }
    static void sync_NW(Chunk2d<T>& target, Chunk2d<T>& neighbour) {
        neighbour.current_data_buffer()[HALO_SE_INDEX] = target.current_data_buffer()[CHUNK_NW_INDEX];
    }

};
}

template <ValidGridData T>
void Grid2d<T>::sync_dirty_halos() {
    struct DirEntry {
        ChunkCoord2d neighbourOffset;
        void (*sync)(Chunk& target, Chunk& neighbour);
    };

    using Sync = detail::Sync<T>;

    static constexpr std::array<DirEntry, 8> syncFuncTable = {
        /*N */ DirEntry{ChunkCoord2d{ 0,  1}, Sync::sync_N },
        /*E */ DirEntry{ChunkCoord2d{ 1,  0}, Sync::sync_E },
        /*S */ DirEntry{ChunkCoord2d{ 0, -1}, Sync::sync_S },
        /*W */ DirEntry{ChunkCoord2d{-1,  0}, Sync::sync_W },
        /*NE*/ DirEntry{ChunkCoord2d{ 1,  1}, Sync::sync_NE},
        /*SE*/ DirEntry{ChunkCoord2d{ 1, -1}, Sync::sync_SE},
        /*SW*/ DirEntry{ChunkCoord2d{-1, -1}, Sync::sync_SW},
        /*NW*/ DirEntry{ChunkCoord2d{-1,  1}, Sync::sync_NW}
    };

    for (auto& coord : m_DirtyChunks) {
        auto pChunk{m_Chunks[coord].get()};

        while (pChunk->dirtyEdges) { // iterate while there dirty edge bits set
            auto& entry{ syncFuncTable[std::countr_zero(pChunk->dirtyEdges)] }; 
            load_chunk_asleep(entry.neighbourOffset + coord);
            entry.sync(*pChunk, *m_Chunks[entry.neighbourOffset + coord]);
            pChunk->dirtyEdges &= pChunk->dirtyEdges - 1; // clear lowest set bit
        }
    }

    m_DirtyChunks.clear();
}

template <ValidGridData T>
void Grid2d<T>::mark_chunk_dirty(ChunkCoord2d coord, uint8_t dirtyFlags) {
    m_ChunksMutex.lock();
    Chunk& chunk{ *m_Chunks[coord] };
    m_ChunksMutex.unlock();

    if (chunk.dirtyEdges == 0) {
        std::scoped_lock lock{m_DirtyChunksMutex};
        m_DirtyChunks.emplace_back(coord);
    }
    
    chunk.dirtyEdges |= dirtyFlags;
}

template <ValidGridData T>
const std::array<T, CHUNK_DATA_SIZE>* Grid2d<T>::get_chunk_data_if_loaded(ChunkCoord2d coord) const {
    if (!m_Chunks.contains(coord))
        return nullptr;

    return &m_Chunks.at(coord)->read_buffer();
}

}