#pragma once

#include <cstdint>

#include <MultiGrid2d.h>

namespace tile {

struct Fertility {
	int32_t value;
};

struct WaterContent {
	int32_t value;
};

struct WaterTileType {
	int8_t value;
};

}

using FertilityGrid = grid::MultiGrid2d<tile::Fertility>;

using WaterGrid = grid::MultiGrid2d<tile::WaterContent, tile::WaterTileType>;