#pragma once

#include "Chunk2d.h"
#include "GridDefines.h"
#include "Coord2d.h"

#include <array>

#include <Utils.h>

namespace grid {

template <ValidGridData T>
class AbstractChunk2dFactory {
public:
	virtual std::array<T, CHUNK_DATA_SIZE> generate(ChunkCoord2d) = 0;
};

template <ValidGridData T> requires std::is_default_constructible_v<T>
class DefaultChunk2dFactory final : public AbstractChunk2dFactory<T> {
	DefaultChunk2dFactory() = default;
public:
	std::array<T, CHUNK_DATA_SIZE> generate(ChunkCoord2d) override{
		return std::array<T, CHUNK_DATA_SIZE>{};
	}

	static DefaultChunk2dFactory* get() {
		static DefaultChunk2dFactory f{};
		return &f;
	}
};

template <ValidGridData T>
class FillChunk2dFactory : public AbstractChunk2dFactory<T> {
	const T defaultValue;

public:
	FillChunk2dFactory(T _defaultValue): defaultValue(_defaultValue) {}

	std::array<T, CHUNK_DATA_SIZE> generate(ChunkCoord2d) override {
		return utils::make_filled_array<CHUNK_DATA_SIZE, T>(defaultValue);
	}
};

}