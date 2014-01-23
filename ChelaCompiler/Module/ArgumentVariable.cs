namespace Chela.Compiler.Module
{
    public class ArgumentVariable: Variable
    {
        private LexicalScope parentScope;
        private int index;
        private bool isPseudoArgument;
        private Variable actualVariable;

        public ArgumentVariable (int index, string name, LexicalScope parentScope, IChelaType type)
            : base(type, parentScope.GetModule())
        {
            base.SetName(name);
            this.parentScope = parentScope;

            // Add the variable into the scope.
            this.index = index;
            parentScope.AddArgument(this);
        }

        public override bool IsArgument ()
        {
            return true;
        }

        public override void SetName (string name)
        {
            // Do nothing.
        }

        public override Scope GetParentScope()
        {
            return this.parentScope;
        }

        public bool IsPseudoArgument {
            get {
                return isPseudoArgument;
            }
        }

        public void MakePseudoArgument()
        {
            isPseudoArgument = true;
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

        public int GetArgumentIndex()
        {
            return this.index;
        }
    }
}

