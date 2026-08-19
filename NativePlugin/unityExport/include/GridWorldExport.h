#pragma once

#include <cstdint>

#include "UnityDefines.h"

// --- Grid Management
PLUGIN_API uint32_t create_gridworld(uint32_t worldType = 0);
PLUGIN_API void destroy_gridworld(uint32_t id);
PLUGIN_API void update_gridworld(uint32_t id, float deltatime);

// ---- Chunk state management
PLUGIN_API void manage_chunks(uint32_t worldId, uint32_t funcId,
	int32_t x, int32_t y,
	int32_t width, int32_t height);

// ---- Get/set tiles
PLUGIN_API void get_tile_data(uint32_t worldId, uint32_t tileDataType,
	int32_t x, int32_t y,
	int32_t width, int32_t height,
	void* pOut);