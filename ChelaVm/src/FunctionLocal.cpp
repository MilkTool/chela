#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionLocal.hpp"

namespace ChelaVm
{
	FunctionLocal::FunctionLocal(Function *parent)
		: parent(parent), module(parent->GetModule())
	{
        isRefCounted = false;
	}
	
	FunctionLocal::~FunctionLocal()
	{
	}
		
	void FunctionLocal::Read(ModuleReader &reader)
	{
        // Read the type id.
		uint32_t typeId, lflags;
		reader >> typeId >> lflags;

        // Get the actual type.
        type = module->GetType(typeId);

        // The type cannot be void.
        VirtualMachine *vm = module->GetVirtualMachine();
        if(type == ChelaType::GetVoidType(vm) ||
            type == ChelaType::GetConstVoidType(vm))
            parent->Error("variables cannot be void.");
	}
	
	void FunctionLocal::Declare(llvm::IRBuilder<> &builder)
	{
		value = builder.CreateAlloca(type->GetTargetType(), 0, "l");
	}
	
	const ChelaType *FunctionLocal::GetType()
	{
		return type;
	}
	
	llvm::Value	*FunctionLocal::GetValue()
	{
		return value;
	}

    bool FunctionLocal::IsRefCounted()
    {
        return isRefCounted;
    }

    void FunctionLocal::SetRefCounted(bool refCounted)
    {
        this->isRefCounted = refCounted;
    }

    FunctionLocal *FunctionLocal::InstanceGeneric(Function *parent, const GenericInstance *instance)
    {
        // Create the new local.
        FunctionLocal *ret = new FunctionLocal(parent);

        // Instance the local type.
        ret->type = type->InstanceGeneric(instance);

        // Return.
        return ret;
    }
}
