set(LIBNAME "${PROJECT_NAME}_common")

add_library(${LIBNAME} STATIC 
	src/Utils.cpp)

target_include_directories(${LIBNAME} PUBLIC 
	${CMAKE_CURRENT_SOURCE_DIR}/include)

target_link_libraries(${LIBNAME} PRIVATE no_exceptions_rtti)