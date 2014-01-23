namespace Chela.Compiler.Ast
{
    public class GenericInstanceExpr: Expression
    {
        private Expression genericExpression;
        private AstNode parameters;

        public GenericInstanceExpr (Expression genericExpression, AstNode parameters, TokenPosition position)
            : base(position)
        {
            this.genericExpression = genericExpression;
            this.parameters = parameters;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetGenericExpression()
        {
            return genericExpression;
        }

        public AstNode GetParameters()
        {
            return parameters;
        }
    }
}

