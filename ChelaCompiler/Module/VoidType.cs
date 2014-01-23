using System;
namespace Chela.Compiler.Module
{
	public class VoidType: PrimitiveType
	{
		internal VoidType ()
			: base(PrimitiveTypeId.Void)
		{
		}
		
		public override string GetName ()
		{
			return "void";
		}
		
		public override bool IsVoid ()
		{
			return true;
		}
		
		public override bool IsPrimitive ()
		{
			return true;
		}
	}
}

