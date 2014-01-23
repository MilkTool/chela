using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class NewArrayExpression: Expression
    {
        private Expression typeExpression;
        private Expression size;
        private AstNode initializers;
        private IChelaType arrayType;
        private IChelaType objectType;
        private IChelaType initCoercionType;
        private int dimensions;

        public NewArrayExpression (Expression typeExpression, Expression size,
                                   AstNode initializers, int dimensions, TokenPosition position)
            : base(position)
        {
            this.typeExpression = typeExpression;
            this.size = size;
            this.initializers = initializers;
            this.dimensions = dimensions;
            this.objectType = null;
            this.initCoercionType = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
        
        public Expression GetTypeExpression()
        {
            return typeExpression;
        }

        public Expression GetSize()
        {
            return size;
        }

        public void SetSize(Expression size)
        {
            this.size = size;
        }

        public AstNode GetInitializers()
        {
            return initializers;
        }

        public int GetDimensions()
        {
            return dimensions;
        }

        public void SetDimensions(int dimensions)
        {
            this.dimensions = dimensions;
        }

        public IChelaType GetObjectType()
        {
            return this.objectType;
        }
        
        public void SetObjectType(IChelaType objectType)
        {
            this.objectType = objectType;
        }

        public IChelaType GetArrayType()
        {
            return this.arrayType;
        }

        public void SetArrayType(IChelaType arrayType)
        {
            this.arrayType = arrayType;
        }

        public IChelaType GetInitCoercionType()
        {
            return this.initCoercionType;
        }

        public void SetInitCoercionType(IChelaType initCoercionType)
        {
            this.initCoercionType = initCoercionType;
        }
    }
}

