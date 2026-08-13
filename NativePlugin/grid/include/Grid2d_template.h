///================================================================================================ 
/// This file is the template header for the Grid2d class. It does not include the template's
/// method definitions, only its declarations, for the sake of readability and indexing its methods
/// 
/// Being a template, this class does not have a corresponding source file. Instead, its methods
/// are defined in the header file "Grid2.h". Always include that file if you require the Grid2d
/// class, NEVER include this one.
///================================================================================================

#pragma once

#include <array>
#include <unordered_map>
#include <concepts>
#include <execution>
#include <functional>
#include <mutex>

#include "GridDefines.h"
#include "GridHelpers.h"

#include "Chunk2d.h"
#include "ChunkFactory.h"

namespace grid {	

template <ValidGridData T>
class Grid2d;
										
template<typename F, typename T, typename ...Args>
concept Chunk2dAlgorithm = std::invocable<F, Chunk2d<T>*, Grid2d<T>*, Args...>;
																	
class AbstractChunkAlgoRunner {							/// ╔═════════════════════════════════════════════════════════════════════╗
public:													/// ║ We use the strategy pattern to select, at runtime, how we want the  ║
	virtual void Run(std::function<void()> func) = 0;	/// ║ grid to execute chunk-based alghoritms. The default strategy is	  ║
};														/// ║ simply to run the algo sequentially for all chunks. Note that this  ║
														/// ║ strategy carries no state and thus is a nameless type with a single ║
class : public AbstractChunkAlgoRunner {				/// ║ global instance, as to not pollute the namespace with useless types.║
public:													/// ╚═════════════════════════════════════════════════════════════════════╝
	void Run(std::function<void()> func) override {		
		func();														
	}															
} sequentialChunkAlgoRunner;										

template <ValidGridData T>
class Grid2d final {
public:
	using Chunk = Chunk2d<T>;

	// ---- Ctor and co
	//Grid2d() : Grid2d(&sequentialChunkAlgoRunner, DefaultChunk2dFactory<T>::get()) {}
	Grid2d(AbstractChunk2dFactory<T>* gen = DefaultChunk2dFactory<T>::get(), AbstractChunkAlgoRunner* chunkAlgoRunner = &sequentialChunkAlgoRunner) : m_ChunkFactory(gen), m_pAlgoRunner(chunkAlgoRunner) {};
	explicit Grid2d(AbstractChunkAlgoRunner* chunkAlgoRunner) : m_ChunkFactory(DefaultChunk2dFactory<T>::get()), m_pAlgoRunner(chunkAlgoRunner) {};

	~Grid2d() = default;

	Grid2d(Grid2d&& other) noexcept
		: m_Chunks(std::move(other.m_Chunks))
		, m_LoadedChunks(std::move(other.m_LoadedChunks))
		, m_AwakeChunkCount(other.m_AwakeChunkCount)
		, m_ChunkFactory(other.m_ChunkFactory)
		, m_pAlgoRunner(other.m_pAlgoRunner)
		, m_DirtyChunks(std::move(other.m_DirtyChunks))
	{
	}

	Grid2d& operator=(Grid2d&& other) noexcept {
		if (this != &other) {
			std::scoped_lock lock(m_ChunksMutex, other.m_ChunksMutex);
			m_Chunks = std::move(other.m_Chunks);
			m_LoadedChunks = std::move(other.m_LoadedChunks);
			m_AwakeChunkCount = other.m_AwakeChunkCount;
			m_ChunkFactory = other.m_ChunkFactory;
			m_pAlgoRunner = other.m_pAlgoRunner;
			m_DirtyChunks = std::move(other.m_DirtyChunks);
			other.m_ChunkFactory = nullptr;
			other.m_pAlgoRunner = nullptr;
		}
		return *this;
	}

	// ---- Get/set tiles
	T		get_tile(GridTileCoord2d coord);
	void	set_tile(GridTileCoord2d coord, T value);
	std::vector<T>	get_tile_rect(GridTileRect rect);
	void			fill_tile_rect(GridTileRect rect, T value); 

	// ---- Chunk algorithm methods
	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_chunk(ChunkCoord2d chunkCoord, F&& func, Args&&... args);

	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_awake_chunks(F&& func, Args&&... args);

	template <typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_loaded_chunks(F&& func, Args&&... args);

	// ---- control chunks
	void load_chunk_asleep(ChunkCoord2d coord);
	void wake_chunk(ChunkCoord2d coord);
	void sleep_chunk(ChunkCoord2d coord);
		
		// rect overloads
	void load_chunks_asleep(ChunkRect rect);
	void wake_chunks(ChunkRect rect);
	void sleep_chunks(ChunkRect rect);

	// ---- Halo
	void sync_dirty_halos();
	void mark_chunk_dirty(ChunkCoord2d chunk, uint8_t dirtyFlags);

	// ---- Info
	int loaded_chunk_count();
	int awake_chunk_count();
	int sleeping_chunk_count();

private: 
	std::unordered_map<ChunkCoord2d, std::unique_ptr<Chunk>> m_Chunks{};
	std::mutex m_ChunksMutex{};

	/// This vector stores a pointer to all currently loaded chunks. The vector is sorted in such a way that
	/// all 'awake' chunks are in the front, and all asleep chunks in the back. The int member ``m_AwakeChunkCount``
	/// keeps track of how many chunks are currently awake.
	std::vector<Chunk*> m_LoadedChunks{};
	int m_AwakeChunkCount{};

	AbstractChunk2dFactory<T>* m_ChunkFactory;

	AbstractChunkAlgoRunner* m_pAlgoRunner{};

	std::vector<ChunkCoord2d> m_DirtyChunks{};
	std::mutex m_DirtyChunksMutex{};
};

template <typename T>
concept IsGrid2d = utils::is_specialization_of<T, Grid2d>::value;

}

/// A simple macro to ensure this header is not accidentally included unless absolutely intended.
#ifndef SUPRESS_GRID2D_TEMPLATE_INCLUDE_ERROR
#error
#endif