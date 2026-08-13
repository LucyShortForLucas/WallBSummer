set(EXENAME ${PROJECT_NAME}_tests)

# --- Catch2 ---
include(FetchContent)
FetchContent_Declare(
  Catch2
  GIT_REPOSITORY https://github.com/catchorg/Catch2.git
  GIT_TAG  v3.15.2  
)
FetchContent_MakeAvailable(Catch2)

add_executable(${EXENAME} grid/Grid2d_tests.cpp "grid/Grid2dTestValues.h" "grid/GridWorld2d_tests.cpp")

target_link_libraries(${EXENAME} PRIVATE Catch2::Catch2WithMain)

# Link against your actual library under test, e.g.:
target_link_libraries(${EXENAME} PRIVATE "${PROJECT_NAME}_grid")

# Register with CTest
include(CTest)
include(Catch)  
catch_discover_tests(${EXENAME})

target_compile_options(${EXENAME} PRIVATE /EHsc)