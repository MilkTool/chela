namespace Chela.Compiler.Ast
{
    public class UnsafeBlockNode: BlockNode
    {
        public UnsafeBlockNode (AstNode children, TokenPosition position)
         : base(children, position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

