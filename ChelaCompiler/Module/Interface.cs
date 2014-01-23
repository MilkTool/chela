namespace Chela.Compiler.Module
{
    public class Interface: Structure
    {
        protected Interface(ChelaModule module)
            : base(module)
        {
        }

        public Interface (string name, MemberFlags flags, Scope parentScope)
            : base(name, flags, parentScope)
        {
        }

        public override bool IsInterface ()
        {
            return true;
        }

        public override bool IsStructure ()
        {
            return false;
        }

        public override bool IsClass ()
        {
            return false;
        }

        public override bool IsPassedByReference ()
        {
            return true;
        }

        internal new static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal interface and register it.
            Interface iface = new Interface(module);
            module.RegisterMember(iface);

            // Read the name.
            iface.name = module.GetString(header.memberName);
            iface.flags = (MemberFlags)header.memberFlags;

            // Skip the class elements.
            reader.Skip(header.memberSize);
        }
    }
}

