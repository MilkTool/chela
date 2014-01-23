using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class NamespaceDefinition: ScopeNode
	{
		private Namespace nspace;
		
		public NamespaceDefinition (string name, AstNode children, TokenPosition position)
			: base(children, position)
		{
			SetName(name);
			this.nspace = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Namespace GetNamespace()
		{
			return this.nspace;
		}
		
		public void SetNamespace(Namespace nspace)
		{
			this.nspace = nspace;
		}
	}
}

