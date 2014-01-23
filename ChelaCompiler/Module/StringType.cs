namespace Chela.Compiler.Module
{
	public class StringType: ChelaType
	{
		internal StringType ()
		{
		}

		public override string GetName ()
		{
			return "string";
		}

		public override bool IsString ()
		{
			return true;
		}
		
	}
}

