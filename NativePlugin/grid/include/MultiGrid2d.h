#pragma once

#include <tuple>

#include "Grid2d.h"
#include "ChunkFactory.h"

namespace grid {

template <ValidGridData ...Args>
class MultiGrid2d {
public:
	// ---- ctors
	MultiGrid2d(AbstractChunkAlgoRunner* chunkAlgoRunner = &sequentialChunkAlgoRunner) :
		m_Grids{ std::make_tuple<Grid2d<Args>...>(Grid2d<Args>{chunkAlgoRunner}...) } {};

	MultiGrid2d(
		AbstractChunkAlgoRunner* chunkAlgoRunner = &sequentialChunkAlgoRunner,
		AbstractChunk2dFactory<Args>*... gens) : m_Grids({ Grid2d<Args>{chunkAlgoRunner, gens} }...) {}

	// ---- Get grids
	template <ValidGridData T>
	Grid2d<T>* get_grid() { return &std::get<Grid2d<T>>(m_Grids); }

	// ---- Mass grid control functions
	void load_chunk_asleep(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids.load_chunk_asleep(coord), ...);
		}, m_Grids);
	}

	void wake_chunk(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids.wake_chunk(coord), ...);
		}, m_Grids);
	}

	void sleep_chunk(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids.sleep_chunk(coord), ...);
		}, m_Grids);
	}

	void load_chunks_asleep(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids.load_chunks_asleep(rect), ...);
		}, m_Grids);
	}

	void wake_chunks(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids.wake_chunks(rect), ...);
		}, m_Grids);
	}

	void sleep_chunks(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids.sleep_chunks(rect), ...);
		}, m_Grids);
	}

	// ---- Mass sync method
	void sync_halo() {
		std::apply([](auto&... grids) {
			(grids.sync_halo(), ...);
		});
	}

	// ---- algo methods
	template <typename T, typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_chunk(ChunkCoord2d chunkCoord, F&& func, Args&&... args) {
		std::get<Grid2d<T>>(m_Grids).run_on_chunk(chunkCoord, std::forward<F>(func), std::forward<Args>(args)...);
	}

	template <typename T, typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_awake_chunks(F&& func, Args&&... args) {
		std::get<Grid2d<T>>(m_Grids).run_on_awake_chunks(std::forward<F>(func), std::forward<Args>(args)...);
	}

	template <typename T, typename F, typename ...Args> requires Chunk2dAlgorithm<F, T, Args...>
	void run_on_loaded_chunks(F&& func, Args&&... args) {
		std::get<Grid2d<T>>(m_Grids).run_on_loaded_chunks(std::forward<F>(func), std::forward<Args>(args)...);
	}

private:
	std::tuple<Grid2d<Args>...> m_Grids;
};

template <typename T>
concept IsMultiGrid2d = utils::is_specialization_of<T, MultiGrid2d>::value;

template <typename T>
concept IsGridOrMultiGrid2d = IsGrid2d<T> or IsMultiGrid2d<T>;

}