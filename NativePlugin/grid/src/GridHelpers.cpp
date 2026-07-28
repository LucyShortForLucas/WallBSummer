#include "GridHelpers.h"

#include <Utils.h>

namespace grid {

GridTileCoord2d get_chunk_origin(ChunkCoord2d chunkIndex) {
	return { chunkIndex.value * CHUNK_EXTENDS_2D.value };
}


GridTileCoord2d chunk_to_grid_tile(ChunkCoord2d chunkIndex, ChunkTileCoord2d tileIndex) {
	return { (get_chunk_origin(chunkIndex).value + tileIndex.value) };
}

std::pair<ChunkCoord2d, ChunkTileCoord2d> grid_to_chunk_tile(GridTileCoord2d tileIndex) {
    auto xChunk = utils::floor_div(tileIndex.value.x, CHUNK_EXTENDS_2D.value.x);
    auto yChunk = utils::floor_div(tileIndex.value.y, CHUNK_EXTENDS_2D.value.y);

    return {
        ChunkCoord2d{xChunk, yChunk},
        ChunkTileCoord2d{
            tileIndex.value.x - xChunk * CHUNK_EXTENDS_2D.value.x,
            tileIndex.value.y - yChunk * CHUNK_EXTENDS_2D.value.y
        }
    };
}

size_t coord_to_data_index(ChunkTileCoord2d coord) {
	int x{ coord.value.x + 1 };	// <--┐
	int y{ coord.value.y + 1 };	// <--- Account for halo

	return y * CHUNK_DATA_WIDTH + x;
}

std::vector<std::pair<ChunkCoord2d, ChunkTileRect>> grid_to_chunk_rect(GridTileRect rect) {
    const auto [topLeftChunk, topLeftChunkTile] = grid_to_chunk_tile(rect.coord);
    const auto [bottomRightChunk, bottomRightChunkTile] = grid_to_chunk_tile(rect.coord + GridTileCoord2d{rect.width-1, rect.height-1});

    auto topLeftChunkRect{ rect_from_to(topLeftChunkTile, CHUNK_BOTTOMRIGHT_2D) };
    auto bottomRightChunkRect{ rect_from_to(CHUNK_TOPLEFT_2D, bottomRightChunkTile) };

    if (topLeftChunk.value.y == bottomRightChunk.value.y) {
        int height{ bottomRightChunkTile.value.y - topLeftChunkTile.value.y };
        topLeftChunkRect.height = height;
        bottomRightChunkRect.height = height;
    }

    if (topLeftChunk.value.x == bottomRightChunk.value.x) {
        int width{ bottomRightChunkTile.value.x - topLeftChunkTile.value.x };
        topLeftChunkRect.width = width;
        bottomRightChunkRect.width = width;
    }

    std::vector<std::pair<ChunkCoord2d, ChunkTileRect>> result{};

    for (int y{ topLeftChunk.value.y }; y <= bottomRightChunk.value.y; ++y) {
        for (int x{ topLeftChunk.value.x }; x <= bottomRightChunk.value.x; ++x) {
            ChunkTileRect rect{};
            
            if (x == topLeftChunk.value.x) {
                rect.coord.value.x = topLeftChunkTile.value.x;
                rect.width = topLeftChunkRect.width;
            }
            else if (x == bottomRightChunk.value.x)
                rect.width = bottomRightChunkRect.width;
            else 
                rect.width = CHUNK_WIDTH;

            if (y == topLeftChunk.value.y) {
                rect.coord.value.y = topLeftChunkTile.value.y;
                rect.height = topLeftChunkRect.height;
            }
            else if (y == bottomRightChunk.value.y) 
                rect.height = bottomRightChunkRect.height;
            else 
                rect.height = CHUNK_WIDTH;

            result.emplace_back(ChunkCoord2d{ x, y }, rect);
        }
    }

    return result;
}



} // !grid