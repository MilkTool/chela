namespace Chela.Compiler.Ast
{
	public class CastOperation: Expression
	{
		private Expression target;
		private Expression value;
		
		public CastOperation (Expression target, Expression value, TokenPosition position)
			: base(position)
		{
			this.target = target;
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetTarget()
		{
			return this.target;
		}

        public void SetValue(Expression value)
        {
            this.value = value;
        }
        
		public Expression GetValue()
		{
			return this.value;
		}
	}
}

