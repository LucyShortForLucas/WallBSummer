#include "GridWorldExport.h"

#include <vector>

#include <GridWorld2d.h>

#include "GameGrids.h"

using WorldType = grid::GridWorld2d<FertilityGrid, WaterGrid>;

std::vector<std::unique_ptr<WorldType>> g_GridWordls{};

// World management
PLUGIN_API uint32_t create_gridworld() {
	auto& pWorld{ g_GridWordls.emplace_back(std::make_unique<WorldType>(&grid::sequentialChunkAlgoRunner)) };
	pWorld->init_grids_default();
}

PLUGIN_API void destroy_gridworld(uint32_t id) {
	g_GridWordls[id].reset();
}

PLUGIN_API void update_gridworld(uint32_t id, float deltatime) {
	g_GridWordls[id]->update(deltatime);
}

// Get/set tiles
PLUGIN_API void get_tiles_water_content(uint32_t worldId,
		int32_t x, int32_t y,
		int32_t width, int32_t height,
		int32_t* pOut) {
	auto pGrid{ g_GridWordls[worldId]->get_multigrid<WaterGrid>()->get_grid<tile::WaterContent>() };
	auto tiles{ pGrid->get_tile_rect({ {x,y}, width, height }) };
	std::ranges::transform(tiles, pOut, [](const tile::WaterContent& w) { return w.value; });
}

PLUGIN_API void get_tiles_fertility(uint32_t worldId,
		int32_t x, int32_t y,
		int32_t width, int32_t height,
		int32_t* pOut) {
	auto pGrid{ g_GridWordls[worldId]->get_grid<FertilityGrid>() };
	auto tiles{ pGrid->get_tile_rect({ {x,y}, width, height }) };
	std::ranges::transform(tiles, pOut, [](const tile::Fertility& f) { return f.value; });
}