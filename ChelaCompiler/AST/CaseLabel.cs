using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class CaseLabel: AstNode
    {
        private Expression constant;
        private AstNode children;
        private BasicBlock block;

        public CaseLabel (Expression constant, AstNode children, TokenPosition position)
            : base(position)
        {
            this.constant = constant;
            this.children = children;
            this.block = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetConstant()
        {
            return this.constant;
        }

        public void SetConstant(Expression constant)
        {
            this.constant = constant;
        }

        public AstNode GetChildren()
        {
            return this.children;
        }

        public BasicBlock GetBlock()
        {
            return block;
        }

        public void SetBlock(BasicBlock block)
        {
            this.block = block;
        }
    }
}

