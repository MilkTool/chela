set(EcmaCompat_SRC
    System.chela
    System.Globalization.chela
    System.Reflection.chela
    System.Runtime.InteropServices.chela
    System.Text.chela
    System.Threading.chela
)

chela_module(ChelaEcmaCompat LIBRARY SRC ${EcmaCompat_SRC})

