using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class ClassDefinition: StructDefinition
	{
		public ClassDefinition(MemberFlags flags, string name,
		                       AstNode bases, AstNode children, TokenPosition position)
			: base(flags, name, bases, children, position)
		{
		}

        public ClassDefinition(MemberFlags flags, string name,
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

