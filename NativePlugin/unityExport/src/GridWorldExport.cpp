#include "GridWorldExport.h"

#include <vector>

#include <GridWorld2d.h>

#include "GameGrids.h"

using WorldType = grid::GridWorld2d<FertilityGrid, WaterGrid>;

std::vector<std::unique_ptr<WorldType>> g_GridWorlds{};

template <typename ReturnType, typename... Args>  
using FnPtr = ReturnType(*)(Args...);

// ---- World management
std::vector<FnPtr<void, uint32_t>> g_worldTypeInitFuncs{
	/* 0, default */ [](uint32_t worldId) { g_GridWorlds[worldId]->init_grids_default(); },
	/* 1, TEST */	 [](uint32_t worldId) {
		auto& pGrid {g_GridWorlds[worldId]};
		pGrid->init_grids_default();
		auto pFertilityGrid{ pGrid->get_multigrid<FertilityGrid>()->get_grid<tile::Fertility>() };
		pFertilityGrid->set_tile({0,0}, {1000});
	}
};

PLUGIN_API uint32_t create_gridworld(uint32_t worldType) {
	auto index{ g_GridWorlds.size()};
	auto& pWorld{ g_GridWorlds.emplace_back(std::make_unique<WorldType>(&grid::sequentialChunkAlgoRunner)) };
	g_worldTypeInitFuncs.at(worldType)(index);
	return index;
}

PLUGIN_API void destroy_gridworld(uint32_t id) {
	g_GridWorlds[id].reset();
}

PLUGIN_API void update_gridworld(uint32_t id, float deltatime) {
	g_GridWorlds[id]->update(deltatime);
}

// ---- Chunk state management
std::vector<FnPtr<void, uint32_t, int32_t, int32_t, int32_t, int32_t>> g_ChunkStateFuncs{
	/* 0, load_chunks_asleep */ [](uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h) {  
		auto& pWorld{ g_GridWorlds[worldId] };
		pWorld->load_chunks_asleep({ {x, y}, w, h });
	},
	/* 1, wake_chunks */		[](uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h) { 
		auto& pWorld{ g_GridWorlds[worldId] };
		pWorld->wake_chunks({ {x, y}, w, h });
	},
	/* 2, sleep_chunks */		[](uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h) {  
		auto& pWorld{ g_GridWorlds[worldId] };
		pWorld->sleep_chunks({ {x, y}, w, h });
	}
};

PLUGIN_API void manage_chunks(uint32_t worldId, uint32_t funcId,
	int32_t x, int32_t y, 
	int32_t width, int32_t height) {
	g_ChunkStateFuncs.at(funcId)(worldId, x, y, width, height);
} 

// ---- Get/set tiles

template <typename Mg, typename T>
std::vector<T> GetTilesFromMultiGrid(uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h) {
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid<grid::Grid2d<T>>() };
	return pGrid->get_tile_rect({ {x,y}, w, h });
}

template <typename Mg, typename T>
void GetTilesFromMultiGridAndTransformValue(uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h, void* pOut) {
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid< grid::Grid2d<T>>() };
	std::vector<T> tiles{ pGrid->get_tile_rect({ {x,y}, w, h }) };
	std::ranges::transform(tiles, static_cast<decltype(T::value)*>(pOut), [](const T& w) { return w.value; });
}

std::vector<FnPtr<void, uint32_t, int32_t, int32_t, int32_t, int32_t, void*>> g_GetTileFuncs{
	/* 0, water content*/	GetTilesFromMultiGridAndTransformValue<WaterGrid, tile::WaterContent>,
	/* 1, water type*/		GetTilesFromMultiGridAndTransformValue<WaterGrid, tile::WaterTileType>,
	/* 2, fertility*/		GetTilesFromMultiGridAndTransformValue<FertilityGrid, tile::Fertility>
};


PLUGIN_API void get_tile_data(uint32_t worldId, uint32_t tileDataType,
		int32_t x, int32_t y,
		int32_t width, int32_t height,
		void* pOut) {
	g_GetTileFuncs.at(tileDataType)(worldId, x, y, width, height, pOut);
}

template <typename Mg, typename T>
void fill_tiles_in_multiGrid(uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h, void* pIn) {
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid<grid::Grid2d<T>>() };
	pGrid->fill_tile_rect({ {x,y},w,h }, T{ *static_cast<const decltype(T::value)*>(pIn) });
}

std::vector<FnPtr<void, uint32_t, int32_t, int32_t, int32_t, int32_t, void*>> g_SetTileFuncs{
	/* 0, water content*/	fill_tiles_in_multiGrid<WaterGrid, tile::WaterContent>,
	/* 1, water type*/		fill_tiles_in_multiGrid<WaterGrid, tile::WaterTileType>,
	/* 2, fertility*/		fill_tiles_in_multiGrid<FertilityGrid, tile::Fertility>,
};

PLUGIN_API void fill_tile_data(uint32_t worldId, uint32_t tileDataType,
	int32_t x, int32_t y,
	int32_t width, int32_t height,
	void* pIn) {
	g_SetTileFuncs.at(tileDataType)(worldId, x, y, width, height, pIn);
}