#include <stdio.h>
#include "ChelaVm/ArgumentData.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/Closure.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionArgument.hpp"
#include "ChelaVm/FunctionBlock.hpp"
#include "ChelaVm/FunctionLocal.hpp"
#include "ChelaVm/FunctionGroup.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/Property.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "llvm/Analysis/Verifier.h"
#include "llvm/Intrinsics.h"

namespace ChelaVm
{
    namespace MethodAttributes
    {
        enum MA
        {
            MemberAccessMask = 7,        //MemberAccessMask
            PrivateScope = 0,            //PrivateScope
            Private = 1,                 //Private
            FamANDAssem = 2,             //FamANDAssem
            Assembly = 3,                //Assembly
            Family = 4,                  //Family
            FamORAssem = 5,              //FamORAssem
            Public = 6,                  //Public
            Static = 16,                 //Static
            Final = 32,                  //Final
            Virtual = 64,                //Virtual
            HideBySig = 128,             //HideBySig
            VtableLayoutMask = 256,      //VtableLayoutMask
            CheckAccessOnOverride = 512, //CheckAccessOnOverride
            ReuseSlot = 0,               //PrivateScope
            NewSlot = 256,               //VtableLayoutMask
            Abstract = 1024,             //Abstract
            SpecialName = 2048,          //SpecialName
            PinvokeImpl = 8192,          //PinvokeImpl
            UnmanagedExport = 8,         //UnmanagedExport
            RTSpecialName = 4096,        //RTSpecialName
            ReservedMask = 53248,        //ReservedMask
            HasSecurity = 16384,         //HasSecurity
            RequireSecObject = 32768,    //RequireSecObject
        };
    }

	Function::Function(Module *module)
		: Member(module), genericPrototype(module)
	{
        declaringModule = module;
		targetFunction = NULL;
		defined = false;
        declarations = NULL;
        initializations = NULL;
        topLandingPad = NULL;
        topCleanup = NULL;
        exceptionStructLocal = NULL;
        exceptionLocal = NULL;
        selectedExceptionLocal = NULL;
        returnPointer = NULL;
        exceptionCaught = NULL;
        handlingExceptions = true;
        methodInfo = NULL;
        genericInstance = NULL;
        templateFunction = NULL;

        kernelBinderType = NULL;
        kernelThreadType = NULL;
        kernelClosure = NULL;
        kernelBinder = NULL;
        kernelThread = NULL;
	}
	
	Function::~Function()
	{
        for(size_t i = 0; i < argumentData.size(); ++i)
            delete argumentData[i];
		for(size_t i = 0; i < arguments.size(); ++i)
			delete arguments[i];
		for(size_t i = 0; i < locals.size(); ++i)
			delete locals[i];
		for(size_t i = 0; i < blocks.size(); ++i)
			delete blocks[i];
	}
		
    Module *Function::GetDeclaringModule() const
    {
        return declaringModule;
    }

	std::string Function::GetName() const
	{
		return name;
	}

    std::string Function::GetFullName() const
    {
        // Use the raw name as full name for cdecl.
        if(IsCdecl())
            return GetName();

        return Member::GetFullName() + " " + GetType()->GetFullName();
    }

    std::string Function::GetMangledName() const
    {
        if(IsCdecl())
            return GetName();

        return Member::GetMangledName() + GetType()->GetMangledName().substr(1);
    }

	MemberFlags Function::GetFlags() const
	{
		return flags;
	}
	
	int Function::GetVSlot() const
	{
		return vslot;
	}
	
	const ChelaType *Function::GetType() const
	{
		return functionType;
	}

    const FunctionType *Function::GetFunctionType() const
    {
        return functionType;
    }

	bool Function::IsFunction() const
	{
		return true;
	}
	
	llvm::Function *Function::GetTarget()
	{
		return targetFunction;
	}

    llvm::Constant *Function::ImportFunction(llvm::Module *targetModule)
    {
        // Get the original function.
        llvm::Function *original = GetTarget();

        // Avoid unnecessary import.
        if(original->getParent() == targetModule)
            return original;

        // Import the function.
        return targetModule->getOrInsertFunction(original->getName(),
                       original->getFunctionType(), original->getAttributes());
    }

    llvm::Constant *Function::ImportFunction(Module *targetModule)
    {
        return ImportFunction(targetModule->GetTargetModule());
    }

	FunctionArgument *Function::GetArgument(uint32_t id)
	{
		if(id >= arguments.size())
			throw ModuleException("Invalid argument id.");
		return arguments[id];
	}

    size_t Function::GetLocalCount() const
    {
        return locals.size();
    }
	
	FunctionLocal *Function::GetLocal(uint32_t id)
	{
		if(id >= locals.size())
			throw ModuleException("Invalid local id.");
		return locals[id];
	}
	
	FunctionBlock *Function::GetBlock(uint32_t id)
	{
		if(id >= blocks.size())
			throw ModuleException("Invalid block id.");
		return blocks[id];
	}

    bool Function::IsHandlingExceptions() const
    {
        return handlingExceptions;
    }

    Function *Function::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        //printf("Sizes: member %zu, function %zu, block %zu\n",
        //sizeof(Member), sizeof(Function), sizeof(FunctionBlock));
        Function *res = new Function(module);

        // Store the name and the flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Use a special name for static constructors.
        if(res->IsStaticConstructor())
            res->name = "..sctor";

        // Skip the data.
        reader.Skip(header.memberSize);
        return res;
    }

    void Function::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the member attributes.
        ReadAttributes(reader, header.memberAttributes);
        
		// Read the function header.
		FunctionHeader fheader;
		reader >> fheader;
        vslot = fheader.vslot;

        // Read the function type.
        Module *module = GetModule();
        const ChelaType *type = module->GetType(fheader.functionType);
        if(!type->IsFunction())
            throw ModuleException("expected function type.");
        functionType = static_cast<const FunctionType*> (type);

        // Check the argument count.
        if(fheader.numargs != functionType->GetArgumentCount())
            throw ModuleException("not matching argument count in function.");

        // Read the generic prototype.
        genericPrototype.Read(reader);

        // Read the arguments.
        argumentData.reserve(fheader.numargs);
        for(uint32_t i = 0; i < fheader.numargs; i++)
        {
            ArgumentData *data = new ArgumentData(module);
            data->Read(reader);
            argumentData.push_back(data);
        }
		
		// Read the local variables.
		for(uint32_t i = 0; i < fheader.numlocals; i++)
		{
			FunctionLocal *local = new FunctionLocal(this);
			local->Read(reader);
			locals.push_back(local);
		}
		
		// Read the basic blocks.
		for(uint32_t i = 0; i < fheader.numblocks; i++)
		{
			FunctionBlock *block = new FunctionBlock(this, i);
			block->Read(reader);
			blocks.push_back(block);
		}

        // Read the exception contexts.
        ExceptionContextHeader ch;
        size_t numexceptions = fheader.numexceptions;
        exceptions.resize(numexceptions);
        for(size_t i = 0; i < numexceptions; i++)
        {
            ExceptionContext &context = exceptions[i];

            // Read the context header.
            reader >> ch;

            // Link the parent

            if(ch.parentId >= (int)numexceptions)
                throw ModuleException("Invalid parent exception context index.");
            if(ch.parentId >= 0)
            {
                context.parent = &exceptions[ch.parentId];
                context.parentId = ch.parentId;
            }

            // Link the cleanup.
            if(ch.cleanup >= 0)
                context.cleanup = GetBlock(ch.cleanup);

            // Read the blocks.
            for(uint32_t j = 0; j < ch.numblocks; j++)
            {
                uint16_t blockIndex;
                reader >> blockIndex;

                // Set the block exception context.
                FunctionBlock *block = GetBlock(blockIndex);
                if(block->GetExceptionContext() != NULL)
                    throw ModuleException("A block only can have one exception context.");
                block->SetExceptionContext(&context);
                context.blocks.insert(block);
            }

            // Read the handlers.
            for(uint32_t j = 0; j < ch.numhandlers; j++)
            {
                uint32_t exceptionType;
                uint16_t handlerBlock;
                reader >> exceptionType >> handlerBlock;

                // Create the handler structure.
                ExceptionHandler handler;
                handler.exceptionType = module->GetClassType(exceptionType);
                handler.handler = GetBlock(handlerBlock);
                handler.handler->SetCatchBlock(true);
                handler.handler->SetExceptionType(exceptionType);
                if(!handler.exceptionType)
                    throw ModuleException("expected exception type.");
                context.handlers.push_back(handler);
            }
        }
	}
	
	void Function::DeclarePass()
	{
		// Don't redeclare functions.
		if(IsGeneric() || targetFunction != NULL)
			return;

        // Don't declare abstract/contract methods.
        if(IsAbstract() || IsContract())
            return;

        // Check safetyness.
        if(!IsUnsafe() && functionType->IsUnsafe())
            Error("safe function with unsafe parameters or return type.");

		// Get the target module.
        Module *module = GetModule();
		llvm::Module *targetModule = module->GetTargetModule();

		// Prepare the name.
		std::string name = GetFullName();

		// TODO: Check duplicated cdecl functions.

        // Get the target function type.
		llvm::FunctionType *targetFunctionType =
			static_cast<llvm::FunctionType*> (functionType->GetTargetType());

		// Compute the function linkage.		
		llvm::Function::LinkageTypes linkage = ComputeLinkage();
        if(IsCdecl() && IsExtern())
        {
            llvm::Constant *constant = targetModule->getOrInsertFunction(name, targetFunctionType);
            targetFunction = llvm::dyn_cast<llvm::Function> (constant);
            if(!targetFunction)
            {
                constant->dump();
                Error("Function is another kind of constant");
            }
        }
        else
        {
		    targetFunction = llvm::Function::Create(targetFunctionType,
					    linkage, GetMangledName(), targetModule);
        }

        // Set no return for special functions.
        if(name == "__chela_eh_personality__")
        {
            targetFunction->addFnAttr(llvm::Attribute::NoInline);
            handlingExceptions = false;
        }

        if(name == "__chela_eh_throw__")
        {
            targetFunction->setDoesNotReturn();
            handlingExceptions = false;
        }

        // Set the GC.
        //targetFunction->setGC("ChelaRefCount");
					
		// Get the return pointer.
		llvm::Function::arg_iterator targ = targetFunction->arg_begin();
        if(functionType->GetReturnType()->IsComplexStructure())
        {
            // The first parameter is the return pointer.
            returnPointer = targ;

            // Set the sret attribute.
            (targ++)->addAttr(llvm::Attribute::StructRet);
        }

        // Declare the return values.
		FunctionType::arg_iterator carg = functionType->arg_begin();
		for(int index = 0; targ != targetFunction->arg_end(); ++targ, ++carg, ++index)
		{
			FunctionArgument *arg = new FunctionArgument(this, *carg, targ, argumentData[index], index);
			arguments.push_back(arg);

            // Set the argument name.
            targ->setName(argumentData[index]->GetName());
		}

        // Create a dummy block, needed to initialize the GC.
        if(!IsAbstract() && !IsContract() && !blocks.empty())
        {
            llvm::BasicBlock::Create(GetLlvmContext(), "dummy", targetFunction);

            // Register static constructor.
            if(IsStaticConstructor())
                GetModule()->RegisterStaticConstructor(this);

            // Declare debug information.
            DebugInformation *debugInfo = module->GetDebugInformation();
            if(debugInfo)
            {
                subprogramScope = debugInfo->DeclareFunction(this);
                if(subprogramScope)
                    topLexicalScope = debugInfo->GetTopLexicalScope(this);
            }
        }

        // Declare kernel support.
        if(IsKernel())
            DeclareKernelSupport();
	}

    llvm::Value *Function::CreateIntermediate(const ChelaType *type, bool refCounted)
    {
        Intermediate inter;

        // Get the target type.
        llvm::Type *targetType = type->GetTargetType();

        // Store old insertion point.
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();
        llvm::BasicBlock *oldBlock = builder.GetInsertBlock();

        // Create the intermediate.
        builder.SetInsertPoint(declarations);
        inter.type = type;
        inter.value = builder.CreateAlloca(targetType, 0, "i");
        inter.refCounted = refCounted;
        intermediates.push_back(inter);

        // Initialize the intermediate.
        if(refCounted)
        {
            // Initialize to null.
            builder.SetInsertPoint(initializations);
            llvm::Value *nullValue = llvm::Constant::getNullValue(targetType);
            builder.CreateStore(nullValue, inter.value);
        }

        // Restore the insertio point.
        builder.SetInsertPoint(oldBlock);

        // Return the intermediate.
        return inter.value;
    }

    size_t Function::GetIntermediateCount() const
    {
        return intermediates.size();
    }

    const Function::Intermediate &Function::GetIntermediate(size_t index) const
    {
        if(index >= intermediates.size())
            throw ModuleException("Invalid intermediate index.");
        return intermediates[index];
    }

    llvm::BasicBlock *Function::GetTopLandingPad() const
    {
        return topLandingPad;
    }

    llvm::BasicBlock *Function::GetTopCleanup() const
    {
        return topCleanup;
    }

    llvm::Value *Function::GetExceptionCaught() const
    {
        return exceptionCaught;
    }

    llvm::Value *Function::GetExceptionLocal() const
    {
        return exceptionLocal;
    }

    llvm::Value *Function::GetReturnPointer() const
    {
        return returnPointer;
    }

    llvm::DILexicalBlock Function::GetTopLexicalScope() const
    {
        return topLexicalScope;
    }

    inline void ReleaseVariable(llvm::IRBuilder<> &builder, llvm::Constant *release,
        llvm::Value *local, const ChelaType *localType)
    {
        llvm::PointerType *int8PtrType = builder.getInt8PtrTy();

        // Release sub references.
        if(localType->IsStructure() && localType->HasSubReferences())
        {
            const Structure *building = static_cast<const Structure*> (localType);
            for(size_t i = 0; i < building->GetValueRefCount(); ++i)
            {
                // Get the reference field.
                const Field *field = building->GetValueRefField(i);

                // Load the field value.
                size_t fieldIndex = field->GetStructureIndex();
                llvm::Value *fieldPtr = builder.CreateStructGEP(local, fieldIndex);
                llvm::Value *fieldValue = builder.CreateLoad(fieldPtr);

                // Release the field.
                fieldValue = builder.CreatePointerCast(fieldValue, int8PtrType);
                builder.CreateCall(release, fieldValue);
            }
        }

        // Release the reference.
        if(localType->IsReference())
        {
            localType = DeReferenceType(localType);
            if(localType->IsPassedByReference())
            {
                // Load the reference.
                llvm::Value *value = builder.CreateLoad(local);

                // Release it.
                value = builder.CreatePointerCast(value, int8PtrType);
                builder.CreateCall(release, value);
            }
        }
    }

    void Function::PerformCleanup()
    {
        // Get the release functions.
        Module *module = GetModule();
        llvm::IRBuilder<> &builder = module->GetIRBuilder();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Constant *release = vm->GetRelRef(module);

        // Set the ref counted locals to null.
        size_t numlocals = GetLocalCount();
        for(size_t i = 0; i < numlocals; i++)
        {
            // Get the local.
            FunctionLocal *local = GetLocal(i);

            // Only process reference counted locals.
            if(!local->IsRefCounted())
                continue;

            // Release the local.
            ReleaseVariable(builder, release, local->GetValue(), local->GetType());
        }

        // Set the ref counted intermediates to null.
        size_t numinter = GetIntermediateCount();
        for(size_t i = 0; i < numinter; i++)
        {
            // Get the intermediate.
            const Function::Intermediate &inter = GetIntermediate(i);

            // Only process reference counted locals.
            if(!inter.refCounted)
                continue;

            // Release the intermediate.
            ReleaseVariable(builder, release, inter.value, inter.type);
        }
    }

    static void ComputeHandlers(ExceptionContext &context, std::vector<ExceptionHandler*> &handlers)
    {
        // TODO: Avoid duplications.

        // Store all of the context handlers.
        for(size_t i = 0; i < context.handlers.size(); i++)
            handlers.push_back(&context.handlers[i]);

        // Process the parent handlers.
        if(context.parent)
            ComputeHandlers(*context.parent, handlers);
    }

    void Function::ImplementLandingPath(ExceptionContext &context)
    {
        // Get some functions.
        Module *module = GetModule();
        llvm::IRBuilder<> &builder = module->GetIRBuilder();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::LLVMContext &llvmContext = vm->getContext();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::Function *eh_typeid_for = llvm::Intrinsic::getDeclaration(targetModule, llvm::Intrinsic::eh_typeid_for);
        llvm::Value *eh_personality = vm->GetEhPersonality(module);

        // Compute the exceptions to catch.
        std::vector<ExceptionHandler*> handlers;
        ComputeHandlers(context, handlers);

        // Get the i8* type.
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(llvmContext);

        // Create the exception structure.
        llvm::StructType *excStructTy = llvm::StructType::get(llvm::Type::getInt8PtrTy(llvmContext),
            llvm::Type::getInt32Ty(llvmContext), NULL);

        // Write the landing path.
        builder.SetInsertPoint(context.landingPad);

        // Add the landing pad instruction.
        llvm::LandingPadInst *landingPath = builder.CreateLandingPad(excStructTy, eh_personality, handlers.size());
        landingPath->setCleanup(true);
        for(size_t i = 0; i < handlers.size(); i++)
        {
            ExceptionHandler *h = handlers[i];
            landingPath->addClause(h->exceptionType->GetTypeInfo(module));
        }

        // Store the exception structure.
        builder.CreateStore(landingPath, exceptionStructLocal);

        // Get the exception.
        llvm::Value *exception = builder.CreateExtractValue(landingPath, 0, "ehptr");

        // Store the exception.
        builder.CreateStore(exception, exceptionLocal);

        // Get the selection.
        llvm::Value *selection = builder.CreateExtractValue(landingPath, 1, "ehptr");

        // Unset the caught flag.
        builder.CreateStore(builder.getInt1(false), exceptionCaught);

        // Store the selection.
        builder.CreateStore(selection, selectedExceptionLocal);

        // Move to the selection pad.
        builder.CreateBr(context.selectionPad);
        builder.SetInsertPoint(context.selectionPad);

        // Load the selection.
        selection = builder.CreateLoad(selectedExceptionLocal);

        // Select the correct handler
        llvm::BasicBlock *next = NULL;
        size_t numhandlers = context.handlers.size();
        for(size_t i = 0; i < numhandlers; i++)
        {
            const ExceptionHandler &handler = context.handlers[i];

            // Create the next block.
            next = llvm::BasicBlock::Create(GetLlvmContext(), "csel", targetFunction);

            // Compare the type index
            llvm::Value *typeinfo = builder.CreatePointerCast(handler.exceptionType->GetTypeInfo(module), int8PtrTy);
            llvm::Value *compareIndex = builder.CreateCall(eh_typeid_for, typeinfo);
            llvm::Value *compare = builder.CreateICmpEQ(compareIndex, selection);
            builder.CreateCondBr(compare, handler.handler->GetBlock(), next);

            // Set the insert point.
            builder.SetInsertPoint(next);
        }

        // Branch to the cleanup/parent selection.
        if(context.cleanup != NULL)
            builder.CreateBr(context.cleanup->GetBlock());
        else if(context.parent != NULL)
            builder.CreateBr(context.parent->selectionPad);
        else
            builder.CreateBr(topCleanup);
    }

    void Function::ImplementTopLandingPath()
    {
        // Get some functions.
        Module *module = GetModule();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::LLVMContext &context = vm->getContext();
        llvm::Value *eh_personality = vm->GetEhPersonality(module);

        // Create the exception structure.
        llvm::StructType *excStructTy = llvm::StructType::get(llvm::Type::getInt8PtrTy(context),
            llvm::Type::getInt32Ty(context), NULL);

        // Write the top landing path.
        builder.SetInsertPoint(topLandingPad);

        // Add the landing pad instruction.
        llvm::LandingPadInst *landingPath = builder.CreateLandingPad(excStructTy, eh_personality, 0);
        landingPath->setCleanup(true);

        // Store the exception struct.
        builder.CreateStore(landingPath, exceptionStructLocal);

        // Get the exception.
        //llvm::Value *exception = builder.CreateExtractValue(landingPath, 0, "ehptr");

        // Store the exception.
        //builder.CreateStore(exception, exceptionLocal);

        // Branch to the cleanup.
        builder.CreateBr(topCleanup);

        // Write top cleanup.
        builder.SetInsertPoint(topCleanup);

        // Perform cleanup.
        PerformCleanup();

        // Resume exception handling.
        llvm::Value *excStruct = builder.CreateLoad(exceptionStructLocal);
        builder.CreateResume(excStruct);
    }

	void Function::DefinitionPass()
	{
        // Just in case declare.
        if(targetFunction == NULL)
            DeclarePass();

        // Only define once.
        if(defined)
            return;
        defined = true;

        // Get the module and the virtual machine.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();
        IRBeginFunction irState(builder);

        // Create reflection data.
        if(module->HasReflection())
        {
            // Create the member info.
            CreateMemberInfo();

            // Define the custom attributes.
            DefineAttributes();
        }

        // Don't define generic functions.
        if(IsGeneric())
            return;

        // Ignore abstract/contract.
        if(IsAbstract() || IsContract())
            return;

        // Some functions are defined by the runtime.
        if(IsRuntime())
        {
            DefineSpecialFunction();
        }

        // Check for system api.
        if(IsCdecl() && IsExtern() && blocks.empty())
        {
            const void *sysApi = vm->GetSystemApi(GetName());
            if(sysApi)
            {
                // Connect with the system api.
                llvm::ExecutionEngine *ee = vm->GetExecutionEngine();
                ee->updateGlobalMapping(targetFunction, const_cast<void*> (sysApi));
            }
        }

		// Ignore pure declarations.
		if(blocks.empty())
			return;

        //printf("Define function %s\n", GetName().c_str());
        // Get the target module.
        //llvm::Module *targetModule = module->GetTargetModule();

        // Delete the previous body.
        targetFunction->dropAllReferences();

		// Create the local declarations block.
		declarations = llvm::BasicBlock::Create(GetLlvmContext(), "decls", targetFunction);
		builder.SetInsertPoint(declarations);
		
		// Declare the locals.
		for(size_t i = 0; i < locals.size(); i++)
		{
			FunctionLocal *local = locals[i];
			local->Declare(builder);
		}

        // Declare exception variables.
        if(handlingExceptions)
        {
            // Create the exception structure.
            llvm::StructType *excStructTy = llvm::StructType::get(builder.getInt8PtrTy(),
                builder.getInt32Ty(), NULL);

            exceptionStructLocal = builder.CreateAlloca(excStructTy, 0, "eh_struct");
            exceptionLocal = builder.CreateAlloca(builder.getInt8PtrTy(), 0, "eh_exception");
            selectedExceptionLocal = builder.CreateAlloca(builder.getInt32Ty(), 0, "eh_selected");
            exceptionCaught = builder.CreateAlloca(builder.getInt1Ty(), 0, "eh_caught");
        }

        // Create the initializations block.
        initializations = llvm::BasicBlock::Create(GetLlvmContext(), "inits", targetFunction);
        builder.SetInsertPoint(initializations);

        // Initialize reference locals to null.
        for(size_t i = 0; i < locals.size(); i++)
        {
            // Get the local type.
            FunctionLocal *local = locals[i];
            const ChelaType *localType = local->GetType();

            // Must be a reference type.
            if(!localType->IsReference())
            {
                // Sub references are counted.
                bool subRefs = localType->HasSubReferences();
                local->SetRefCounted(subRefs);
                if(!subRefs)
                    continue;
            }
            else
            {
                const ChelaType *objectType = DeReferenceType(localType);
                if(objectType->IsPassedByReference())
                    local->SetRefCounted(true);
            }

            // Initialize the local to null.
            llvm::Value *nullValue = llvm::Constant::getNullValue(localType->GetTargetType());
            builder.CreateStore(nullValue, local->GetValue());
        }

        // Initialize  exception handling variables.
        if(handlingExceptions)
        {
            builder.CreateStore(llvm::ConstantPointerNull::get(builder.getInt8PtrTy()), exceptionLocal);
            builder.CreateStore(builder.getInt32(0),  selectedExceptionLocal);
            builder.CreateStore(builder.getInt1(false),  exceptionCaught);
        }

        // Declare the landing paths.
        if(handlingExceptions)
        {
            topLandingPad = llvm::BasicBlock::Create(GetLlvmContext(), "tlpath", targetFunction);
            topCleanup = llvm::BasicBlock::Create(GetLlvmContext(), "tcleanup", targetFunction);
            for(size_t i = 0; i < exceptions.size(); i++)
            {
                ExceptionContext &context = exceptions[i];

                // Create the context blocks.
                context.landingPad = llvm::BasicBlock::Create(GetLlvmContext(), "lpath", targetFunction);
                context.selectionPad = llvm::BasicBlock::Create(GetLlvmContext(), "spath", targetFunction);
            }
        }

		// Declare the blocks.
		for(size_t i = 0; i < blocks.size(); i++)
		{
			FunctionBlock *block = blocks[i];
			block->Declare(builder);
		}
		
		// Calculate block dependencies.
		blocks[0]->CalculateDependencies();
		
		// Define the blocks.
		for(size_t i = 0; i < blocks.size(); i++)
		{
			FunctionBlock *block = blocks[i];
			block->Define(builder);
		}

        // Connect the declarations, initializations and blocks.
        builder.SetInsertPoint(declarations);
        DeclareDebugVariables();
        builder.CreateBr(initializations);
        builder.SetInsertPoint(initializations);
        builder.CreateBr(blocks[0]->GetBlock());

        // Finish blocks.
        for(size_t i = 0; i < blocks.size(); i++)
        {
            FunctionBlock *block = blocks[i];
            block->Finish(builder);
        }

        // Implement the landing paths.
        if(handlingExceptions)
        {
            for(size_t i = 0; i < exceptions.size(); i++)
                ImplementLandingPath(exceptions[i]);
            ImplementTopLandingPath();
        }

		// Verify the function.
		if(llvm::verifyFunction(*targetFunction, llvm::PrintMessageAction))
        {
            targetFunction->dump();
            Error("Invalid generated function.");
        }
		
		// Optimize the function.
		module->GetFunctionPassManager()->run(*targetFunction);

        // Create kernel support functions.
        if(IsKernel())
            CreateKernelSupport();
	}

    void Function::DeclareDebugVariables()
    {
        // Make sure there is debug info.
        Module *module = GetModule();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();
        DebugInformation *debugInfo = module->GetDebugInformation();
        if(!debugInfo)
            return;

        // Get my debug info.
        FunctionDebugInfo *functionDebug = debugInfo->GetFunctionDebug(GetMemberId());
        if(!functionDebug)
            return;

         // Declare each local variable.
        llvm::DIBuilder &diBuilder = debugInfo->GetDebugBuilder();
        llvm::BasicBlock *block = builder.GetInsertBlock();
        for(size_t i = 0; i < locals.size(); ++i)
        {
            // Get the local and debug local.
            FunctionLocal *local = locals[i];
            LocalDebugInfo *debugLocal = functionDebug->GetLocal(i);
            const SourcePosition &pos = debugLocal->GetPosition();

            // Get the local lexical scope.
            llvm::DIDescriptor scope =
                functionDebug->GetLexicalScope(debugLocal->GetScopeIndex())
                    ->GetDescriptor();

            // Declare the debug local.
            llvm::Value *localValue = local->GetValue();
            llvm::DIVariable localInfo = debugLocal->CreateDescriptor(local, scope);
            llvm::Instruction *inst = diBuilder.insertDeclare(localValue,
                localInfo, block);

            // Set the declare position.
            inst->setDebugLoc(llvm::DebugLoc::get(pos.GetLine(), pos.GetColumn(), subprogramScope));
        }

        // Update the insert point.
        builder.SetInsertPoint(block);
    }

    llvm::DIDescriptor Function::GetDebugNode(DebugInformation *context) const
    {
        // Find the cached debug info.
        llvm::DIDescriptor debugNode = context->GetMemberNode(this);
        if(debugNode)
            return debugNode;

        // TODO: Create my debug node.
        return debugNode;
    }

    void Function::AddDependency(Function *dependency)
    {
        dependencies.insert(dependency);
    }

    void Function::BuildDependencies()
    {
        std::set<Function*>::const_iterator it = dependencies.begin();
        for(; it != dependencies.end(); ++it)
        {
            Function *dep = *it;
            dep->DeclarePass();
            dep->DefinitionPass();
            dep->BuildDependencies();
        }
    }

    llvm::GlobalVariable *Function::GetMemberInfo()
    {
        // Create the method info variable.
        if(!methodInfo)
        {
            // Don't create function info when there's not reflection support.
            Module *module = GetModule();
            if(!module->HasReflection())
                return NULL;

            // Get the virtual machine.
            VirtualMachine *vm = module->GetVirtualMachine();

            // Select the correct method info class.
            Class *methodInfoClass = (IsConstructor() || IsStaticConstructor()) ?
                vm->GetConstructorInfoClass() : vm->GetMethodInfoClass();

            // Create the method info variable.
            llvm::Module *targetModule = module->GetTargetModule();
            methodInfo = new llvm::GlobalVariable(*targetModule, methodInfoClass->GetTargetType(),
                                    false, ComputeMetadataLinkage(), NULL, GetMangledName() + "_methodinfo_");
        }

        return methodInfo;
    }

    void Function::CreateMemberInfo()
    {
        // Get the virtual machine.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::LLVMContext &ctx = targetModule->getContext();
        llvm::Type *intType = llvm::Type::getInt32Ty(ctx);
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(ctx);

        // Select the correct method info class.
        Class *methodInfoClass = (IsConstructor() || IsStaticConstructor()) ?
            vm->GetConstructorInfoClass() : vm->GetMethodInfoClass();
        methodInfoClass->DeclarePass();

        // Create the property info value.
        ConstantStructurePtr methodInfoValue(methodInfoClass->CreateConstant(module));

        // Store the MemberInfo attributes.
        SetMemberInfoData(methodInfoValue);

        // Set the method pointer.
        methodInfoValue->SetField("methodPointer", targetFunction);

        // Set the method vslot.
        methodInfoValue->SetField("vslot", llvm::ConstantInt::get(intType, vslot));

        // Set the return type.
        if(!IsConstructor() && !IsStaticConstructor())
            methodInfoValue->SetField("returnType", GetReflectedType(functionType->GetReturnType()));

        // Don't store the implicit this parameter.
        size_t startParameter = 0;
        if(!IsStatic() && !IsStaticConstructor())
            startParameter = 1;

        // Create the parameter info array.
        std::vector<llvm::Constant*> parameters;
        for(size_t i = startParameter; i < arguments.size(); ++i)
        {
            llvm::Constant *paramInfo = arguments[i]->CreateParameterInfo();
            paramInfo = llvm::ConstantExpr::getPointerCast(paramInfo, int8PtrTy);
            parameters.push_back(paramInfo);
        }

        // Create a global array with the parameters.
        llvm::ArrayType *paramArrayType = llvm::ArrayType::get(int8PtrTy, parameters.size());
        llvm::Constant *paramArrayConstant = llvm::ConstantArray::get(paramArrayType, parameters);
        llvm::Constant *paramArray = new llvm::GlobalVariable(*targetModule, paramArrayType,
                                    true, llvm::GlobalVariable::PrivateLinkage, paramArrayConstant, "_paramInfoArray_");
        paramArray = llvm::ConstantExpr::getPointerCast(paramArray, int8PtrTy);

        // Set the parameter info.
        methodInfoValue->SetField("numparameters", llvm::ConstantInt::get(intType, parameters.size()));
        methodInfoValue->SetField("parameters", paramArray);

        // Compute the method attributes.
        int methodAttributes = 0;
        if(IsPublic())
            methodAttributes |= MethodAttributes::Public;
        else if(IsPrivate())
            methodAttributes |= MethodAttributes::Private;
        else if(IsProtected())
            methodAttributes |= MethodAttributes::Family;
        else if(IsProtectedInternal())
            methodAttributes |= MethodAttributes::FamORAssem;
        else if(IsInternal())
            methodAttributes |= MethodAttributes::Assembly;

        if(IsStatic() || IsStaticConstructor())
            methodAttributes |= MethodAttributes::Static;
        else if(IsVirtual() || IsOverride())
            methodAttributes |= MethodAttributes::Virtual;
        else if(IsAbstract())
            methodAttributes |= MethodAttributes::Abstract;
            
        if(!IsCdecl())
            methodAttributes |= MethodAttributes::HideBySig;
        if(IsRuntime())
            methodAttributes |= MethodAttributes::RTSpecialName;

        // Store the method attributes.
        methodInfoValue->SetField("attributes", llvm::ConstantInt::get(intType, methodAttributes));

        if(blocks.size() && IsKernel())
        {
            // Create the block data structure.
            llvm::StructType *blockDataTy = llvm::StructType::get(intType, intType, intType, NULL);
            std::vector<llvm::Constant*> blockArrayData;
            for(size_t i = 0; i < blocks.size(); ++i)
            {
                // Get the block.
                FunctionBlock *block = GetBlock(i);

                // Encapsulate the block data.
                llvm::Constant *blockData =
                    llvm::ConstantStruct::get(blockDataTy, llvm::ConstantInt::get(intType, block->GetRawBlockSize()),
                        llvm::ConstantInt::get(intType, block->GetBlockOffset()),
                        llvm::ConstantInt::get(intType, block->IsUnsafe()),
                        NULL);

                // Append his data.
                blockArrayData.push_back(blockData);
            }

            // Create the block data array.
            llvm::ArrayType *blockDataArrayTy = llvm::ArrayType::get(blockDataTy, blockArrayData.size());
            llvm::Constant *blockDataArrayConstant = llvm::ConstantArray::get(blockDataArrayTy, blockArrayData);
            llvm::GlobalVariable *blockDataGlobal =
                new llvm::GlobalVariable(*targetModule, blockDataArrayTy,
                                    true, llvm::GlobalVariable::PrivateLinkage, blockDataArrayConstant, "_blockRefs_");

            // Store the block data.
            methodInfoValue->SetField("numblocks", llvm::ConstantInt::get(intType, blockArrayData.size()));
            methodInfoValue->SetField("blocks", blockDataGlobal);
        }

        // Set the method info.
        GetMemberInfo()->setInitializer(methodInfoValue->Finish());
    }

    const GenericPrototype *Function::GetGenericPrototype() const
    {
        return &genericPrototype;
    }

    const GenericInstance *Function::GetGenericInstanceData() const
    {
        return genericInstance;
    }

    Member *Function::GetTemplateMember() const
    {
        return templateFunction;
    }

    bool Function::IsGeneric() const
    {
        if(genericPrototype.GetPlaceHolderCount() != 0)
        {
            if(genericInstance == NULL || genericInstance->IsGeneric())
                return true;
        }

        return IsParentGeneric() ||
           functionType->IsGeneric();
    }

    void Function::InstanceExceptionContext(Function *newFunction, ExceptionContext &context)
    {
        // Use the correct blocks.
        std::set<FunctionBlock*> newBlocks;
        std::set<FunctionBlock*>::iterator it = context.blocks.begin();
        for(; it != context.blocks.end(); ++it)
        {
            FunctionBlock *oldBlock = *it;
            newBlocks.insert(newFunction->GetBlock(oldBlock->GetBlockId()));
        }
        context.blocks = newBlocks;

        // Instance the exception handlers.
        size_t numhandlers = context.handlers.size();
        for(size_t i = 0; i < numhandlers; ++i)
        {
            ExceptionHandler &handler = context.handlers[i];
            handler.handler = GetBlock(handler.handler->GetBlockId());

            Structure *exceptionType = (Structure *)handler.exceptionType;
            if(exceptionType->IsGenericType())
            {
                exceptionType = exceptionType->InstanceGeneric(newFunction->GetModule(), newFunction->GetCompleteGenericInstance());
                if(exceptionType == NULL || exceptionType->IsGenericType())
                    Error("Failed to instance generic type " + handler.exceptionType->GetFullName());
                else if(!exceptionType->IsClass())
                    Error("Expected a class instance for generic instance.");
                handler.exceptionType = static_cast<const Class*> (exceptionType);
            }
        }

        // Use the correct cleanup block
        if(context.cleanup)
            context.cleanup = newFunction->GetBlock(context.cleanup->GetBlockId());
    }

    Function *Function::InstanceGeneric(Member *factory, Module *module, const GenericInstance *instance)
    {
        // Don't instance if I'm not generic
        if(!IsGeneric())
            return this;

        // Use the correct template.
        Function *templateFun = templateFunction ? templateFunction : this;

        // Use the normalized instance.
        if(genericInstance != NULL)
        {
            GenericInstance *newInstance = new GenericInstance(module);
            newInstance->InstanceFrom(*GetCompleteGenericInstance(), instance);
            instance = newInstance;
        }
        else
        {
            instance = instance->Normalize(templateFun->GetCompleteGenericPrototype());
        }

        // Find an existing implementation in the using module.
        Function *impl = module->FindGenericImplementation(templateFun, instance);
        if(impl)
            return impl;

        // Create a new implementation of the generic.
        impl = new Function(module);
        impl->declaringModule = GetDeclaringModule();
        impl->templateFunction = templateFun;
        impl->UpdateParent(factory);

        // Copy the name, flags and vslot
        impl->name = name;
        impl->flags = flags;
        impl->vslot = vslot;

        // Store the generic instance.
        impl->genericPrototype = genericPrototype;
        impl->genericInstance = instance->Normalize(&genericPrototype);

        // Instance the function type.
        impl->functionType = static_cast<const FunctionType*> (templateFun->functionType->InstanceGeneric(instance));

        // Copy the argument data.
        impl->argumentData.reserve(argumentData.size());
        for(size_t i = 0; i < templateFun->argumentData.size(); ++i)
        {
            ArgumentData *data = templateFun->argumentData[i];
            impl->argumentData.push_back(new ArgumentData(*data));
        }

        // Instance the locals.
        for(size_t i = 0; i < templateFun->locals.size(); ++i)
        {
            FunctionLocal *local = templateFun->locals[i];
            impl->locals.push_back(local->InstanceGeneric(impl, instance));
        }

        // Instance the blocks.
        for(size_t i = 0; i < templateFun->blocks.size(); ++i)
        {
            FunctionBlock *block = templateFun->blocks[i];
            impl->blocks.push_back(block->InstanceGeneric(impl, instance));
        }

        // Copy the exception contexts.
        for(size_t i = 0; i < templateFun->exceptions.size(); ++i)
        {
            impl->exceptions.push_back(templateFun->exceptions[i]);
            InstanceExceptionContext(impl, impl->exceptions[i]);
        }

        // Update the exception context parents.
        for(size_t i = 0; i < exceptions.size(); ++i)
        {
            ExceptionContext &context = exceptions[i];
            if(context.parent)
                context.parent = &exceptions[context.parentId];
        }

        // Register the implementation in the module.
        module->RegisterGenericImplementation(templateFun, impl, instance);
        
        // Return the new implementation.
        return impl;
    }

    Member *Function::InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        return InstanceGeneric(factory, implementingModule, instance);
    }

    const FunctionType *Function::GetKernelBinderType()
    {
        return kernelBinderType;
    }

    llvm::Function *Function::GetKernelBinder()
    {
        return kernelBinder;
    }

    void Function::DeclareKernelSupport()
    {
        if(!IsKernel())
            return;

        // Kernel support requires reflection.
        Module *module = GetModule();
        if(!module->HasReflection())
            Error("Reflection is required for kernel functions.");

        // This is only required by kernel entry points.
        // A kernel entry points is a function with void return type
        // at least one stream input and at least one stream output.
        // Non stream reference types aren't allowed.
        if(!functionType->GetReturnType()->IsVoid())
            return;

        // Check the arguments for at least one stream input and one stream output.
        bool streamInput = false;
        bool streamOutput = false;
        for(size_t i = 0; i < functionType->GetArgumentCount(); ++i)
        {
            // Only checking stream references.
            const ChelaType *argType = functionType->GetArgument(i);
            if(!argType->IsReference())
                continue;

            // Ignore no stream and not array reference.
            const ReferenceType *refType = static_cast<const ReferenceType*> (argType);
            if(!refType->IsStreamReference())
            {
                argType = refType->GetReferencedType();
                if(argType->IsArray())
                    continue;

                // Not array reference aren't allowed in kernel entry points.
                return;
            }

            // Check if this is an input stream.
            if(refType->IsOutReference())
                streamOutput = true;
            else
                streamInput = true;

            // Don't stop the loop to check for the non-stream references.
        }

        // Don't generate if there's not input stream or output stream.
        if(!streamInput || !streamOutput)
            return;

        // For streams, use an StreamHolder instance.
        VirtualMachine *vm = module->GetVirtualMachine();
        Structure *streamHolder = vm->GetStreamHolderClass();
        Structure *streamHolder1D = vm->GetStreamHolder1DClass();
        Structure *streamHolder2D = vm->GetStreamHolder2DClass();
        Structure *streamHolder3D = vm->GetStreamHolder3DClass();
        Structure *uniformHolder = vm->GetUniformHolderClass();

        // Create the stream binder and closure type.
        std::vector<const ChelaType*> closureTypes;
        for(size_t i = 0; i < functionType->GetArgumentCount(); ++i)
        {
            // Store non stream references verbatim.
            Structure *holder = NULL;
            const ChelaType *argType = functionType->GetArgument(i);
            if(!argType->IsReference())
            {
                holder = uniformHolder;
            }
            else
            {
                // De-reference the argument type.
                const ReferenceType *refType = static_cast<const ReferenceType*> (argType);
                argType = refType->GetReferencedType();

                // Parse array types.
                int dimensions = 0;
                if(argType->IsArray())
                {
                    const ArrayType *arrayType = static_cast<const ArrayType*> (argType);
                    dimensions = arrayType->GetDimensions();
                    argType = arrayType->GetValueType();
                }
                else
                {
                    // TODO: read the dimensions from the reference type.
                }

                // Select the correct holder.
                switch(dimensions)
                {
                case 0:
                    holder = streamHolder;
                    break;
                case 1:
                    holder = streamHolder1D;
                    break;
                case 2:
                    holder = streamHolder2D;
                    break;
                case 3:
                    holder = streamHolder3D;
                    break;
                default:
                    throw ModuleException("Unsupported arrays with more than 3 dimensions in kernels.");
                }
            }

            // Instance the argument holder.
            GenericInstance *instance = GenericInstance::Create(module, holder, &argType, 1);
            Structure *holderInstance = holder->InstanceGeneric(module, instance);
            holderInstance->DeclarePass();
            if(holderInstance->IsPassedByReference())
                closureTypes.push_back(ReferenceType::Create(holderInstance));
            else
                closureTypes.push_back(holderInstance);
        }

        // Find the binding type.
        Function *computeBind = vm->GetRuntimeFunction("__chela_compute_cpu_bind__", true);
        const FunctionType *computeBindFunctionType = computeBind->GetFunctionType();
        const ChelaType *bindingType = computeBindFunctionType->GetReturnType();

        // Create the kernel binder type.
        kernelBinderType = FunctionType::Create(bindingType, closureTypes);

        // Create the kernel closure.
        kernelClosure = Closure::Create(module, NULL, closureTypes);
        kernelClosure->UpdateParent(this);
        kernelClosure->DeclarePass();

        // Create the kernel thread type.
        std::vector<const ChelaType*> kernelThreadArgs;
        kernelThreadArgs.push_back(ReferenceType::Create(kernelClosure));
        kernelThreadArgs.push_back(ChelaType::GetIntType(vm));
        kernelThreadArgs.push_back(ChelaType::GetIntType(vm));
        kernelThreadType = FunctionType::Create(ChelaType::GetVoidType(vm), kernelThreadArgs);

        // Compute the kernel support function linkage.
        llvm::GlobalValue::LinkageTypes linkage = ComputeLinkage();

        // Declare the kernel binder function.
        llvm::Module *targetModule = module->GetTargetModule();
        kernelBinder = llvm::Function::Create(kernelBinderType->GetTargetFunctionType(), linkage,
            GetMangledName() + "_binder", targetModule);

        // Declare the kernel thread function.
        kernelThread = llvm::Function::Create(kernelThreadType->GetTargetFunctionType(), linkage,
            GetMangledName() + "_thread", targetModule);
    }

    void Function::CreateKernelSupport()
    {
        if(!IsKernel() || !kernelBinderType)
            return;

        // Define the kernel closure.
        kernelClosure->DefinitionPass();

        // Define the kernel binder.
        CreateKernelBinder();

        // Define the kernel thread.
        CreateKernelThread();
    }

    void Function::CreateKernelBinder()
    {
        // Get the target module.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::LLVMContext &ctx = targetModule->getContext();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();

        // Create the binder basic blocks.
        llvm::BasicBlock *topBlock = llvm::BasicBlock::Create(ctx, "top", kernelBinder);
        llvm::BasicBlock *cpuBlock = llvm::BasicBlock::Create(ctx, "cpu", kernelBinder);
        llvm::BasicBlock *notCpuBlock = llvm::BasicBlock::Create(ctx, "notcpu", kernelBinder);

        // Create null return value.
        llvm::Constant *addRef = vm->GetAddRef(module);
        llvm::PointerType *int8PtrTy = builder.getInt8PtrTy();
        llvm::Type *sizeTy = ChelaType::GetSizeType(vm)->GetTargetType();

        // Bind for local cpu driver.
        builder.SetInsertPoint(cpuBlock);

        // Allocate the closure.
        const ReferenceType *closureRef = ReferenceType::Create(kernelClosure);
        llvm::Value *closureSize = llvm::ConstantExpr::getSizeOf(kernelClosure->GetTargetType());
        closureSize = builder.CreateIntCast(closureSize, sizeTy, false);
        llvm::Value *closureTypeInfo = builder.CreatePointerCast(kernelClosure->GetTypeInfo(module), int8PtrTy);
        llvm::Value *closureI8 = builder.CreateCall2(vm->GetManagedAlloc(module), closureSize, closureTypeInfo);
        llvm::Value *closure = builder.CreatePointerCast(closureI8, closureRef->GetTargetType());

        // Store the parameters in the closure.
        std::vector<llvm::Value*> streamArgs;
        size_t numparams = kernelBinderType->GetArgumentCount();
        llvm::Function::arg_iterator arg = kernelBinder->arg_begin();
        for(size_t i = 0; i < numparams; ++i, ++arg)
        {
            // Get the argument type.
            const ChelaType *argType = kernelBinderType->GetArgument(i);
            const ChelaType *rawArgType = functionType->GetArgument(i);

            // Check if the argument is a stream.
            bool isStreamArg = false;
            if(rawArgType->IsReference())
            {
                const ReferenceType *rawRefArg = static_cast<const ReferenceType*> (rawArgType);
                isStreamArg = rawRefArg->IsStreamReference();
            }

            // Get the closure field pointer.
            Field *closureField = kernelClosure->GetLocal(i);
            llvm::Value *closureFieldPtr = builder.CreateStructGEP(closure, closureField->GetStructureIndex());

            // Reference arguments are counted.
            if(argType->IsReference())
            {
                // Increase the reference count.
                llvm::Value *refPtr = builder.CreatePointerCast(arg, int8PtrTy);
                builder.CreateCall(addRef, refPtr);

                // Store stream references.
                if(isStreamArg)
                    streamArgs.push_back(refPtr);
            }

            // Store the argument in the closure.
            builder.CreateStore(arg, closureFieldPtr);
        }

        // Allocate space for the stream array.
        builder.SetInsertPoint(topBlock);
        llvm::Value *streamArray = builder.CreateAlloca(int8PtrTy, builder.getInt32(streamArgs.size()));

        // Store the streams in a array.
        builder.SetInsertPoint(cpuBlock);
        for(size_t i = 0; i < streamArgs.size(); ++i)
        {
            llvm::Value *arraySlot = builder.CreateConstInBoundsGEP1_32(streamArray, i);
            builder.CreateStore(streamArgs[i], arraySlot);
        }

        // Bind the cpu kernel.
        llvm::Value *functionPtr = builder.CreatePointerCast(kernelThread, int8PtrTy);
        llvm::Value *streamCount = builder.getInt32(streamArgs.size());
        llvm::Value *binding = builder.CreateCall4(vm->GetComputeCpuBind(module),
                            closureI8, functionPtr, streamArray, streamCount);

        builder.CreateRet(binding);

        // Bind for not local cpu driver.
        builder.SetInsertPoint(notCpuBlock);

        // Store all of the parameters in an array.
        llvm::Value *paramArray = builder.CreateAlloca(int8PtrTy, builder.getInt32(numparams));
        arg = kernelBinder->arg_begin();
        for(size_t i = 0; i < numparams; ++i, ++arg)
        {
            // Get the argument type.
            const ChelaType *argType = kernelBinderType->GetArgument(i);
            if(!argType->IsReference())
                throw ModuleException("Expected reference argument.");

            // Cast the argument
            llvm::Value *castedArg = builder.CreatePointerCast(arg, int8PtrTy);

            // Store the argument in the param array.
            llvm::Value *paramSlot = builder.CreateConstInBoundsGEP1_32(paramArray, i);
            builder.CreateStore(castedArg, paramSlot);
        }

        // Invoke the non-local cpu kernel binder.
        llvm::Value *methodInfo = builder.CreatePointerCast(GetMemberInfo(), int8PtrTy);
        llvm::Value *numparamsValue = builder.getInt32(numparams);
        binding = builder.CreateCall3(vm->GetComputeBind(module), methodInfo, numparamsValue, paramArray);
        builder.CreateRet(binding);

        // Check for cpu driver.
        builder.SetInsertPoint(topBlock);
        llvm::Value *isCpu = builder.CreateCall(vm->GetComputeIsCpu(module));
        builder.CreateCondBr(isCpu, cpuBlock, notCpuBlock);

        // Verify the function.
        llvm::verifyFunction(*kernelBinder);

        // Optimize the kernel binder.
        module->GetFunctionPassManager()->run(*kernelBinder);
    }

    void Function::CreateKernelThread()
    {
        // Get the target module.
        Module *module = GetModule();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::LLVMContext &ctx = targetModule->getContext();
        llvm::IRBuilder<> &builder = GetModule()->GetIRBuilder();

        // Get the thread arguments.
        llvm::Function::arg_iterator arg = kernelThread->arg_begin();
        llvm::Value *closure = arg++;
        llvm::Value *start = arg++;
        llvm::Value *end = arg++;

        // Create the kernel thread basic blocks.
        llvm::BasicBlock *topBlock = llvm::BasicBlock::Create(ctx, "top", kernelThread);
        llvm::BasicBlock *forCond = llvm::BasicBlock::Create(ctx, "cond", kernelThread);
        llvm::BasicBlock *forContent = llvm::BasicBlock::Create(ctx, "content", kernelThread);
        llvm::BasicBlock *endBlock = llvm::BasicBlock::Create(ctx, "end", kernelThread);

        // Declare and intialize the index variable
        builder.SetInsertPoint(topBlock);
        llvm::Value *indexVar = builder.CreateAlloca(builder.getInt32Ty());
        builder.CreateStore(start, indexVar);

        // Read the closure locals.
        std::vector<llvm::Value*> closureLocals;
        closureLocals.reserve(kernelClosure->GetLocalCount());
        for(size_t i = 0; i < kernelClosure->GetLocalCount(); ++i)
        {
            // Get the local field pointer.
            Field *localField = kernelClosure->GetLocal(i);
            llvm::Value *localPtr = builder.CreateStructGEP(closure, localField->GetStructureIndex());

            // Read the local.
            llvm::Value *readedLocal = builder.CreateLoad(localPtr);

            // Read the uniforms from the holder.
            const ChelaType *localActualType = functionType->GetArgument(i);
            bool isStreamLocal = false;
            if(localActualType->IsReference())
            {
                const ReferenceType *refType = static_cast<const ReferenceType*> (localActualType);
                isStreamLocal = refType->IsStreamReference();
            }

            // Push stream references verbatim.
            if(isStreamLocal)
            {
                closureLocals.push_back(readedLocal);
                continue;
            }

            // Extract the local from the holder.
            const ChelaType *argType = kernelBinderType->GetArgument(i);
            if(!argType->IsReference())
            {
                // Wasn't wrapped.
                closureLocals.push_back(readedLocal);
                continue;
            }

            // Make sure is a structure/class.
            argType = DeReferenceType(argType);
            if(!argType->IsStructure() && !argType->IsClass())
                throw ModuleException("expected a structure/class for a compute resource holder.");

            // Use the storage or value property.
            const Structure *holder = static_cast<const Structure*> (argType);
            Property *storage = holder->GetProperty("Storage");
            if(!storage)
                storage = holder->GetProperty("Value");
            if(!storage)
                throw ModuleException("couldn't find storage or value property in resource holder " + holder->GetFullName());

            // Get and check the storage getter.
            Function *storageGetter = storage->GetGetAccessor();
            if(!storageGetter || storageGetter->IsAbstract())
                throw ModuleException("unacceptable storage getter in holder " + holder->GetFullName());

            // Get the actual variable.
            if(localActualType->IsComplexStructure())
            {
                // Allocate storage for the local.
                llvm::Value *buildingStorage = builder.CreateAlloca(localActualType->GetTargetType());

                // Get the structure.
                builder.CreateCall2(storageGetter->GetTarget(), buildingStorage, readedLocal);

                // Load the structure.
                readedLocal = builder.CreateLoad(buildingStorage);
            }
            else
            {
                // Just call the getter.
                readedLocal = builder.CreateCall(storageGetter->GetTarget(), readedLocal);
            }

            // Push the local.
            closureLocals.push_back(readedLocal);
        }

        // Enter into the loop.
        builder.CreateBr(forCond);

        // Check if the index is still in range.
        builder.SetInsertPoint(forCond);
        llvm::Value *index = builder.CreateLoad(indexVar);
        llvm::Value *cont = builder.CreateICmpULT(index, end);
        builder.CreateCondBr(cont, forContent, endBlock);

        // Evaluate the stream position.
        builder.SetInsertPoint(forContent);
        index = builder.CreateLoad(indexVar);

        // Read the iteration parameters.
        std::vector<llvm::Value*> iterationParams;
        iterationParams.reserve(closureLocals.size());
        for(size_t i = 0; i < kernelBinderType->GetArgumentCount(); ++i)
        {
            // Get the argument type.
            const ChelaType *rawArgType = functionType->GetArgument(i);
            const ChelaType *argType = kernelBinderType->GetArgument(i);

            // Is this a stream argument?
            bool isStream = false;
            if(rawArgType->IsReference())
            {
                const ReferenceType *rawRefType = static_cast<const ReferenceType*> (rawArgType);
                isStream = rawRefType->IsStreamReference();
            }

            // No stream values are passed verbatim.
            if(!isStream || !argType->IsReference())
            {
                iterationParams.push_back(closureLocals[i]);
                continue;
            }

            // De-Reference the type.
            argType = DeReferenceType(argType);

            // The type must be a class.
            if(!argType->IsClass())
                throw ModuleException("Expected a compute resource holder instance.");
            Structure *building = (Structure*)argType;

            // Make sure the stream holder is defined.
            building->DefinitionPass();

            // Get the element pointer function.
            FunctionGroup *elementPointerGroup = building->GetFunctionGroup("GetElementPtr");
            Function *elementPointerFunction = NULL;
            for(size_t j = 0; j < elementPointerGroup->GetFunctionCount(); ++j)
            {
                Function *function = elementPointerGroup->GetFunction(j);
                if(function && !function->IsStatic())
                {
                    elementPointerFunction = function;
                    break;
                }
            }

            // Make sure the element pointer function was found.
            if(!elementPointerFunction)
                throw ModuleException("Couldn't find GetElementPtr in stream holder " + building->GetFullName());

            // Avoid virtual calls.
            llvm::Value *stream = closureLocals[i];
            if(building->IsSealed())
            {
                // Get the element pointer.
                llvm::Constant *elementPtrFun = elementPointerFunction->GetTarget();
                llvm::Value *elementPtr = builder.CreateCall2(elementPtrFun, stream, index);
                iterationParams.push_back(elementPtr);
            }
            else
            {
                // Get the stream vtable.
                llvm::Value *streamVtable = builder.CreateStructGEP(stream, 0);
                streamVtable = builder.CreateLoad(streamVtable);

                // Get the element pointer function.
                llvm::Value *elementPtrFun = builder.CreateStructGEP(streamVtable, elementPointerFunction->GetVSlot() + 2);
                elementPtrFun = builder.CreateLoad(elementPtrFun);

                // Get the element pointer.
                llvm::Value *elementPtr = builder.CreateCall2(elementPtrFun, stream, index);
                iterationParams.push_back(elementPtr);
            }
        }

        // Perform the thread iteration.
        builder.CreateCall(targetFunction, iterationParams);

        // Increase the index, and check the condition again.
        index = builder.CreateAdd(index, builder.getInt32(1));
        builder.CreateStore(index, indexVar);
        builder.CreateBr(forCond);

        // End the thread.
        builder.SetInsertPoint(endBlock);
        builder.CreateRetVoid();

        // Verify the thread function.
        llvm::verifyFunction(*kernelThread);

        // Optimize it.
        module->GetFunctionPassManager()->run(*kernelThread);
    }
};
