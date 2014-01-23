using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class EnumDefinition: ScopeNode
    {
        private MemberFlags flags;
        private Structure building;
        private Expression typeExpr;

        public EnumDefinition (MemberFlags flags, string name, AstNode children,
            Expression typeExpr, TokenPosition position)
            : base(children, position)
        {
            SetName(name);
            this.flags = flags;
            this.typeExpr = typeExpr;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public Expression GetTypeExpression()
        {
            return typeExpr;
        }

        public Structure GetStructure()
        {
            return building;
        }

        public void SetStructure(Structure building)
        {
            this.building = building;
            SetScope(building);
        }
    }
}

