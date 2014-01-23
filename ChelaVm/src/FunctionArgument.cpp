#include "ChelaVm/ArgumentData.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionArgument.hpp"
#include "ChelaVm/VirtualMachine.hpp"

namespace ChelaVm
{
	FunctionArgument::FunctionArgument(Function *parent, const ChelaType *type,
                                        llvm::Value *value, ArgumentData *argumentData,
                                        size_t index)
		: parent(parent), type(type), value(value), argumentData(argumentData), index(index)
	{
	}
	
	FunctionArgument::~FunctionArgument()
	{
	}
		
	Function *FunctionArgument::GetParent()
	{
		return parent;
	}
	
	const ChelaType *FunctionArgument::GetType()
	{
		return type;
	}
	
	llvm::Value	*FunctionArgument::GetValue()
	{
		return value;
	}

    llvm::Constant *FunctionArgument::CreateParameterInfo()
    {
        // Get the module and virtual machine.
        Module *module = parent->GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::Type *intType = llvm::Type::getInt32Ty(targetModule->getContext());

        // Select the parameter info class.
        Class *parameterInfoClass  = vm->GetParameterInfoClass();

        // Create the parameter info value.
        ConstantStructurePtr paramInfo(parameterInfoClass->CreateConstant(module));

        // Set the parameter name.
        paramInfo->SetField("name", module->CompileString(argumentData->GetName()));

        // Set the parameter type.
        paramInfo->SetField("parameterType", module->GetTypeInfo(type));

        // Set the parameter position.
        paramInfo->SetField("position", llvm::ConstantInt::get(intType, index));

        // Create the paramter info global.
        return new llvm::GlobalVariable(*targetModule, parameterInfoClass->GetTargetType(),
                                    false, llvm::GlobalVariable::PrivateLinkage, paramInfo->Finish(), "_paraminfo_");
    }
}
