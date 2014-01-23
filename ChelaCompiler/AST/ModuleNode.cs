namespace Chela.Compiler.Ast
{
	public class ModuleNode: ScopeNode 
	{
		public ModuleNode (AstNode children, TokenPosition position)
			: base(children, position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

