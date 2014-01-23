namespace Chela.Compiler.Ast
{
	public class DeleteRawArrayStatement: Statement
	{
		private Expression pointer;
		
		public DeleteRawArrayStatement (Expression pointer, TokenPosition position)
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

