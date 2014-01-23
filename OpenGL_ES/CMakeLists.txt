set(OpenGL_ES_SRC
    ES20/Delegates.chela
    ES20/Enums.chela
    ES20/Helpers.chela
    ES20/ES.chela
    Half.chela
)

chela_module(ChelaGL_ES LIBRARY REFS ChelaEcmaCompat SRC ${OpenGL_ES_SRC})

