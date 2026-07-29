#include <catch2/catch_template_test_macros.hpp>
#include <catch2/generators/catch_generators.hpp>

#include <string>
#include <bitset>

#include <Grid2d.h>

#include "Grid2dTestValues.h"

using namespace grid;

TEMPLATE_TEST_CASE("Grid chunk Factories", "[grid]", GRID_TEST_TYPES) {
    int testX{ GENERATE(
        0, 5, 3,-3, -21,48, 101,-30065, -21,-71
    ) };

    int testY{ GENERATE(
        0, 52, 13,-13, -1, 25, 11, 3788, 10,-710
    ) };

    GridTileCoord2d testCoord{ testX, testY };

    auto v = TestValues<TestType>::v;

    SECTION(std::string("Default Chunk Factory, checking tile [") + std::to_string(testX) + ", " + std::to_string(testY) + "]") {
        Grid2d<TestType, DefaultChunk2dFactory<TestType>> testGrid{};
        REQUIRE(testGrid.get_tile(testCoord) == TestType{});
    }

    SECTION("Fill Chunk Factory") {
        Grid2d<TestType, FillChunk2dFactory<TestType>> testGrid{ FillChunk2dFactory<TestType>{v[0]}};
        REQUIRE(testGrid.get_tile(testCoord) == v[0]);
    }
}

#define CHUNKCOUNTREQUIRE(a, b, c) REQUIRE(testGrid.loaded_chunk_count() == a); REQUIRE(testGrid.awake_chunk_count() == b); REQUIRE(testGrid.sleeping_chunk_count() == c);

TEMPLATE_TEST_CASE("Basic Grid Funcionality works across arbitrary types", "[grid]", GRID_TEST_TYPES) {
    Grid2d<TestType> testGrid{};
    auto v = TestValues<TestType>::v;

    CHUNKCOUNTREQUIRE(0, 0, 0);

    SECTION("Set/get a single tile") {
        testGrid.set_tile({ 0,0 }, v[0]);
        REQUIRE(testGrid.get_tile({ 0,0 }) == v[0]);
        CHUNKCOUNTREQUIRE(1, 0, 1);
    }

    SECTION("Fill/get a whole rect of tiles") {

        // Within a single chunk
        Grid2d<TestType, FillChunk2dFactory<TestType>> v0Grid{ FillChunk2dFactory<TestType>{v[0]} };
        testGrid.fill_tile_rect({ {4, 4}, 6, 4 }, v[0]);
        REQUIRE(testGrid.get_tile_rect({ { 4, 4 }, 6, 4 }) == utils::make_filled_vector<6 * 4, TestType>(v[0]));
        REQUIRE(testGrid.get_tile_rect({ { 4, 4 }, 6, 4 }) == v0Grid.get_tile_rect({ { 4, 4 }, 6, 4 }));

        // Across multiple chunks
        Grid2d<TestType, FillChunk2dFactory<TestType>> v1Grid{ FillChunk2dFactory<TestType>{v[1]} };
        testGrid.fill_tile_rect({ { -4, -4 }, 20, 31 }, v[1]);
        REQUIRE(testGrid.get_tile_rect({ { -4, -4 }, 20, 31 }) == utils::make_filled_vector<20 * 31, TestType>(v[1]));
        REQUIRE(testGrid.get_tile_rect({ { -4, -4 }, 20, 31 }) == v1Grid.get_tile_rect({{ -4, -4 }, 20, 31}));

        // Long and thin
        Grid2d<TestType, FillChunk2dFactory<TestType>> v2Grid{ FillChunk2dFactory<TestType>{v[2]} };
        testGrid.fill_tile_rect({ { 4, -4 }, 300, 1 }, v[2]);
        REQUIRE(testGrid.get_tile_rect({ { 4, -4 }, 300, 1 }) == utils::make_filled_vector<300 * 1, TestType>(v[2]));
        REQUIRE(testGrid.get_tile_rect({ { 4, -4 }, 300, 1 }) == v2Grid.get_tile_rect({ { 4, -4 }, 300, 1 }));

        // Single tile rects, Side by side
        Grid2d<TestType, FillChunk2dFactory<TestType>> v3Grid{ FillChunk2dFactory<TestType>{v[3]} };
        testGrid.fill_tile_rect({ {5,9}, 1,1 }, v[2]);
        testGrid.fill_tile_rect({ {5,8}, 1,1 }, v[3]);
        REQUIRE(testGrid.get_tile_rect({ { 5, 9 }, 1, 1 }) == v2Grid.get_tile_rect({ { 5, 9 }, 1, 1 }));
        REQUIRE(testGrid.get_tile_rect({ { 5, 8 }, 1, 1 }) == v3Grid.get_tile_rect({ { 5, 8 }, 1, 1 }));
        REQUIRE(testGrid.get_tile({ 5, 9 }) == v[2]);
        REQUIRE(testGrid.get_tile({ 5, 8 }) == v[3]);
    }

    SECTION("Wake up/put chunks to sleep") {
        testGrid.load_chunk_asleep(ChunkCoord2d{ 0,0 });
        CHUNKCOUNTREQUIRE(1, 0, 1);
        testGrid.load_chunk_asleep(ChunkCoord2d{ 1,0 });
        CHUNKCOUNTREQUIRE(2, 0, 2);
        testGrid.wake_chunk(ChunkCoord2d{ 0,0 });
        CHUNKCOUNTREQUIRE(2, 1, 1);
        testGrid.wake_chunk(ChunkCoord2d{ 0,1 });
        CHUNKCOUNTREQUIRE(3, 2, 1);
        testGrid.sleep_chunk(ChunkCoord2d{ 0,1 });
        CHUNKCOUNTREQUIRE(3, 1, 2);

    }
}