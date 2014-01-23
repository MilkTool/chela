namespace Chela.Compiler.Ast
{
    public class UsingStatement: ScopeNode
    {
        private Expression member;

        public UsingStatement(Expression member, TokenPosition position)
            : base(null, position)
        {
           this.member = member;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetMember()
        {
            return member;
        }
    }
}

