namespace Chela.Compiler.Ast
{
	public class ForStatement: ScopeNode
	{
		private AstNode decls;
		private Expression cond;
		private AstNode incr;
		
		public ForStatement (AstNode decls, Expression cond, AstNode incr, AstNode job,
		                     TokenPosition position)
			: base(job, position)
		{
			this.decls = decls;
			this.cond = cond;
			this.incr = incr;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public AstNode GetDecls()
		{
			return this.decls;
		}
		
		public Expression GetCondition()
		{
			return this.cond;
		}
		
		public AstNode GetIncr()
		{
			return this.incr;
		}
	}
}

