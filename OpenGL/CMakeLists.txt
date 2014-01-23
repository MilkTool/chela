set(OpenGL2_SRC
    GLDelegates.chela
    GLEnums.chela
    GLHelpers.chela
    GL.chela
)

set(ChelaGL_SRC
    ${OpenGL2_SRC}
    Half.chela
)

# mingw32 ld cannot link ChelaGL with reflection support due to a memory problem
chela_module(ChelaGL LIBRARY FLAGS -noreflection REFS ChelaEcmaCompat LIBS ChelaSysLayer SRC ${ChelaGL_SRC})

