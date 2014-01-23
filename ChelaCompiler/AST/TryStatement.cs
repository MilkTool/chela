using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class TryStatement: Statement
    {
        private AstNode tryStatement;
        private AstNode catchList;
        private AstNode finallyStatement;
        private ExceptionContext context;

        public TryStatement (AstNode tryStatement, AstNode catchList,
                             AstNode finallyStatement, TokenPosition position)
            : base(position)
        {
            this.tryStatement = tryStatement;
            this.catchList = catchList;
            this.finallyStatement = finallyStatement;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public AstNode GetTryStatement()
        {
            return tryStatement;
        }

        public AstNode GetCatchList()
        {
            return catchList;
        }

        public AstNode GetFinallyStatement()
        {
            return finallyStatement;
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

