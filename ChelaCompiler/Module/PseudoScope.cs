using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    public class PseudoScope: Scope
    {
        private PlaceHolderType placeHolder;
        private Scope parentScope;
        private Dictionary<string, ScopeMember> members;
        private Namespace chainNamespace;

        public PseudoScope (Scope parent)
            : base(parent.GetModule())
        {
            this.placeHolder = null;
            this.parentScope = parent;
            this.members = new Dictionary<string, ScopeMember> ();
            this.chainNamespace = null;
        }

        public PseudoScope (Scope parent, GenericPrototype genProto)
            : this(parent)
        {
            for(int i = 0; i < genProto.GetPlaceHolderCount(); ++i)
            {
                PlaceHolderType placeHolder = genProto.GetPlaceHolder(i);
                AddAlias(placeHolder.GetName(), placeHolder);
            }
        }

        public PseudoScope(PlaceHolderType placeHolder)
            : base(placeHolder.GetModule())
        {
            this.placeHolder = placeHolder;
            this.members = null;
            this.parentScope = null;
            this.chainNamespace = null;
        }

        public override bool IsPseudoScope ()
        {
            return true;
        }

        public void AddAlias(string alias, ScopeMember member)
        {
            this.members.Add(alias, member);
        }

        public override ScopeMember FindMember(string member)
        {
            // Place holder type scope is special
            if(placeHolder != null)
            {
                // Get the module.
                ChelaModule module = placeHolder.GetModule();

                // Find in the object class.
                Class objectClass = module.GetObjectClass();
                ScopeMember found = objectClass.FindMember(member);
                if(found != null)
                    return found;

                // Find in the implemented interfaces.
                for(int i = 0; i < placeHolder.GetBaseCount(); ++i)
                {
                    Structure iface = placeHolder.GetBase(i);
                    found = iface.FindMember(member);
                    if(found != null)
                        return found;
                }

                // Not found
                return null;
            }

            // Get the alias/chained.
            ScopeMember ret;
            if(this.members.TryGetValue(member, out ret))
                return ret;
            else if(chainNamespace != null)
                return chainNamespace.FindMember(member);
            return null;
        }

        public override ScopeMember FindMemberRecursive(string member)
        {
            // Place holder type scope is special
            if(placeHolder != null)
            {
                // Get the module.
                ChelaModule module = placeHolder.GetModule();

                // Find in the object class.
                Class objectClass = module.GetObjectClass();
                ScopeMember found = objectClass.FindMemberRecursive(member);
                if(found != null)
                    return found;

                // Find in the implemented interfaces.
                for(int i = 0; i < placeHolder.GetBaseCount(); ++i)
                {
                    Structure iface = placeHolder.GetBase(i);
                    found = iface.FindMemberRecursive(member);
                    if(found != null)
                        return found;
                }

                // Not found
                return null;
            }

            ScopeMember ret;
            if(this.members.TryGetValue(member, out ret))
                return ret;
            else if(chainNamespace != null)
                return chainNamespace.FindMemberRecursive(member);
            return null;
        }

        public Namespace GetChainNamespace()
        {
            return chainNamespace;
        }

        public void SetChainNamespace(Namespace chain)
        {
            this.chainNamespace = chain;
        }

        public override ICollection<ScopeMember> GetMembers()
        {
            return this.members.Values;
        }

        public override Scope GetParentScope()
        {
            return this.parentScope;
        }
    }
}

