namespace Chela.Compiler.Module
{
    public class FieldInstance: FieldVariable
    {
        private ScopeMember factory;
        private FieldVariable template;

        internal FieldInstance (ScopeMember factory, GenericInstance instance, FieldVariable template)
            : base(factory.GetModule())
        {
            // Store the factory and the template.
            this.factory = factory;
            this.template = template;

            // Use the factory as the parent scope.
            this.parentScope = (Scope)factory;

            // Copy the name and the flags.
            this.SetName(template.GetName());
            this.flags = template.flags;

            // Copy the slot.
            this.slot = template.slot;

            // Instance the type.
            this.type = template.GetVariableType().InstanceGeneric(instance, GetModule());

            // Use the factory as parent scope.
            this.parentScope = (Scope)factory;
        }

        internal override void PrepareSerialization ()
        {
            base.PrepareSerialization ();

            // Register the template and the factory.
            ChelaModule module = GetModule();
            module.RegisterMember(template);
            module.RegisterMember(factory);
        }

        public override void Write (ModuleWriter writer)
        {
            // Create the member header.
            MemberHeader mheader = new MemberHeader();
            mheader.memberType = (byte) MemberHeaderType.MemberInstance;
            mheader.memberFlags = (uint) GetFlags();
            mheader.memberName = 0;
            mheader.memberSize = 8;
            mheader.memberAttributes = 0;

            // Write the header.
            mheader.Write(writer);

            // Write the template.
            ChelaModule module = GetModule();
            writer.Write(module.RegisterMember(template));

            // Write the factory.
            writer.Write(module.RegisterMember(factory));

        }
    }
}

