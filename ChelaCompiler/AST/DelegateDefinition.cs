using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class DelegateDefinition: AstNode
    {
        private MemberFlags flags;
        private Expression returnType;
        private AstNode arguments;
        private Structure building;
        private GenericSignature genericSignature;
        private PseudoScope genericScope;

        public DelegateDefinition (MemberFlags flags, Expression returnType,
                                   AstNode arguments, string name,
                                   GenericSignature genericSignature,
                                   TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.genericSignature = genericSignature;
            this.flags = flags;
            this.returnType = returnType;
            this.arguments = arguments;
        }

        public DelegateDefinition (MemberFlags flags, Expression returnType,
                                   AstNode arguments, string name,
                                   TokenPosition position)
            : this(flags, returnType, arguments, name, null, position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public Expression GetReturnType()
        {
            return returnType;
        }

        public AstNode GetArguments()
        {
            return arguments;
        }

        public Structure GetStructure()
        {
            return building;
        }

        public void SetStructure(Structure building)
        {
            this.building = building;
        }

        public GenericSignature GetGenericSignature()
        {
            return genericSignature;
        }

        public void SetGenericSignature(GenericSignature genericSignature)
        {
            this.genericSignature = genericSignature;
        }

        public void SetGenericScope(PseudoScope genericScope)
        {
            this.genericScope = genericScope;
        }

        public PseudoScope GetGenericScope()
        {
            return this.genericScope;
        }
    }
}

