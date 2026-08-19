#include <catch2/catch_template_test_macros.hpp>
#include <catch2/generators/catch_generators.hpp>
#include <catch2/benchmark/catch_benchmark.hpp>

#include <iostream>

#include <string>
#include <bitset>

#include <Grid2d.h>

#include "Grid2dTestValues.h"

#include <GridAlgorithms.h>

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
        Grid2d<TestType> testGrid{};
        REQUIRE(testGrid.get_tile(testCoord) == TestType{});
    }

    SECTION("Fill Chunk Factory") {
        FillChunk2dFactory<TestType> f0{ v[0] };
        Grid2d<TestType> testGrid{ &f0 };
        REQUIRE(testGrid.get_tile(testCoord) == v[0]);
    }
}

#define CHUNKCOUNTREQUIRE(a, b, c) REQUIRE(testGrid.loaded_chunk_count() == a); REQUIRE(testGrid.awake_chunk_count() == b); REQUIRE(testGrid.sleeping_chunk_count() == c);

TEMPLATE_TEST_CASE("Basic Grid Funcionality works across arbitrary types", "[grid]", GRID_TEST_TYPES) {
    Grid2d<TestType> testGrid{};
    auto v = TestValues<TestType>::v;

    FillChunk2dFactory<TestType> f0{ v[0] };
    FillChunk2dFactory<TestType> f1{ v[1] };
    FillChunk2dFactory<TestType> f2{ v[2] };
    FillChunk2dFactory<TestType> f3{ v[3] };

    CHUNKCOUNTREQUIRE(0, 0, 0);

    SECTION("Set/get a single tile") {
        testGrid.set_tile({ 0,0 }, v[0]);
        REQUIRE(testGrid.get_tile({ 0,0 }) == v[0]);
        CHUNKCOUNTREQUIRE(1, 0, 1);
    }

    SECTION("Fill/get a whole rect of tiles") {

        // Within a single chunk
        Grid2d<TestType> v0Grid{ &f0 };
        testGrid.fill_tile_rect({ {4, 4}, 6, 4 }, v[0]);
        REQUIRE(testGrid.get_tile_rect({ { 4, 4 }, 6, 4 }) == std::vector<TestType>(6 * 4, v[0]));
        REQUIRE(testGrid.get_tile_rect({ { 4, 4 }, 6, 4 }) == v0Grid.get_tile_rect({ { 4, 4 }, 6, 4 }));

        // Across multiple chunks
        Grid2d<TestType> v1Grid{ &f1 };
        testGrid.fill_tile_rect({ { -4, -4 }, 20, 31 }, v[1]);
        REQUIRE(testGrid.get_tile_rect({ { -4, -4 }, 20, 31 }) == std::vector<TestType>(20 * 31, v[1]));
        REQUIRE(testGrid.get_tile_rect({ { -4, -4 }, 20, 31 }) == v1Grid.get_tile_rect({ { -4, -4 }, 20, 31 }));

        // Long and thin
        Grid2d<TestType> v2Grid{ &f2 };
        testGrid.fill_tile_rect({ { 4, -4 }, 300, 1 }, v[2]);
        REQUIRE(testGrid.get_tile_rect({ { 4, -4 }, 300, 1 }) == std::vector<TestType>(300, v[2]));
        REQUIRE(testGrid.get_tile_rect({ { 4, -4 }, 300, 1 }) == v2Grid.get_tile_rect({ { 4, -4 }, 300, 1 }));

        // Single tile rects, Side by side
        Grid2d<TestType> v3Grid{ &f3 };
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

    SECTION("Wake up/put chunks to sleep rects at a time") {
        testGrid.load_chunks_asleep({ {-1,1}, 2, 2 });
        CHUNKCOUNTREQUIRE(4, 0, 4);
        testGrid.load_chunks_asleep({ {-1, 1}, 3, 2 });
        CHUNKCOUNTREQUIRE(6, 0, 6);
        testGrid.load_chunks_asleep({ {-1, 1}, 2, 2 });
        CHUNKCOUNTREQUIRE(6, 0, 6);
        testGrid.wake_chunks({ { -1, 1 }, 3, 3 });
        CHUNKCOUNTREQUIRE(9, 9, 0);
        testGrid.wake_chunks({ { -2, 1 }, 3, 3 });
        CHUNKCOUNTREQUIRE(12, 12, 0);
        testGrid.sleep_chunks({ {-1,0}, 2, 2 });
        CHUNKCOUNTREQUIRE(14, 10, 4);
    }

    SECTION("Getting a 'chunk rect' of tiles that exactly fits the real borders of a chunk, fetches that chunk with its spatial partitioning in-tact (without halo)") {
        testGrid.set_tile({ 5,4 }, v[0]);
        testGrid.set_tile({ 10,3 }, v[1]);
        testGrid.set_tile({ 1,0 }, v[2]);
        testGrid.set_tile({0,10}, v[3]);
        std::vector<TestType> chunkData{ testGrid.get_tile_rect({{0,0}, CHUNK_WIDTH, CHUNK_WIDTH }) };
        REQUIRE(chunkData[5 + 4 * CHUNK_WIDTH] == v[0]);
        REQUIRE(chunkData[10 + 3 * CHUNK_WIDTH] == v[1]);
        REQUIRE(chunkData[1 + 0 * CHUNK_WIDTH] == v[2]);
        REQUIRE(chunkData[0 + 10 * CHUNK_WIDTH] == v[3]);
    };

    SECTION("Halo syncing") {
        auto setAllTiles{ [](Chunk2d<TestType>* chunk, Grid2d<TestType>* grid, TestType v) {
            for (int y{}; y < CHUNK_WIDTH; ++y) { for (int x{}; x < CHUNK_WIDTH; ++x) {
                chunk->current_data_buffer()[coord_to_data_index({ x,y })] = v;
                grid->mark_chunk_dirty(chunk->coord, 255);
        } } } };

        testGrid.run_on_chunk({ 0, 1 }, setAllTiles, v[0]);
        testGrid.run_on_chunk({ 1, 0 }, setAllTiles, v[1]);
        testGrid.run_on_chunk({ 0, -1 }, setAllTiles, v[2]);
        testGrid.run_on_chunk({-1, 0}, setAllTiles, v[3]);

        testGrid.run_on_chunk({ 1, 1 }, setAllTiles, v[0]);
        testGrid.run_on_chunk({ 1, -1 }, setAllTiles, v[1]);
        testGrid.run_on_chunk({ -1, -1 }, setAllTiles, v[2]);
        testGrid.run_on_chunk({ -1, 1 }, setAllTiles, v[3]);
        testGrid.sync_dirty_halos();

        testGrid.run_on_chunk({ 0, 0 }, [&](Chunk2d<TestType>* pChunk, Grid2d<TestType>* pGrid) {
            auto buffer{pChunk->current_data_buffer()};

            for (int i{ 1 }; i < CHUNK_WIDTH; ++i) {
                REQUIRE(buffer[i] == v[0]);
                REQUIRE(buffer[i * CHUNK_DATA_WIDTH + CHUNK_DATA_WIDTH - 1] == v[1]);
                REQUIRE(buffer[i + HALO_SW_INDEX] == v[2]);
                REQUIRE(buffer[i * CHUNK_DATA_WIDTH] == v[3]);
            }

            REQUIRE(buffer[HALO_SW_INDEX] == v[0]);
            REQUIRE(buffer[HALO_NW_INDEX] == v[1]);
            REQUIRE(buffer[HALO_NE_INDEX] == v[2]);
            REQUIRE(buffer[HALO_SE_INDEX] == v[3]);
        });
    }
}

TEST_CASE("Chunk Alghoritms") {
    FillChunk2dFactory<int> factory{100};
    Grid2d<int> testGrid{ &factory };
    
    auto addToTiles{ [] (Chunk2d<int>* chunk, Grid2d<int>* grid,  int v) {
        for (int y{}; y < CHUNK_WIDTH; ++y) {
            for (int x{}; x < CHUNK_WIDTH; ++x) {
                chunk->current_data_buffer()[coord_to_data_index({ x,y })] += v;
            }
        }
    } };

    SECTION("Single chunk algo") {
        testGrid.run_on_chunk( {0, 0}, addToTiles, 5);
        auto tiles{ testGrid.get_tile_rect({{0, 0}, CHUNK_WIDTH, CHUNK_WIDTH }) };
        for (auto tile : tiles) {
            REQUIRE(tile == 105);
        }
    }

    SECTION("Multi chunk algos") {
        testGrid.run_on_loaded_chunks(addToTiles, 25); // No-op, nothing loaded (just here to make sure this does not break anything)
        testGrid.load_chunks_asleep({ {0, 0}, 5, 5 });
        testGrid.run_on_loaded_chunks(addToTiles, 25);

        SECTION("run_on_loaded_chunks runs on all loaded chunks") {
            auto tiles{ testGrid.get_tile_rect({{0, 0}, CHUNK_WIDTH * 5, CHUNK_WIDTH * 5 }) };
            for (auto tile : tiles) {
                REQUIRE(tile == 125);
            }
        }

        SECTION("run_on_awake_chunks runs only all loaded chunks") {
            testGrid.run_on_awake_chunks(addToTiles, 25); // No-op again
            auto tiles{ testGrid.get_tile_rect({{0, 0}, CHUNK_WIDTH * 5, CHUNK_WIDTH * 5 }) };
            for (auto tile : tiles) {
                REQUIRE(tile == 125);
            }

            testGrid.wake_chunks({ {0,0}, 5, 1 });
            testGrid.run_on_awake_chunks(addToTiles, 75); 
            auto tiles2{ testGrid.get_tile_rect({{0, CHUNK_WIDTH}, CHUNK_WIDTH * 5, CHUNK_WIDTH * 4 }) };
            for (auto tile : tiles2) {
                REQUIRE(tile == 125);
            }
            auto tiles3{ testGrid.get_tile_rect({{0, 0}, CHUNK_WIDTH * 5, CHUNK_WIDTH }) };
            for (auto tile : tiles3) {
                REQUIRE(tile == 200);
            }

            testGrid.run_on_loaded_chunks(addToTiles, 50);
            auto tiles4{ testGrid.get_tile_rect({{0, CHUNK_WIDTH}, CHUNK_WIDTH * 5, CHUNK_WIDTH * 4 }) };
            for (auto tile : tiles4) {
                REQUIRE(tile == 175);
            }
            auto tiles5{ testGrid.get_tile_rect({{0, 0}, CHUNK_WIDTH * 5, CHUNK_WIDTH }) };
            for (auto tile : tiles5) {
                REQUIRE(tile == 250);
            }
        }
    }

    SECTION("Neumann Stencil Algos") {
        auto set_as_total_3x3{ [](Chunk2d<int>* pChunk, Grid2d<int>* pGrid) {
            std::unique_lock chunkLock{pChunk->mutex};
            std::array<int, CHUNK_DATA_SIZE> intermediate{};

            auto total_of{ [](const int a, const int b, const int c) -> int {
                return a + b + c;
            } };

            row_reduce_3x3(pChunk->read_buffer(), intermediate, total_of);
            column_reduce_3x3(intermediate, pChunk->write_buffer(), total_of);
            pChunk->swap_buffers();

            uint8_t dirtyFlags{flag_nonequal_edges(pChunk->read_buffer(), pChunk->write_buffer())};
            chunkLock.unlock();
            
            if (dirtyFlags)
                pGrid->mark_chunk_dirty(pChunk->coord, dirtyFlags);
        } };

        testGrid.run_on_chunk({0, 0}, set_as_total_3x3);
        REQUIRE(testGrid.get_tile_rect({ {0, 0}, CHUNK_WIDTH, CHUNK_WIDTH }) == std::vector<int>(CHUNK_WIDTH * CHUNK_WIDTH, 900));

        testGrid.fill_tile_rect({ {0, 0}, CHUNK_WIDTH, CHUNK_WIDTH }, 0);
        testGrid.run_on_chunk({ 0, 0 }, set_as_total_3x3);
        REQUIRE(testGrid.get_tile_rect({ {0, 0}, CHUNK_WIDTH, CHUNK_WIDTH }) == std::vector<int>(CHUNK_WIDTH * CHUNK_WIDTH, 0));

        testGrid.set_tile({ 5, 5 }, 1);
        testGrid.run_on_chunk({ 0, 0 }, set_as_total_3x3);
        REQUIRE( testGrid.get_tile_rect({ {4, 4}, 3, 3 }) == std::vector<int>(9, 1));

        testGrid.run_on_chunk({ 0, 0 }, set_as_total_3x3);
        REQUIRE(testGrid.get_tile_rect({ {2, 2}, 7, 7 }) == std::vector<int>{
            0, 0, 0, 0, 0, 0, 0,
            0, 1, 2, 3, 2, 1, 0,
            0, 2, 4, 6, 4, 2, 0,
            0, 3, 6, 9, 6, 3, 0,
            0, 2, 4, 6, 4, 2, 0,
            0, 1, 2, 3, 2, 1, 0,
            0, 0, 0, 0, 0, 0, 0,
        });
    }
}