#pragma once

#include "Chunk2d.h"
#include "GridDefines.h"
#include "Coord2d.h"

#include <array>

namespace grid {

template<typename F, typename T>
concept Chunk2dFactory = std::invocable<F, ChunkCoord2d>&&
std::same_as<std::invoke_result_t<F, ChunkCoord2d>, std::array<T, CHUNK_DATA_SIZE>>;

template <std::default_initializable T>
std::array<T, CHUNK_DATA_SIZE> DefaultChunk2dFactory(ChunkCoord2d) {
	return std::array<T, CHUNK_DATA_SIZE>{};
}

}