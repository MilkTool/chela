set(OpenCL_SRC
    CL10/Delegates.chela
    CL10/Enums.chela
    CL10/Helpers.chela
    CL10/ImageFormat.chela
    CL10/CL.chela
)

chela_module(ChelaCL LIBRARY REFS ChelaEcmaCompat SRC ${OpenCL_SRC})

