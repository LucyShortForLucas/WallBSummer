#pragma once

#include <utility>
#include <cstdint>

#include "Coord2d.h"

namespace grid {

template <ValidGridData T>
struct Chunk2d {
	explicit Chunk2d(ChunkCoord2d pos, std::array<T, CHUNK_DATA_SIZE>&& data) : buffer(std::move(data), {}), coord(pos) {};

	std::array<std::array<T, CHUNK_DATA_SIZE>, 2> buffer;
	ChunkCoord2d coord;
	uint8_t dirtyEdges{ 0 };

	std::array<T, CHUNK_DATA_SIZE>& current_data_buffer() {
		return buffer[dataBufferIndex];
	}

	void swap_buffers() {
		if (dataBufferIndex != 0)
			dataBufferIndex = 0;
		else
			dataBufferIndex = 1;
	}

private:
	uint8_t dataBufferIndex{ 0 };
};

} // !grid