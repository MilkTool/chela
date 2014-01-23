set(GLTests_SRC
    GLTest.chela
)

chela_executable(GLTest EXECUTABLE REFS ChelaGui ChelaGL SRC ${GLTests_SRC})

