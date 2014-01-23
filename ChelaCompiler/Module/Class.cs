using Chela.Compiler.Ast;

namespace Chela.Compiler.Module
{
	public class Class: Structure
	{
        protected Class(ChelaModule module)
            : base(module)
        {
        }

		public Class(string name, MemberFlags flags, Scope parentScope)
			: base(name, flags, parentScope)
		{
		}
		
		public override bool IsStructure()
		{
			return false;
		}
		
		public override bool IsClass ()
		{
			return true;
		}

        public override bool IsPassedByReference ()
        {
            return true;
        }

        public bool IsPartial()
        {
            return (GetFlags() & MemberFlags.ImplFlagMask) == MemberFlags.Partial;
        }

        internal new static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal structure and register it.
            Class clazz = new Class(module);
            module.RegisterMember(clazz);

            // Read the name.
            clazz.name = module.GetString(header.memberName);
            clazz.flags = (MemberFlags)header.memberFlags;

            // Skip the class elements.
            reader.Skip(header.memberSize);
        }
	}
}

