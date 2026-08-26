#include "GridWorldExport.h"

#include <vector>

#include <GridWorld2d.h>

#include "GameGrids.h"
#include "GridWorldInfo.h"
#include "Updates.h"
#include "UnityDefines.h"

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
		const int f = gridWorldInfo.maxFertility;


		// left block (still life, anchors the gun)
		pFertilityGrid->fill_tile_rect({ {0, 4}, 2, 2 }, { f });

		// left Q-shape
		pFertilityGrid->fill_tile_rect({ {10, 4}, 1, 3 }, { f }); // (10,4)(10,5)(10,6)
		pFertilityGrid->fill_tile_rect({ {11, 3}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {11, 7}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {12, 2}, 2, 1 }, { f }); // (12,2)(13,2)
		pFertilityGrid->fill_tile_rect({ {12, 8}, 2, 1 }, { f }); // (12,8)(13,8)
		pFertilityGrid->fill_tile_rect({ {14, 5}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {15, 3}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {15, 7}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {16, 4}, 1, 3 }, { f }); // (16,4)(16,5)(16,6)
		pFertilityGrid->fill_tile_rect({ {17, 5}, 1, 1 }, { f });

		// middle block
		pFertilityGrid->fill_tile_rect({ {20, 2}, 2, 3 }, { f }); // (20,2)(21,2)(20,3)(21,3)(20,4)(21,4)
		pFertilityGrid->fill_tile_rect({ {22, 1}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {22, 5}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {24, 0}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {24, 5}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {24, 6}, 1, 1 }, { f });

		// right block (still life)
		pFertilityGrid->fill_tile_rect({ {34, 2}, 2, 2 }, { f });

		// left block
		pFertilityGrid->fill_tile_rect({ {1, 65}, 2, 2 }, { f });

		// left ship
		pFertilityGrid->fill_tile_rect({ {11, 65}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {12, 64}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {12, 68}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {13, 63}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {13, 69}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {14, 63}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {14, 69}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {15, 66}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {16, 64}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {16, 68}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {17, 65}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {18, 66}, 1, 1 }, { f });

		// right ship
		pFertilityGrid->fill_tile_rect({ {21, 63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {22, 63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {23, 62}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {23, 66}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {25, 61}, 1, 2 }, { f });
		pFertilityGrid->fill_tile_rect({ {25, 66}, 1, 2 }, { f });

		// right ship
		pFertilityGrid->fill_tile_rect({ {31, 63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {32, 63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {33, 62}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {33, 66}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {35, 61}, 1, 2 }, { f });
		pFertilityGrid->fill_tile_rect({ {35, 66}, 1, 2 }, { f });

		// right ship
		pFertilityGrid->fill_tile_rect({ {51, -63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {52, -63}, 1, 3 }, { f });
		pFertilityGrid->fill_tile_rect({ {53, -62}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {53, -66}, 1, 1 }, { f });
		pFertilityGrid->fill_tile_rect({ {55, -61}, 1, 2 }, { f });
		pFertilityGrid->fill_tile_rect({ {55, -66}, 1, 2 }, { f });

		// right block
		pFertilityGrid->fill_tile_rect({ {35, 33}, 2, 2 }, { f });

		// right block
		pFertilityGrid->fill_tile_rect({ {21, 65}, 3, 2 }, { f });

		// right block
		pFertilityGrid->fill_tile_rect({ {32, 44}, 3, 1 }, { f });

		// right block
		pFertilityGrid->fill_tile_rect({ {17, 8}, 2, 2 }, { f });


		pFertilityGrid->sync_dirty_halos();

		// right block (still life)
		pFertilityGrid->fill_tile_rect({ {34, 62}, 2, 2 }, { f });
		pGrid->set_update<FertilityGrid>(conway_fertility);
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
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid<T>() };
	return pGrid->get_tile_rect({ {x,y}, w, h });
}

template <typename Mg, typename T>
void GetTilesFromMultiGridAndTransformValue(uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h, void* pOut) {
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid< T>() };
	std::vector<T> tiles{ pGrid->get_tile_rect({ {x,y}, w, h }) };
	std::ranges::transform(tiles, static_cast<decltype(T::value)*>(pOut), [](const T& w) { return w.value; });
}

std::vector<FnPtr<void, uint32_t, int32_t, int32_t, int32_t, int32_t, void*>> g_GetTileFuncs{
	/* 0, water content*/		GetTilesFromMultiGridAndTransformValue<WaterGrid, tile::WaterContent>,
	/* 1, water type*/			GetTilesFromMultiGridAndTransformValue<WaterGrid, tile::WaterTileType>,
	/* 2, fertility*/			GetTilesFromMultiGridAndTransformValue<FertilityGrid, tile::Fertility>,
	/* 2, Build Obstruction*/	GetTilesFromMultiGridAndTransformValue<BuildGrid, tile::BuildObstructionType>
};


PLUGIN_API void get_tile_data(uint32_t worldId, uint32_t tileDataType,
		int32_t x, int32_t y,
		int32_t width, int32_t height,
		void* pOut) {
	g_GetTileFuncs.at(tileDataType)(worldId, x, y, width, height, pOut);
}

template <typename Mg, typename T>
void fill_tiles_in_multiGrid(uint32_t worldId, int32_t x, int32_t y, int32_t w, int32_t h, void* pIn) {
	grid::Grid2d<T>* pGrid{ g_GridWorlds[worldId]->get_multigrid<Mg>()->get_grid<T>() };
	pGrid->fill_tile_rect({ {x,y},w,h }, T{ *static_cast<const decltype(T::value)*>(pIn) });
}

std::vector<FnPtr<void, uint32_t, int32_t, int32_t, int32_t, int32_t, void*>> g_SetTileFuncs{
	/* 0, water content*/		fill_tiles_in_multiGrid<WaterGrid, tile::WaterContent>,
	/* 1, water type*/			fill_tiles_in_multiGrid<WaterGrid, tile::WaterTileType>,
	/* 2, fertility*/			fill_tiles_in_multiGrid<FertilityGrid, tile::Fertility>,
	/* 3, Build Obstruction*/	fill_tiles_in_multiGrid<BuildGrid, tile::BuildObstructionType>
};

PLUGIN_API void fill_tile_data(uint32_t worldId, uint32_t tileDataType,
	int32_t x, int32_t y,
	int32_t width, int32_t height,
	void* pIn) {
	g_SetTileFuncs.at(tileDataType)(worldId, x, y, width, height, pIn);
}

// ---- Gridworld info

PLUGIN_API GridWorldInfo get_gridworld_info() {
	return gridWorldInfo;
}