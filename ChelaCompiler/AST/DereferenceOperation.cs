namespace Chela.Compiler.Ast
{
    public class DereferenceOperation: Expression
    {
        private Expression pointer;

        public DereferenceOperation (Expression pointer, TokenPosition position)
            : base(position)
        {
            this.pointer = pointer;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetPointer()
        {
            return pointer;
        }
    }
}

