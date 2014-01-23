namespace Chela.Compiler.Ast
{
	public class BreakStatement: Statement
	{
		public BreakStatement (TokenPosition position)
			: base(position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

