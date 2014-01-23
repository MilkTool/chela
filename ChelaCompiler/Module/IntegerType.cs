using System;
namespace Chela.Compiler.Module
{
	public class IntegerType: PrimitiveType
	{
		private string name;
		private uint size;
		private bool unsigned;
		
		internal IntegerType (string name, uint size, bool unsigned, PrimitiveTypeId primitiveId)
			: base(primitiveId)
		{
			this.name = name;
			this.size = size;
			this.unsigned = unsigned;
		}
		
		public override string GetName ()
		{
			return this.name;
		}
		
		public override bool IsPrimitive ()
		{
			return true;
		}

        public override bool IsNumber ()
        {
            return true;
        }

		public override bool IsInteger ()
		{
			return true;
		}
		
		public override bool IsFloat ()
		{
			return false;
		}
		
		public override bool IsUnsigned ()
		{
			return this.unsigned;
		}
		
		public override uint GetSize ()
		{
			return this.size;
		}

        public IntegerType GetSignedVersion()
        {
            switch(GetPrimitiveId())
            {
            case PrimitiveTypeId.UInt8:
            case PrimitiveTypeId.Int8:
                return ChelaType.GetSByteType();
            case PrimitiveTypeId.UInt16:
            case PrimitiveTypeId.Int16:
                return ChelaType.GetShortType();
            case PrimitiveTypeId.UInt32:
            case PrimitiveTypeId.Int32:
                return ChelaType.GetIntType();
            case PrimitiveTypeId.UInt64:
            case PrimitiveTypeId.Int64:
                return ChelaType.GetLongType();
            case PrimitiveTypeId.Size:
                return ChelaType.GetSizeType();
            case PrimitiveTypeId.Char:
                return ChelaType.GetCharType();
            case PrimitiveTypeId.Bool:
                throw new System.NotSupportedException();
            default:
                throw new System.NotImplementedException();
            }
        }
	}
}

