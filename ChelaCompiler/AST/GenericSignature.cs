using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class GenericSignature: AstNode
    {
        private GenericParameter parameters;
        private GenericConstraint constraints;
        private GenericPrototype prototype;

        public GenericSignature (GenericParameter parameters, GenericConstraint constraints,
                                 TokenPosition position)
            : base(position)
        {
            this.parameters = parameters;
            this.constraints = constraints;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public GenericParameter GetParameters()
        {
            return parameters;
        }

        public GenericConstraint GetConstraints()
        {
            return constraints;
        }

        public void SetConstraints(GenericConstraint constraints)
        {
            this.constraints = constraints;
        }

        public GenericPrototype GetPrototype()
        {
            return prototype;
        }

        public void SetPrototype(GenericPrototype prototype)
        {
            this.prototype = prototype;
        }
    }
}

