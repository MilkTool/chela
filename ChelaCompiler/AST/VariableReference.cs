using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class VariableReference: ObjectReferenceExpression
	{
        private Variable aliasVariable;

		public VariableReference (string name, TokenPosition position)
			: base(position)
		{
			SetName(name);
            this.aliasVariable = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}

        public void SetAliasVariable(Variable variable)
        {
            this.aliasVariable = variable;
        }

        public Variable GetAliasVariable()
        {
            return aliasVariable;
        }
	}
}

