namespace Chela.Compiler.Module
{
    public class EventVariable: Variable
    {
        private MemberFlags flags;
        private Scope parentScope;
        private Function addModifier;
        private Function removeModifier;
        private FieldVariable associatedField;

        protected EventVariable (ChelaModule module)
            : base(module)
        {
            this.flags = MemberFlags.Default;
            this.parentScope = null;
            this.associatedField = null;
        }

        public EventVariable (string name, MemberFlags flags, IChelaType type, Scope parentScope)
            : base(type, parentScope.GetModule())
        {
            SetName(name);
            this.flags = flags;
            this.parentScope = parentScope;
            this.associatedField = null;
        }

        public override Scope GetParentScope ()
        {
            return parentScope;
        }

        public override MemberFlags GetFlags ()
        {
            return flags;
        }

        public override bool IsEvent ()
        {
            return true;
        }

        public Function AddModifier {
            get {
                return addModifier;
            }
            set {
                addModifier = value;
            }
        }

        public Function RemoveModifier {
            get {
                return removeModifier;
            }
            set {
                removeModifier = value;
            }
        }

        public FieldVariable AssociatedField {
            get {
                return associatedField;
            }
            set {
                associatedField = value;
            }
        }

        public override void Dump ()
        {
            Dumper.Printf("event %s %s", GetVariableType().GetName(), GetName());
            Dumper.Printf("{");
            Dumper.Incr();

            // Dump the accessors.
            if(addModifier != null)
                Dumper.Printf("add = %s", addModifier.GetName());

            if(removeModifier != null)
                Dumper.Printf("remove = %s", removeModifier.GetName());

            Dumper.Decr();
            Dumper.Printf("}");
        }

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Register event field type.
            GetModule().RegisterType(GetVariableType());
        }

        public override void Write (ModuleWriter writer)
        {
            // Write the header.
            MemberHeader header = new MemberHeader();
            header.memberType = (byte) MemberHeaderType.Event;
            header.memberName = GetModule().RegisterString(GetName());
            header.memberFlags = (uint) GetFlags();
            header.memberSize = 12;
            header.Write(writer);
            
            // Write the type and accessors..
            writer.Write((uint)GetModule().RegisterType(GetVariableType()));
            if(addModifier != null)
                writer.Write((uint)addModifier.GetSerialId());
            else
                writer.Write((uint)0);

            if(removeModifier != null)
                writer.Write((uint)removeModifier.GetSerialId());
            else
                writer.Write((uint)0);
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal event and register it.
            EventVariable ev = new EventVariable(module);
            module.RegisterMember(ev);

            // Read the name and flags.
            ev.SetName(module.GetString(header.memberName));
            ev.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Get the module.
            ChelaModule module = GetModule();

            // Read the type.
            type = module.GetType(reader.ReadUInt());

            // Read the add modifier.
            addModifier = (Function)module.GetMember(reader.ReadUInt());

            // Read the remove modifier.
            removeModifier = (Function)module.GetMember(reader.ReadUInt());
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;
        }
    }
}
