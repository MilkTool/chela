namespace Chela.Compiler.Ast
{
	public class DeleteStatement: Statement
	{
		private Expression pointer;
		
		public DeleteStatement (Expression pointer, TokenPosition position)
			: base(position)
		{
			this.pointer = pointer;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetPointer()
		{
			return pointer;
		}
	}
}

