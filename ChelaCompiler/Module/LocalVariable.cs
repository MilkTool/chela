namespace Chela.Compiler.Module
{
	public class LocalVariable: Variable
	{
		public const int LocalDataSize = 8;
		
		private LexicalScope parentScope;
		private int index;
        private TokenPosition position;
        private LocalType localType;
        private int argumentIndex;
        private bool isPseudoLocal;
        private Variable actualVariable;
		
		public LocalVariable (string name, LexicalScope parentScope, IChelaType type, bool pseudoLocal)
			: base(type, parentScope.GetModule())
		{
			base.SetName(name);
			this.parentScope = parentScope;
			
			// Add the variable into this.
            this.isPseudoLocal = pseudoLocal;
			this.index = parentScope.AddLocal(this);
            this.argumentIndex = -1;
            this.position = null;

            // Check for generated locals.
            if(name.StartsWith("._gsym"))
                localType = LocalType.Generated;
		}

        public LocalVariable (string name, LexicalScope parentScope, IChelaType type)
            : this(name, parentScope, type, false)
        {
        }
		
		public override bool IsLocal ()
		{
			return true;
		}
		
		public override void SetName (string name)
		{
			// Do nothing.
		}

        public override TokenPosition Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }

		public override Scope GetParentScope()
		{
			return this.parentScope;
		}

        internal void SetLocalIndex(int index)
        {
            this.index = index;
        }

		public int GetLocalIndex()
		{
			return this.index;
		}

        public LocalType Type {
            get {
                return localType;
            }
            set {
                localType = value;
            }
        }

        public int ArgumentIndex {
            get {
                return argumentIndex;
            }
            set {
                argumentIndex = value;
            }
        }

        public void MakePseudoLocal()
        {
            isPseudoLocal = true;
        }

        public bool IsPseudoLocal {
            get {
                return isPseudoLocal;
            }
        }

        /// <summary>
        /// The actual variable represented by a pseudo-argument..
        /// </summary>
        public Variable ActualVariable {
            get {
                return actualVariable;
            }
            set {
                actualVariable = value;
            }
        }

		public override void Dump ()
		{
			Dumper.Printf("@local %s '%s';", GetVariableType().GetName(), GetName());
		}

        internal override void PrepareSerialization ()
        {
            // Register the variable type.
            GetModule().RegisterType(GetVariableType());
        }
		public override void Write (ModuleWriter writer)
		{
			writer.Write((uint)GetModule().RegisterType(GetVariableType()));
			writer.Write((uint)0); // Local flags.
		}
	}
}

