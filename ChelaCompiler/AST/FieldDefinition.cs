using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class FieldDefinition: AstNode
	{
		private MemberFlags flags;
		private Expression typeNode;
		private FieldDeclaration declarations;
		
		public FieldDefinition (MemberFlags flags, Expression typeNode,
		                        FieldDeclaration declarations, TokenPosition position)
			: base(position)
		{
			this.flags = flags;
			this.typeNode = typeNode;
			this.declarations = declarations;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public MemberFlags GetFlags()
		{
			return flags;
		}

        public void SetFlags(MemberFlags flags)
        {
            this.flags = flags;
        }
		
		public Expression GetTypeNode()
		{
			return this.typeNode;
		}
		
		public FieldDeclaration GetDeclarations()
		{
			return declarations;
		}
	}
}

