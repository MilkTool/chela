using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class TypedefDefinition: AstNode
    {
        private MemberFlags flags;
        private Expression typeExpression;
        private TypeNameMember typeName;
        private Scope[] expansionScope;

        public TypedefDefinition (MemberFlags flags, Expression typeExpression,
                                  string name, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.flags = flags;
            this.typeExpression = typeExpression;
            this.expansionScope = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return this.flags;
        }

        public Expression GetTypeExpression()
        {
            return this.typeExpression;
        }

        public void SetTypeName(TypeNameMember typeName)
        {
            this.typeName = typeName;
        }
        
        public TypeNameMember GetTypeName()
        {
            return typeName;
        }

        public Scope[] GetExpansionScope()
        {
            return expansionScope;
        }

        public void SetExpansionScope(Scope[] scope)
        {
            expansionScope = scope;
        }
    }
}

