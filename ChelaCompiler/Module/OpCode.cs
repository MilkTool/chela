namespace Chela.Compiler.Module
{
	public enum OpCode
	{
		Nop=0,  		//<nop>
		
		// Variable load/store
		LoadArg,		//<ldarg,u8>
		LoadLocal,		//<ldloc,u8>
		LoadLocalS,		//<ldlocs,u16>
		StoreLocal,		//<stloc,u8>
		StoreLocalS, 	//<stlocs,u16>
		LoadField,		//<ldfld,fldid>
		StoreField,		//<stfld,fldid>
        LoadValue,      //<ldval>
        StoreValue,     //<stval>
		LoadArraySlot,	//<ldaslt,tyid>
		StoreArraySlot,	//<staslt,tyid>
		LoadGlobal,		//<ldgbl,gblid>
		StoreGlobal,	//<stgbl,gblid>
        LoadSwizzle,    //<ldswi,u8,u8>
        StoreSwizzle,   //<stswi,u8,u8>

        // Variable reference.
        LoadLocalAddr,      //<ldloca,u16>
        LoadLocalRef,       //<ldlocr,u16>
        LoadFieldAddr,      //<ldflda,fldid>
        LoadFieldRef,       //<ldfldr,fldid>
        LoadGlobalAddr,     //<ldgbla,gblid>
        LoadGlobalRef,      //<ldgblr,gblid>
        LoadArraySlotAddr,  //<ldaslta,tyid>
        LoadArraySlotRef,   //<ldasltr,tyid>
        LoadFunctionAddr,   //<ldfna,fnid>
		
		// Constant loading
		LoadBool,		//<ldbool,u8>
        LoadChar,       //<ldchar,u16>
		LoadInt8,		//<ldint8,i8>
		LoadUInt8,		//<lduint8,u8>
		LoadInt16,		//<ldint16,i16>
		LoadUInt16,		//<lduint16,u16>
		LoadInt32,		//<ldint32,i32>
		LoadUInt32,		//<lduint32,u32>
		LoadInt64,		//<ldint64,i64>
		LoadUInt64,		//<lduint64,u64>
		LoadFp32,		//<ldfp32,fp32>
		LoadFp64,		//<ldfp64,fp64>
		LoadNull,		//<ldnull>
        LoadCString,    //<ldstr,sid>
		LoadString,		//<ldstr,sid>
        LoadDefault,    //<lddef,tyid>
		
		// Maths
		Add,	//<add>
		Sub,	//<sub>
		Mul,	//<mul>
		Div,	//<div>
		Mod,	//<mod>
		Neg,	//<neg>

        // Mathematical functions.
        ACos,   //<acos>
        ASin,   //<asin>
        ATan,   //<atan>
        ATan2,  //<atan2>
        Cos,    //<cos>
        Ln,     //<ln>
        Exp,    //<exp>
        Sqrt,   //<sqrt>
        Sin,    //<sin>
        Tan,    //<tan>

        // Vectorial operations.
        Dot,    //<dot>
        Cross,  //<cross>

        // Matricial operation.
        MatMul,     //<mmul>
        MatInv,     //<minv>
        Transpose,  //<trans>

		// Bitwise,
		Not,		//<not>
		And,		//<and>
		Or,			//<or>
		Xor,		//<xor>
		ShLeft,		//<shl>
		ShRight,	//<shr>
		
		// Comparison
		CmpZ,		//<cmp.z>
		CmpNZ,		//<cmp.nz>
		CmpEQ,		//<cmp.eq>
		CmpNE,		//<cmp.ne>
		CmpLT,		//<cmp.lt>
		CmpLE,		//<cmp.le>
		CmpGT,		//<cmp.gt>
		CmpGE,		//<cmp.ge>
		
		// Casting
		IntCast,	//<intcast,i8>
		IntToFP,	//<int2fp,u8>
		FPToInt,	//<fp2int,i8>
		FPCast,		//<fpcast,u8>
		PtrToSize,	//<ptr2sz>
		SizeToPtr,	//<sz2ptr,tyid>
		SizeToRef,	//<sz2ref,tyid>
		RefToSize,	//<ref2sz>
		RefToPtr,	//<ref2ptr,tyid>
		PtrToRef,	//<ptr2ref,tyid>
		PtrCast,	//<ptrcast,tyid>
		RefCast,	//<refcast,tyid>
        BitCast,    //<bitcast,tyid>
        Cast,       //<cast,tyid>
        GCast,      //<gcast,tyid>
        IsA,        //<isa,tyid>
		
		// Object management.
		NewObject,		//<newobj,tyid,fnid,u8>
        NewStruct,      //<newstr,tyid,fnid,u8>
        NewVector,      //<newvec,tyid>
        NewMatrix,      //<newvec,tyid>
        NewDelegate,    //<newdel,tyid,fnid>
		NewArray,		//<newarray,tyid>
        NewStackObject, //<newskobj,tyid,fnid,u8>
        NewStackArray,  //<newskrarray,tyid>
        NewRawObject,   //<newraobj,tyid,fnid,u8>
		NewRawArray,	//<newrarray,tyid>
        DeleteObject,   //<delobj>
		DeleteRawArray,	//<delrarray>
        Box,            //<box,tyid>
        Unbox,          //<unbox,tyid>
        PrimBox,        //<pribox,tyid>
        ExtractPrim,    //<extpri>
        SizeOf,         //<sizeof,tyid>
        TypeOf,         //<typeof,tyid>

        // Exception handling.
        Throw,         //<throw>
		
		// Branching
		Jmp,		//<jmp,bblid>
		Br,			//<br,bblid,bblid>
        JumpResume, //<jmprs,bblid>
        Switch,     //<switch,jmptbl>
		
		// Function call/return.
		Call,			//<call,fnid,u8>
		CallVirtual,	//<vcall,fnid,u8>
        CallIface,      //<vcall,tyid,u16,u8>
		CallIndirect,	//<icall,u8>
		CallDynamic,	//<dcall,u8>
        BindKernel,     //<bindk,tyid,fnid>
		Ret,			//<ret>
		RetVoid,		//<retv>

        // Instruction prefixes.
        Checked,        //<checked>

		// Stack tiding.
        Push,           //<push,u8>
		Pop,			//<pop>
		Dup,			//<dup,u8>
		Dup1,			//<dup1>
		Dup2,			//<dup2>
        Remove,         //<rm,u8>
		Invalid,		//<invalid>
	}
}

