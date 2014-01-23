namespace Chela.Compiler.Ast
{
    public class AliasDeclaration: ScopeNode
    {
        private Expression member;

        public AliasDeclaration(string name, Expression member, TokenPosition position)
            : base(null, position)
        {
            SetName(name);
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

