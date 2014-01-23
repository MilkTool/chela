namespace Chela.Compiler.Ast
{
	public class IndirectAccess: Expression
	{
		private Expression basePointer;
		private int slot;
		
		public IndirectAccess (Expression basePointer, string name, TokenPosition position)
			: base(position)
		{
			SetName(name);
			this.basePointer = basePointer;
			this.slot = -1;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetBasePointer()
		{
			return this.basePointer;
		}
		
		public int GetSlot()
		{
			return this.slot;
		}
		
		public void SetSlot(int slot)
		{
			this.slot = slot;
		}
	}
}

