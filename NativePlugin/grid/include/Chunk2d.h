#pragma once

#include <utility>
#include <cstdint>

#include "Coord2d.h"

namespace grid {

template <ValidGridData T>
class Chunk2d {
private:
	std::array<std::array<T, CHUNK_DATA_SIZE>, 2> buffer;

public:
	std::recursive_mutex mutex{};
	const ChunkCoord2d coord;
	uint8_t dirtyEdges{ 0 };

	Chunk2d(ChunkCoord2d pos, std::array<T, CHUNK_DATA_SIZE>&& data) : buffer(std::move(data), {}), coord(pos) {};

	std::array<T, CHUNK_DATA_SIZE>& current_data_buffer() {
		return buffer[dataBufferIndex];
	}

	const std::array<T, CHUNK_DATA_SIZE>& read_buffer() const {
		return buffer[dataBufferIndex];
	}

	std::array<T, CHUNK_DATA_SIZE>& write_buffer() {
		return buffer[dataBufferIndex ^ 1];
	}

	void swap_buffers() {
		dataBufferIndex = dataBufferIndex ^ 1;
	}

private: 
	uint8_t dataBufferIndex{ 0 };
};

} // !grid