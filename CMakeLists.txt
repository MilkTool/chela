cmake_minimum_required(VERSION 2.6)
project(Chela)

# Check the build type
if (CMAKE_BUILD_TYPE STREQUAL "")
	# CMake defaults to leaving CMAKE_BUILD_TYPE empty. This screws up
	# differentiation between debug and release builds.
	set(CMAKE_BUILD_TYPE "RelWithDebInfo" CACHE STRING "Choose the type of build, options are: None (CMAKE_CXX_FLAGS or CMAKE_C_FLAGS used) Debug Release RelWithDebInfo MinSizeRel." FORCE)
endif ()

if(CMAKE_BUILD_TYPE STREQUAL "RelWithDebInfo" OR CMAKE_BUILD_TYPE STREQUAL "Debug")
    set(CSHARP_FLAGS "-debug")
endif()

# Find X11
find_package(X11)

# Disable X11 under windows by default.
if(${CMAKE_HOST_WIN32})
    set(X11_FOUND FALSE)
endif()

# Find OpenGL
find_package(OpenGL)

# Find dl
find_library(LIB_DL dl)

# Option to build OpenGL bindings.
if(OPENGL_FOUND)
    set(CHELA_BUILD_OPENGL TRUE CACHE BOOL "Choose to build the OpenGL bindings.")
else()
    set(CHELA_BUILD_OPENGL FALSE)
endif()

# Option to build X11 gui driver
if(X11_FOUND)
    set(CHELA_BUILD_X11DRIVER TRUE CACHE BOOL "Choose to build the OpenGL bindings.")
else()
    set(CHELA_BUILD_X11DRIVER FALSE)
endif()

# Set executable output path.
set(EXECUTABLE_OUTPUT_PATH "${Chela_BINARY_DIR}/dist")
set(LIBRARY_OUTPUT_PATH "${Chela_BINARY_DIR}/dist")

# Enable NASM, used by the dynamic reflection calling.
enable_language(ASM_NASM)

# Compile with full warning information.
add_definitions(-Wall)

# Find llvm
include(FindLLVM.cmake)

# Build the system layer
add_subdirectory(ChelaSysLayer)

# Build the VM
add_subdirectory(ChelaVm)

# Load the c# macro.
include(CSharp.cmake)

# Load the chela macro.
include(Chela.cmake)

# Build the chela compiler
add_subdirectory(ChelaCompiler)

# Build the chela runtime.
add_subdirectory(ChelaCore)

# Build the XML library
add_subdirectory(ChelaXml)

# Build ECMA 354 compatibility layer.
add_subdirectory(EcmaCompat)

# Build the chela gui.
add_subdirectory(ChelaGui)

# Build the binding generator
add_subdirectory(Bind)

# Build the OpenAL bindings
add_subdirectory(OpenAL)

# Build the OpenGL bindings.
if(CHELA_BUILD_OPENGL)
    add_subdirectory(OpenGL)
endif()

# Build the OpenGL ES bindings.
add_subdirectory(OpenGL_ES)

# Build the OpenCL bindings
add_subdirectory(OpenCL)

# Build the tests.
add_subdirectory(Tests)

# Build the samples.
add_subdirectory(Samples)

