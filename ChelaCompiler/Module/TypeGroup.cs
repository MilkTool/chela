using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Chela.Compiler.Module
{
    public class TypeGroup: ChelaType
    {
        private string name;
        private string fullName;
        private string displayName;
        private SimpleSet<TypeGroupName> types;
        private List<Structure> memberList;
        private Scope parentScope;
        private bool mergedGroup;

        protected TypeGroup (ChelaModule module)
            : base(module)
        {
            this.name = string.Empty;
            this.fullName = null;
            this.parentScope = null;
            this.types = new SimpleSet<TypeGroupName> ();
        }

        public TypeGroup (string name, Scope parentScope)
         : this(parentScope.GetModule())
        {
            this.name = name;
            this.parentScope = parentScope;
        }

        protected TypeGroup(TypeGroup group)
            : base(group.GetModule())
        {
            this.name = group.name;
            this.fullName = null;
            this.parentScope = group.GetParentScope();
            this.types = new SimpleSet<TypeGroupName> ();
        }

        public override MemberFlags GetFlags ()
        {
            return MemberFlags.Public;
        }

        public override Scope GetParentScope ()
        {
            return parentScope;
        }

        public override string GetFullName ()
        {
            if(fullName == null)
            {
                StringBuilder builder = new StringBuilder();
                if(parentScope != null)
                {
                    builder.Append(parentScope.GetFullName());
                    builder.Append('.');
                }
                builder.Append(name);
                builder.Append(" <TG>");
                fullName = builder.ToString();
            }

            return fullName;
        }

        public override string GetDisplayName()
        {
            if(displayName == null)
            {
                StringBuilder builder = new StringBuilder();
                if(parentScope != null)
                {
                    builder.Append(parentScope.GetDisplayName());
                    builder.Append('.');
                }
                builder.Append(name);
                displayName = builder.ToString();
            }

            return displayName;
        }

        public override string GetName ()
        {
            return name;
        }

        public override bool IsTypeGroup ()
        {
            return true;
        }

        public int GetBuildingCount ()
        {
            return types.Count;
        }

        public IEnumerable GetBuildings ()
        {
            return types;
        }

        public TypeGroup CreateMerged(TypeGroup firstLevel, bool isNamespace)
        {
            TypeGroup ret = new TypeGroup(this);
            ret.mergedGroup = true;
            ret.AppendLevel(firstLevel, isNamespace);
            return ret;
        }

        public void AppendLevel(TypeGroup level, bool isNamespaceLevel)
        {
            foreach(TypeGroupName gname in level.types)
            {
                // Check if the object belongs to the included namespace level
                bool isNamespace = gname.IsNamespaceLevel || isNamespaceLevel;

                // Add not found types.
                TypeGroupName old = Find(gname);
                if(old == null)
                {
                    TypeGroupName newName = new TypeGroupName(gname);
                    newName.IsNamespaceLevel = isNamespace;
                    types.Add(newName);
                    continue;
                }

                // Ignore names of lower scopes.
                if(!isNamespace || !old.IsNamespaceLevel)
                    continue;

                // Now the old name is a namespace level, and we are adding another
                // namespace level type, in other words, we have detected an ambiguity.
                Structure oldType = old.GetBuilding();
                AmbiguousStructure amb;
                if(!oldType.IsAmbiguity())
                {
                    amb = new AmbiguousStructure(oldType.GetName(), oldType.GetFlags(), oldType.GetParentScope());
                    amb.AddCandidate(oldType);
                    old.SetBuilding(oldType);
                }
                else
                {
                    amb = (AmbiguousStructure)oldType;
                }

                // Add the new function into the ambiguity list.
                amb.AddCandidate(gname.GetBuilding());
            }
        }

        /// <summary>
        /// Determines whether this instance is a merged group.
        /// </summary>
        public bool IsMergedGroup()
        {
            return mergedGroup;
        }

        public Structure Find(GenericPrototype prototype)
        {
            TypeGroupName gname = new TypeGroupName(null, prototype);
            TypeGroupName res = types.Find(gname);
            if(res != null)
                return res.GetBuilding();
            return null;
        }

        public TypeGroupName Find(TypeGroupName gname)
        {
            return types.Find(gname);
        }
     
        public void Insert (Structure type)
        {
            TypeGroupName gname = new TypeGroupName(type, type.GetGenericPrototype());
            types.Add (gname);
        }

        public Structure GetDefaultType()
        {
            // Get the type without generic parameters.
            foreach(TypeGroupName gname in types)
            {
                if(gname.GetGenericPrototype().GetPlaceHolderCount() == 0)
                    return gname.GetBuilding();
            }

            return null;
        }

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Prepare the children.
            foreach (TypeGroupName gname in types)
                gname.GetBuilding ().PrepareSerialization ();
        }

        internal override void PrepareDebug (DebugEmitter debugEmitter)
        {
            foreach (TypeGroupName gname in types)
                gname.GetBuilding ().PrepareDebug (debugEmitter);
        }
     
        public override void Write (ModuleWriter writer)
        {
            if(mergedGroup)
                throw new ModuleException("Cannot write temporal type group " + GetFullName());

            // Write the header.
            MemberHeader header = new MemberHeader ();
            header.memberName = GetModule ().RegisterString (GetName ());
            header.memberType = (byte)MemberHeaderType.TypeGroup;
            header.memberFlags = (uint)GetFlags ();
            header.memberSize = (uint)(types.Count * 4);
            header.Write (writer);
         
            // Write the functions ids.
            foreach (TypeGroupName gname in types)
                writer.Write ((uint)gname.GetBuilding().GetSerialId ());
        }

        internal static void PreloadMember (ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal event and register it.
            TypeGroup tgroup = new TypeGroup (module);
            module.RegisterMember (tgroup);

            // Read the name.
            tgroup.name = module.GetString (header.memberName);

            // Skip the structure elements.
            reader.Skip (header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Read the types into the member list.
            memberList = new List<Structure>();
            int numtypes = (int)header.memberSize / 4;
            for (int i = 0; i < numtypes; ++i)
                memberList.Add((Structure)GetModule().GetMember (reader.ReadUInt ()));
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            return null;//new FunctionGroupInstance (this, instance, factory);
        }

        internal override void FinishLoad()
        {
            // Finish loading the structures.
            foreach(Structure type in memberList)
            {
                Insert(type);
                type.FinishLoad();
            }
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the parent.
            this.parentScope = parentScope;

            // Update the children parent.
            if(memberList != null)
            {
                foreach(ScopeMember member in memberList)
                    member.UpdateParent (parentScope);
            }
        }

        public override void Dump ()
        {
            // Dump each one of the functions.
            foreach (TypeGroupName gname in types)
                gname.GetBuilding().Dump ();
        }
    }
}

