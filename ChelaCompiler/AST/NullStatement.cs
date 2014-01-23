namespace Chela.Compiler.Ast
{
	public class NullStatement: Statement
	{
		public NullStatement(TokenPosition position)
			: base(position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}	
	}
}

