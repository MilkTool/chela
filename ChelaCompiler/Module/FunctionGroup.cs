using System.Collections;
using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Function group.
    /// </summary>
    public class FunctionGroup: ScopeMember
    {
        internal struct LevelData
        {
            public LevelData(FunctionGroup level, bool isNamespaceLevel)
            {
                this.data = level;
                this.isNamespaceLevel = isNamespaceLevel;
            }

            public FunctionGroup data;
            public bool isNamespaceLevel;
        }

        private string name;
        protected SimpleSet<FunctionGroupName> functions;
        private Scope parentScope;
        private bool mergedGroup;
        private bool isConcrete;
        private List<LevelData> levels;
        private List<Function> memberList;

        protected FunctionGroup (ChelaModule module)
            : base(module)
        {
            this.name = string.Empty;
            this.parentScope = null;
            this.functions = new SimpleSet<FunctionGroupName> ();
        }

        public FunctionGroup (string name, Scope parentScope)
            : this(parentScope.GetModule())
        {
            this.name = name;
            this.parentScope = parentScope;
        }

        /// <summary>
        /// Creates a merged group with a level of superior scope.
        /// </summary>
        public FunctionGroup CreateMerged(FunctionGroup firstLevel, bool isNamespace)
        {
            FunctionGroup ret = new FunctionGroupInstance(this);
            ret.mergedGroup = true;
            ret.AppendLevel(this, false);
            ret.AppendLevel(firstLevel, isNamespace);
            return ret;
        }

        /// <summary>
        /// Appends a function group level, detecting ambiguity for namespace functions.
        /// </summary>
        public void AppendLevel(FunctionGroup level, bool isNamespaceLevel)
        {
            if(levels == null)
                levels = new List<LevelData>();
            levels.Add(new LevelData(level, isNamespaceLevel));
        }

        private void AppendLevelContent(FunctionGroup level, bool isNamespaceLevel)
        {
            level.MakeConcrete();
            foreach(FunctionGroupName gname in level.functions)
            {
                // Check if the object belongs to the included namespace level
                bool isNamespace = gname.IsNamespaceLevel || isNamespaceLevel;

                // Read the name data.
                FunctionType type = gname.GetFunctionType();
                bool isStatic = gname.IsStatic();

                // Find a similar group.
                FunctionGroupName old = Find(gname);
                if(old == null)
                {
                    FunctionGroupName newName = new FunctionGroupName(type, isStatic);
                    newName.IsNamespaceLevel = isNamespace;
                    newName.SetFunction(gname.GetFunction());
                    functions.Add(newName);
                    continue;
                }

                // Ignore names of lower scopes.
                if(!isNamespace || !old.IsNamespaceLevel)
                    continue;

                // Now the old name is a namespace level, and we are adding another
                // namespace level function, in other words, we have detected an ambiguity.
                Function oldFunction = old.GetFunction();
                FunctionAmbiguity amb;
                if(!oldFunction.IsAmbiguity())
                {
                    amb = new FunctionAmbiguity(oldFunction.GetName(), oldFunction.GetFlags(), oldFunction.GetParentScope());
                    amb.AddCandidate(oldFunction);
                    old.SetFunction(amb);
                }
                else
                {
                    amb = (FunctionAmbiguity)oldFunction;
                }

                // Add the new function into the ambiguity list.
                amb.AddCandidate(gname.GetFunction());
            }
        }

        /// <summary>
        /// Make this function group a concrete group.
        /// </summary>
        private void MakeConcrete()
        {
            if(!mergedGroup || isConcrete)
                return;
            isConcrete = true;

            // Append the level data
            foreach(LevelData level in levels)
                AppendLevelContent(level.data, level.isNamespaceLevel);
            levels.Clear();
        }

        /// <summary>
        /// Determines whether this instance is a merged group.
        /// </summary>
        public bool IsMergedGroup()
        {
            return mergedGroup;
        }

        public override MemberFlags GetFlags ()
        {
            return MemberFlags.Public;
        }

        public override Scope GetParentScope ()
        {
            return parentScope;
        }
        
        public override string GetFullName()
        {
            if(parentScope != null)
                return parentScope.GetFullName() + "." + GetName();
            else
                return GetName();
        }
        
        public override string GetName()
        {
            return name;
        }
        
        public override bool IsFunctionGroup ()
        {
            return true;
        }

        public int GetFunctionCount()
        {
            MakeConcrete();
            return functions.Count;
        }

        public IEnumerable GetFunctions()
        {
            MakeConcrete();
            return functions;
        }
        
        public Function Find(FunctionType type, bool isStatic)
        {
            MakeConcrete();
            FunctionGroupName gname = new FunctionGroupName(type, isStatic);
            FunctionGroupName oldName = functions.Find(gname);
            if(oldName != null)
                return oldName.GetFunction();
            return null;
        }

        /// <summary>
        /// Find a group with the same function type than another group.
        /// </summary>
        private FunctionGroupName Find(FunctionGroupName group)
        {
            return (FunctionGroupName)functions.Find(group);
        }

        public void Insert(Function function)
        {
            FunctionGroupName gname = new FunctionGroupName(function.GetFunctionType(), function.IsStatic());
            gname.SetFunction(function);
            functions.Add(gname);
        }

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Prepare the children.
            foreach(FunctionGroupName gname in functions)
                gname.GetFunction().PrepareSerialization();
        }

        internal override void PrepareDebug(DebugEmitter debugEmitter)
        {
            foreach(FunctionGroupName gname in functions)
                gname.GetFunction().PrepareDebug(debugEmitter);
        }
        
        public override void Write (ModuleWriter writer)
        {
            // Write the header.
            MemberHeader header = new MemberHeader();
            header.memberName = GetModule().RegisterString(GetName());
            header.memberType = (byte)MemberHeaderType.FunctionGroup;
            header.memberFlags = (uint)GetFlags();
            header.memberSize = (uint)(functions.Count*4);
            header.Write(writer);
            
            // Write the functions ids.
            foreach(FunctionGroupName gname in functions)
                writer.Write((uint)gname.GetFunction().GetSerialId());
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal event and register it.
            FunctionGroup fgroup = new FunctionGroup(module);
            module.RegisterMember(fgroup);

            // Read the name.
            fgroup.name = module.GetString(header.memberName);

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Read the functions into the member list.
            memberList = new List<Function> ();
            int numfunctions = (int)header.memberSize/4;
            for(int i = 0; i < numfunctions; ++i)
                memberList.Add((Function)GetModule().GetMember(reader.ReadUInt()));
        }

        internal override void FinishLoad()
        {
            // Finish he functions.
            foreach(Function function in memberList)
            {
                // Insert the functions in the table.
                Insert(function);

                // Finish the functions loading.
                function.FinishLoad();
            }
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            return new FunctionGroupInstance(this, instance, factory);
        }
        
        internal override void UpdateParent (Scope parentScope)
        {
            // Store the parent.
            this.parentScope = parentScope;

            // Update the children parent.
            if(memberList != null)
            {
                foreach(ScopeMember member in memberList)
                    member.UpdateParent(parentScope);
            }
        }
        
        public override void Dump ()
        {
            // Dump each one of the functions.
            foreach(FunctionGroupName gname in functions)
            {
                gname.GetFunction().Dump();
            }
        }
    }
}

