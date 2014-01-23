namespace Chela.Compiler.Ast
{
	public class IfStatement: Statement
	{
		private Expression condition;
		private AstNode thenNode;
		private AstNode elseNode;
			
		public IfStatement (Expression condition, AstNode thenNode, AstNode elseNode,
		                    TokenPosition position)
			: base(position)
		{
			this.condition = condition;
			this.thenNode = thenNode;
			this.elseNode = elseNode;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetCondition()
		{
			return this.condition;
		}
		
		public AstNode GetThen()
		{
			return this.thenNode;
		}
		
		public AstNode GetElse()
		{
			return this.elseNode;
		}
	}
}

