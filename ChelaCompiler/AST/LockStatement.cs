namespace Chela.Compiler.Ast
{
    public class LockStatement: ScopeNode
    {
        private Expression mutexExpression;

        public LockStatement (Expression mutexExpression, AstNode children, TokenPosition position)
            : base(children, position)
        {
            this.mutexExpression = mutexExpression;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetMutexExpression()
        {
            return mutexExpression;
        }
    }
}

