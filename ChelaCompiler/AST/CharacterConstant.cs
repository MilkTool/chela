using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class CharacterConstant: ConstantExpression
    {
        private char value;

        public CharacterConstant (char value, TokenPosition position)
            : base(position)
        {
            SetNodeType(ConstantType.Create(ChelaType.GetCharType()));
            this.value = value;
        }

        public override AstNode Accept(AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public char GetValue()
        {
            return this.value;
        }
    }
}

