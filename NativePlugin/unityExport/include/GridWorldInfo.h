#pragma once

#pragma pack(push, 8)   
extern "C" {

	struct GridWorldInfo {
		int const minFertilityToSpread{ 1500 };
		int const maxFertility{ 2000 };
		int const minWaterForFertility{ 1000 };
		int const minGroundWaterToSpread{ 100 };
		int const maxGroundWater{5000};
	} gridWorldInfo;
}
#pragma pack(pop)