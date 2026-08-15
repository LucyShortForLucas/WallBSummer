#include <catch2/catch_template_test_macros.hpp>
#include <catch2/generators/catch_generators.hpp>
#include <catch2/benchmark/catch_benchmark.hpp>

#include <iostream>

#include <string>
#include <bitset>

#include <GridWorld2d.h>

#include "Grid2dTestValues.h"

using namespace grid;

using GridTAggr = Grid2d<TestGridDataAggregate>;
using GridTInt = Grid2d<int>;
using MultiGridT1 = MultiGrid2d<std::string, std::bitset<4>, std::vector<int>>;

TEST_CASE("Gridworld basics") {
	GridWorld2d<GridTAggr, GridTInt, MultiGridT1> testGridWorld{&sequentialChunkAlgoRunner};

	testGridWorld.init_grids_default();

	auto gridIntPtr{ testGridWorld.get_grid<GridTInt>() };
		
	gridIntPtr->set_tile({ 5, 9 }, 99);

	REQUIRE(gridIntPtr->get_tile({ 5,9 }) == 99);

	auto multiGrdPtr{ testGridWorld.get_multigrid<MultiGridT1>() };

	auto stringGridPtr{ multiGrdPtr->get_grid<std::string>() };

	stringGridPtr->set_tile({ 5, 9 }, TestValues<std::string>::v[0]);

	REQUIRE(stringGridPtr->get_tile({ 5,9 }) == TestValues<std::string>::v[0]);

}

//TEST_CASE("Gridworld Update algos") {
//	GridWorld2d<Grid2d<float>, Grid2d<int>> testGridWorld{ &sequentialChunkAlgoRunner };
//
//	auto add_deltatime{ [](Chunk2d<int>* pChunk, Grid2d<int>* pGrid, float deltatime) {
//		std::unique_lock chunkLock{pChunk->mutex};
//
//		for (auto& tile : pChunk->current_data_buffer()) {
//			tile += deltatime;
//		}
//
//		chunkLock.unlock();
//
//		pGrid->mark_chunk_dirty(pChunk->coord, 255);
//	} };
//}