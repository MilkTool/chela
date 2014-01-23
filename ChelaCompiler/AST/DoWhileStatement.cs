namespace Chela.Compiler.Ast
{
	public class DoWhileStatement: WhileStatement
	{
		public DoWhileStatement (Expression cond, AstNode job, TokenPosition position)
			: base(cond, job, position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

