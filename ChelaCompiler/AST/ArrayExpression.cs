namespace Chela.Compiler.Ast
{
    public class ArrayExpression: Expression
    {
        private Expression elements;

        public ArrayExpression(Expression elements, TokenPosition position)
            : base(position)
        {
            this.elements = elements;
        }

        public Expression GetElements()
        {
            return elements;
        }
    }
}
