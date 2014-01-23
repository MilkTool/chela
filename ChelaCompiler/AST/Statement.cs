namespace Chela.Compiler.Ast
{
	public class Statement: AstNode
	{
		public Statement(TokenPosition position)
			: base(position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

