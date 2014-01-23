using System.Collections.Generic;

namespace Chela.Compiler.Module
{
	public class LexicalScope: Scope
	{
		private Dictionary<string, ScopeMember> members;
		private Scope parentScope;
		private Function parentFunction;
        private int index;
        private TokenPosition position;
		
		public LexicalScope (Scope parentScope, Function parentFunction)
			: base(parentScope.GetModule())
		{
			this.members = new Dictionary<string, ScopeMember> ();
			this.parentScope = parentScope;
			this.parentFunction = parentFunction;
            this.index = parentFunction.AddLexicalScope(this);
		}

        public override bool IsLexicalScope()
        {
            return true;
        }

        public int Index {
            get {
                return index;
            }
        }

        public override TokenPosition Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }

		internal int AddLocal(LocalVariable local)
		{
			// TODO: Throw an exception for duplicated variables.
			members.Add(local.GetName(), local);
			
			// Notify the parent function.
			return parentFunction.AddLocal(local);
		}

        internal void AddArgument(ArgumentVariable arg)
        {
            // TODO: Throw an exception for duplicated variables.
            members.Add(arg.GetName(), arg);
        }

		public override ScopeMember FindMember(string member)
		{
            ScopeMember ret;
            if(this.members.TryGetValue(member, out ret))
                return ret;
            else
                return null;
		}
		
		public override ICollection<ScopeMember> GetMembers()
		{
			return this.members.Values;
		}
		
		public override Scope GetParentScope()
		{
			return this.parentScope;
		}
		
		public Function GetParentFunction()
		{
			return this.parentFunction;
		}
	}
}

