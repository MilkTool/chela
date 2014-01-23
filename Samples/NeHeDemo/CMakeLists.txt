set(NeHeDemo_SRC
    NeHeDemo.chela
    Lesson.chela
    Lesson1.chela
    Lesson2.chela
    Lesson3.chela
    Lesson4.chela
    Lesson5.chela
    Lesson6.chela
    Lesson7.chela
)

set(NeHeDemo_Resources
    Crate.bmp
    NeHe.bmp
    Glass.bmp
)

chela_executable(NeHeDemo REFS ChelaGui ChelaGL
                SRC ${NeHeDemo_SRC}
                RESOURCES ${NeHeDemo_Resources})

