using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    public class PropertyVariable: Variable
    {
        internal MemberFlags flags;
        internal Function getAccessor;
        internal Function setAccessor;
        internal IChelaType[] indices;
        internal Scope parentScope;

        public PropertyVariable (ChelaModule module)
            : base(module)
        {
            this.flags = MemberFlags.Default;
            this.indices = null;
            this.parentScope = null;
        }


        public PropertyVariable (string name, MemberFlags flags, IChelaType type, IChelaType[] indices, Scope parentScope)
            : base(type, parentScope.GetModule())
        {
            SetName(name);
            this.flags = flags;
            this.indices = indices;
            this.parentScope = parentScope;
        }

        public override Scope GetParentScope ()
        {
            return parentScope;
        }

        public override bool IsProperty ()
        {
            return true;
        }

        public override MemberFlags GetFlags ()
        {
            return flags;
        }

        public IChelaType GetIndexType(int index)
        {
            return indices[index];
        }

        public int GetIndexCount()
        {
            if(indices == null)
                return 0;
            return indices.Length;
        }

        public IList<IChelaType> Indices {
            get {
                return indices;
            }
        }

        public Function GetAccessor {
            get {
                return getAccessor;
            }
            set {
                getAccessor = value;
            }
        }

        public Function SetAccessor {
            get {
                return setAccessor;
            }
            set {
                setAccessor = value;
            }
        }

        public override void Dump ()
        {
            Dumper.Printf("property %s %s", GetVariableType().GetName(), GetName());
            Dumper.Printf("{");
            Dumper.Incr();

            // Dump the accessors.
            if(getAccessor != null)
                Dumper.Printf("get = %s", getAccessor.GetName());

            if(setAccessor != null)
                Dumper.Printf("set = %s", setAccessor.GetName());

            Dumper.Decr();
            Dumper.Printf("}");
        }

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Register the property type.
            ChelaModule module = GetModule();
            module.RegisterType(GetVariableType());

            // Register the indices type.
            int numindices = GetIndexCount();
            for(int i = 0; i < numindices; ++i)
                module.RegisterType((IChelaType)indices[i]);
        }

        public override void Write (ModuleWriter writer)
        {
            // Write the header.
            MemberHeader header = new MemberHeader();
            header.memberType = (byte) MemberHeaderType.Property;
            header.memberName = GetModule().RegisterString(GetName());
            header.memberFlags = (uint) GetFlags();
            header.memberSize = (uint) (13 + 4*GetIndexCount());
            header.Write(writer);
            
            // Write the type.
            ChelaModule module = GetModule();
            writer.Write((uint)module.RegisterType(GetVariableType()));

            // Write the indices.
            byte numIndices = (byte)GetIndexCount();
            writer.Write(numIndices);
            for(int i = 0; i < numIndices; ++i)
                writer.Write(module.RegisterType((IChelaType)indices[i]));

            // Write the get accessor.
            if(getAccessor != null)
                writer.Write((uint)getAccessor.GetSerialId());
            else
                writer.Write((uint)0);

            // Write the set accessor.
            if(setAccessor != null)
                writer.Write((uint)setAccessor.GetSerialId());
            else
                writer.Write((uint)0);
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal event and register it.
            PropertyVariable prop = new PropertyVariable(module);
            module.RegisterMember(prop);

            // Read the name and flags.
            prop.SetName(module.GetString(header.memberName));
            prop.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Get the module.
            ChelaModule module = GetModule();

            // Read the type.
            type = module.GetType(reader.ReadUInt());

            // Read the number of indices.
            int numIndices = reader.ReadByte();
            if(numIndices > 0)
                indices = new IChelaType[numIndices];

            // Read the indices.
            for(int i = 0; i < numIndices; ++i)
                indices[i] = module.GetType(reader.ReadUInt());

            // Read the get accessor.
            getAccessor = (Function)module.GetMember(reader.ReadUInt());

            // Read the set accessor.
            setAccessor = (Function)module.GetMember(reader.ReadUInt());
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            return new PropertyInstance(factory, instance, this);
        }
    }
}

