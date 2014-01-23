namespace Chela.Compiler.Ast
{
    public class ReinterpretCast: Expression
    {
        private Expression targetType;
        private Expression value;

        public ReinterpretCast(Expression targetType, Expression value, TokenPosition position)
            : base(position)
        {
            this.targetType = targetType;
            this.value = value;
        }

        public override AstNode Accept(AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetTargetType()
        {
            return targetType;
        }

        public Expression GetValue()
        {
            return value;
        }
    }
}

