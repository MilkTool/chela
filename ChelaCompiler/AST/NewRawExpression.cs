namespace Chela.Compiler.Ast
{
    public class NewRawExpression: NewExpression
    {
        private bool heap;

        public NewRawExpression (Expression typeExpression, Expression arguments,
                                 bool heap, TokenPosition position)
            : base(typeExpression, arguments, position)
        {
            this.heap = heap;
        }
        
        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public bool IsHeapAlloc()
        {
            return heap;
        }
    }
}

