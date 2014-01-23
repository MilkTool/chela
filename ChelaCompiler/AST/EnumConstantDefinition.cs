using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class EnumConstantDefinition: AstNode
    {
        private Expression value;
        private FieldVariable variable;

        public EnumConstantDefinition (string name, Expression value, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetValue()
        {
            return this.value;
        }

        public void SetValue(Expression value)
        {
            this.value = value;
        }

        public FieldVariable GetVariable()
        {
            return this.variable;
        }

        public void SetVariable(FieldVariable variable)
        {
            this.variable = variable;
        }
    }
}

