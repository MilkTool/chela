using System.Collections.Generic;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
    public class Namespace: Scope
    {
        private Namespace parentScope;
        private Dictionary<string, ScopeMember> members;
        private string name;
        private List<FieldDeclaration> globalInitialization;
        private Function staticConstructor;
        
        internal Namespace(ChelaModule module)
            : base(module)
        {
            this.parentScope = null;
            this.members = new Dictionary<string, ScopeMember> ();
            this.name = string.Empty;
            this.globalInitialization = null;
            this.staticConstructor = null;
        }
        
        public Namespace (Namespace parent)
            : base(parent.GetModule())
        {
            this.parentScope = parent;
            this.members = new Dictionary<string, ScopeMember> ();
            this.globalInitialization = null;
            this.staticConstructor = null;
            this.name = string.Empty;
        }

        public override MemberFlags GetFlags ()
        {
            return MemberFlags.Public;
        }
        
        public override bool IsNamespace ()
        {
            return true;
        }
        
        public override string GetName ()
        {
            return this.name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
        
        public void AddMember(ScopeMember member)
        {
            if(member == null)
                return;
            this.members.Add(member.GetName(), member);

            // Store the static constructor.
            if(member.IsFunction())
            {
                Function function = (Function)member;
                if(function.IsStaticConstructor())
                    staticConstructor = function;
            }
        }

        public void AddType(Structure type)
        {
            // Find the previous group.
            ScopeMember oldMember = FindMember(type.GetName());
            if(oldMember != null)
            {
                if(!oldMember.IsTypeGroup())
                    throw new ModuleException("expected type group.");

                // Find the previous definition.
                TypeGroup oldGroup = (TypeGroup)oldMember;
                if(oldGroup.Find(type.GetGenericPrototype()) != null)
                    throw new ModuleException("matching type already exists.");

                // Add the type into the group.
                oldGroup.Insert(type);
            }
            else
            {
                // Create a new type group.
                TypeGroup newGroup = new TypeGroup(type.GetName(), this);
                newGroup.Insert(type);
                AddMember(newGroup);
            }
        }

        public ICollection<FieldDeclaration> GetGlobalInitializations()
        {
            return globalInitialization;
        }

        public void AddGlobalInitialization(FieldDeclaration decl)
        {
            if(globalInitialization == null)
                globalInitialization = new List<FieldDeclaration> ();
            globalInitialization.Add(decl);
        }

        public Function GetStaticConstructor()
        {
            return staticConstructor;
        }

        public override ScopeMember FindMember(string member)
        {
            // Find the member here.
            ScopeMember ret;
            if(this.members.TryGetValue(member, out ret))
                return ret;
            return null;
        }

        public override ScopeMember FindMemberRecursive (string member)
        {
            // Find the member here.
            ScopeMember ret = FindMember(member);
            if(ret != null)
                return ret;

            // Find the member in the sister namespaces.
            ChelaModule myModule = GetModule();
            string myname = GetFullName();
            foreach(ChelaModule refMod in ChelaModule.GetLoadedModules())
            {
                if(refMod == myModule)
                    continue;

                ScopeMember sister = refMod.GetMember(myname);
                if(sister != null && sister.IsNamespace())
                {
                    Namespace test = (Namespace)sister;
                    ret = test.FindMember(member);
                    if(ret != null)
                        return ret;
                }
            }

            // Couldn't find the member.
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
        
        internal override void PrepareSerialization()
        {
            // Prepare the base.
            base.PrepareSerialization();

            // Prepare the children.
            foreach(ScopeMember member in members.Values)
                member.PrepareSerialization();
        }

        internal override void PrepareDebug(DebugEmitter debugEmitter)
        {
            foreach(ScopeMember member in members.Values)
                member.PrepareDebug(debugEmitter);
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal namespace and register it.
            Namespace space = new Namespace(module);
            module.RegisterMember(space);

            // Read the name.
            space.name = module.GetString(header.memberName);

            // Skip the namespace elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Read the children.
            uint nummembers = header.memberSize/4u;
            for(uint i = 0; i < nummembers; ++i)
            {
                uint child = reader.ReadUInt();
                AddMember(GetModule().GetMember(child));
            }
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = (Namespace)parentScope;

            // Update the children.
            foreach(ScopeMember child in members.Values)
                child.UpdateParent(this);
        }

        internal override void FinishLoad()
        {
            // Update the children.
            foreach(ScopeMember child in members.Values)
                child.FinishLoad();
        }

        public override void Write (ModuleWriter writer)
        {
            // The global namespace hasn't name.
            string name = GetName();
            if(parentScope == null)
                name = string.Empty;
            
            // Write the header.
            MemberHeader header = new MemberHeader();
            header.memberName = (uint)GetModule().RegisterString(name);
            header.memberType = (byte)MemberHeaderType.Namespace;
            header.memberFlags = (uint)GetFlags();
            header.memberSize = (uint) (members.Count*4);
            header.Write(writer);
            
            // Write the member ids.
            foreach(ScopeMember member in this.members.Values)
                writer.Write((uint)member.GetSerialId());
        }
        
        public override void Dump ()
        {
            Dumper.Printf("namespace %s", name);
            Dumper.Printf("{");
            {
                Dumper.Incr();
                
                // Dump the members.
                foreach(ScopeMember member in members.Values)
                    member.Dump();
                
                Dumper.Decr();
            }
            Dumper.Printf("}");
        }
    }
}

