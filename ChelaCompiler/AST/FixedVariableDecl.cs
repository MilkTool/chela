using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class FixedVariableDecl: AstNode
    {
        private Expression value;
        private LocalVariable variable;

        public FixedVariableDecl (string name, Expression value, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.value = value;
            this.variable = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetValueExpression()
        {
            return value;
        }

        public LocalVariable GetVariable()
        {
            return variable;
        }

        public void SetVariable(LocalVariable variable)
        {
            this.variable = variable;
        }
    }
}

