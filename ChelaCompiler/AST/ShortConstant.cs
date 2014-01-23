using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class ShortConstant: ConstantExpression
    {
        private short value;

        public ShortConstant (short value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetShortType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public short GetValue()
        {
            return this.value;
        }
    }
}

