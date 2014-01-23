using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class AttributeArgument: AstNode
    {
        private Expression value;
        private Variable property;
        private Class attributeClass;

        public AttributeArgument (Expression value, string name, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.value = value;
            this.property = null;
            this.attributeClass = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetValueExpression()
        {
            return value;
        }

        public void SetValueExpression(Expression value)
        {
            this.value = value;
        }

        public Variable GetProperty()
        {
            return property;
        }

        public void SetProperty(Variable property)
        {
            this.property = property;
        }

        public Class GetAttributeClass()
        {
            return this.attributeClass;
        }

        public void SetAttributeClass(Class attributeClass)
        {
            this.attributeClass = attributeClass;
        }
    }
}

