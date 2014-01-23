namespace Chela.Compiler.Ast
{
	public class LocalVariablesDeclaration: AstNode
	{
		private Expression type;
		private VariableDeclaration variables;
		
		public LocalVariablesDeclaration (Expression type, VariableDeclaration variables,
		                                  TokenPosition position)
			: base(position)
		{
			this.type = type;
			this.variables = variables;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetTypeExpression()
		{
			return this.type;
		}
		
		public VariableDeclaration GetDeclarations()
		{
			return this.variables;
		}
	}
}

