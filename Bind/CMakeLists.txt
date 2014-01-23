set(Bind_SRC
    CL/CLGenerator.cs
    ES/ESGenerator.cs
    GL2/Generator.cs
    Glu/Generator.cs
    Glx/Generator.cs
    Properties/AssemblyInfo.cs
    Properties/GlobalAssemblyInfo.cs
    Structures/Constant.cs
    Structures/Delegate.cs
    Structures/Enum.cs
    Structures/FlowDirection.cs
    Structures/Function.cs
    Structures/Parameter.cs
    Structures/Type.cs
    Wgl/Generator.cs
    BindStreamWriter.cs
    DocProcessor.cs
    IBind.cs
    ISpecReader.cs
    ISpecWriter.cs
    Main.cs
    Settings.cs
    Utilities.cs
)

CSHARP_MODULE(Bind EXECUTABLE REFS System System.Core System.Xml SRC ${Bind_SRC})

