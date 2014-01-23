using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class MakeReference: Expression
    {
        private Expression referencedType;
        private ReferenceFlow flow;
        private bool streamReference;

        public MakeReference (Expression referencedType, ReferenceFlow flow, bool streamRef, TokenPosition position)
            : base(position)
        {
            this.referencedType = referencedType;
            this.flow = flow;
            this.streamReference = streamRef;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetReferencedType()
        {
            return referencedType;
        }

        public ReferenceFlow GetFlow()
        {
            return flow;
        }

        public bool IsStreamReference()
        {
            return streamReference;
        }
    }
}

