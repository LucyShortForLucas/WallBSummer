#pragma once

#include <utility>
#include <vector>
#include <algorithm>

#include "GridDefines.h"
#include "Coord2d.h"

namespace grid {

// ---- Tile conversion functions
GridTileCoord2d get_chunk_origin(ChunkCoord2d chunkIndex);
GridTileCoord2d chunk_to_grid_tile(ChunkCoord2d chunkIndex, ChunkTileCoord2d tileIndex);
std::pair<ChunkCoord2d, ChunkTileCoord2d> grid_to_chunk_tile(GridTileCoord2d tileIndex);

// ---- Rect functions

template<typename T>
CoordRect<T> rect_from_to(Coord2dWrapper<T> from, Coord2dWrapper<T> to) {
	auto [x1, x2] = std::minmax(from.value.x, to.value.x);
	auto [y1, y2] = std::minmax(from.value.y, to.value.y);

	return {x1, y1, x2-x1+1, y2-y1+1};
}

std::vector<std::pair<ChunkCoord2d, ChunkTileRect>> grid_to_chunk_rect(GridTileRect rect);

// ---- Chunk helper functions
size_t coord_to_data_index(ChunkTileCoord2d coord);



} // !grid