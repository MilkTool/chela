namespace Chela.Compiler.Module
{
    public class PropertyInstance: PropertyVariable
    {
        internal PropertyInstance (ScopeMember factory, GenericInstance instance, PropertyVariable template)
            : base(factory.GetModule())
        {
            this.type = template.GetVariableType().InstanceGeneric(instance, GetModule());
            this.flags = template.flags;
            this.SetName(template.GetName());
            this.parentScope = (Scope)factory;
            if(template.getAccessor != null)
                this.getAccessor = (Function)template.getAccessor.InstanceMember(factory, instance);
            if(template.setAccessor != null)
                this.setAccessor = (Function)template.setAccessor.InstanceMember(factory, instance);

            // Instance the indexers.
            if(template.indices != null)
            {
                int numindices = template.indices.Length;
                this.indices = new IChelaType[numindices];
                for(int i = 0; i < numindices; ++i)
                    this.indices[i] = template.indices[i].InstanceGeneric(instance, GetModule());
            }
        }

        internal override void PrepareSerialization ()
        {
            // Don't prepare the property.
        }

        public override void Write (ModuleWriter writer)
        {
            throw new ModuleException("Cannot write property instance " + GetFullName());
        }
    }
}

