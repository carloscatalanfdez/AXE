#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "Polycode2DPhysics" for configuration "Release"
set_property(TARGET Polycode2DPhysics APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(Polycode2DPhysics PROPERTIES
  IMPORTED_LINK_INTERFACE_LANGUAGES_RELEASE "CXX"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/Modules/lib/Polycode2DPhysics.lib"
  )

list(APPEND _IMPORT_CHECK_TARGETS Polycode2DPhysics )
list(APPEND _IMPORT_CHECK_FILES_FOR_Polycode2DPhysics "${_IMPORT_PREFIX}/Modules/lib/Polycode2DPhysics.lib" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
