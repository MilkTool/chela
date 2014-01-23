namespace Chela.Compiler.Ast
{
    public class RefExpression: Expression
    {
        private bool isOut;
        private Expression variable;

        public RefExpression (bool isOut, Expression variable, TokenPosition position)
            : base(position)
        {
            this.isOut = isOut;
            this.variable = variable;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public bool IsOut()
        {
            return isOut;
        }

        public Expression GetVariableExpr()
        {
            return variable;
        }
    }
}

