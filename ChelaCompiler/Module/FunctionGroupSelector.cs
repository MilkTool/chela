namespace Chela.Compiler.Module
{
	public class FunctionGroupSelector: ScopeMember
	{
		private FunctionGroup functionGroup;
		private Function selected;
        private bool supressVirtual;
        private bool implicitThis;
		private IChelaType[] genericParameters;

		public FunctionGroupSelector (FunctionGroup functionGroup)
			: base(null)
		{
			this.functionGroup = functionGroup;
			this.selected = null;
            this.supressVirtual = false;
            this.implicitThis = false;
            this.genericParameters = null;
		}

        public override Scope GetParentScope ()
        {
            throw new System.NotImplementedException ();
        }

        public override bool IsFunctionGroupSelector()
        {
            return true;
        }

		public FunctionGroup GetFunctionGroup()
		{
			return this.functionGroup;
		}
		
		public Function GetSelected()
		{
			return this.selected;
		}
		
		public void Select(Function function)
		{
			this.selected = function;
		}

        public bool SuppressVirtual {
            get {
                return supressVirtual;
            }
            set {
                supressVirtual = value;
            }
        }

        public bool ImplicitThis {
            get {
                return implicitThis;
            }
            set {
                implicitThis = value;
            }
        }

        public IChelaType[] GenericParameters {
            get {
                return genericParameters;
            }
            set {
                genericParameters = value;
            }
        }
	}
}

