using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class StructDefinition: ScopeNode
	{
		private MemberFlags flags;
		private AstNode bases;
        private Method defaultConstructor;
        private GenericSignature genericSignature;
        private PseudoScope genericScope;
		
		public StructDefinition(MemberFlags flags, string name,
                                GenericSignature genericSignature,
		                        AstNode bases, AstNode children,
                                TokenPosition position)
			: base(children, position)
		{
			SetName(name);
			this.flags = flags;
			this.bases = bases;
            this.genericSignature = genericSignature;
            this.defaultConstructor = null;
            this.genericScope = null;
		}

        public StructDefinition(MemberFlags flags, string name,
                                AstNode bases, AstNode children,
                                TokenPosition position)
            : this(flags, name, null, bases, children, position)
        {
        }


        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
		
		public MemberFlags GetFlags()
		{
			return this.flags;
		}

        public void SetFlags(MemberFlags flags)
        {
            this.flags = flags;
        }
		
		public AstNode GetBases()
		{
			return this.bases;
		}
		
		public Structure GetStructure()
		{
			return (Structure)GetScope();
		}
		
		public void SetStructure(Structure structure)
		{
			SetScope(structure);
		}

        public Method GetDefaultConstructor()
        {
            return defaultConstructor;
        }

        public void SetDefaultConstructor(Method constructor)
        {
            defaultConstructor = constructor;
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

