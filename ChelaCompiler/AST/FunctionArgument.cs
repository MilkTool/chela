using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class FunctionArgument: AstNode
	{
		private LocalVariable variable;
		private Expression typeExpression;
        private bool @params;
		
		public FunctionArgument (Expression typeExpression, bool @params, string name, TokenPosition position)
			: base(position)
		{
			SetName(name);
			this.typeExpression = typeExpression;
            this.@params = @params;
			this.variable = null;
		}

        public FunctionArgument (Expression typeExpression, string name, TokenPosition position)
            : this(typeExpression, false, name, position)
        {
        }

		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetTypeNode()
		{
			return typeExpression;
		}

        public bool IsParams()
        {
            return this.@params;
        }
		
		public LocalVariable GetVariable()
		{
			return this.variable;
		}
		
		public void SetVariable(LocalVariable variable)
		{
			this.variable = variable;
		}
	}
}

