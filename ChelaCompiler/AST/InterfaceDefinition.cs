using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
    public class InterfaceDefinition: StructDefinition
    {
        public InterfaceDefinition(MemberFlags flags, string name,
                               AstNode bases, AstNode children, TokenPosition position)
            : base(flags, name, bases, children, position)
        {
        }

        public InterfaceDefinition(MemberFlags flags, string name,
                               GenericSignature genericSignature,
                               AstNode bases, AstNode children, TokenPosition position)
            : base(flags, name, genericSignature,
                   bases, children, position)
        {
        }
        
        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }   
    }
}
