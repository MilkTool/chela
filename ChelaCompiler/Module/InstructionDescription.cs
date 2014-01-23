// THIS CODE IS AUTOMATICALLY GENERATED, DO NOT MODIFY

namespace Chela.Compiler.Module
{
    public enum InstructionArgumentType
    {
        UInt8 = 0,
        UInt8V = 1,
        Int8 = 2,
        Int8V = 3,
        UInt16 = 4,
        UInt16V = 5,
        Int16 = 6,
        Int16V = 7,
        UInt32 = 8,
        UInt32V = 9,
        Int32 = 10,
        Int32V = 11,
        UInt64 = 12,
        UInt64V = 13,
        Int64 = 14,
        Int64V = 15,
        Fp32 = 16,
        Fp32V = 17,
        Fp64 = 18,
        Fp64V = 19,
        TypeID = 20,
        GlobalID = 21,
        FieldID = 22,
        FunctionID = 23,
        StringID = 24,
        BasicBlockID = 25,
        JumpTable = 26
    }

    public sealed class InstructionDescription
    {
        private string mnemonic;
        private InstructionArgumentType[] args;
        private string description;

        public InstructionDescription(string mnemonic, InstructionArgumentType[] args, string description)
        {
            this.mnemonic = mnemonic;
            this.args = args;
            this.description = description;
        }

        public string GetMnemonic()
        {
            return this.mnemonic;
        }

        public InstructionArgumentType[] GetArguments()
        {
            return this.args;
        }

        public string GetDescription()
        {
            return this.description;
        }

        private static InstructionDescription[] instructionTable = new InstructionDescription[]
        {
            new InstructionDescription("nop", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("ldarg", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ldloc", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ldlocs", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("stloc", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("stlocs", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("ldfld", new InstructionArgumentType[] {InstructionArgumentType.FieldID}, ""),
            new InstructionDescription("stfld", new InstructionArgumentType[] {InstructionArgumentType.FieldID}, ""),
            new InstructionDescription("ldval", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("stval", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("ldaslt", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("staslt", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ldgbl", new InstructionArgumentType[] {InstructionArgumentType.GlobalID}, ""),
            new InstructionDescription("stgbl", new InstructionArgumentType[] {InstructionArgumentType.GlobalID}, ""),
            new InstructionDescription("ldswi", new InstructionArgumentType[] {InstructionArgumentType.UInt8, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("stswi", new InstructionArgumentType[] {InstructionArgumentType.UInt8, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ldloca", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("ldlocr", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("ldflda", new InstructionArgumentType[] {InstructionArgumentType.FieldID}, ""),
            new InstructionDescription("ldfldr", new InstructionArgumentType[] {InstructionArgumentType.FieldID}, ""),
            new InstructionDescription("ldgbla", new InstructionArgumentType[] {InstructionArgumentType.GlobalID}, ""),
            new InstructionDescription("ldgblr", new InstructionArgumentType[] {InstructionArgumentType.GlobalID}, ""),
            new InstructionDescription("ldaslta", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ldasltr", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ldfna", new InstructionArgumentType[] {InstructionArgumentType.FunctionID}, ""),
            new InstructionDescription("ldbool", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ldchar", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("ldint8", new InstructionArgumentType[] {InstructionArgumentType.Int8}, ""),
            new InstructionDescription("lduint8", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ldint16", new InstructionArgumentType[] {InstructionArgumentType.Int16}, ""),
            new InstructionDescription("lduint16", new InstructionArgumentType[] {InstructionArgumentType.UInt16}, ""),
            new InstructionDescription("ldint32", new InstructionArgumentType[] {InstructionArgumentType.Int32}, ""),
            new InstructionDescription("lduint32", new InstructionArgumentType[] {InstructionArgumentType.UInt32}, ""),
            new InstructionDescription("ldint64", new InstructionArgumentType[] {InstructionArgumentType.Int64}, ""),
            new InstructionDescription("lduint64", new InstructionArgumentType[] {InstructionArgumentType.UInt64}, ""),
            new InstructionDescription("ldfp32", new InstructionArgumentType[] {InstructionArgumentType.Fp32}, ""),
            new InstructionDescription("ldfp64", new InstructionArgumentType[] {InstructionArgumentType.Fp64}, ""),
            new InstructionDescription("ldnull", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("ldstr", new InstructionArgumentType[] {InstructionArgumentType.StringID}, ""),
            new InstructionDescription("ldstr", new InstructionArgumentType[] {InstructionArgumentType.StringID}, ""),
            new InstructionDescription("lddef", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("add", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("sub", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("mul", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("div", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("mod", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("neg", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("acos", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("asin", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("atan", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("atan2", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cos", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("ln", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("exp", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("sqrt", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("sin", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("tan", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("dot", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cross", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("mmul", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("minv", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("trans", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("not", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("and", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("or", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("xor", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("shl", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("shr", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.z", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.nz", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.eq", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.ne", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.lt", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.le", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.gt", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("cmp.ge", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("intcast", new InstructionArgumentType[] {InstructionArgumentType.Int8}, ""),
            new InstructionDescription("int2fp", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("fp2int", new InstructionArgumentType[] {InstructionArgumentType.Int8}, ""),
            new InstructionDescription("fpcast", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("ptr2sz", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("sz2ptr", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("sz2ref", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ref2sz", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("ref2ptr", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ptr2ref", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("ptrcast", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("refcast", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("bitcast", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("cast", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("gcast", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("isa", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("newobj", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("newstr", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("newvec", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("newvec", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("newdel", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID}, ""),
            new InstructionDescription("newarray", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("newskobj", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("newskrarray", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("newraobj", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("newrarray", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("delobj", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("delrarray", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("box", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("unbox", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("pribox", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("extpri", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("sizeof", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("typeof", new InstructionArgumentType[] {InstructionArgumentType.TypeID}, ""),
            new InstructionDescription("throw", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("jmp", new InstructionArgumentType[] {InstructionArgumentType.BasicBlockID}, ""),
            new InstructionDescription("br", new InstructionArgumentType[] {InstructionArgumentType.BasicBlockID, InstructionArgumentType.BasicBlockID}, ""),
            new InstructionDescription("jmprs", new InstructionArgumentType[] {InstructionArgumentType.BasicBlockID}, ""),
            new InstructionDescription("switch", new InstructionArgumentType[] {InstructionArgumentType.JumpTable}, ""),
            new InstructionDescription("call", new InstructionArgumentType[] {InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("vcall", new InstructionArgumentType[] {InstructionArgumentType.FunctionID, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("vcall", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.UInt16, InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("icall", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("dcall", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("bindk", new InstructionArgumentType[] {InstructionArgumentType.TypeID, InstructionArgumentType.FunctionID}, ""),
            new InstructionDescription("ret", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("retv", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("checked", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("push", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("pop", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("dup", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("dup1", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("dup2", new InstructionArgumentType[] {}, ""),
            new InstructionDescription("rm", new InstructionArgumentType[] {InstructionArgumentType.UInt8}, ""),
            new InstructionDescription("invalid", new InstructionArgumentType[] {}, "")
        };

        public static InstructionDescription[] GetInstructionTable()
        {
            return instructionTable;
        }
    }
}
