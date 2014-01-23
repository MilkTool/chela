namespace Chela.Compiler.Ast
{
    public class ThrowStatement: Statement
    {
        private Expression exception;

        public ThrowStatement (Expression exception, TokenPosition position)
            : base(position)
        {
            this.exception = exception;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
        
        public Expression GetException()
        {
            return this.exception;
        }
    }
}

