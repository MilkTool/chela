# Use the includes.
include_directories(include src)

# Select the ABI reflection support.
if("${CMAKE_SYSTEM_PROCESSOR}" MATCHES "i.86" OR "${CMAKE_SYSTEM_PROCESSOR}" STREQUAL "x86")
    if(${CMAKE_HOST_UNIX})
        set(ReflectionABI_SRC
            src/Reflection_x86.cpp
            src/Reflection_UnixTrap_x86.nasm
        )
    elseif(${CMAKE_HOST_WIN32})
        set(ReflectionABI_SRC
            src/Reflection_x86.cpp
            src/Reflection_Win32Trap.nasm
        )
    else()
        message(FATAL_ERROR "unsupported operating system ${CMAKE_HOST_SYSTEM_NAME}")
    endif()
elseif("${CMAKE_SYSTEM_PROCESSOR}" STREQUAL "x86_64")
    if(${CMAKE_HOST_UNIX})
        set(ReflectionABI_SRC
            src/Reflection_Unix_AMD64.cpp
            src/Reflection_UnixTrap_AMD64.nasm
        )
    elseif(${CMAKE_HOST_WIN32})
        message(FATAL_ERROR "unimplemented reflection ABI in windows 64 bits")
    else()
        message(FATAL_ERROR "unsupported operating system ${CMAKE_HOST_SYSTEM_NAME}")
    endif()
else()
    message(FATAL_ERROR "unsupported architecture ${CMAKE_SYSTEM_PROCESSOR}")
endif()

if(${CMAKE_HOST_WIN32})
    # Compile a Win32 API based gui driver
    set(Win32Gui_SRC
        src/Win32GuiDriver/Display.cpp
        src/Win32GuiDriver/Drawable.cpp
        src/Win32GuiDriver/Driver.cpp
        src/Win32GuiDriver/GraphicContext.cpp
        src/Win32GuiDriver/Screen.cpp
        src/Win32GuiDriver/Window.cpp
        src/Win32GuiDriver/WglContext.cpp
    )

    set(GuiDriver_SRC
        ${GuiDriver_SRC}
        ${Win32Gui_SRC}
    )

    # Window specific dependencies.
    set(SysLayer_LIBS Msimg32.lib)
elseif(${CMAKE_HOST_UNIX})
    # Unix required libraries.
    set(SysLayer_LIBS pthread rt)
    if(LIB_DL)
        set(SysLayer_LIBS ${SysLayer_LIBS} dl)
    endif()
endif()

# Add X11 driver.
if(CHELA_BUILD_X11DRIVER)
    set(X11Driver_SRC
        src/X11Driver/GlxContext.cpp
        src/X11Driver/X11Display.cpp
        src/X11Driver/X11Drawable.cpp
        src/X11Driver/X11Driver.cpp
        src/X11Driver/X11GraphicContext.cpp
        src/X11Driver/X11Screen.cpp
        src/X11Driver/X11Window.cpp
    )

    set(GuiDriver_SRC
        ${GuiDriver_SRC}
        ${X11Driver_SRC}
    )

    # Set platform specific libraries
    add_definitions(-DUSE_X11)
    include_directories(${X11_INCLUDE_DIR})
    set(SysLayer_LIBS ${SysLayer_LIBS} ${X11_LIBRARIES})
endif()

# Add OpenGL
if(CHELA_BUILD_OPENGL)
    add_definitions(-DUSE_OPENGL)
    set(SysLayer_LIBS ${SysLayer_LIBS} ${OPENGL_LIBRARIES})
endif()

# ChelaVm source files.
set(ChelaSysLayer_SRC
    ${GuiDriver_SRC}
    ${ReflectionABI_SRC}
    src/Atomic.cpp
    src/DataConversion.cpp
    src/GuiDriver.cpp
    src/Math.cpp
    src/Memory.cpp
    src/Module.cpp
    src/OpenGL.cpp
    src/PosixTime.cpp
    src/PThreads.cpp
    src/IO_Common.cpp
    src/IO_Unix.cpp
    src/IO_Win32.cpp
    src/SysLayer.cpp
    src/System.cpp
    src/Win32_Common.cpp
    src/Win32_Threading.cpp
    src/Win32_Time.cpp    
)

# Search nasm dependencies in the same directory.
set(CMAKE_ASM_NASM_FLAGS ${CMAKE_ASM_NASM_FLAGS} -I.)

# Create the virtual machine.
add_definitions(-DBUILD_SYSLAYER)
add_library(ChelaSysLayer SHARED ${ChelaSysLayer_SRC})
target_link_libraries(ChelaSysLayer ${SysLayer_LIBS})

