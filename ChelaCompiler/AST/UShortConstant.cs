using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class UShortConstant: ConstantExpression
    {
        private ushort value;

        public UShortConstant (ushort value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetUShortType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public ushort GetValue()
        {
            return this.value;
        }
    }
}

