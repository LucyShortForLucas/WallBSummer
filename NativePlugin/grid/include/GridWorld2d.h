#pragma once

#include "MultiGrid2d.h"

#include <concepts>
#include <utility>
#include <tuple>

namespace grid {

template <IsMultiGrid2d ...GridArgs>
class GridWorld2d {
public:
	// ---- typedefs
	
	template <IsMultiGrid2d T>
	using UpdateFunctionPtr = void(*)(T*, float);
	
	// ---- Ctor and grid lifetime methods
	explicit GridWorld2d(AbstractChunkAlgoRunner* pRunner) : m_pAlgoRunner(pRunner) {
	}
	
	template <typename T, typename ...Args>
	void init_grid(Args&& ...args) {
		auto& pGrid{ std::get<std::unique_ptr<T>>(m_Grids) };
		if (pGrid) return;
		pGrid = std::make_unique<T>(m_pAlgoRunner, std::forward<Args>(args)...);
	}

	void init_grids_default() {
		(init_grid<GridArgs>(), ...);
	}

	template <typename T>
	void free_grid() {
		std::unique_ptr<T>& pGrid{ std::get<std::unique_ptr<T>>(m_Grids) };
		if (!pGrid) return;
		pGrid.reset();
	}

	// ---- Getters
	template <IsMultiGrid2d T>
	T* get_multigrid() { return std::get<std::unique_ptr<T>>(m_Grids).get(); };

	// ---- Update methods

	void update(float deltaTime) {
		utils::for_each_pair(m_Grids, m_UpdateAlgos, [deltaTime](auto& pGrid, auto& updateFunc) {
			if (updateFunc == nullptr || pGrid == nullptr) 
				return;
			updateFunc(pGrid.get(), deltaTime);
		});
	}

	template <typename T>
	void set_update(UpdateFunctionPtr<T>&& func) {
		std::get<UpdateFunctionPtr<T>>(m_UpdateAlgos) = std::forward(func);
	}

		// ---- Mass grid control functions
	void load_chunk_asleep(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids->load_chunk_asleep(coord), ...);
			}, m_Grids);
	}

	void wake_chunk(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids->wake_chunk(coord), ...);
			}, m_Grids);
	}

	void sleep_chunk(ChunkCoord2d coord) {
		std::apply([coord](auto&... grids) {
			(grids->sleep_chunk(coord), ...);
			}, m_Grids);
	}

	void load_chunks_asleep(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids->load_chunks_asleep(rect), ...);
			}, m_Grids);
	}

	void wake_chunks(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids->wake_chunks(rect), ...);
			}, m_Grids);
	}

	void sleep_chunks(ChunkRect rect) {
		std::apply([rect](auto&... grids) {
			(grids->sleep_chunks(rect), ...);
			}, m_Grids);
	}

private:
	std::tuple<std::unique_ptr<GridArgs>...> m_Grids{};
	std::tuple<UpdateFunctionPtr<GridArgs>...> m_UpdateAlgos{};
	AbstractChunkAlgoRunner* m_pAlgoRunner;
};

} // !grid