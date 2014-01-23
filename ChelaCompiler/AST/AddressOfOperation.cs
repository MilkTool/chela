namespace Chela.Compiler.Ast
{
    public class AddressOfOperation: Expression
    {
        private Expression reference;
        private bool isFunction;

        public AddressOfOperation (Expression reference, TokenPosition position)
            : base(position)
        {
            this.reference = reference;
            this.isFunction = false;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetReference()
        {
            return reference;
        }

        public bool IsFunction()
        {
            return this.isFunction;
        }

        public void SetIsFunction(bool isFunction)
        {
            this.isFunction = isFunction;
        }
    }
}

