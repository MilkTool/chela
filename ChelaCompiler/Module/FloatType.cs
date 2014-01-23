using System;
namespace Chela.Compiler.Module
{
	public class FloatType: PrimitiveType
	{
		private string name;
		private uint size;
		
		public FloatType (string name, uint size, PrimitiveTypeId primitiveId)
			: base(primitiveId)
		{
			this.name = name;
			this.size = size;
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
			return false;
		}
		
		public override bool IsFloat ()
		{
			return size == 4;
		}
		
		public override bool IsDouble ()
		{
			return size == 8;
		}
		
		public override bool IsFloatingPoint ()
		{
			return true;
		}
		
		public override uint GetSize ()
		{
			return size;
		}
	}
}

