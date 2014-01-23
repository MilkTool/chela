using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
    public class TypeNameMember: ScopeMember
    {
        private string name;
        private Scope parentScope;
        private IChelaType actualType;
        private TypedefDefinition typedefNode;
        private bool isExpanding;
        private MemberFlags flags;

        public TypeNameMember (MemberFlags flags, string name, Scope parentScope)
            : this(parentScope.GetModule())
        {
            this.flags = flags;
            this.name = name;
            this.parentScope = parentScope;
        }

        public TypeNameMember (ChelaModule module)
            : base(module)
        {
            this.name = string.Empty;
            this.parentScope = null;
            this.typedefNode = null;
            this.actualType = null;
            this.isExpanding = false;
            this.flags = MemberFlags.Default;
        }

        public void SetTypedefNode(TypedefDefinition typedefNode)
        {
            this.typedefNode = typedefNode;
        }

        public TypedefDefinition GetTypedefNode()
        {
            return typedefNode;
        }

        public void SetActualType(IChelaType actualType)
        {
            this.actualType = actualType;
        }

        public IChelaType GetActualType()
        {
            return actualType;
        }

        public bool IsExpanding {
            get {
                return isExpanding;
            }
            set {
                isExpanding = value;
            }
        }

        public override bool IsTypeName ()
        {
            return true;
        }

        public override string GetName ()
        {
            return name;
        }

        public override Scope GetParentScope ()
        {
            return parentScope;
        }

        public override MemberFlags GetFlags ()
        {
            return flags;
        }

        public override void Dump ()
        {
            Dumper.Printf("typedef %s %s", actualType.GetFullName(), GetName());
        }

        internal override void PrepareSerialization ()
        {
            // Prepare myself.
            base.PrepareSerialization ();

            // Register the actualtype.
            GetModule().RegisterType(GetActualType());
        }

        public override void Write (ModuleWriter writer)
        {
            // Write the header.
            MemberHeader header = new MemberHeader();
            header.memberType = (byte) MemberHeaderType.TypeName;
            header.memberName = GetModule().RegisterString(GetName());
            header.memberFlags = (uint) GetFlags();
            header.memberSize = 4;
            header.Write(writer);

            // Write the type and accessors..
            writer.Write((uint)GetModule().RegisterType(GetActualType()));
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal type name and register it.
            TypeNameMember typeName = new TypeNameMember(module);
            module.RegisterMember(typeName);

            // Read the name and flags.
            typeName.name = module.GetString(header.memberName);
            typeName.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Get the module.
            ChelaModule module = GetModule();

            // Read the type.
            actualType = module.GetType(reader.ReadUInt());
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;
        }
    }
}

