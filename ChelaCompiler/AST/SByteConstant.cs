using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class SByteConstant: ConstantExpression
    {
        private sbyte value;

        public SByteConstant (sbyte value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetSByteType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public sbyte GetValue()
        {
            return this.value;
        }
    }
}

