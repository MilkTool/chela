namespace Chela.Compiler.Ast
{
    public class BaseExpression: Expression
    {
        public BaseExpression (TokenPosition position)
            : base(position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
