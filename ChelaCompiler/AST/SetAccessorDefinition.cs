using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class SetAccessorDefinition: PropertyAccessor
    {
        private LocalVariable valueLocal;

        public SetAccessorDefinition (MemberFlags flags, AstNode content, TokenPosition position)
            : base(flags, content, position)
        {
            valueLocal = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override bool IsSetAccessor ()
        {
            return true;
        }

        public LocalVariable GetValueLocal()
        {
            return valueLocal;
        }

        public void SetValueLocal(LocalVariable local)
        {
            valueLocal = local;
        }
    }
}

