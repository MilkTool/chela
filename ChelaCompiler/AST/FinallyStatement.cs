using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class FinallyStatement: AstNode
    {
        private AstNode children;
        private ExceptionContext context;

        public FinallyStatement (AstNode children, TokenPosition position)
            : base(position)
        {
            this.children = children;
            this.context = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public AstNode GetChildren()
        {
            return children;
        }

        public void SetContext(ExceptionContext context)
        {
            this.context = context;
        }

        public ExceptionContext GetContext()
        {
            return context;
        }
    }
}

