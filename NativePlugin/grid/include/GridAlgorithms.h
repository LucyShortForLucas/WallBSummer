#pragma once

#include "Chunk2d.h"
#include "GridDefines.h"

namespace grid {

// ---- Helper constants

// ---- Stencil sweep algo templates

template <typename T, typename R, typename Op> requires
	std::invocable<Op, const T&, const T&, const T&> &&
	std::same_as<std::invoke_result_t<Op, const T&, const T&, const T&>, R>
void row_reduce_3x3(const std::array<T, CHUNK_DATA_SIZE>& in, std::array<R, CHUNK_DATA_SIZE>& intermediateOut, Op op) {
	for (int y{}; y < CHUNK_DATA_WIDTH; ++y) {
		for (int x{ 1 }; x <= CHUNK_WIDTH; ++x) {
			int index{ x + y * CHUNK_DATA_WIDTH };
			intermediateOut[index] = op(in[index - 1], in[index], in[index + 1]);
		}
	}
}

template <typename R, typename T, typename Op> requires
std::invocable<Op, const R&, const R&, const R&>&&
std::same_as<std::invoke_result_t<Op, const R&, const R&, const R&>, T>
void column_reduce_3x3(
	const std::array<R, CHUNK_DATA_SIZE>& intermediateIn,
	std::array<T, CHUNK_DATA_SIZE>& out,
	Op op) {
	for (int y{1}; y <= CHUNK_WIDTH; ++y) {
		const R*  r0{ intermediateIn.data() + CHUNK_DATA_WIDTH * (y - 1)};
		const R*  r1{ intermediateIn.data() + CHUNK_DATA_WIDTH * (y) };
		const R*  r2{ intermediateIn.data() + CHUNK_DATA_WIDTH * (y + 1) };
		for (int x{ 1 }; x <= CHUNK_WIDTH; ++x) {
			int index{ x + y * CHUNK_DATA_WIDTH };
			out[index] = op(r0[x], r1[x], r2[x]);
		}
	}
}

template <typename R, typename T, typename OpCol, typename OpEdge> requires
	std::invocable<OpCol, const R&, const R&, const R&> &&
	std::invocable<OpEdge, const T&, const T&> &&
	std::same_as<std::invoke_result_t<OpCol, const R&, const R&, const R&>, T> &&
	std::same_as<std::invoke_result_t<OpEdge, const T&, const T&>, R>
void column_reduce_3x3_exclusive(
	const std::array<R, CHUNK_DATA_SIZE>& intermediateIn,
	std::array<T, CHUNK_DATA_SIZE>& out,
	OpCol opCol, OpEdge opEdge
	) {
	for (int y{ 1 }; y <= CHUNK_WIDTH; ++y) {

		const R* r0{ intermediateIn.data() + CHUNK_DATA_WIDTH * (y - 1) };
		const R* r2{ intermediateIn.data() + CHUNK_DATA_WIDTH * (y + 1) };

		for (int x{ 1 }; x <= CHUNK_WIDTH; ++x) {
			
			int index{ x + y * CHUNK_DATA_WIDTH };
			auto r1v = opEdge(intermediateIn[index - 1], intermediateIn[index + 1]);
			out[index] = opCol(r0[x], r1v, r2[x]);
		}
	}
}

template <typename T>
uint8_t flag_nonequal_edges(const std::array<T, CHUNK_DATA_SIZE>& a, const std::array<T, CHUNK_DATA_SIZE>& b) {
	uint8_t flags{};

	// NE
	if (a[CHUNK_NE_INDEX] == b[CHUNK_NE_INDEX])
		flags |= EdgeTileDir::N | EdgeTileDir::NE | EdgeTileDir::E;

	// SE
	if (a[CHUNK_SE_INDEX] == b[CHUNK_SE_INDEX])
		flags |= EdgeTileDir::S | EdgeTileDir::SE | EdgeTileDir::E;

	// NW
	if (a[CHUNK_NW_INDEX] == b[CHUNK_NW_INDEX])
		flags |= EdgeTileDir::N | EdgeTileDir::NW | EdgeTileDir::W;

	// NW
	if (a[CHUNK_NW_INDEX] == b[CHUNK_NW_INDEX])
		flags |= EdgeTileDir::N | EdgeTileDir::NW | EdgeTileDir::W;

	// N
	if (!(flags & EdgeTileDir::N)) {
		const auto itAn{ a.begin() + 1 + CHUNK_DATA_WIDTH };
		const auto itBn{ b.begin() + 1 + CHUNK_DATA_WIDTH };
		if (std::equal(itAn, itAn + CHUNK_WIDTH, itBn, itBn + CHUNK_WIDTH))
			flags |= EdgeTileDir::N;
	}

	// S
	if (!(flags & EdgeTileDir::S)) {
		const auto itAs{ a.begin() + 1 + CHUNK_DATA_WIDTH * (CHUNK_DATA_WIDTH-2) };
		const auto itBs{ b.begin() + 1 + CHUNK_DATA_WIDTH * (CHUNK_DATA_WIDTH - 2) };
		if (std::equal(itAs, itAs + CHUNK_WIDTH, itBs, itBs + CHUNK_WIDTH))
			flags |= EdgeTileDir::S;
	}

	// E
	if (!(flags & EdgeTileDir::E)) {
		const int indexE{ CHUNK_DATA_WIDTH - 2 };
		for (int i{}; i < CHUNK_WIDTH; ++i) {
			if (a[indexE + i * CHUNK_DATA_WIDTH] != b[indexE + i * CHUNK_DATA_WIDTH]) {
				flags |= EdgeTileDir::E;
				break;
			}
		}
	}

	// W
	if (!(flags & EdgeTileDir::W)) {
		const int indexW{ CHUNK_DATA_WIDTH + 1 };
		for (int i{}; i < CHUNK_WIDTH; ++i) {
			if (a[indexW + i * CHUNK_DATA_WIDTH] != b[indexW + i * CHUNK_DATA_WIDTH]) {
				flags |= EdgeTileDir::E;
				break;
			}
		}
	}

	return flags;
}

}