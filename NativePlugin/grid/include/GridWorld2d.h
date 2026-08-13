#pragma once

#include "MultiGrid2d.h"

#include <concepts>
#include <utility>
#include <tuple>

namespace grid {

template <IsGridOrMultiGrid2d ...GridArgs>
class GridWorld2d {
public:
	// ---- typedefs
	
	template <ValidGridData T>
	using UpdateFunction = std::function<void(Chunk2d<T>*, Grid2d<T>*, float)>;
	
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
	template <IsGrid2d T>
	T* get_grid() { return std::get<std::unique_ptr<T>>(m_Grids).get(); };

	template <IsMultiGrid2d T>
	T* get_multigrid() { return std::get<std::unique_ptr<T>>(m_Grids).get(); };

	// ---- Update methods

	void update(float deltaTime) {
		(std::get<std::unique_ptr<GridArgs>>(m_Grids)->run_on_awake_chunk(
			std::get<UpdateFunction<GridArgs>>(m_UpdateAlgos) ? std::get<UpdateFunction<GridArgs>>(m_UpdateAlgos) : [](Chunk2d<GridArgs>* c, Grid2d<GridArgs>* g) {},
			deltaTime
		), ...);
	}

	template <typename T>
	void set_update(UpdateFunction<T>&& func) {
		std::get<UpdateFunction<T>>(m_UpdateAlgos) = std::forward(func);
	}

private:
	std::tuple<std::unique_ptr<GridArgs>...> m_Grids{};
	std::tuple<UpdateFunction<GridArgs>...> m_UpdateAlgos{};
	AbstractChunkAlgoRunner* m_pAlgoRunner;
};

} // !grid