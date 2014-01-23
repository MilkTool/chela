namespace Chela.Compiler.Ast
{
    public class FileNode: AstNode
    {
        private AstNode children;

        public FileNode (AstNode children, TokenPosition position)
            : base(position)
        {
            this.children = children;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public AstNode GetChildren()
        {
            return this.children;
        }

        public void SetChildren(AstNode children)
        {
            this.children = children;
        }
    }
}

