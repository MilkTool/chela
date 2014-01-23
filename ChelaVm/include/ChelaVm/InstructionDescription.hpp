// THIS CODE IS AUTOMATICALLY GENERATED, DO NOT MODIFY

#ifndef CHELA_VM_INSTRUCTION_DESCRIPTION_HPP
#define CHELA_VM_INSTRUCTION_DESCRIPTION_HPP

namespace ChelaVm
{
    struct OpCode
    {
        static const int Nop = 0; //<nop> 
        static const int LoadArg = 1; //<ldarg,u8> 
        static const int LoadLocal = 2; //<ldloc,u8> 
        static const int LoadLocalS = 3; //<ldlocs,u16> 
        static const int StoreLocal = 4; //<stloc,u8> 
        static const int StoreLocalS = 5; //<stlocs,u16> 
        static const int LoadField = 6; //<ldfld,fldid> 
        static const int StoreField = 7; //<stfld,fldid> 
        static const int LoadValue = 8; //<ldval> 
        static const int StoreValue = 9; //<stval> 
        static const int LoadArraySlot = 10; //<ldaslt,tyid> 
        static const int StoreArraySlot = 11; //<staslt,tyid> 
        static const int LoadGlobal = 12; //<ldgbl,gblid> 
        static const int StoreGlobal = 13; //<stgbl,gblid> 
        static const int LoadSwizzle = 14; //<ldswi,u8,u8> 
        static const int StoreSwizzle = 15; //<stswi,u8,u8> 
        static const int LoadLocalAddr = 16; //<ldloca,u16> 
        static const int LoadLocalRef = 17; //<ldlocr,u16> 
        static const int LoadFieldAddr = 18; //<ldflda,fldid> 
        static const int LoadFieldRef = 19; //<ldfldr,fldid> 
        static const int LoadGlobalAddr = 20; //<ldgbla,gblid> 
        static const int LoadGlobalRef = 21; //<ldgblr,gblid> 
        static const int LoadArraySlotAddr = 22; //<ldaslta,tyid> 
        static const int LoadArraySlotRef = 23; //<ldasltr,tyid> 
        static const int LoadFunctionAddr = 24; //<ldfna,fnid> 
        static const int LoadBool = 25; //<ldbool,u8> 
        static const int LoadChar = 26; //<ldchar,u16> 
        static const int LoadInt8 = 27; //<ldint8,i8> 
        static const int LoadUInt8 = 28; //<lduint8,u8> 
        static const int LoadInt16 = 29; //<ldint16,i16> 
        static const int LoadUInt16 = 30; //<lduint16,u16> 
        static const int LoadInt32 = 31; //<ldint32,i32> 
        static const int LoadUInt32 = 32; //<lduint32,u32> 
        static const int LoadInt64 = 33; //<ldint64,i64> 
        static const int LoadUInt64 = 34; //<lduint64,u64> 
        static const int LoadFp32 = 35; //<ldfp32,fp32> 
        static const int LoadFp64 = 36; //<ldfp64,fp64> 
        static const int LoadNull = 37; //<ldnull> 
        static const int LoadCString = 38; //<ldstr,sid> 
        static const int LoadString = 39; //<ldstr,sid> 
        static const int LoadDefault = 40; //<lddef,tyid> 
        static const int Add = 41; //<add> 
        static const int Sub = 42; //<sub> 
        static const int Mul = 43; //<mul> 
        static const int Div = 44; //<div> 
        static const int Mod = 45; //<mod> 
        static const int Neg = 46; //<neg> 
        static const int ACos = 47; //<acos> 
        static const int ASin = 48; //<asin> 
        static const int ATan = 49; //<atan> 
        static const int ATan2 = 50; //<atan2> 
        static const int Cos = 51; //<cos> 
        static const int Ln = 52; //<ln> 
        static const int Exp = 53; //<exp> 
        static const int Sqrt = 54; //<sqrt> 
        static const int Sin = 55; //<sin> 
        static const int Tan = 56; //<tan> 
        static const int Dot = 57; //<dot> 
        static const int Cross = 58; //<cross> 
        static const int MatMul = 59; //<mmul> 
        static const int MatInv = 60; //<minv> 
        static const int Transpose = 61; //<trans> 
        static const int Not = 62; //<not> 
        static const int And = 63; //<and> 
        static const int Or = 64; //<or> 
        static const int Xor = 65; //<xor> 
        static const int ShLeft = 66; //<shl> 
        static const int ShRight = 67; //<shr> 
        static const int CmpZ = 68; //<cmp.z> 
        static const int CmpNZ = 69; //<cmp.nz> 
        static const int CmpEQ = 70; //<cmp.eq> 
        static const int CmpNE = 71; //<cmp.ne> 
        static const int CmpLT = 72; //<cmp.lt> 
        static const int CmpLE = 73; //<cmp.le> 
        static const int CmpGT = 74; //<cmp.gt> 
        static const int CmpGE = 75; //<cmp.ge> 
        static const int IntCast = 76; //<intcast,i8> 
        static const int IntToFP = 77; //<int2fp,u8> 
        static const int FPToInt = 78; //<fp2int,i8> 
        static const int FPCast = 79; //<fpcast,u8> 
        static const int PtrToSize = 80; //<ptr2sz> 
        static const int SizeToPtr = 81; //<sz2ptr,tyid> 
        static const int SizeToRef = 82; //<sz2ref,tyid> 
        static const int RefToSize = 83; //<ref2sz> 
        static const int RefToPtr = 84; //<ref2ptr,tyid> 
        static const int PtrToRef = 85; //<ptr2ref,tyid> 
        static const int PtrCast = 86; //<ptrcast,tyid> 
        static const int RefCast = 87; //<refcast,tyid> 
        static const int BitCast = 88; //<bitcast,tyid> 
        static const int Cast = 89; //<cast,tyid> 
        static const int GCast = 90; //<gcast,tyid> 
        static const int IsA = 91; //<isa,tyid> 
        static const int NewObject = 92; //<newobj,tyid,fnid,u8> 
        static const int NewStruct = 93; //<newstr,tyid,fnid,u8> 
        static const int NewVector = 94; //<newvec,tyid> 
        static const int NewMatrix = 95; //<newvec,tyid> 
        static const int NewDelegate = 96; //<newdel,tyid,fnid> 
        static const int NewArray = 97; //<newarray,tyid> 
        static const int NewStackObject = 98; //<newskobj,tyid,fnid,u8> 
        static const int NewStackArray = 99; //<newskrarray,tyid> 
        static const int NewRawObject = 100; //<newraobj,tyid,fnid,u8> 
        static const int NewRawArray = 101; //<newrarray,tyid> 
        static const int DeleteObject = 102; //<delobj> 
        static const int DeleteRawArray = 103; //<delrarray> 
        static const int Box = 104; //<box,tyid> 
        static const int Unbox = 105; //<unbox,tyid> 
        static const int PrimBox = 106; //<pribox,tyid> 
        static const int ExtractPrim = 107; //<extpri> 
        static const int SizeOf = 108; //<sizeof,tyid> 
        static const int TypeOf = 109; //<typeof,tyid> 
        static const int Throw = 110; //<throw> 
        static const int Jmp = 111; //<jmp,bblid> 
        static const int Br = 112; //<br,bblid,bblid> 
        static const int JumpResume = 113; //<jmprs,bblid> 
        static const int Switch = 114; //<switch,jmptbl> 
        static const int Call = 115; //<call,fnid,u8> 
        static const int CallVirtual = 116; //<vcall,fnid,u8> 
        static const int CallIface = 117; //<vcall,tyid,u16,u8> 
        static const int CallIndirect = 118; //<icall,u8> 
        static const int CallDynamic = 119; //<dcall,u8> 
        static const int BindKernel = 120; //<bindk,tyid,fnid> 
        static const int Ret = 121; //<ret> 
        static const int RetVoid = 122; //<retv> 
        static const int Checked = 123; //<checked> 
        static const int Push = 124; //<push,u8> 
        static const int Pop = 125; //<pop> 
        static const int Dup = 126; //<dup,u8> 
        static const int Dup1 = 127; //<dup1> 
        static const int Dup2 = 128; //<dup2> 
        static const int Remove = 129; //<rm,u8> 
        static const int Invalid = 130; //<invalid> 
    };

    struct InstructionDescription
    {
        enum ArgumentType
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
        };

        const char *mnemonic;
        int numargs;
        const ArgumentType *args;
        const char *description;
    };

    const InstructionDescription::ArgumentType InstructionArgumentTable[] =
    {
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt16,
        InstructionDescription::UInt8,
        InstructionDescription::UInt16,
        InstructionDescription::FieldID,
        InstructionDescription::FieldID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::GlobalID,
        InstructionDescription::GlobalID,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt16,
        InstructionDescription::UInt16,
        InstructionDescription::FieldID,
        InstructionDescription::FieldID,
        InstructionDescription::GlobalID,
        InstructionDescription::GlobalID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::UInt16,
        InstructionDescription::Int8,
        InstructionDescription::UInt8,
        InstructionDescription::Int16,
        InstructionDescription::UInt16,
        InstructionDescription::Int32,
        InstructionDescription::UInt32,
        InstructionDescription::Int64,
        InstructionDescription::UInt64,
        InstructionDescription::Fp32,
        InstructionDescription::Fp64,
        InstructionDescription::StringID,
        InstructionDescription::StringID,
        InstructionDescription::TypeID,
        InstructionDescription::Int8,
        InstructionDescription::UInt8,
        InstructionDescription::Int8,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::TypeID,
        InstructionDescription::BasicBlockID,
        InstructionDescription::BasicBlockID,
        InstructionDescription::BasicBlockID,
        InstructionDescription::BasicBlockID,
        InstructionDescription::JumpTable,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::UInt16,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::TypeID,
        InstructionDescription::FunctionID,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8,
        InstructionDescription::UInt8
    };

    const InstructionDescription InstructionTable[] =
    {
        {"nop", 0, &InstructionArgumentTable[0], ""},
        {"ldarg", 1, &InstructionArgumentTable[0], ""},
        {"ldloc", 1, &InstructionArgumentTable[1], ""},
        {"ldlocs", 1, &InstructionArgumentTable[2], ""},
        {"stloc", 1, &InstructionArgumentTable[3], ""},
        {"stlocs", 1, &InstructionArgumentTable[4], ""},
        {"ldfld", 1, &InstructionArgumentTable[5], ""},
        {"stfld", 1, &InstructionArgumentTable[6], ""},
        {"ldval", 0, &InstructionArgumentTable[7], ""},
        {"stval", 0, &InstructionArgumentTable[7], ""},
        {"ldaslt", 1, &InstructionArgumentTable[7], ""},
        {"staslt", 1, &InstructionArgumentTable[8], ""},
        {"ldgbl", 1, &InstructionArgumentTable[9], ""},
        {"stgbl", 1, &InstructionArgumentTable[10], ""},
        {"ldswi", 2, &InstructionArgumentTable[11], ""},
        {"stswi", 2, &InstructionArgumentTable[13], ""},
        {"ldloca", 1, &InstructionArgumentTable[15], ""},
        {"ldlocr", 1, &InstructionArgumentTable[16], ""},
        {"ldflda", 1, &InstructionArgumentTable[17], ""},
        {"ldfldr", 1, &InstructionArgumentTable[18], ""},
        {"ldgbla", 1, &InstructionArgumentTable[19], ""},
        {"ldgblr", 1, &InstructionArgumentTable[20], ""},
        {"ldaslta", 1, &InstructionArgumentTable[21], ""},
        {"ldasltr", 1, &InstructionArgumentTable[22], ""},
        {"ldfna", 1, &InstructionArgumentTable[23], ""},
        {"ldbool", 1, &InstructionArgumentTable[24], ""},
        {"ldchar", 1, &InstructionArgumentTable[25], ""},
        {"ldint8", 1, &InstructionArgumentTable[26], ""},
        {"lduint8", 1, &InstructionArgumentTable[27], ""},
        {"ldint16", 1, &InstructionArgumentTable[28], ""},
        {"lduint16", 1, &InstructionArgumentTable[29], ""},
        {"ldint32", 1, &InstructionArgumentTable[30], ""},
        {"lduint32", 1, &InstructionArgumentTable[31], ""},
        {"ldint64", 1, &InstructionArgumentTable[32], ""},
        {"lduint64", 1, &InstructionArgumentTable[33], ""},
        {"ldfp32", 1, &InstructionArgumentTable[34], ""},
        {"ldfp64", 1, &InstructionArgumentTable[35], ""},
        {"ldnull", 0, &InstructionArgumentTable[36], ""},
        {"ldstr", 1, &InstructionArgumentTable[36], ""},
        {"ldstr", 1, &InstructionArgumentTable[37], ""},
        {"lddef", 1, &InstructionArgumentTable[38], ""},
        {"add", 0, &InstructionArgumentTable[39], ""},
        {"sub", 0, &InstructionArgumentTable[39], ""},
        {"mul", 0, &InstructionArgumentTable[39], ""},
        {"div", 0, &InstructionArgumentTable[39], ""},
        {"mod", 0, &InstructionArgumentTable[39], ""},
        {"neg", 0, &InstructionArgumentTable[39], ""},
        {"acos", 0, &InstructionArgumentTable[39], ""},
        {"asin", 0, &InstructionArgumentTable[39], ""},
        {"atan", 0, &InstructionArgumentTable[39], ""},
        {"atan2", 0, &InstructionArgumentTable[39], ""},
        {"cos", 0, &InstructionArgumentTable[39], ""},
        {"ln", 0, &InstructionArgumentTable[39], ""},
        {"exp", 0, &InstructionArgumentTable[39], ""},
        {"sqrt", 0, &InstructionArgumentTable[39], ""},
        {"sin", 0, &InstructionArgumentTable[39], ""},
        {"tan", 0, &InstructionArgumentTable[39], ""},
        {"dot", 0, &InstructionArgumentTable[39], ""},
        {"cross", 0, &InstructionArgumentTable[39], ""},
        {"mmul", 0, &InstructionArgumentTable[39], ""},
        {"minv", 0, &InstructionArgumentTable[39], ""},
        {"trans", 0, &InstructionArgumentTable[39], ""},
        {"not", 0, &InstructionArgumentTable[39], ""},
        {"and", 0, &InstructionArgumentTable[39], ""},
        {"or", 0, &InstructionArgumentTable[39], ""},
        {"xor", 0, &InstructionArgumentTable[39], ""},
        {"shl", 0, &InstructionArgumentTable[39], ""},
        {"shr", 0, &InstructionArgumentTable[39], ""},
        {"cmp.z", 0, &InstructionArgumentTable[39], ""},
        {"cmp.nz", 0, &InstructionArgumentTable[39], ""},
        {"cmp.eq", 0, &InstructionArgumentTable[39], ""},
        {"cmp.ne", 0, &InstructionArgumentTable[39], ""},
        {"cmp.lt", 0, &InstructionArgumentTable[39], ""},
        {"cmp.le", 0, &InstructionArgumentTable[39], ""},
        {"cmp.gt", 0, &InstructionArgumentTable[39], ""},
        {"cmp.ge", 0, &InstructionArgumentTable[39], ""},
        {"intcast", 1, &InstructionArgumentTable[39], ""},
        {"int2fp", 1, &InstructionArgumentTable[40], ""},
        {"fp2int", 1, &InstructionArgumentTable[41], ""},
        {"fpcast", 1, &InstructionArgumentTable[42], ""},
        {"ptr2sz", 0, &InstructionArgumentTable[43], ""},
        {"sz2ptr", 1, &InstructionArgumentTable[43], ""},
        {"sz2ref", 1, &InstructionArgumentTable[44], ""},
        {"ref2sz", 0, &InstructionArgumentTable[45], ""},
        {"ref2ptr", 1, &InstructionArgumentTable[45], ""},
        {"ptr2ref", 1, &InstructionArgumentTable[46], ""},
        {"ptrcast", 1, &InstructionArgumentTable[47], ""},
        {"refcast", 1, &InstructionArgumentTable[48], ""},
        {"bitcast", 1, &InstructionArgumentTable[49], ""},
        {"cast", 1, &InstructionArgumentTable[50], ""},
        {"gcast", 1, &InstructionArgumentTable[51], ""},
        {"isa", 1, &InstructionArgumentTable[52], ""},
        {"newobj", 3, &InstructionArgumentTable[53], ""},
        {"newstr", 3, &InstructionArgumentTable[56], ""},
        {"newvec", 1, &InstructionArgumentTable[59], ""},
        {"newvec", 1, &InstructionArgumentTable[60], ""},
        {"newdel", 2, &InstructionArgumentTable[61], ""},
        {"newarray", 1, &InstructionArgumentTable[63], ""},
        {"newskobj", 3, &InstructionArgumentTable[64], ""},
        {"newskrarray", 1, &InstructionArgumentTable[67], ""},
        {"newraobj", 3, &InstructionArgumentTable[68], ""},
        {"newrarray", 1, &InstructionArgumentTable[71], ""},
        {"delobj", 0, &InstructionArgumentTable[72], ""},
        {"delrarray", 0, &InstructionArgumentTable[72], ""},
        {"box", 1, &InstructionArgumentTable[72], ""},
        {"unbox", 1, &InstructionArgumentTable[73], ""},
        {"pribox", 1, &InstructionArgumentTable[74], ""},
        {"extpri", 0, &InstructionArgumentTable[75], ""},
        {"sizeof", 1, &InstructionArgumentTable[75], ""},
        {"typeof", 1, &InstructionArgumentTable[76], ""},
        {"throw", 0, &InstructionArgumentTable[77], ""},
        {"jmp", 1, &InstructionArgumentTable[77], ""},
        {"br", 2, &InstructionArgumentTable[78], ""},
        {"jmprs", 1, &InstructionArgumentTable[80], ""},
        {"switch", 1, &InstructionArgumentTable[81], ""},
        {"call", 2, &InstructionArgumentTable[82], ""},
        {"vcall", 2, &InstructionArgumentTable[84], ""},
        {"vcall", 3, &InstructionArgumentTable[86], ""},
        {"icall", 1, &InstructionArgumentTable[89], ""},
        {"dcall", 1, &InstructionArgumentTable[90], ""},
        {"bindk", 2, &InstructionArgumentTable[91], ""},
        {"ret", 0, &InstructionArgumentTable[93], ""},
        {"retv", 0, &InstructionArgumentTable[93], ""},
        {"checked", 0, &InstructionArgumentTable[93], ""},
        {"push", 1, &InstructionArgumentTable[93], ""},
        {"pop", 0, &InstructionArgumentTable[94], ""},
        {"dup", 1, &InstructionArgumentTable[94], ""},
        {"dup1", 0, &InstructionArgumentTable[95], ""},
        {"dup2", 0, &InstructionArgumentTable[95], ""},
        {"rm", 1, &InstructionArgumentTable[95], ""},
        {"invalid", 0, &InstructionArgumentTable[96], ""}
    };
}

#endif //CHELA_VM_INSTRUCTION_DESCRIPTION_HPP
