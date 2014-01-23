namespace Chela.Compiler.Module
{
	public class PrimitiveType: ChelaType
	{
		private PrimitiveTypeId primitiveId;
		
		internal PrimitiveType (PrimitiveTypeId primitiveId)
		{
			this.primitiveId = primitiveId;
		}
		
		public PrimitiveTypeId GetPrimitiveId()
		{
			return this.primitiveId;
		}

        public static IChelaType GetPrimitiveType(uint id)
        {
            switch((PrimitiveTypeId)id)
            {
            case PrimitiveTypeId.Void:
                return ChelaType.GetVoidType();
            case PrimitiveTypeId.UInt8:
                return ChelaType.GetByteType();
            case PrimitiveTypeId.Int8:
                return ChelaType.GetSByteType();
            case PrimitiveTypeId.UInt16:
                return ChelaType.GetUShortType();
            case PrimitiveTypeId.Int16:
                return ChelaType.GetShortType();
            case PrimitiveTypeId.UInt32:
                return ChelaType.GetUIntType();
            case PrimitiveTypeId.Int32:
                return ChelaType.GetIntType();
            case PrimitiveTypeId.UInt64:
                return ChelaType.GetULongType();
            case PrimitiveTypeId.Int64:
                return ChelaType.GetLongType();
            case PrimitiveTypeId.Fp32:
                return ChelaType.GetFloatType();
            case PrimitiveTypeId.Fp64:
                return ChelaType.GetDoubleType();
            case PrimitiveTypeId.Bool:
                return ChelaType.GetBoolType();
            case PrimitiveTypeId.Size:
                return ChelaType.GetSizeType();
            case PrimitiveTypeId.PtrDiff:
                return ChelaType.GetPtrDiffType();
            case PrimitiveTypeId.Char:
                return ChelaType.GetCharType();
            default:
                throw new System.NotImplementedException();
            }

        }
	}
}

