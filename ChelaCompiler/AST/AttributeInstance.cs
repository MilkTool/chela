using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class AttributeInstance: AstNode
    {
        private Expression attributeExpr;
        private AttributeArgument arguments;
        private Class attributeClass;
        private Method attributeConstructor;

        public AttributeInstance (Expression attributeExpr, AttributeArgument arguments,
                                  TokenPosition position)
            : base(position)
        {
            this.attributeExpr = attributeExpr;
            this.arguments = arguments;
            this.attributeClass = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetAttributeExpr()
        {
            return attributeExpr;
        }

        public AttributeArgument GetArguments()
        {
            return arguments;
        }

        public Class GetAttributeClass()
        {
            return attributeClass;
        }

        public void SetAttributeClass(Class attributeClass)
        {
            this.attributeClass = attributeClass;
        }

        public Method GetAttributeConstructor()
        {
            return this.attributeConstructor;
        }

        public void SetAttributeConstructor(Method attributeConstructor)
        {
            this.attributeConstructor = attributeConstructor;
        }
    }
}

