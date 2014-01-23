namespace Chela.Compiler.Ast
{
    public class MakeArray: Expression
    {
        private Expression valueType;
        private int dimensions;
        private bool isReadOnly;
        
        public MakeArray (Expression valueType, int dimensions, TokenPosition position)
            : base(position)
        {
            this.valueType = valueType;
            this.dimensions = dimensions;
            this.isReadOnly = false;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetValueType()
        {
            return valueType;
        }

        public int GetDimensions()
        {
            return dimensions;
        }

        public bool IsReadOnly {
            get {
                return isReadOnly;
            }
            set {
                isReadOnly = value;
            }
        }
    }
}

