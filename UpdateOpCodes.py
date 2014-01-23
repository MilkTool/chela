#!/usr/bin/python

from Tools.OpDesc import *

args = """
opdesc.py -opcodes ChelaCompiler/Module/OpCode.cs
    -cstables ChelaCompiler/Module/InstructionDescription.cs
    -chtables ChelaCore/Reflection/Emition/Instructions.chela
    -ctables ChelaVm/include/ChelaVm/InstructionDescription.hpp    
""".split()
opdesc = OpDesc()
opdesc.Run(args)



