cmake_minimum_required(VERSION 3.21)
project(NativePlugin LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Keep binaries in predictable locations regardless of platform
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

option(NATIVEPLUGIN_BUILD_TESTS "Build Catch2 test suite" ON)

# Interface library to turn off exceptions and rtti in child libs
add_library(no_exceptions_rtti INTERFACE)
target_compile_options(no_exceptions_rtti INTERFACE
    $<$<CXX_COMPILER_ID:MSVC>:/EHs-c- /GR->
    $<$<NOT:$<CXX_COMPILER_ID:MSVC>>:-fno-exceptions -fno-rtti>
)

# Common lib
add_subdirectory(common)

# Feature domain libs
add_subdirectory(grid)

# Export dll
add_subdirectory(unityExport)

# Test exe
if(NATIVEPLUGIN_BUILD_TESTS)
    add_subdirectory(tests)
endif()