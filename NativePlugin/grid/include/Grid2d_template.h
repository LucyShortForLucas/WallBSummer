///============================================================================ 
/// This file is the template header for the Grid2d class. It does not include
/// the template's method definitions, only its declarations, for the sake of 
/// readability and indexing its methods.
/// 
/// Being a template, this class does not have a corresponding source file. 
/// Instead, its methods are defined in the header file "Grid2.h". Always
/// include that file if you require the Grid2d class, NEVER include this one.
///============================================================================ 

#pragma once

#include <array>
#include <unordered_map>
#include <concepts>
#include <execution>

#include "GridDefines.h"
#include "GridHelpers.h"

#include "Chunk2d.h"
#include "ChunkFactory.h"

namespace grid {

template <typename T, Chunk2dFactory<T> ChunkGen>
class Grid2d {
public:
	using Chunk = Chunk2d<T>;

	// ---- Ctor and co
	Grid2d(ChunkGen gen = DefaultChunk2dFactory<T>);

	// ---- Get/set tiles
	T		get_tile(GridTileCoord2d coord);
	void	set_tile(GridTileCoord2d coord, T value);
	std::vector<T>	get_tile_rect(GridTileCoord2d coord, int width, int height);
	void			fill_tile_rect(GridTileCoord2d coord, int width, int height, T value);

	// ---- Chunk algorithm methods
	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_chunk(ChunkCoord2d chunkCoord, F&& func, Args&&... args);

	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_awake_chunks(ExecutionPolicy policy, F&& func, Args&&... args);

	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_loaded_chunks(ExecutionPolicy policy, F&& func, Args&&... args);

	// ---- control chunks
	void load_chunk_asleep(ChunkCoord2d coord);
	void wake_chunk(ChunkCoord2d coord);
	void sleep_chunk(ChunkCoord2d coord);

	// ---- Halo
	void sync_dirty_halos();

	// ---- Info
	int loaded_chunk_count();
	int awake_chunk_count();
	int sleeping_chunk_count();

private: 
	std::unordered_map<ChunkCoord2d, Chunk> m_Chunks{};
	std::vector<Chunk*> m_DirtyEdgeChunks{};

	/// This vector stores a pointer to all currently loaded chunks. The vector is sorted in such a way that
	/// all 'awake' chunks are in the front, and all asleep chunks in the back. The int member ``m_AwakeChunkCount``
	/// keeps track of how many chunks are currently awake.
	std::vector<Chunk*> m_LoadedChunks{};
	int m_AwakeChunkCount;

	ChunkGen m_ChunkFactory;
};

}

/// A simple macro to ensure this header is not accidentally included unless absolutely intended.
#ifndef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR
#error
#endif