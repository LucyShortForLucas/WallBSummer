#pragma once

#define SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR 1
#include "Grid2d_template.h"
#undef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR

#include "GridHelpers.h"
#include "Chunk2d.h"

namespace grid {

template<typename T, Chunk2dFactory<T> ChunkGen>
inline T Grid2d<T, ChunkGen>::get_tile(GridTileCoord2d coord) {
	auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

	load_chunk_asleep(chunkCoord);

	return m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)];
}

template<typename T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::set_tile(GridTileCoord2d coord, T value) {
	auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

	load_chunk_asleep(chunkCoord);

	m_Chunks[chunkCoord]->current_data_buffer()[coord_to_data_index(chunkTile)] = value;
}

template<typename T, Chunk2dFactory<T> ChunkGen>
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

template<typename T, Chunk2dFactory<T> ChunkGen>
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


template<typename T, Chunk2dFactory<T> ChunkGen>
void Grid2d<T, ChunkGen>::load_chunk_asleep(ChunkCoord2d chunkCoord) {
	if (!m_Chunks.contains(chunkCoord)) {
		m_Chunks[chunkCoord] = std::make_unique<Chunk2d<T>>();
		m_Chunks[chunkCoord]->current_data_buffer() = m_ChunkFactory(chunkCoord);
	}
}

}