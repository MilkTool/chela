#include "ChelaVm/Class.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "llvm/Analysis/Verifier.h"

namespace ChelaVm
{
    inline void ReleaseRef(llvm::IRBuilder<> &builder, Module *mod, VirtualMachine *vm, llvm::Value *value)
    {
        // Get the release functions.
        llvm::Constant *release = vm->GetRelRef(mod);

        // Get the i8* type and create a (i8*)null.
        llvm::PointerType *int8PtrType = builder.getInt8PtrTy();

        // Release value reference.
        value = builder.CreatePointerCast(value, int8PtrType);
        builder.CreateCall(release, value);
    }

    void Function::DefineSpecialFunction()
    {
        // Get the module.
        Module *module = GetModule();

        // Find the function type.
        const std::string &name = GetName();
        if(IsConstructor())
        {
            DefineSpecialConstructor();
        }
        else if(name == "Invoke")
        {
            DefineDelegateInvoke();
        }
        else
        {
            throw ModuleException("unknown runtime defined function " + GetFullName());
        }

        // Verify the function.
        //targetFunction->dump();
        llvm::verifyFunction(*targetFunction);

        // Optimize the function.
        module->GetFunctionPassManager()->run(*targetFunction);
    }

    void Function::DefineSpecialConstructor()
    {
        // Get the virtual machine.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();

        // The parent must be a class.
        Member *parent = GetParent();
        if(!parent->IsClass())
            throw ModuleException("runtime defined constructor must in a class");

        // Cast the parent.
        Class *clazz = static_cast<Class*> (parent);
        if(!clazz->IsDerivedFrom(vm->GetDelegateClass()))
            throw ModuleException("unknown runtime defined constructor " + GetFullName());

        // Clear the old definition.
        targetFunction->dropAllReferences();

        // Create the top block
        llvm::BasicBlock *topBlock = llvm::BasicBlock::Create(GetLlvmContext(), "top", targetFunction);
        builder.SetInsertPoint(topBlock);

        // Create an empty constructor, for now.
        builder.CreateRetVoid();
    }

    void Function::DefineDelegateInvoke()
    {
        // Get the virtual machine.
        Module *module = GetModule();
        VirtualMachine *vm = GetVM();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();

        // The parent must be a class.
        Member *parent = GetParent();
        if(!parent->IsClass())
            throw ModuleException("delegates must be classes");

        // Cast the parent.
        Class *clazz = static_cast<Class*> (parent);
        if(!clazz->IsDerivedFrom(vm->GetDelegateClass()))
            throw ModuleException("expected method in a delegate: " + GetFullName());

        // Clear the old definition.
        targetFunction->dropAllReferences();

        // Create the top block
        llvm::LLVMContext &context = GetLlvmContext();
        llvm::BasicBlock *topBlock = llvm::BasicBlock::Create(context, "top", targetFunction);
        builder.SetInsertPoint(topBlock);

        // The first argument must be self
        llvm::Function::arg_iterator ait = targetFunction->arg_begin();
        llvm::Value *self = ait++;
        llvm::Type *selfType = self->getType();

        // Read the instance pointer.
        Field *instanceField = clazz->GetField("instance");
        llvm::Value *instance = builder.CreateLoad(builder.CreateStructGEP(self, instanceField->GetStructureIndex()), "instance");

        // Read the function pointer.
        Field *functionField = clazz->GetField("function");
        llvm::Value *function = builder.CreateLoad(builder.CreateStructGEP(self, functionField->GetStructureIndex()), "function");

        // Invoke the correct function version.
        llvm::BasicBlock *staticInvoke = llvm::BasicBlock::Create(context, "staticInvoke", targetFunction);
        llvm::BasicBlock *instanceInvoke = llvm::BasicBlock::Create(context, "instanceInvoke", targetFunction);
        llvm::BasicBlock *chainChoice = llvm::BasicBlock::Create(context, "chainChoice", targetFunction);
        builder.CreateCondBr(builder.CreateIsNull(instance), staticInvoke, instanceInvoke);

        //-----------------------------------------------------------
        // Static invocation
        builder.SetInsertPoint(staticInvoke);

        // Get the function type.
        const FunctionType *functionType = static_cast<const FunctionType*> (GetType());
        const ChelaType *returnType = functionType->GetReturnType();

        // Build the static function type.
        std::vector<const ChelaType*> argTypes(functionType->arg_begin() + 1, functionType->arg_end());
        const FunctionType *staticFunctionType = FunctionType::Create(returnType, argTypes);
        const PointerType *staticFunctionPointerType = PointerType::Create(staticFunctionType);

        // Cast the function pointer type.
        llvm::Value *staticFunction = builder.CreatePointerCast(function, staticFunctionPointerType->GetTargetType());

        // Call it.
        std::vector<llvm::Value*> args;
        for(llvm::Function::arg_iterator a = ait; a != targetFunction->arg_end(); ++a)
            args.push_back(a);

        llvm::Value *staticRetVal = builder.CreateCall(staticFunction, args);
        builder.CreateBr(chainChoice);

        //-----------------------------------------------------------
        // Instance invocation.
        builder.SetInsertPoint(instanceInvoke);

        // Build the instance function type.
        argTypes.clear();
        argTypes.push_back(instanceField->GetType()); // Self type.
        FunctionType::arg_iterator cit = functionType->arg_begin() + 1;
        for(; cit != functionType->arg_end(); ++cit)
            argTypes.push_back(*cit);
        const FunctionType *instanceFunctionType = FunctionType::Create(returnType, argTypes);
        const PointerType *instanceFunctionPointerType = PointerType::Create(instanceFunctionType);

        // Cast the function pointer type.
        llvm::Value *instanceFunction = builder.CreatePointerCast(function, instanceFunctionPointerType->GetTargetType());

        // Call it.
        args.clear();
        args.push_back(instance);
        for(llvm::Function::arg_iterator a = ait; a != targetFunction->arg_end(); ++a)
            args.push_back(a);

        llvm::Value *instanceRetVal = builder.CreateCall(instanceFunction, args);
        builder.CreateBr(chainChoice);

        //-----------------------------------------------------------
        // Decide if calling next value, or return.
        builder.SetInsertPoint(chainChoice);
        llvm::Value *retVal = NULL;
        if(returnType != ChelaType::GetVoidType(vm))
        {
            llvm::PHINode *retPhi = builder.CreatePHI(returnType->GetTargetType(), 2);
            retPhi->addIncoming(staticRetVal, staticInvoke);
            retPhi->addIncoming(instanceRetVal, instanceInvoke);
            retVal = retPhi;
        }

        // Read the next delegate.
        Field *nextField = clazz->GetField("next");
        llvm::Value *next = builder.CreateLoad(builder.CreateStructGEP(self, nextField->GetStructureIndex()), "next");

        // Call the next delegate or return value.
        llvm::BasicBlock *invokeNext = llvm::BasicBlock::Create(context, "invokeNext", targetFunction);
        llvm::BasicBlock *justReturn = llvm::BasicBlock::Create(context, "justReturn", targetFunction);
        builder.CreateCondBr(builder.CreateIsNull(next), justReturn, invokeNext);

        //------------------------------------------------------------
        // Return the value
        builder.SetInsertPoint(justReturn);
        if(returnType != ChelaType::GetVoidType(vm))
            builder.CreateRet(retVal);
        else
            builder.CreateRetVoid();

        //------------------------------------------------------------
        // Invoke next delegate.
        builder.SetInsertPoint(invokeNext);

        // Decrement the return value reference.
        if(returnType->IsReference())
        {
            const ChelaType *referencedType = DeReferenceType(returnType);
            if(referencedType->IsPassedByReference())
                ReleaseRef(builder, module, vm, retVal);
        }

        // Invoke the next.
        args.clear();
        args.push_back(builder.CreatePointerCast(next, selfType));
        for(llvm::Function::arg_iterator a = ait; a != targetFunction->arg_end(); ++a)
            args.push_back(a);
        llvm::CallInst *nextRet = builder.CreateCall(targetFunction, args);

        // Use tail call for chain calling.
        nextRet->setTailCall();
        if(returnType != ChelaType::GetVoidType(vm))
            builder.CreateRet(nextRet);
        else
            builder.CreateRetVoid();
    }
}

