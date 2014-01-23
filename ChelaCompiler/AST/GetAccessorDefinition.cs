using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class GetAccessorDefinition: PropertyAccessor
    {
        public GetAccessorDefinition (MemberFlags flags, AstNode content, TokenPosition position)
            : base(flags, content, position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override bool IsGetAccessor ()
        {
            return true;
        }
    }
}

