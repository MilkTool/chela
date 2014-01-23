namespace Chela.Compiler.Module
{
    public class FunctionGroupInstance: FunctionGroup
    {
        public FunctionGroupInstance (FunctionGroup template, GenericInstance instance, ScopeMember factory)
            : base(template.GetName(), (Scope)factory)
        {
            // Instance all of the functions.
            foreach(FunctionGroupName gname in template.GetFunctions())
            {
                // Instance the function.
                Function tmplFunction = gname.GetFunction();
                Function function = (Function)tmplFunction.InstanceMember(factory, instance);

                // Create the new group name.
                FunctionGroupName groupName = new FunctionGroupName(function.GetFunctionType(), gname.IsStatic());
                groupName.SetFunction(function);

                // Store the group name.
                functions.Add(groupName);
            }
        }

        public FunctionGroupInstance (FunctionGroup template)
            : base(template.GetName(), template.GetParentScope())
        {
        }

        internal override void PrepareSerialization ()
        {
            // Don't prepare the group.
        }

        public override void Write (ModuleWriter writer)
        {
            throw new ModuleException("Cannot write function group instance " + GetFullName());
        }
    }
}

