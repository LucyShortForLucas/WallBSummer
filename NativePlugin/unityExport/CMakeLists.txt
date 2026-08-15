set(LIBNAME ${PROJECT_NAME}_unityExport)

add_library(${LIBNAME} SHARED 
	src/GridWorldExport.cpp
 )

 target_link_libraries(${LIBNAME} PRIVATE ${PROJECT_NAME}_grid no_exceptions_rtti)

target_include_directories(${LIBNAME} PRIVATE include)

# Defines MYPLUGIN_EXPORTS only when building the DLL itself
target_compile_definitions(${LIBNAME} PRIVATE PLUGIN_EXPORTS)

set_target_properties(${LIBNAME} PROPERTIES
    CXX_VISIBILITY_PRESET hidden
    VISIBILITY_INLINES_HIDDEN ON
)

if (MSVC)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>DLL")
endif()