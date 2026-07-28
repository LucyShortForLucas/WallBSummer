#pragma once

#include <array>
#include <vector>
#include <utility>

namespace utils {

// ---- General helper functions

namespace detail {

template <typename T, std::size_t... Is>
constexpr std::array<T, sizeof...(Is)>
make_filled_array_impl(const T& value, std::index_sequence<Is...>) {
    return { (static_cast<void>(Is), value)... };
}

template <typename T, std::size_t... Is>
constexpr std::vector<T>
make_filled_vector_impl(const T& value, std::index_sequence<Is...>) {
    return { (static_cast<void>(Is), value)... };
}

}

template <std::size_t N, typename T>
constexpr std::array<T, N> make_filled_array(const T& value) {
    return detail::make_filled_array_impl(value, std::make_index_sequence<N>{});
}

template <std::size_t N, typename T>
constexpr std::vector<T> make_filled_vector(const T& value) {
    return detail::make_filled_vector_impl(value, std::make_index_sequence<N>{});
}

inline int floor_div(int a, int b) {
    int q = a / b;
    int r = a % b;

    if (r != 0 && ((r < 0) != (b < 0)))
        --q;

    return q;
}

template <typename T, std::size_t N>
void fill_rect_in_square_array(std::array<T, N* N>& toFill, int x, int y, int width, int height, const T& v) {
    // Clamp rect to grid bounds (cheap, and avoids UB on bad input).
    int x0 = std::max(x, 0);
    int y0 = std::max(y, 0);
    int x1 = std::min(x + width, static_cast<int>(N));
    int y1 = std::min(y + height, static_cast<int>(N));

    if (x0 >= x1 || y0 >= y1) return;

    // Fast path: rect spans the full row width -> the whole block
    // is one contiguous run of memory, fill it in a single call.
    if (x0 == 0 && x1 == static_cast<int>(N)) {
        T* start = toFill.data() + static_cast<std::size_t>(y0) * N;
        std::fill(start, start + static_cast<std::size_t>(y1 - y0) * N, v);
        return;
    }

    // General case: one std::fill per row.
    for (int row = y0; row < y1; ++row) {
        T* rowStart = toFill.data() + static_cast<std::size_t>(row) * N + x0;
        std::fill(rowStart, rowStart + (x1 - x0), v);
    }
}

template <typename T, std::size_t N>
void fetch_rect_from_square_array(const std::array<T, N* N>& src, int x, int y, int width, int height, std::vector<T>& out) {
    int x0 = std::max(x, 0);
    int y0 = std::max(y, 0);
    int x1 = std::min(x + width, static_cast<int>(N));
    int y1 = std::min(y + height, static_cast<int>(N));

    if (x0 >= x1 || y0 >= y1) return;

    const int outW = x1 - x0;
    const int outH = y1 - y0;

    // Reserve so the whole append happens without reallocation mid-copy.
    out.reserve(out.size() + static_cast<std::size_t>(outW) * outH);

    // Fast path: full-width rows are contiguous -> single insert.
    if (x0 == 0 && x1 == static_cast<int>(N)) {
        const T* start = src.data() + static_cast<std::size_t>(y0) * N;
        out.insert(out.end(), start, start + static_cast<std::size_t>(outH) * N);
        return;
    }

    // General case: one insert per row.
    for (int row = y0; row < y1; ++row) {
        const T* rowStart = src.data() + static_cast<std::size_t>(row) * N + x0;
        out.insert(out.end(), rowStart, rowStart + outW);
    }
}

}