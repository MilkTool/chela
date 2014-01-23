# Build the gui demo.
add_subdirectory(GuiDemo)

# Build the NeHe lessons demo.
if(CHELA_BUILD_OPENGL)
    add_subdirectory(NeHeDemo)
endif()

