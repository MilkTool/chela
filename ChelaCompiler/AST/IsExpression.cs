using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class IsExpression: Expression
    {
        private Expression compare;
        private Expression value;
        private IChelaType targetType;

        public IsExpression (Expression value, Expression compare, TokenPosition position)
            : base(position)
        {
            this.value = value;
            this.compare = compare;
            this.targetType = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetCompare()
        {
            return this.compare;
        }

        public void SetValue(Expression value)
        {
            this.value = value;
        }

        public Expression GetValue()
        {
            return this.value;
        }

        public void SetTargetType(IChelaType targetType)
        {
            this.targetType = targetType;
        }

        public IChelaType GetTargetType()
        {
            return this.targetType;
        }
    }
}

