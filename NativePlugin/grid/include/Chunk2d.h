#pragma once

#include <utility>
#include <cstdint>

namespace grid {

template <ValidGridData T>
struct Chunk2d {
	explicit Chunk2d(std::array<T, CHUNK_DATA_SIZE>&& data) : buffer(std::move(data), {}) {};

	std::array<std::array<T, CHUNK_DATA_SIZE>, 2> buffer;
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

	/// Function reference used to pass around alghoritms/procedures that run on chunks.
template<typename F, typename T, typename ...Args>
concept Chunk2dAlgorithm = std::invocable<F, Chunk2d<T>&, Args...>;

} // !grid