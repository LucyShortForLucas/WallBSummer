#pragma once

#include "GridDefines.h"

namespace grid {

// ---- Types

/// Basic 2-d point type intended for spatial indexing in 2d chunks and grids
struct Coord2d {
	int x;
	int y;

	const auto operator<=>(const Coord2d& other) const = default;

#define MATH_OP(o) Coord2d&  operator##o##=(const Coord2d& rh) { x ##o##= rh.x; y ##o##= rh.y;  return *this; }
	MATH_OP(+)
	MATH_OP(-)
	MATH_OP(*)
	MATH_OP(/)
	MATH_OP(%)
}; // !Coord2d

#undef MATH_OP
#define MATH_OP(o) Coord2d  operator##o##(const Coord2d& lh, const Coord2d& rh) { 	Coord2d result{ lh }; result ##o##= rh; return result; }
MATH_OP(+)
MATH_OP(-)
MATH_OP(*)
MATH_OP(/ )
MATH_OP(%)

/// Simple wrapper to allow different index types 
template<typename T>
struct Coord2dWrapper {
	Coord2d value;
#undef MATH_OP
#define MATH_OP(o) Coord2dWrapper<T>&  operator##o##=(const Coord2dWrapper<T>& rh) { value ##o##= rh.value;  return *this; }
	MATH_OP(+)
	MATH_OP(-)
	MATH_OP(*)
	MATH_OP(/)
	MATH_OP(%)
}; // !Coord2dWrapper

#undef MATH_OP
#define MATH_OP(o) template<typename T> Coord2dWrapper<T>  operator##o##(const Coord2dWrapper<T>& lh, const Coord2dWrapper<T>& rh) { 	Coord2dWrapper<T> result{ lh }; result ##o##= rh; return result; }
MATH_OP(+)
MATH_OP(-)
MATH_OP(*)
MATH_OP(/)
MATH_OP(%)

#undef MATH_OP

// ---- Tagged typedefs

	/// Each of these is an identical implementation of Coord2d, but are separate types as to not confuse or mix them.
	/// Each of these coordinate types represent a coordinate in a different space, and it does not make logical sense
	/// to do arithmetic between them, hence they are seperate types to enforce this and make the meaning of a coord
	/// clear. 
	
using ChunkCoord2d = Coord2dWrapper<tag::Chunk>;
using ChunkTileCoord2d = Coord2dWrapper<tag::ChunkTile>;
using GridTileCoord2d = Coord2dWrapper<tag::GridTile>;

// ---- Helper constants

constexpr GridTileCoord2d CHUNK_EXTENDS_2D{CHUNK_WIDTH, CHUNK_WIDTH};

} // !grid


// ---- std::hash implementation for Index2d and wrappers

template<>
struct std::hash<grid::Coord2d> {
	std::size_t operator()(const grid::Coord2d& index) const noexcept {
		std::size_t h1 = std::hash<int>{}(index.x);
		std::size_t h2 = std::hash<int>{}(index.y);

		// Hash combine (similar to boost::hash_combine)
		return h1 ^ (h2 + 0x9e3779b9 + (h1 << 6) + (h1 >> 2));
	}
};

template<typename T>
struct std::hash<grid::Coord2dWrapper<T>> {
	std::size_t operator()(const grid::Coord2dWrapper<T>& wrapper) const noexcept {
		return std::hash(wrapper.value);
	}
};