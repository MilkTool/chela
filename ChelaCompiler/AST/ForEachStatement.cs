namespace Chela.Compiler.Ast
{
    public class ForEachStatement: ScopeNode
    {
        private Expression typeExpression;
        private Expression container;

        public ForEachStatement (Expression typeExpression, string name, Expression container,
                AstNode children, TokenPosition position)
            : base(children, position)
        {
            SetName(name);
            this.typeExpression = typeExpression;
            this.container = container;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetTypeExpression()
        {
            return typeExpression;
        }

        public Expression GetContainerExpression()
        {
            return container;
        }
    }
}

