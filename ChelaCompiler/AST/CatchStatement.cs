using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class CatchStatement: ScopeNode
    {
        private Expression exceptionType;
        private ExceptionContext context;
        private LocalVariable variable;

        public CatchStatement (Expression exceptionType, string name,
                               AstNode children, TokenPosition position)
            : base(children, position)
        {
            SetName(name);
            this.exceptionType = exceptionType;
            this.context = null;
            this.variable = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetExceptionType()
        {
            return exceptionType;
        }

        public void SetContext(ExceptionContext context)
        {
            this.context = context;
        }

        public ExceptionContext GetContext()
        {
            return context;
        }

        public void SetVariable(LocalVariable variable)
        {
            this.variable = variable;
        }

        public LocalVariable GetVariable()
        {
            return variable;
        }
    }
}

