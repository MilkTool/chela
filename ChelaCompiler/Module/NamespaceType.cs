namespace Chela.Compiler.Module
{
	public class NamespaceType: ChelaType
	{
		internal NamespaceType ()
		{
		}
		
		public override string GetName ()
		{
			return "<namespace>";
		}
		
		public override bool IsNamespace ()
		{
			return true;
		}
		
	}
}

