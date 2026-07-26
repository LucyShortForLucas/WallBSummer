#pragma once

#define SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR 1
#include "Grid2d_template.h"
#undef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR

#include "GridHelpers.h"
#include "Chunk2d.h"

namespace grid {

template<ValidGridData T>
inline T Grid2d<T>::get_tile(GridTileCoord2d coord) {
	auto [chunkCoord, chunkTile] {grid_to_chunk_tile(coord)};

	if (!m_Chunks.contains(chunkCoord))
		m_Chunks;

	return m_Chunks[];
}

template<ValidGridData T>
void Grid2d<T>::set_tile(GridTileCoord2d coord, T value) {

}

}