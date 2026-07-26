///============================================================================
/// This header primarily serves to define a set of templates types and 
/// random value of said types to be used in template test cases for the Grid2d
/// class. In this way one single test can be used across a wide range of 
/// arbitrary types.
///============================================================================

#pragma once

#include <string>
#include <bitset>

struct TestGridDataAggregate {
    int i{};
    float f{};
    bool b{};

    auto operator<=>(const TestGridDataAggregate&) const = default;
};

#define GRID_TEST_TYPES int, float, std::string, TestGridDataAggregate, \
    std::bitset<4>, std::vector<int>

template<typename T>
struct TestValues {
    static inline const std::array<T, 4> v{};
};

template<>
struct TestValues<int> {
    static inline const std::array<int, 4> v{ -54, 127345, 8, - 42};
};

template<>
struct TestValues<float> {
    static inline const std::array<float, 4> v{ -54.f, 12.7345f, 8.9868f, -42.00001f };
};

template<>
struct TestValues<std::string> {
    static inline const std::array<std::string, 4> v{ 
        "This is a string!", "Hello World!",
        "",
        "         \"\"               " // <-- Weird string just because. Python devs must love all that white space -Lucy
    };
};

template<>
struct TestValues<TestGridDataAggregate> {
    static inline const std::array<TestGridDataAggregate, 4> v{
        TestGridDataAggregate{555, -54.f, true},
        TestGridDataAggregate{0, 1, true},
        TestGridDataAggregate{ 100, 100.f, false },
        TestGridDataAggregate{ -55325, -0.097896f, false }
    };
};

template<>
struct TestValues<std::bitset<4>> {
    static inline const std::array<std::bitset<4>, 4> v{ 0b0001, 0b1111, 0b1001, 0b1100 };
};

template<>
struct TestValues<std::vector< int>> {
    static inline const std::array<std::vector< int>, 4> v{
        std::vector<int>{ 543564537, 33263564 },
        std::vector< int>{ 5464537, 332633564, 24334 },
        std::vector< int>{ 53464537, 3564, 24334, 4234},
        std::vector< int>{ 1 }
    };
};