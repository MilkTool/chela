namespace Chela.Compiler.Ast
{
	public class ExpressionStatement: Statement
	{
		private Expression expression;
		
		public ExpressionStatement (Expression expression, TokenPosition position)
			: base(position)
		{
			this.expression = expression;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetExpression()
		{
			return this.expression;
		}
	}
}

