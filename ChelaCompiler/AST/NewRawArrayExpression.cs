using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class NewRawArrayExpression: Expression
    {
        private Expression typeExpression;
        private Expression size;
        private bool heap;
        private IChelaType objectType;

        public NewRawArrayExpression (Expression typeExpression, Expression size,
                                      bool heap, TokenPosition position)
            : base(position)
        {
            this.typeExpression = typeExpression;
            this.size = size;
            this.heap = heap;
            this.objectType = null;
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

        public IChelaType GetObjectType()
        {
            return this.objectType;
        }

        public bool IsHeapAlloc()
        {
            return this.heap;
        }
        
        public void SetObjectType(IChelaType objectType)
        {
            this.objectType = objectType;
        }
    }
}
