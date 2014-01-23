namespace Chela.Compiler.Module
{
	public class FunctionGroupType: ChelaType
	{
		private int tag;
		
		internal FunctionGroupType (int tag)
		{
			this.tag = tag;
		}
		
		public override string GetName ()
		{
			return "<function-group>";
		}
		
		public override bool IsFunctionGroup ()
		{
			return true;
		}
		
		public int GetTag ()
		{
			return this.tag;
		}
	}
}

