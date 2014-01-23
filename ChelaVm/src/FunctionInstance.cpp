#include "ChelaVm/FunctionInstance.hpp"

namespace ChelaVm
{
    FunctionInstance::FunctionInstance(Module *module)
        : Member(module), instance(module)
    {
        function = NULL;
        implementation = NULL;
    }

    FunctionInstance::~FunctionInstance()
    {
    }

    bool FunctionInstance::IsAnonymous() const
    {
        return true;
    }
    
    bool FunctionInstance::IsFunctionInstance() const
    {
        return true;
    }

    Function *FunctionInstance::GetFunction()
    {
        return function;
    }

    Function *FunctionInstance::GetImplementation()
    {
        return implementation;
    }

    const GenericInstance &FunctionInstance::GetGenericInstance() const
    {
        return instance;
    }

    void FunctionInstance::DeclarePass()
    {
        // Check the prototype.
        instance.CheckPrototype();

        // Make sure the function is declared.
        function->DeclarePass();

        // Instance the template.
        Module *module = GetModule();
        implementation = function->InstanceGeneric(function->GetParent(), module, &instance);
        implementation->DeclarePass();
    }

    void FunctionInstance::DefinitionPass()
    {
        implementation->DefinitionPass();
    }

    FunctionInstance *FunctionInstance::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Skip the member data.
        reader.Skip(header.memberSize);

        // Create the function instance.
        return new FunctionInstance(module);
    }

    void FunctionInstance::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the function id.
        uint32_t functionId;
        reader >> functionId;

        // Get the function.
        Module *module = GetModule();
        function = module->GetFunction(functionId);

        // Read the generic instance
        instance.Read(reader);

        // Use the function generic prototype.
        instance.SetPrototype(function->GetGenericPrototype());
    }
}

