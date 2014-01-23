using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class FieldDeclaration: AstNode
	{
		private Expression defaultValue;
		private Variable variable;
		
		public FieldDeclaration (string name, Expression defaultValue, TokenPosition position)
			: base(position)
		{
			SetName(name);
			this.defaultValue = defaultValue;
			this.variable = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetDefaultValue()
		{
			return this.defaultValue;
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

