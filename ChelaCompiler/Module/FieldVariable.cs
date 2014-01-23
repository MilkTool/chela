namespace Chela.Compiler.Module
{
	public class FieldVariable: Variable
	{
		internal MemberFlags flags;
		internal int slot;
        private ConstantValue initializer;
        protected Scope parentScope;
        private TokenPosition position;

        public FieldVariable (ChelaModule module)
            : base(module)
        {
            this.flags = MemberFlags.Default;
            this.slot = -1;
            this.initializer = null;
            this.parentScope = null;
            this.position = null;
        }

		public FieldVariable (string name, MemberFlags flags, IChelaType type, Scope parentScope)
			: base(type, parentScope.GetModule())
		{
			SetName(name);
			this.flags = flags;
            this.parentScope = parentScope;
            this.slot = -1;
            this.initializer = null;
            this.position = null;
		}

        public override string ToString ()
        {
            return GetName();
        }
		
		public override bool IsField ()
		{
			return true;
		}
		
		public override MemberFlags GetFlags ()
		{
			return this.flags;
		}

        public override Scope GetParentScope ()
        {
            return parentScope;
        }
		
		public int GetSlot()
		{
			return this.slot;
		}

        public ConstantValue GetInitializer()
        {
            return initializer;
        }

        public void SetInitializer(ConstantValue initializer)
        {
            this.initializer = initializer;
        }

        public override TokenPosition Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }

		public override void Dump ()
		{
			Dumper.Printf("%s field(%d) %s %s%s%s;", GetFlagsString(), this.slot, GetVariableType().GetFullName(),
			              GetName(),
                          initializer != null ? " = " : "",
                          initializer != null ? initializer.ToString() : "");
		}

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Register the field type.
            GetModule().RegisterType(GetVariableType());
        }

		public override void Write (ModuleWriter writer)
		{
            // Compute the field size.
            int fieldSize = 8;
            if(initializer != null)
                fieldSize += initializer.GetQualifiedSize();

			// Write the header.
			MemberHeader header = new MemberHeader();
			header.memberType = (byte) MemberHeaderType.Field;
			header.memberName = GetModule().RegisterString(GetName());
			header.memberFlags = (uint) GetFlags();
			header.memberSize = (uint)fieldSize;
            header.memberAttributes = GetAttributeCount();
			header.Write(writer);

            // Write the attributes.
            WriteAttributes(writer);

			// Write the type and the slot.
			writer.Write((uint)GetModule().RegisterType(GetVariableType()));
			writer.Write((int)slot);

            // Write the initializer.
            if(initializer != null)
                initializer.WriteQualified(GetModule(), writer);
		}

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal field and register it.
            FieldVariable field = new FieldVariable(module);
            module.RegisterMember(field);

            // Read the name and flags.
            field.SetName(module.GetString(header.memberName));
            field.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Read the type and slot.
            uint typeId = reader.ReadUInt();
            slot = reader.ReadInt();

            // Load the field type.
            this.type = GetModule().GetType(typeId);

            uint initializerSize = header.memberSize - 8;
            if(!type.IsConstant() || IsExternal())
            {
                // Skip the initializer.
                reader.Skip(initializerSize);
            }
            else
            {
                // Load the initializer.
                initializer = ConstantValue.ReadQualified(GetModule(), reader);
            }
        }
        
        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;
        }

        internal override void PrepareDebug (DebugEmitter debugEmitter)
        {
            debugEmitter.AddField(this);
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            return new FieldInstance(factory, instance, this);
        }
	}
}

