#include <catch2/catch_template_test_macros.hpp>

#include <string>
#include <bitset>

#include <Grid2d.h>

#include "Grid2dTestValues.h"

using namespace grid;

TEMPLATE_TEST_CASE("Basic Grid Funcionality works across arbitrary types", "[grid]", GRID_TEST_TYPES) {
    Grid2d<TestType> testGrid{};
    TestValues<TestType> testVals{};

    SECTION("Set/get a single tile") {
        auto v{ testVals.v[0] };

        testGrid.set_tile({ 0,0 }, v);
        
        REQUIRE(testGrid.get_tile({ 0,0 }) == v);
    }
}