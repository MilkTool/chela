namespace Chela.Compiler.Ast
{
	public class BlockNode: ScopeNode
	{
		public BlockNode (AstNode children, TokenPosition position)
			: base(children, position)
		{
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

