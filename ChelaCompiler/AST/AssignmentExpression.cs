using System;
namespace Chela.Compiler.Ast
{
	public class AssignmentExpression: Expression
	{
		private Expression variable;
		private Expression value;
        private bool delayedCoercion;
		
		public AssignmentExpression (Expression variable, Expression value, TokenPosition position)
			: base(position)
		{
			this.variable = variable;
			this.value = value;
            this.delayedCoercion = false;
		}

        public bool DelayedCoercion {
            get {
                return delayedCoercion;
            }
            set {
                delayedCoercion = true;
            }
        }
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetVariable()
		{
			return this.variable;
		}
		
		public Expression GetValue()
		{
			return this.value;
		}
	}
}

