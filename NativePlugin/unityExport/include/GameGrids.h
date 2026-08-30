#pragma once

#include <cstdint>

#include <MultiGrid2d.h>

namespace tile {

struct Fertility {
	int32_t value{};
};

struct FertilitySpreadType {
	uint8_t value{};
};

struct WaterContent {
	int32_t value{};
};

struct WaterTileType {
	int8_t value{};
};

struct BuildObstructionType {
	int8_t value{};
};

}

using FertilityGrid = grid::MultiGrid2d<tile::Fertility, tile::FertilitySpreadType>;

using WaterGrid = grid::MultiGrid2d<tile::WaterContent, tile::WaterTileType>;

using BuildGrid = grid::MultiGrid2d<tile::BuildObstructionType>;