namespace Chela.Compiler.Ast
{
    public class TernaryOperation: Expression
    {
        private Expression cond;
        private Expression left;
        private Expression right;
        
        public TernaryOperation (Expression cond, Expression left, Expression right,
                                TokenPosition position)
            : base(position)
        {
            this.cond = cond;
            this.left = left;
            this.right = right;
        }
        
        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
        
        public Expression GetCondExpression()
        {
            return this.cond;
        }

        public Expression GetLeftExpression()
        {
            return this.left;
        }
        
        public Expression GetRightExpression()
        {
            return this.right;
        }
    }
}

