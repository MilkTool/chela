using Chela.Compiler.Module;
	
namespace Chela.Compiler.Ast
{
	public class VariableDeclaration: AstNode
	{
		private Expression initialValue;
		private Variable variable;
		
		public VariableDeclaration (string name, Expression initialValue,
		                            TokenPosition position)
			: base(position)
		{
			SetName(name);
			this.initialValue = initialValue;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetInitialValue()
		{
			return this.initialValue;
		}
		
		public Variable GetVariable()
		{
			return this.variable;
		}
		
		public void SetVariable(Variable variable)
		{
			this.variable = variable;
		}
	}
}

