#include "GridHelpers.h"

namespace grid {

GridTileCoord2d get_chunk_origin(ChunkCoord2d chunkIndex) {
	return { chunkIndex.value * CHUNK_EXTENDS_2D.value };
}


GridTileCoord2d chunk_to_grid_tile(ChunkCoord2d chunkIndex, ChunkTileCoord2d tileIndex) {
	return { (get_chunk_origin(chunkIndex).value + tileIndex.value) };
}

std::pair<ChunkCoord2d, ChunkTileCoord2d> grid_to_chunk_tile(GridTileCoord2d tileIndex) {
	return std::make_pair(
		ChunkCoord2d{ tileIndex.value / CHUNK_EXTENDS_2D.value },
		ChunkTileCoord2d{ tileIndex.value % CHUNK_EXTENDS_2D.value }
	);
}

size_t coord_to_data_index(ChunkTileCoord2d coord) {
	int x{ coord.value.x + 1 };	// <--┐
	int y{ coord.value.y + 1 };	// <--- Account for halo

	return y * CHUNK_DATA_WIDTH + x;
}

} // !grid