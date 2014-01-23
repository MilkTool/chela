using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class PropertyDefinition: AstNode
    {
        private MemberFlags flags;
        private Expression propertyType;
        private PropertyVariable property;
        private GetAccessorDefinition getAccessor;
        private SetAccessorDefinition setAccessor;
        private AstNode indices;
        private AstNode accessors;
        private Expression nameExpression;

        public PropertyDefinition (MemberFlags flags, Expression propertyType,
                                  string name, AstNode indices, AstNode accessors, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.flags = flags;
            this.propertyType = propertyType;
            this.property = null;
            this.indices = indices;
            this.accessors = accessors;
        }

        public PropertyDefinition (MemberFlags flags, Expression propertyType,
                                   Expression nameExpr, AstNode indices, AstNode accessors, TokenPosition position)
            : this(flags, propertyType, "", indices, accessors, position)
        {
            this.nameExpression = nameExpr;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public void SetFlags(MemberFlags flags)
        {
            this.flags = flags;
        }

        public Expression GetPropertyType()
        {
            return propertyType;
        }

        public PropertyVariable GetProperty()
        {
            return property;
        }

        public void SetProperty(PropertyVariable property)
        {
            this.property = property;
        }

        public AstNode GetAccessors()
        {
            return accessors;
        }

        public AstNode GetIndices()
        {
            return indices;
        }

        public Expression GetNameExpression()
        {
            return nameExpression;
        }

        public GetAccessorDefinition GetAccessor {
            get {
                return getAccessor;
            }
            set {
                getAccessor = value;
            }
        }

        public SetAccessorDefinition SetAccessor {
            get {
                return setAccessor;
            }
            set {
                setAccessor = value;
            }
        }
    }
}

