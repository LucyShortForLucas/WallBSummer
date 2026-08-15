#pragma once

#include <cstdint>

#include "UnityDefines.h"

// World management
PLUGIN_API uint32_t create_gridworld();
PLUGIN_API void destroy_gridworld(uint32_t id);
PLUGIN_API void update_gridworld(uint32_t id, float deltatime);

// Get/set tiles

PLUGIN_API void get_tiles_water_content(uint32_t worldId,
	int32_t x, int32_t y,
	int32_t width, int32_t height,
	int32_t* pOut);

PLUGIN_API void get_tiles_fertility(uint32_t worldId,
	int32_t x, int32_t y,
	int32_t width, int32_t height,
	int32_t* pOut);