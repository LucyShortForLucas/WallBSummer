#pragma once
#include <cstdint>
#include <functional>

namespace grid {

// ---- Constants
/// The 'width' N defining a chunk as NxN real tiles. Chunks have a halo, making the 'actual' size of the data (N+2) * (N+2)
constexpr int CHUNK_WIDTH = 16;

/// The actual size of the 2-dimensional internal container of a chunk along one axis.
constexpr int CHUNK_DATA_WIDTH = CHUNK_WIDTH + 2;

/// The size of the internal array of a chunk
constexpr int CHUNK_DATA_SIZE = CHUNK_DATA_WIDTH * CHUNK_DATA_WIDTH;

constexpr int CHUNK_NW_INDEX{ CHUNK_DATA_WIDTH + 1 };
constexpr int CHUNK_SW_INDEX{ CHUNK_DATA_WIDTH * (CHUNK_DATA_WIDTH - 2) + 1 };
constexpr int CHUNK_NE_INDEX{ CHUNK_NW_INDEX + CHUNK_WIDTH - 1 };
constexpr int CHUNK_SE_INDEX{ CHUNK_SW_INDEX + CHUNK_WIDTH - 1 };

constexpr int HALO_NW_INDEX{ 0 };
constexpr int HALO_SW_INDEX{ CHUNK_DATA_WIDTH * (CHUNK_DATA_WIDTH - 1) };
constexpr int HALO_NE_INDEX{ CHUNK_DATA_WIDTH - 1 };
constexpr int HALO_SE_INDEX{ CHUNK_DATA_SIZE - 1 };

// ---- Enums
/// A simple flag-enum (using a struct merely for scope) to denote the 8 neighbours of a 2d chunk.
struct EdgeTileDir {
	enum : uint8_t { // We use a regular enum here as opposed to an enum class so it automatically converts to a uint8_t; these are glorified constants
		N =  0b1,		// 1
		E =  0b10,		// 2
		S =  0b100,		// 4
		W =  0b1000,	// 8
		NE = 0b10000,	// 16
		SE = 0b100000,	// 32
		SW = 0b1000000,	// 64
		NW = 0b10000000	// 128
	};
};


// ---- Tag (empty) structs
namespace tag {

struct Grid {};
struct Chunk {};
struct ChunkTile {};
struct GridTile {};

}


// ---- Concepts
template<typename T>
concept ValidGridData = requires { std::is_trivially_copy_assignable_v<T>; };

template <typename T>
concept ExecutionPolicy = requires {std::is_execution_policy_v<T>; };

// ---- Stencils

} // !grid