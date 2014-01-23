using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class ULongConstant: ConstantExpression
    {
        private ulong value;

        public ULongConstant (ulong value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetULongType()));
            this.value = value;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
        
        public ulong GetValue()
        {
            return this.value;
        }
    }
}

