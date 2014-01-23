namespace Chela.Compiler.Ast
{
	public class UnaryOperation: Expression
	{
		public const int OpNop = 0;
		public const int OpNeg = 1;
		public const int OpNot = 2;
		public const int OpBitNot = 3;
		
		private int operation;
		private Expression argument;
		
		public UnaryOperation (int operation, Expression argument, TokenPosition position)
			: base(position)
		{
			this.operation = operation;
			this.argument = argument;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public int GetOperation()
		{
			return this.operation;
		}
		
		public Expression GetExpression()
		{
			return this.argument;
		}

        public void SetExpression(Expression expr)
        {
            this.argument = expr;
        }
	}
}

