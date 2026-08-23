#pragma once

#include <GridWorld2d.h>
#include <GameGrids.h>
#include <GridAlgorithms.h>
#include <GridWorldInfo.h>

void conway_fertility(FertilityGrid* pGrid, int) {
	pGrid->wake_chunks({ {-10, -10}, 20, 20 });

	auto gameOfLife = [](grid::Chunk2d<tile::Fertility>* pChunk, grid::Grid2d<tile::Fertility>* pGrid) {
		std::array<int, grid::CHUNK_DATA_SIZE> intermediate{};
		std::array<int, grid::CHUNK_DATA_SIZE> totalNeighbours{};

		auto total_live_cells_rows{ [](const tile::Fertility a, const tile::Fertility b, const tile::Fertility c) -> int {
			int result{};
			if (a.value > 0) ++result;
			if (b.value > 0) ++result;
			if (c.value > 0) ++result;
			return result;
		} };

		auto total_live_cells_columns{ [](const int a, const int b, const int c) -> int {
			return a + b + c;
		} };
		auto total_live_cells_edge{ [](const tile::Fertility a, const tile::Fertility b) -> int {
			int result{};
			if (a.value > 0) ++result;
			if (b.value > 0) ++result;
			return result;
		} };

		auto& readBuffer{ pChunk->read_buffer() };
		auto& writeBuffer{ pChunk->write_buffer() };
		grid::row_reduce_3x3(readBuffer, intermediate, total_live_cells_rows);
		grid::column_reduce_3x3_exclusive(readBuffer, intermediate, totalNeighbours, total_live_cells_columns, total_live_cells_edge);

		int i{-1};
		while (++i < grid::CHUNK_DATA_SIZE) {
			if (readBuffer[i].value <= 0) {
				writeBuffer[i].value = (totalNeighbours[i] == 3) ? gridWorldInfo.maxFertility : 0;
				continue;
			}

			if (totalNeighbours[i] < 2 || totalNeighbours[i] > 3) {
				writeBuffer[i].value = 0;
				continue;
			}

			writeBuffer[i] = readBuffer[i];
		}

		pChunk->swap_buffers();
		pGrid->mark_chunk_dirty(pChunk->coord, 255); // just mark the whole thing dirty for now, this is just a test and we don't care about perfomance that much here
	};

	pGrid->run_on_awake_chunks<tile::Fertility>(gameOfLife);
}