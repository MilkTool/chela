namespace Chela.Compiler.Module
{
	public class ChelaType: ScopeMember, IChelaType
	{
		internal ChelaType()
			: base(null)
		{
		}
		
		protected ChelaType (ChelaModule module)
			: base(module)
		{
		}

        public override Scope GetParentScope ()
        {
            throw null;
        }
		
		public override string GetName ()
		{
			return "<unknown-type>";
		}
		
		public override string GetFullName()
		{
			return GetName();
		}

        public override string GetDisplayName()
        {
            return GetName();
        }
		
		public override string ToString ()
		{
			return GetDisplayName();
		}

        public override bool IsType ()
        {
            return true;
        }

        public virtual bool IsAbstract()
        {
            return false;
        }
        
		public virtual bool IsPrimitive()
		{
			return false;
		}

        public virtual bool IsFirstClass()
        {
            return IsPrimitive() || IsVector() || IsMatrix();
        }

		public virtual bool IsMetaType()
		{
			return false;
		}
		
		public virtual bool IsNumber()
		{
			return false;
		}
		
		public virtual bool IsInteger()
		{
			return false;
		}
		
		public virtual bool IsFloat()
		{
			return false;
		}
		
		public virtual bool IsDouble()
		{
			return false;
		}
		
		public virtual bool IsFloatingPoint()
		{
			return false;
		}
		
		public virtual bool IsUnsigned()
		{
			return false;
		}
		
		public virtual bool IsVoid()
		{
			return false;
		}

        public virtual bool IsConstant()
        {
            return false;
        }
        
		public virtual bool IsPointer()
		{
			return false;
		}

        public virtual bool IsVector()
        {
            return false;
        }

        public virtual bool IsMatrix()
        {
            return false;
        }

        public virtual bool IsArray()
        {
            return false;
        }

		public virtual bool IsReference()
		{
			return false;
		}

        public virtual bool IsOutReference()
        {
            return false;
        }

        public virtual bool IsStreamReference()
        {
            return false;
        }

		public virtual bool IsString()
		{
			return false;
		}
		
		public virtual bool IsOpaque()
		{
			return false;
		}

        public virtual bool IsIncompleteType()
        {
            return false;
        }

        public virtual bool IsPlaceHolderType()
        {
            return false;
        }

        public virtual bool IsTypeInstance()
        {
            return false;
        }

        public virtual bool IsGenericType()
        {
            return false;
        }

		public override bool IsFunction()
		{
			return false;
		}
		
		public override bool IsFunctionGroup ()
		{
			return false;
		}

        public virtual IChelaType InstanceGeneric(GenericInstance args, ChelaModule instModule)
        {
            return this;
        }
		
		public virtual uint GetSize()
		{
			return 0;
		}

        public virtual uint GetComponentSize()
        {
            return this.GetSize();
        }

		// Built-In types
		private static VoidType voidType = new VoidType();
		private static NamespaceType namespaceType = new NamespaceType();
		private static FunctionGroupType staticFunctionGroupType = new FunctionGroupType(1);
		private static FunctionGroupType functionGroupType = new FunctionGroupType(0);
        private static FunctionGroupType anyFunctionGroupType = new FunctionGroupType(2);
		private static IntegerType boolType = new IntegerType("bool", 1, true, PrimitiveTypeId.Bool);
		private static IntegerType byteType = new IntegerType("byte", 1, true, PrimitiveTypeId.UInt8);
		private static IntegerType sbyteType = new IntegerType("sbyte", 1, false, PrimitiveTypeId.Int8);
		private static IntegerType charType = new IntegerType("char", 2, true, PrimitiveTypeId.Char);
		private static IntegerType shortType = new IntegerType("short", 2, false, PrimitiveTypeId.Int16);
		private static IntegerType ushortType = new IntegerType("ushort", 2, true, PrimitiveTypeId.UInt16);
		private static IntegerType intType = new IntegerType("int", 4, false, PrimitiveTypeId.Int32);
		private static IntegerType uintType = new IntegerType("uint", 4, true, PrimitiveTypeId.UInt32);
		private static IntegerType longType = new IntegerType("long", 8, false, PrimitiveTypeId.Int64);
		private static IntegerType ulongType = new IntegerType("ulong", 8, true, PrimitiveTypeId.UInt64);
		private static IntegerType sizeType = new IntegerType("size_t", 100, true, PrimitiveTypeId.Size);
        private static IntegerType ptrdiffType = new IntegerType("ptrdiff_t", 100, false, PrimitiveTypeId.PtrDiff);
		private static FloatType floatType = new FloatType("float", 4, PrimitiveTypeId.Fp32);
		private static FloatType doubleType = new FloatType("double", 8, PrimitiveTypeId.Fp64);
		private static ChelaType stringType = ReferenceType.Create(new StringType());
		private static ChelaType objectType = ReferenceType.Create(new OpaqueType());
		
		public static VoidType GetVoidType()
		{
			return voidType;
		}

        public static PointerType GetConstVoidPtrType()
        {
            return PointerType.Create(ConstantType.Create(GetVoidType()));
        }

		public static PointerType GetVoidPtrType()
		{
			return PointerType.Create(GetVoidType());
		}
		
		public static NamespaceType GetNamespaceType()
		{
			return namespaceType;
		}
		
		public static FunctionGroupType GetStaticFunctionGroupType()
		{
			return staticFunctionGroupType;
		}
		
		public static FunctionGroupType GetFunctionGroupType()
		{
			return functionGroupType;
		}

        public static FunctionGroupType GetAnyFunctionGroupType()
        {
            return anyFunctionGroupType;
        }
		
		public static IntegerType GetBoolType()
		{
			return boolType;
		}
		
		public static IntegerType GetByteType()
		{
			return byteType;
		}
		
		public static IntegerType GetSByteType()
		{
			return sbyteType;
		}
		
		public static IntegerType GetCharType()
		{
			return charType;
		}
		
		public static IntegerType GetShortType()
		{
			return shortType;
		}
		
		public static IntegerType GetUShortType()
		{
			return ushortType;
		}
		
		public static IntegerType GetIntType()
		{
			return intType;
		}
		
		public static IntegerType GetUIntType()
		{
			return uintType;
		}
		
		public static IntegerType GetLongType()
		{
			return longType;
		}
		
		public static IntegerType GetULongType()
		{
			return ulongType;
		}
		
		public static IntegerType GetSizeType()
		{
			return sizeType;
		}		

        public static IntegerType GetPtrDiffType()
        {
            return ptrdiffType;
        }

		public static FloatType GetFloatType()
		{
			return floatType;
		}
		
		public static FloatType GetDoubleType()
		{
			return doubleType;
		}
		
		public static ChelaType GetStringType()
		{
			return stringType;
		}

		public static ChelaType GetObjectType()
		{
			return objectType;
		}
		
		public static IChelaType GetNullType()
		{
			return ReferenceType.Create(null);
		}
	}
}

