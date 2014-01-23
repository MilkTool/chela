using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class LongConstant: ConstantExpression
    {
        private long value;
        
        public LongConstant (long value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetLongType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
        
        public long GetValue()
        {
            return this.value;
        }
    }
}

