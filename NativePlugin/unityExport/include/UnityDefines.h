#pragma once

#include <cstdint>
#include <GridWorld2d.h>
#include "GameGrids.h"

#ifdef _WIN32
  #ifdef PLUGIN_EXPORTS
    #define PLUGIN_API extern "C" __declspec(dllexport)
  #else
    #define PLUGIN_API extern "C" __declspec(dllimport)
  #endif
#else
  #define PLUGIN_API extern "C" __attribute__((visibility("default")))
#endif

enum class WaterTileType : uint8_t {
    GroundWater = 0, // The water of this tile exists underground, like in soil
    FlowingWater = 1, // The water of this tile flows, like in a river. 
    StillWater = 2, // The water of this tile is still, like in a basin or pond. 
    WaterSource = 3, // The water of this tile is a 'source' and magically fills itself, like the mouth of a river.
    NoWater = 4 // This tile cannot contain water.
};

enum class BuildObstructionType : uint8_t {
    None,
    Natural,
    Building
};

enum class FertilitySpreadType : uint8_t {
    Normal,
    None,
    Always
};

using WorldType = grid::GridWorld2d<FertilityGrid, WaterGrid, BuildGrid>;