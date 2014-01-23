namespace Chela.Compiler.Ast
{
    public class GenericConstraint: AstNode
    {
        private bool valueType;
        private bool defCtor;
        private Expression bases;
        private GenericSignature signature;

        public GenericConstraint (string paramName, bool valueType, bool defCtor, Expression bases,
                                  TokenPosition position)
            : base(position)
        {
            SetName(paramName);
            this.valueType = valueType;
            this.defCtor = defCtor;
            this.bases = bases;
            this.signature = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public bool IsValueType()
        {
            return valueType;
        }

        public bool HasDefaultConstructor()
        {
            return defCtor;
        }

        public Expression GetBases()
        {
            return bases;
        }

        public GenericSignature GetSignature()
        {
            return signature;
        }

        public void SetSignature(GenericSignature signature)
        {
            this.signature = signature;
        }
    }
}

