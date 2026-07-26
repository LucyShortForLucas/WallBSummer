#pragma once

#include <utility>

#include "GridDefines.h"
#include "Coord2d.h"

namespace grid {

// ---- Tile conversion functions
GridTileCoord2d get_chunk_origin(ChunkCoord2d chunkIndex);
GridTileCoord2d chunk_to_grid_tile(ChunkCoord2d chunkIndex, ChunkTileCoord2d tileIndex);
std::pair<ChunkCoord2d, ChunkTileCoord2d> grid_to_chunk_tile(GridTileCoord2d tileIndex);

// ---- Chunk helper functions
size_t coord_to_data_index(ChunkTileCoord2d coord);

} // !grid