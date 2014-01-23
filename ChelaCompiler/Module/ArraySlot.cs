namespace Chela.Compiler.Module
{
	public class ArraySlot: Variable
	{
        private IChelaType arrayType;

		public ArraySlot (IChelaType type, IChelaType arrayType)
			: base(type, null)
		{
            this.arrayType = arrayType;
		}
		
		public override bool IsArraySlot ()
		{
			return true;
		}

        public IChelaType GetArrayType()
        {
            return arrayType;
        }

        public int Dimensions {
            get {
                if(arrayType.IsPointer())
                    return 1;
                ArrayType array = (ArrayType)arrayType;
                return array.GetDimensions();
            }
        }
	}
}

