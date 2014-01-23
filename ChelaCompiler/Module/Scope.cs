using System.Collections.Generic;

namespace Chela.Compiler.Module
{
	public abstract class Scope: ScopeMember
	{
		public Scope (ChelaModule module)
			: base (module)
		{
		}
		
		public abstract ScopeMember FindMember(string member);
		public virtual ScopeMember FindMemberRecursive(string member)
		{
			return FindMember(member);
		}

        protected static bool IsRecursiveContinue(ScopeMember member)
        {
            return member == null || member.IsTypeGroup() || member.IsFunctionGroup();
        }

        protected static bool RecursiveMerge(ref ScopeMember res, ScopeMember level)
        {
            if(level != null && res != null)
            {
                if(res.IsTypeGroup() && level.IsTypeGroup())
                {
                    // Merge type groups.
                    TypeGroup lower = (TypeGroup)res;
                    TypeGroup next = (TypeGroup)level;
                    if(!lower.IsMergedGroup())
                        res = lower.CreateMerged(next, false);
                    else
                        lower.AppendLevel(next, false);
                }
                else if(res.IsFunctionGroup() && level.IsFunctionGroup())
                {
                    // Merge function groups.
                    FunctionGroup lower = (FunctionGroup)res;
                    FunctionGroup next = (FunctionGroup)level;
                    if(!lower.IsMergedGroup())
                        res = lower.CreateMerged(next, false);
                    else
                        lower.AppendLevel(next, false);
                }
            }
            else if(res == null)
            {
                // Set the result to the next level.
                res = level;
            }

            return IsRecursiveContinue(res);
        }

        public virtual Structure FindType(string name, GenericPrototype prototype)
        {
            // Find the member.
            ScopeMember member = FindMember(name);
            if(member == null)
                return null;

            // Use the matching type in the type group.
            if(member.IsTypeGroup())
            {
                TypeGroup group = (TypeGroup)member;
                return group.Find(prototype);
            }
            else if(!member.IsClass() && !member.IsInterface() && !member.IsStructure())
                throw new ModuleException("found no structural type " + member.GetFullName());

            // Cast the member.
            return (Structure) member;
        }
		
		public abstract ICollection<ScopeMember> GetMembers();

        public override bool IsScope ()
        {
            return true;
        }

        public virtual bool IsLexicalScope()
        {
            return false;
        }
	}
}

