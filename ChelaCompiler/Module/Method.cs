namespace Chela.Compiler.Module
{
	public class Method: Function
	{
		internal int vslot;
        private bool ctorLeaf;
        private Method ctorParent;
        private Function explicitContract;

        internal Method(ChelaModule module)
            : base(module)
        {
            this.vslot = -1;
            this.ctorLeaf = false;
            this.ctorParent = null;
            this.explicitContract = null;
        }

		public Method (string name, MemberFlags flags, Scope parentScope)
			: base (name, flags, parentScope)
		{
			this.vslot = -1;
            this.ctorLeaf = false;
            this.ctorParent = null;
            this.explicitContract = null;
		}
		
		public override bool IsMethod ()
		{
			return true;
		}
		
        public override bool IsVirtualCallable()
        {
            return IsAbstract() || IsVirtual() || IsContract();
        }
		
		public int GetVSlot()
		{
			return this.vslot;
		}

        public bool IsCtorLeaf()
        {
            return this.ctorLeaf;
        }
        
        public void SetCtorLeaf(bool ctorLeaf)
        {
            this.ctorLeaf = ctorLeaf;
        }

        public Method GetCtorParent()
        {
            return this.ctorParent;
        }

        public void SetCtorParent(Method ctorParent)
        {
            this.ctorParent = ctorParent;
        }

        public Function GetExplicitContract()
        {
            return explicitContract;
        }
        
        public void SetExplicitContract(Function contract)
        {
            this.explicitContract = contract;
        }

		public override void Dump ()
		{
			IChelaType type = GetFunctionType();
			Dumper.Printf("%s method(%d) %s %s", GetFlagsString(), vslot, GetName(),
			              type != null ? type.GetFullName() : "void ()");
			Dumper.Printf("{");
			{
			DumpContent();
			}
			Dumper.Printf("}");
		}
	}
}

