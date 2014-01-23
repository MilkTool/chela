namespace Chela.Compiler.Module
{
	public class OpaqueType: ChelaType
	{
		internal OpaqueType ()
		{
		}
		
		public override string GetName ()
		{
			return "<opaque>";
		}
		
		public override bool IsOpaque()
		{
			return true;
		}
	}
}

