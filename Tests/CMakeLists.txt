# Build the runtime tests.
add_subdirectory(RuntimeTest)

# Build the reflection map test.
add_subdirectory(ReflectionMap)

# Build the compute tests.
add_subdirectory(ComputeTest)

# Build the OpenGL test.
if(CHELA_BUILD_OPENGL)
    add_subdirectory(GLTest)
endif()

