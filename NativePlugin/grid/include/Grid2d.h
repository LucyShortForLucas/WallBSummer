#pragma once

#define SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR 1
#include "Grid2d_template.h"
#undef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR

#include "GridHelpers.h"
#include "Chunk2d.h"

namespace grid {

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
inline T Grid2d<T, ChunkGen>::get_tile(GridTileCoord2d coord) {
	auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

	load_chunk_asleep(chunkCoord);

	return m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)];
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::set_tile(GridTileCoord2d coord, T value) {
	auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

	load_chunk_asleep(chunkCoord);

	m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)] = value;
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
std::vector<T> Grid2d<T, ChunkGen>::get_tile_rect(GridTileRect rect) {
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

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::fill_tile_rect(GridTileRect rect, T value) {
	for (auto& chunkRect : grid_to_chunk_rect(rect)) {
		load_chunk_asleep(chunkRect.first);
		auto& chunk{m_Chunks[chunkRect.first]};
		utils::fill_rect_in_square_array<T, CHUNK_DATA_WIDTH>(chunk->current_data_buffer(),
			chunkRect.second.coord.value.x+1, chunkRect.second.coord.value.y+1,
			chunkRect.second.width, chunkRect.second.height,
			value);
	}
}


template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::load_chunk_asleep(ChunkCoord2d chunkCoord) {
	if (!m_Chunks.contains(chunkCoord)) {
		m_Chunks[chunkCoord] = std::make_unique<Chunk2d<T>>(m_ChunkFactory(chunkCoord));
		m_LoadedChunks.emplace_back(m_Chunks[chunkCoord].get());
	}
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::wake_chunk(ChunkCoord2d coord) {
	load_chunk_asleep(coord);
	auto it{ std::find(m_LoadedChunks.begin(), m_LoadedChunks.end(), m_Chunks[coord])};
	if (std::distance(it, m_LoadedChunks.begin()+m_AwakeChunkCount) < m_AwakeChunkCount) return;

	std::swap(*it, m_LoadedChunks(m_AwakeChunkCount));
	++m_AwakeChunkCount;
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::sleep_chunk(ChunkCoord2d coord) {
	load_chunk_asleep(coord);
	auto it{ std::ranges::find(m_LoadedChunks, m_Chunks[coord]) };
	if (std::distance(it, m_LoadedChunks.begin()+m_AwakeChunkCount) >= m_AwakeChunkCount) return;

	std::swap(*it, m_LoadedChunks(m_AwakeChunkCount-1));
	--m_AwakeChunkCount;
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::load_chunks_asleep(ChunkRect rect) {
	for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
		for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
			load_chunk_asleep({x,y});
		}
	}
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::wake_chunks(ChunkRect rect) {
	for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
		for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
			wake_chunk({ x,y });
		}
	}
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::sleep_chunks(ChunkRect rect) {
	for (int x{ rect.coord.value.x }; x < rect.width + rect.coord.value.x; ++x) {
		for (int y{ rect.coord.value.y }; y < rect.height + rect.coord.value.y; ++y) {
			sleep_chunk({ x,y });
		}
	}
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
int Grid2d<T, ChunkGen>::loaded_chunk_count() {
	return m_LoadedChunks.size();
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
int Grid2d<T, ChunkGen>::awake_chunk_count() {
	return m_AwakeChunkCount;
}

template<ValidGridData T, Chunk2dFactory<T> ChunkGen>
int Grid2d<T, ChunkGen>::sleeping_chunk_count() {
	return m_LoadedChunks.size() - m_AwakeChunkCount;
}

}