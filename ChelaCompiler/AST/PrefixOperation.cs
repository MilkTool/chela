namespace Chela.Compiler.Ast
{
    public class PrefixOperation: Expression
    {
        public const int Increment = 0;
        public const int Decrement = 1;

        private int operation;
        private Expression variable;

        public PrefixOperation (int op, Expression variable, TokenPosition position)
            : base(position)
        {
            this.operation = op;
            this.variable = variable;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public int GetOperation()
        {
            return this.operation;
        }

        public Expression GetVariable()
        {
            return variable;
        }
    }
}

