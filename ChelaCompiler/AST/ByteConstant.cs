using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class ByteConstant: ConstantExpression
    {
        private byte value;

        public ByteConstant (byte value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetByteType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public byte GetValue()
        {
            return this.value;
        }
    }
}

