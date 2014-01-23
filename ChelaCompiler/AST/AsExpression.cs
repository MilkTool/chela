namespace Chela.Compiler.Ast
{
    public class AsExpression: Expression
    {
        private Expression value;
        private Expression target;

        public AsExpression (Expression value, Expression target, TokenPosition position)
            : base(position)
        {
            this.target = target;
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetTarget()
        {
            return this.target;
        }

        public void SetValue(Expression value)
        {
            this.value = value;
        }

        public Expression GetValue()
        {
            return this.value;
        }
    }
}

