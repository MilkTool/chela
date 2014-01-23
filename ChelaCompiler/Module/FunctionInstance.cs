namespace Chela.Compiler.Module
{
    public class FunctionInstance: Method
    {
        private Function template;
        private GenericInstance genericInstance;
        private ScopeMember factory;

        public FunctionInstance (Function template, GenericInstance genericInstance, ScopeMember factory)
            : base(factory.GetModule())
        {
            this.factory = factory;
            Initialize(template, genericInstance);

            // Use the factory as a scope.
            this.UpdateParent((Scope)factory);
        }

        public FunctionInstance (Function template, GenericInstance genericInstance, ChelaModule instanceModule)
            : base(instanceModule)
        {
            Initialize(template, genericInstance);
        }

        private void Initialize(Function template, GenericInstance genericInstance)
        {
            // Store the template and the generic instance.
            this.template = template;
            this.genericInstance = genericInstance;

            // Store a name and the flags.
            this.name = template.GetName();
            this.flags = template.GetFlags();

            // Instance the function type.
            SetFunctionType((FunctionType)template.GetFunctionType().InstanceGeneric(genericInstance, GetModule()));

            // Copy the prototype if this is not the final instance.
            if(genericInstance.GetPrototype() != template.GetGenericPrototype())
                SetGenericPrototype(template.GetGenericPrototype());

            // Use the template parent scope.
            this.parentScope = template.GetParentScope();

            // Register myself.
            GetModule().RegisterGenericInstance(this);
        }

        public override bool IsGenericInstance()
        {
            return true;
        }

        public override bool IsMethod ()
        {
            return template.IsMethod();
        }
        
        public override GenericInstance GetGenericInstance ()
        {
            return genericInstance;
        }

        internal override void PrepareSerialization ()
        {
            //base.PrepareSerialization ();
            PrepareSerializationNoBase();

            // Register the template.
            ChelaModule module = GetModule();
            module.RegisterMember(template);
            if(template.IsGenericInstance())
                template.PrepareSerialization();

            // Register the factory.
            module.RegisterMember(factory);
            if(factory != null && factory.IsGenericInstance())
                factory.PrepareSerialization();

            // Register the types used.
            genericInstance.PrepareSerialization(module);
        }

        public override void Write (ModuleWriter writer)
        {
            // Create the member header.
            MemberHeader mheader = new MemberHeader();
            mheader.memberFlags = (uint) GetFlags();
            mheader.memberName = 0;
            mheader.memberAttributes = 0;

            ChelaModule module = GetModule();
            if(factory != null)
            {
                // This is a member instance.
                mheader.memberType = (byte)MemberHeaderType.MemberInstance;
                mheader.memberSize = (uint)8;

                // Write the member header.
                mheader.Write(writer);
                
                // Write the template.
                writer.Write(module.RegisterMember(template));

                // Write the factory id.
                writer.Write(module.RegisterMember(factory));
            }
            else
            {
                // This is a function instance
                mheader.memberType = (byte)MemberHeaderType.FunctionInstance;
                mheader.memberSize = (uint)(4 + genericInstance.GetSize());

                // Write the member header.
                mheader.Write(writer);

                // Write the template id.
                writer.Write(module.RegisterMember(template));

                // Write the template parameters.
                genericInstance.Write(writer, GetModule());
            }
        }
    }
}

