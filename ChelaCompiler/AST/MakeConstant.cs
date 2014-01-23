namespace Chela.Compiler.Ast
{
    public class MakeConstant: Expression
    {
        private Expression valueType;
        
        public MakeConstant (Expression valueType, TokenPosition position)
            : base(position)
        {
            this.valueType = valueType;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetValueType()
        {
            return valueType;
        }
    }
}

