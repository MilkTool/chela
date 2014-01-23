set(RuntimeTest_SRC
    TestLoader.chela
)

set(TestModule_SRC
    DateTimeTest.chela
    InvokeTest.chela
    TestAttribute.chela
    TestUtilities.chela
    VectorTest.chela
    ResourceTest.chela
    StringTest.chela
)

set(TestModule_Resources
    TestText.txt
)

chela_executable(RuntimeTest SRC ${RuntimeTest_SRC})
chela_library(TestModule SRC ${TestModule_SRC} RESOURCES ${TestModule_Resources})
