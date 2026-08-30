#pragma once

#include <GridWorld2d.h>
#include <GameGrids.h>
#include <GridAlgorithms.h>
#include <GridWorldInfo.h>
#include <random>

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

void fertility_spread(FertilityGrid* pGrid, int) {
	auto spreadFertility = [](grid::Chunk2d<tile::Fertility>* pChunk, grid::Grid2d<tile::Fertility>* pGrid, const grid::Grid2d<tile::FertilitySpreadType>* const pTypeGrid) {
		std::array<bool, grid::CHUNK_DATA_SIZE> intermediate;
		std::array<bool, grid::CHUNK_DATA_SIZE> fertility_allowed;

		auto at_least_one_min_fertility{ [](const tile::Fertility a, const tile::Fertility b, const tile::Fertility c) -> bool {
			return a.value > gridWorldInfo.minFertilityToSpread
				|| b.value > gridWorldInfo.minFertilityToSpread
				|| c.value > gridWorldInfo.minFertilityToSpread;
		} };

		auto reduce_bool{ [](const bool a, const bool b, const bool c) -> bool {
			return a || b || c;
		} };

		auto& readBuffer{ pChunk->read_buffer() };
		auto& writeBuffer{ pChunk->write_buffer() };

		grid::row_reduce_3x3(readBuffer, intermediate, at_least_one_min_fertility);
		grid::column_reduce_3x3(intermediate, fertility_allowed, reduce_bool);

		auto typeGridData{pTypeGrid->get_chunk_data_if_loaded(pChunk->coord)};

		class BitCoinFlipper {
		public:
			bool flip() {
				if (bitsLeft == 0) {
					buffer = gen();
					bitsLeft = 64;
				}
				bool result = buffer & 1ULL;
				buffer >>= 1;
				--bitsLeft;
				return result;
			}

		private:
			std::mt19937_64 gen{ std::random_device{}() };
			uint64_t buffer = 0;
			int bitsLeft = 0;
		} thread_local flipper;

		if (typeGridData != nullptr)
			for (int i{}; i < fertility_allowed.size(); ++i) {
				writeBuffer[i] = readBuffer.at(i);
				auto spreadType = static_cast<FertilitySpreadType>(typeGridData->at(i).value);
				if (spreadType == FertilitySpreadType::None)
					writeBuffer[i].value = 0;
				else if ((fertility_allowed[i] || spreadType == FertilitySpreadType::Always) && flipper.flip())
					writeBuffer[i].value += 1;
			}

		pChunk->swap_buffers();
		pGrid->mark_chunk_dirty(pChunk->coord, 255);
	};

	pGrid->run_on_awake_chunks<tile::Fertility>(spreadFertility, pGrid->get_grid<tile::FertilitySpreadType>());
}