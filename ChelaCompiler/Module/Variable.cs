namespace Chela.Compiler.Module
{
	public class Variable: ScopeMember
	{
		protected IChelaType type;
		private string name;

        protected Variable(ChelaModule module)
            : base(module)
        {
            this.type = null;
            this.name = string.Empty;
        }
		
		public Variable (IChelaType type, ChelaModule module)
			: base(module)
		{
			this.type = type;
			this.name = string.Empty;
		}

        public override Scope GetParentScope ()
        {
            return null;
        }

        public virtual bool IsArgument()
        {
            return false;
        }

		public virtual bool IsLocal()
		{
			return false;
		}

        public virtual bool IsField()
		{
			return false;
		}

		public virtual bool IsArraySlot()
		{
			return false;
		}

        public virtual bool IsReferencedSlot()
        {
            return false;
        }

        public virtual bool IsTemporalReferencedSlot()
        {
            return false;
        }

        public virtual bool IsDirectPropertySlot()
        {
            return false;
        }

        public virtual bool IsPointedSlot()
        {
            return false;
        }

        public virtual bool IsSwizzleVariable()
        {
            return false;
        }

        public virtual bool IsVariableReference()
        {
            return false;
        }
        
		public override string GetName ()
		{
			return this.name;
		}
		
		public virtual void SetName(string name)
		{
			this.name = name;
		}
		
		public IChelaType GetVariableType()
		{
			return this.type;
		}
		
		public override bool IsVariable ()
		{
			return true;
		}
	}
}

