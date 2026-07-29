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


// ---- Enums
/// A simple flag-enum (using a struct merely for scope) to denote the 8 neighbours of a 2d chunk.
struct EdgeTileType {
	enum : uint8_t { // We use a regular enum here as opposed to an enum class so it automatically converts to a uint8_t; these are glorified constants
		TopEdge = 0b00000001,
		TopRightCorner = 0b00000010,
		RightEdge = 0b00000100,
		BottomRightCorner = 0b00001000,
		BottomEdge = 0b00010000,
		BottomLeftCorner = 0b00100000,
		LeftEdge = 0b01000000,
		TopLeftCorner = 0b10000000
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

} // !grid