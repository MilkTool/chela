namespace Chela.Compiler.Ast
{
    public class FixedStatement: ScopeNode
    {
        private Expression type;
        private AstNode declarations;

        public FixedStatement (Expression type, AstNode decls, AstNode children, TokenPosition position)
            : base(children, position)
        {
            this.type = type;
            this.declarations = decls;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetTypeExpression()
        {
            return type;
        }

        public AstNode GetDeclarations()
        {
            return declarations;
        }
    }
}

