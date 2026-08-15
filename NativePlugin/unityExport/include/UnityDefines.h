#pragma once

#ifdef _WIN32
  #ifdef PLUGIN_EXPORTS
    #define PLUGIN_API extern "C" __declspec(dllexport)
  #else
    #define PLUGIN_API extern "C" __declspec(dllimport)
  #endif
#else
  #define PLUGIN_API extern "C" __attribute__((visibility("default")))
#endif