namespace Chela.Compiler.Ast
{
	public class WhileStatement: Statement
	{
		private Expression cond;
		private AstNode job;
		
		public WhileStatement (Expression cond, AstNode job, TokenPosition position)
			: base(position)
		{
			this.cond = cond;
			this.job = job;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetCondition()
		{
			return this.cond;
		}
		
		public AstNode GetJob()
		{
			return this.job;
		}
	}
}

