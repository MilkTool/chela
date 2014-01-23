
namespace Chela.Compiler.Ast
{
    public class DefaultExpression: Expression
    {
        private Expression typeExpression;

        public DefaultExpression (Expression typeExpression, TokenPosition position)
            : base(position)
        {
            this.typeExpression = typeExpression;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetTypeExpression()
        {
            return typeExpression;
        }
    }
}

