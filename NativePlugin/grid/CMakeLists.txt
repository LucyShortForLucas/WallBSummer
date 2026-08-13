set(LIBNAME "${PROJECT_NAME}_grid")

add_library(${LIBNAME} STATIC 
	 "include/GridDefines.h" "include/GridHelpers.h" "include/Coord2d.h" "src/GridHelpers.cpp" "include/Chunk2d.h"  "include/Grid2d.h" "include/ChunkFactory.h" "include/MultiGrid2d.h" "include/GridWorld2d.h" "include/GridAlgorithms.h")

target_include_directories(${LIBNAME} PUBLIC 
	${CMAKE_CURRENT_SOURCE_DIR}/include)

target_link_libraries(${LIBNAME} PRIVATE no_exceptions_rtti PUBLIC ${PROJECT_NAME}_common)