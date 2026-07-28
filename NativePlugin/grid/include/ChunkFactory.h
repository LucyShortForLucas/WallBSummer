#pragma once

#include "Chunk2d.h"
#include "GridDefines.h"
#include "Coord2d.h"

#include <array>

#include <Utils.h>

namespace grid {

template<typename F, typename T>
concept Chunk2dFactory = std::invocable<F, ChunkCoord2d>&&
std::same_as<std::invoke_result_t<F, ChunkCoord2d>, std::array<T, CHUNK_DATA_SIZE>>;

template <std::default_initializable T>
struct DefaultChunk2dFactory {
	std::array<T, CHUNK_DATA_SIZE> operator()(ChunkCoord2d) {
		return std::array<T, CHUNK_DATA_SIZE>{};
	}
};

template <typename T>
struct FillChunk2dFactory {
	const T defaultValue;

	FillChunk2dFactory(T _defaultValue): defaultValue(_defaultValue) {}

	std::array<T, CHUNK_DATA_SIZE> operator()(ChunkCoord2d) {
		return utils::make_filled_array<CHUNK_DATA_SIZE, T>(defaultValue);
	}
};

}