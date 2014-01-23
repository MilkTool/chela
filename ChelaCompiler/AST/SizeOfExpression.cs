using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class SizeOfExpression: Expression
    {
        private Expression typeExpression;

        public SizeOfExpression (Expression typeExpression, TokenPosition position)
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

