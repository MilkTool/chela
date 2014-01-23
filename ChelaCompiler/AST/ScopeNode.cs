using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class ScopeNode: AstNode
	{
		private AstNode children;
		private Scope scope;
		
		public ScopeNode (AstNode children, TokenPosition position)
			: base(position)
		{
			this.children = children;
			this.scope = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public AstNode GetChildren()
		{
			return children;
		}
		
		public void SetChildren(AstNode children)
		{
			this.children = children;
		}

        public void AddFirst(AstNode child)
        {
            if(child == null)
                return;
            
            if(this.children == null)
            {
                this.children = child;
                return;
            }

            // Add my children to the end of the list.
            AstNode lastChild = child;
            while(lastChild.GetNext() != null)
                lastChild = lastChild.GetNext();
            lastChild.SetNext(children);

            // Set the first.
            children = child;
        }
		
		public void AddLast(AstNode child)
		{
			if(child == null)
				return;
			
			if(this.children == null)
			{
				this.children = child;
				return;
			}
			
			AstNode lastChild = this.children;
			while(lastChild.GetNext() != null)
				lastChild = lastChild.GetNext();
			lastChild.SetNext(child);
		}
		
		public Scope GetScope()
		{
			return this.scope;
		}
		
		public void SetScope(Scope scope)
		{
			this.scope = scope;
		}
	}
}

