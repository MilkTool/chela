namespace Chela.Compiler.Ast
{
    public class ObjectReferenceExpression: Expression
    {
        private bool isAttributeName;

        public ObjectReferenceExpression (TokenPosition position)
            : base(position)
        {
            this.isAttributeName = false;
        }

        public void SetAttributeName(bool isAttributeName)
        {
            this.isAttributeName = isAttributeName;
        }

        public bool IsAttributeName()
        {
            return this.isAttributeName;
        }
    }
}

