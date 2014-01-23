#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionArgument.hpp"
#include "ChelaVm/FunctionBlock.hpp"
#include "ChelaVm/FunctionLocal.hpp"
#include "ChelaVm/FunctionGroup.hpp"
#include "ChelaVm/FunctionInstance.hpp"
#include "ChelaVm/Field.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/InstructionReader.hpp"
#include "ChelaVm/MemberInstance.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/TypeInstance.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "llvm/Intrinsics.h"

namespace ChelaVm
{
    FunctionBlock::FunctionBlock(Function *parent, size_t blockId)
        : parent(parent), module(parent->GetModule()),
          declaringModule(parent->GetDeclaringModule()), blockId(blockId)
    {
        blockDataOffset = 0;
        rawblockSize = 0;
        instructionCount = 0;
        instructions = NULL;
        generated = false;
        finishCleanup = false;
        returnValue = NULL;
        exceptionContext = NULL;
        isCatchBlock = false;
        exceptionType = 0;
        blockDebugInfo = NULL;
        currentDebugRange = NULL;
        unsafeBlock = true;
    }
    
    FunctionBlock::~FunctionBlock()
    {
    }
        
    void FunctionBlock::Read(ModuleReader &reader)
    {
        // Read the block metadata.
        uint8_t unsafeFlag;
        reader >> rawblockSize >> instructionCount >> unsafeFlag;
        unsafeBlock = unsafeFlag;

        // Use the raw block data.
        blockDataOffset = reader.GetPosition();
        instructions = parent->GetDeclaringModule()->GetModuleData() + blockDataOffset;

        // Skip the block data.
        reader.Skip(rawblockSize);
    }

    size_t FunctionBlock::GetBlockId() const
    {
        return blockId;
    }

    size_t FunctionBlock::GetBlockOffset() const
    {
        return blockDataOffset;
    }

    size_t FunctionBlock::GetRawBlockSize() const
    {
        return rawblockSize;
    }

    const uint8_t *FunctionBlock::GetInstructions()
    {
        return instructions;
    }

    bool FunctionBlock::IsUnsafe() const
    {
        return unsafeBlock;
    }

    llvm::BasicBlock *FunctionBlock::GetBlock()
    {
        if(basicBlocks.empty())
        {
            char message[128];
            sprintf(message, "Using undeclared block in %d", (int)blockId);
            Error(message);
        }

        return basicBlocks.front();
    }

    llvm::BasicBlock *FunctionBlock::GetLastBlock()
    {
        return basicBlocks.back();
    }

    llvm::BasicBlock *FunctionBlock::CreateBlock(const std::string &name)
    {
        llvm::BasicBlock *ret = llvm::BasicBlock::Create(GetLlvmContext(), name, parent->GetTarget());
        basicBlocks.push_back(ret);
        return ret;
    }

    const ChelaType *FunctionBlock::GetType(uint32_t typeId)
    {
        // Get the type.
        const ChelaType *type = declaringModule->GetType(typeId);

        // Perform generic instances.
        if(type->IsGenericType())
            type = type->InstanceGeneric(parent->GetCompleteGenericInstance());

        return type;
    }

    const ChelaType *FunctionBlock::InstanceType(const ChelaType *type)
    {
        // Perform generic instances.
        if(type->IsGenericType())
            type = type->InstanceGeneric(parent->GetCompleteGenericInstance());

        return type;
    }

    Member *FunctionBlock::GetMember(uint32_t memberId)
    {
        // Get the member.
        Member *member = declaringModule->GetMember(memberId);
        if(!member)
        {
            printf("invalid member id %d, in [%lu]%s\n", memberId, (unsigned long)blockId, parent->GetFullName().c_str());
            throw ModuleException("expected member");
        }

        // Get the actual member.
        if(member->IsMemberInstance())
        {
            MemberInstance *instance = static_cast<MemberInstance*> (member);
            member = instance->GetActualMember();
            if(!member)
                member = instance->InstanceMember(NULL, module, parent->GetCompleteGenericInstance());
            if(!member)
                throw ModuleException("invalid member instance for " +
                    instance->GetTemplateMember()->GetFullName());
        }

        return member;
    }

    Function *FunctionBlock::GetFunction(uint32_t functionId)
    {
        // Get the member.
        Member *member = GetMember(functionId);

        // Handle generic functions.
        if(member->IsFunctionInstance())
        {
            FunctionInstance *instance = static_cast<FunctionInstance*> (member);
            member = instance->GetImplementation();
        }

        // Instance the function.
        if(member->IsGeneric())
        {
            member = member->InstanceMember(module, parent->GetCompleteGenericInstance());
            member->DeclarePass();
        }

        // Make sure its a function.
        if(!member->IsFunction())
            throw ModuleException("Expected function id.");

        return static_cast<Function*> (member);
    }

    Field *FunctionBlock::GetField(uint32_t fieldId)
    {
        // Get the member.
        Member *member = GetMember(fieldId);

        // Instance the function.
        if(member->IsGeneric())
            member = member->InstanceMember(module, parent->GetCompleteGenericInstance());

        // Make sure its a field.
        if(!member->IsField())
            throw ModuleException("Expected field id.");

        return static_cast<Field*> (member);
    }

    ExceptionContext *FunctionBlock::GetExceptionContext() const
    {
        return exceptionContext;
    }

    void FunctionBlock::SetExceptionContext(ExceptionContext *context)
    {
        exceptionContext = context;
    }

    bool FunctionBlock::IsCatchBlock() const
    {
        return isCatchBlock;
    }

    void FunctionBlock::SetCatchBlock(bool isCatch)
    {
        isCatchBlock = isCatch;
    }

    uint32_t FunctionBlock::GetExceptionType() const
    {
        return exceptionType;
    }

    void FunctionBlock::SetExceptionType(uint32_t exceptionType)
    {
        this->exceptionType = exceptionType;
    }

    void FunctionBlock::Declare(llvm::IRBuilder<> &builder)
    {
        // Create the first basic block.
        CreateBlock("bb");
    }

    void FunctionBlock::Error(const std::string &message)
    {
        parent->Error(message);
    }

    void FunctionBlock::Unsafe(int op)
    {
        if(!unsafeBlock && !parent->IsUnsafe())
        {
            std::string error = "Unsafe instruction of kind '";
            error += InstructionTable[op].mnemonic;
            error += "' in safe block";
            Error(error);
        }
    }

    void FunctionBlock::CheckDelegateCompatible(Function *invokeFunction, Function *delegatedFunction)
    {
        // Get the function types.
        const FunctionType *invokeType = invokeFunction->GetFunctionType();
        const FunctionType *delegatedType = delegatedFunction->GetFunctionType();

        // Check the return value.
        CheckCompatible(delegatedType->GetReturnType(), invokeType->GetReturnType());

        // Check the arguments.
        int startIndex = delegatedFunction->IsStatic() ? 0 : 1;
        if(invokeType->GetArgumentCount() - 1  !=
           delegatedType->GetArgumentCount() - startIndex)
           Error("Incompatible delegate - invoked argument count.");

        int numArgs = invokeType->GetArgumentCount() - 1;
        for(int i = 0; i < numArgs; ++i)
            CheckCompatible(invokeType->GetArgument(i+1), delegatedType->GetArgument(i + startIndex));
    }

    void FunctionBlock::CheckCompatible(const ChelaType *source, const ChelaType *dest)
    {
        if(source == dest)
            return;

        // Are they derivated.?
        if(source->IsReference() && dest->IsReference())
        {
            const ChelaType *sourceReferenced = DeReferenceType(source);
            const ChelaType *destReferenced = DeReferenceType(dest);
            if(sourceReferenced->IsClass() && destReferenced->IsClass())
            {
                const Class *sourceClass = static_cast<const Class*> (sourceReferenced);
                const Class *destClass = static_cast<const Class*> (destReferenced);
                if(sourceClass == destClass || sourceClass->IsDerivedFrom(destClass))
                    return;
            }
        }

        Error("Incompatibles types " + source->GetName() + " -> "+ dest->GetName());
    }

    inline void FunctionBlock::CheckEqualType(int op, const ChelaType *a, const ChelaType *b)
    {
        if(a != b)
        {
            std::string message = "Expected the same types in '";
            if(op < OpCode::Invalid)
                message += InstructionTable[op].mnemonic;
            else
                message += "invalid";
            message += "'";
            
            Error(message);
        }
    }
    
    inline void FunctionBlock::CheckCompatibleType(int op, const ChelaType *a, const ChelaType *b)
    {
        if(a == b)
            return;

        if(a == ChelaType::GetNullType(GetVM()) && (b->IsReference() || b->IsPointer()))
            return;

        if(b == ChelaType::GetNullType(GetVM()) && (a->IsReference() || a->IsPointer()))
            return;
        
        std::string message = "Expected the compatibles types in '";
        if(op < OpCode::Invalid)
            message += InstructionTable[op].mnemonic;
        else
            message += "invalid";
        message += "'";
            
        Error(message);
    }    
    
    inline llvm::Value *FunctionBlock::MakeCompatible(llvm::IRBuilder<> &builder, llvm::Value *value, const ChelaType *source, const ChelaType *dest)
    {
        VirtualMachine *vm = module->GetVirtualMachine();

        if(source == dest)
            return value;

        if(source->IsInteger() && dest->IsInteger() &&
           source->GetSize() == dest->GetSize() &&
           source != ChelaType::GetBoolType(vm) &&
           dest != ChelaType::GetBoolType(vm))
            return value;

        if(source == ChelaType::GetNullType(vm) && (dest->IsReference() || dest->IsPointer()))
        {
            return AttachPosition(builder.CreatePointerCast(value, dest->GetTargetType(), "nullcast"));
        }
        
        if(source->IsReference() && dest->IsReference())
        {
            const ChelaType *rsource = DeReferenceType(source);
            const ChelaType *rdest = DeReferenceType(dest);
            if(rsource == rdest)
                return value;

            // Use the array class when appropiate.
            if(!rdest->IsArray() && rsource->IsArray())
            {
                rsource = vm->GetArrayClass();
                const ChelaType *arrayRefType = ReferenceType::Create(rsource);
                value = AttachPosition(builder.CreatePointerCast(value, arrayRefType->GetTargetType()));
            }

            if(rsource == rdest)
                return value;

            bool isBoxed = false;
            if(rsource->IsBoxed())
            {
                isBoxed = true;
                const BoxedType *boxed = static_cast<const BoxedType *> (rsource);
                rsource = boxed->GetValueType();
            }

            if(rdest->IsInterface() && (rsource->IsClass() ||
                (rsource->IsStructure() && isBoxed)|| rsource->IsInterface()))
            {
                const Structure *sbuilding = static_cast<const Structure*> (rsource);
                const Structure *diface = static_cast<const Structure*> (rdest);
                if(sbuilding->Implements(diface))
                {
                    // Adjust the pointer to the itable.
                    int itableIndex = sbuilding->GetITableIndex(diface);
                    if(itableIndex < 0)
                        Error("Invalid itable index.");

                    // Get the itable and cast it.
                    llvm::Value *itable = builder.CreateConstGEP2_32(value, 0, itableIndex);
                    return AttachPosition(builder.CreatePointerCast(itable, dest->GetTargetType(), "ifcast"));
                }
            }
            else if((rsource->IsClass() || isBoxed) && rdest->IsClass())
            {
                // Single inheritance uses only a pointer cast.
                const Structure *sbuilding = static_cast<const Structure*> (rsource);
                const Structure *dbuilding = static_cast<const Structure*> (rdest);
                if(sbuilding->IsDerivedFrom(dbuilding))
                    return AttachPosition(builder.CreatePointerCast(value, dest->GetTargetType(), "icast"));
            }
            else if(rsource->IsFirstClass() && rdest->IsStructure())
            {
                // Check for associated types.
                if(rdest == vm->GetAssociatedClass(rsource))
                    return AttachPosition(builder.CreatePointerCast(value, dest->GetTargetType(), "bcast"));
            }
            else if(rdest->IsFirstClass() && rsource->IsStructure())
            {
                // Check for associated types.
                if(rsource == vm->GetAssociatedClass(rdest))
                    return AttachPosition(builder.CreatePointerCast(value, dest->GetTargetType(), "bcast"));
            }
        }
        else if(source->IsPointer() && dest->IsReference())
        {
            const ChelaType *psource = DePointerType(source);
            const ChelaType *pdest = DeReferenceType(dest);
            if(psource == pdest && psource->IsStructure())
                return value;
        }
        else if(source->IsPointer() && dest->IsPointer())
        {
            const ChelaType *psource = DePointerType(source);
            const ChelaType *pdest = DePointerType(dest);
            if(psource->IsStructure() && pdest->IsStructure())
            {
                const Structure *sbuilding = static_cast<const Structure*> (psource);
                const Structure *dbuilding = static_cast<const Structure*> (pdest);
                if(sbuilding->IsDerivedFrom(dbuilding))
                    return AttachPosition(builder.CreatePointerCast(value, dest->GetTargetType(), "nullcast"));
            }
        }
        else if(source->IsReference() && (dest->IsStructure() || dest->IsFirstClass()))
        {
            const ChelaType *rsource = DeReferenceType(source);
            if(rsource == dest)
                return AttachPosition(builder.CreateLoad(value));
        }
        else if(dest->IsReference() && (source->IsStructure() || source->IsFirstClass()))
        {
            // Implicit primitive boxing.
            const ChelaType *destStructure = DeReferenceType(dest);
            const ChelaType *sourceStructure = source;
            if(source->IsFirstClass())
                sourceStructure = vm->GetAssociatedClass(source);

            // Only create the temporary reference if the type is compatible.
            if(destStructure == sourceStructure && destStructure->IsStructure())
            {
                llvm::Value *result = parent->CreateIntermediate(source, source->HasSubReferences());
                RefSetInter(builder, value, result, source);
                return AttachPosition(builder.CreatePointerCast(result, dest->GetTargetType()));
            }
        }
        
        Error("Incompatible types " + source->GetFullName() + "->" + dest->GetFullName());
        return NULL;
    }

    inline llvm::Value *FunctionBlock::ExtractPrimitive(llvm::IRBuilder<> &builder, llvm::Value *value, Field *field)
    {
        // Make sure we are using a pointer to the structure.
        llvm::Type *valueType = value->getType();
        if(!valueType->isPointerTy())
            return AttachPosition(builder.CreateExtractValue(value, field->GetStructureIndex()));

        // Get the field pointer.
        llvm::Value *fieldPtr = builder.CreateStructGEP(value, field->GetStructureIndex());
        return AttachPosition(builder.CreateLoad(fieldPtr));
    }

    inline llvm::Value *FunctionBlock::DoCompare(llvm::IRBuilder<> &builder, int opcode, const FunctionBlock::StackValue &a1, const FunctionBlock::StackValue &a2)
    {
        VirtualMachine *vm = GetVM();

        // Rename the values.
        llvm::Value *left = a1.value;
        llvm::Value *right = a2.value;

        // Find a common type.
        const ChelaType *commonType;
        if(a1.type == a2.type)
            commonType = a1.type;
        else if(a1.type == ChelaType::GetNullType(vm))
        {
            commonType = a2.type;
            if(a2.type != ChelaType::GetNullType(vm))
                left = AttachPosition(builder.CreatePointerCast(left, commonType->GetTargetType(), "nullcast"));
        }
        else if(a2.type == ChelaType::GetNullType(vm))
        {
            commonType = a1.type;
            right = AttachPosition(builder.CreatePointerCast(right, commonType->GetTargetType(), "nullcast"));
        }
        else if((a1.type->IsReference() && a2.type->IsStructure()) ||
                (a1.type->IsStructure() && a2.type->IsReference()))
        {
            const ChelaType *structType = NULL;
            const ChelaType *refType = NULL;
            if(a1.type->IsReference())
            {
                refType = a1.type;
                structType = a2.type;
            }
            else
            {
                refType = a2.type;
                structType = a1.type;
            }

            const ChelaType *referencedType = DeReferenceType(refType);
            if(referencedType != structType)
                Error("Cannot compare " + structType->GetName() + " " + referencedType->GetName());

            commonType = structType;
        }
        else
        {
            Error("Unimplemented comparison " + a1.type->GetName() + " " + a2.type->GetName());
            return NULL;
        }

        if(commonType->IsStructure())
        {
            // Only compare like this some structures.
            const Structure *building = static_cast<const Structure*> (commonType);
            Field *valueField = building->GetField("__value");
            if(!valueField)
            {
                valueField = building->GetField("m_value");
                if(!valueField)
                    Error("Cannot compare structure " + building->GetName() + "using CmpEQ because it doesn't have m_value or __value member.");
            }

            // Extract the primitive.
            left = ExtractPrimitive(builder, left, valueField);
            right = ExtractPrimitive(builder, right, valueField);
        }

        bool fp = commonType->IsFloatingPoint();
        bool sign = commonType->IsInteger() && !commonType->IsUnsigned();
        switch(opcode)
        {
        case OpCode::CmpEQ:
            if(fp)
                return AttachPosition(builder.CreateFCmpOEQ(left, right));
            else
                return AttachPosition(builder.CreateICmpEQ(left, right));
        case OpCode::CmpNE:
            if(fp)
                return AttachPosition(builder.CreateFCmpONE(left, right));
            else
                return AttachPosition(builder.CreateICmpNE(left, right));
        case OpCode::CmpLT:
            if(fp)
                return AttachPosition(builder.CreateFCmpOLT(left, right));
            else if(sign)
                return AttachPosition(builder.CreateICmpSLT(left, right));
            else
                return AttachPosition(builder.CreateICmpULT(left, right));
        case OpCode::CmpLE:
            if(fp)
                return AttachPosition(builder.CreateFCmpOLE(left, right));
            else if(sign)
                return AttachPosition(builder.CreateICmpSLE(left, right));
            else
                return AttachPosition(builder.CreateICmpULE(left, right));
        case OpCode::CmpGT:
            if(fp)
                return AttachPosition(builder.CreateFCmpOGT(left, right));
            else if(sign)
                return AttachPosition(builder.CreateICmpSGT(left, right));
            else
                return AttachPosition(builder.CreateICmpUGT(left, right));
        case OpCode::CmpGE:
            if(fp)
                return AttachPosition(builder.CreateFCmpOGE(left, right));
            else if(sign)
                return AttachPosition(builder.CreateICmpSGE(left, right));
            else
                return AttachPosition(builder.CreateICmpUGE(left, right));
        default:
            Error("Unexpected comparison.");
            return NULL;
        }
    }

    inline llvm::Constant *ImportFunction(Module *module, llvm::Value *value)
    {
        // Cast the function.
        llvm::Function *original = llvm::dyn_cast<llvm::Function> (value);
        if(original == NULL)
            return NULL;

        // Avoid unnecessary importing.
        llvm::Module *targetMod = module->GetTargetModule();
        if(original->getParent() == targetMod)
            return original;

        // Import the function.
        return targetMod->getOrInsertFunction(original->getName(),
                       original->getFunctionType(), original->getAttributes());
    }

    inline llvm::Value *TryImportFunction(Module *module, llvm::Value *value)
    {
        // Cast the function.
        llvm::Function *original = llvm::dyn_cast<llvm::Function> (value);
        if(original == NULL)
            return value;

        // Avoid unnecessary importing.
        llvm::Module *targetMod = module->GetTargetModule();
        if(original->getParent() == targetMod)
            return original;

        // Import the function.
        return targetMod->getOrInsertFunction(original->getName(),
                       original->getFunctionType(), original->getAttributes());
    }

    const ChelaType *FunctionBlock::ParseIntTarget(int n)
    {
        VirtualMachine *vm = GetVM();
        switch(n)
        {
        case -1: return ChelaType::GetSByteType(vm);
        case  1: return ChelaType::GetByteType(vm);
        case -2: return ChelaType::GetShortType(vm);
        case  2: return ChelaType::GetUShortType(vm);
        case -4: return ChelaType::GetIntType(vm);
        case  4: return ChelaType::GetUIntType(vm);
        case -8: return ChelaType::GetLongType(vm);
        case  8: return ChelaType::GetULongType(vm);
        case 100: return ChelaType::GetSizeType(vm);
        default:
            throw ModuleException("Unsupported int target.");
        }
    }
    
    const ChelaType *FunctionBlock::ParseFloatTarget(int n)
    {
        VirtualMachine *vm = GetVM();
        switch(n)
        {
        case 4: return ChelaType::GetFloatType(vm);
        case 8: return ChelaType::GetDoubleType(vm);
        default:
            throw ModuleException("Unsupported float target.");
        }
    }
    
    Field *FunctionBlock::LoadFieldDescription(Function *context, const ChelaType *type, uint32_t fieldId, const Structure *&building)
    {
        // It has to be a reference into a class or struct object.
        if(!type->IsReference() && !type->IsPointer())
            Error("Expected object pointer/reference instead of " + type->GetFullName());

        // De-reference.
        if(type->IsReference())
        {
            const ReferenceType *refType = static_cast<const ReferenceType*> (type);
            const ChelaType *referencedType = refType->GetReferencedType();
            if(!referencedType->IsStructure() && !referencedType->IsClass())
                Error("Expected struct/class instance.");
                    
            // Cast into a structure.
            building = static_cast<const Structure*> (referencedType);
        }
        else
        {
            const PointerType *pointerType = static_cast<const PointerType*> (type);
            const ChelaType *pointedType = pointerType->GetPointedType();
            if(!pointedType->IsStructure() && !pointedType->IsClass())
                Error("Expected struct/class instance.");
                    
            // Cast into a structure.
            building = static_cast<const Structure*> (pointedType);
        }
        
        // Get the field.
        Field *field = GetField(fieldId);

        // Make sure the field is in the building.
        size_t fieldCount = building->GetFieldCount();
        bool found = false;
        for(size_t i = 0; i < fieldCount; ++i)
        {
            Field *testField = building->GetField(i);
            if(testField == field)
            {
                found = true;
                break;
            }
        }

        // If the field wasn't found in the building, raise the error.
        if(!found)
            Error("Trying to access field of other structure.");
        
        // TODO: Check the field access.
        return field;
    }


    void FunctionBlock::LoadArrayIndices(llvm::IRBuilder<> &builder, const ChelaType *rawArrayType, std::vector<llvm::Value*> &indices)
    {
        // Get the number of dimensions.
        int dimensions = 0;
        if(rawArrayType->IsPointer())
        {
            dimensions = 1;
        }
        else if(rawArrayType->IsArray())
        {
            const ArrayType *arrayType = static_cast<const ArrayType*> (rawArrayType);
            dimensions = arrayType->GetDimensions();
        }
        else
        {
            Error("Expected an array type.");
        }

        // Reserve space for the indices.
        indices.resize(dimensions);
        for(int i = 0; i < dimensions; ++i)
        {
            // Read the value from the stack.
            StackValue val = Pop();

            // Make sure its an integer.
            if(!val.type->IsInteger() || !val.type->IsPrimitive())
                Error("Expected integer value.");

            // TODO: support vector indices.
            indices[dimensions - i - 1] = AttachPosition(builder.CreateIntCast(val.value, builder.getInt64Ty(), !val.type->IsUnsigned()));
        }
    }

    llvm::Value *FunctionBlock::GetArrayElement(llvm::IRBuilder<> &builder,
        llvm::Value *arrayRef, const ChelaType *arrayRefType,
        const std::vector<llvm::Value*> &indices,
        const ChelaType **destElementType)
    {
        // Check the array type.
        if(!arrayRefType->IsReference())
            Error("expected array reference.");

        // Make sure thereferenced type is an array.
        const ChelaType *rawArrayType = DeReferenceType(arrayRefType);
        if(!rawArrayType->IsArray())
            Error("expected array reference.");
        const ArrayType *arrayType = static_cast<const ArrayType*> (rawArrayType);

        // Get the element type.
        const ChelaType *elementType = arrayType->GetActualValueType();
        if(destElementType)
            *destElementType = elementType;

        // Get the number of dimensions.
        int dimensions = arrayType->GetDimensions();
        if(dimensions != (int)indices.size())
            Error("Number of indices and array dimensions mismatch.");

        // Get the lengths pointer.
        llvm::Value *lengths = builder.CreateStructGEP(arrayRef, 1);

        // Compute the element weight.
        llvm::Type *sizeType = builder.getInt64Ty();
        llvm::Value *lastWeight = builder.getInt64(1);
        llvm::Value *elementIndex = builder.getInt64(0);
        for(int i = dimensions - 1; i >= 0; --i)
        {
            // Read the index associated index.
            llvm::Value *sizePtr = AttachPosition(builder.CreateConstInBoundsGEP2_32(lengths, 0, i));
            llvm::Value *size = AttachPosition(builder.CreateLoad(sizePtr));
            size = AttachPosition(builder.CreateIntCast(size, sizeType, false));

            // TODO: Perform bound check.

            // Append the index value.
            llvm::Value *indexValue = AttachPosition(builder.CreateMul(lastWeight, indices[i]));
            elementIndex = AttachPosition(builder.CreateAdd(elementIndex, indexValue));

            // Compute the next index weight.
            lastWeight = AttachPosition(builder.CreateMul(lastWeight, size));
        }

        // Get the element pointer.
        llvm::Value *gepArgs[] = {builder.getInt32(0), builder.getInt32(2), elementIndex};
        return builder.CreateGEP(arrayRef, llvm::makeArrayRef(gepArgs, gepArgs + 3));
    }

    void FunctionBlock::AddRef(llvm::IRBuilder<> &builder, llvm::Value *value)
    {
        // Get the add functions.
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Constant *addRef = vm->GetAddRef(module);

        // Get the i8* type and create a (i8*)null.
        llvm::PointerType *int8PtrType = llvm::Type::getInt8PtrTy(GetLlvmContext());

        // Increment value reference.
        value = AttachPosition(builder.CreatePointerCast(value, int8PtrType));
        AttachPosition(builder.CreateCall(addRef, value));
    }

    void FunctionBlock::AddRef(llvm::IRBuilder<> &builder, llvm::Value *value, const ChelaType *type)
    {
        if(type->IsStructure())
        {
            // Increment the references.
            const Structure *building = static_cast<const Structure*> (type);
            for(size_t i = 0; i < building->GetValueRefCount(); ++i)
            {
                // Get the reference field.
                const Field *field = building->GetValueRefField(i);
                const ChelaType *fieldType = field->GetType();

                // Load the field value.
                size_t fieldIndex = field->GetStructureIndex();
                llvm::Value *fieldValue = AttachPosition(builder.CreateExtractValue(value, fieldIndex));

                // Increment structure fields.
                if(fieldType->IsStructure())
                {
                    AddRef(builder, fieldValue, fieldType);
                }
                else if(fieldType->IsReference())
                {
                    // Increment the reference.
                    AddRef(builder, fieldValue);
                }
                else
                {
                    printf("Cannot increment field reference of type %s\n", fieldType->GetFullName().c_str());
                }
            }

            return;
        }
        else if(type->IsReference())
        {
            const ChelaType *refType = DeReferenceType(type);
            if(refType->IsStructure())
            {
                const Structure *building = static_cast<const Structure*> (refType);
                for(size_t i = 0; i < building->GetValueRefCount(); ++i)
                {
                    // Get the reference field.
                    const Field *field = building->GetValueRefField(i);
                    const ChelaType *fieldType = field->GetType();

                    // Get the field pointer.
                    size_t fieldIndex = field->GetStructureIndex();
                    llvm::Value *fieldPtr = AttachPosition(builder.CreateStructGEP(value, fieldIndex));

                    // Increment structure fields.
                    if(fieldType->IsStructure())
                    {
                        AddRef(builder, fieldPtr, ReferenceType::Create(fieldType));
                    }
                    else if(fieldType->IsReference())
                    {
                        // Increment the reference.
                        llvm::Value *fieldValue = AttachPosition(builder.CreateLoad(fieldPtr));
                        AddRef(builder, fieldValue);
                    }
                    else
                    {
                        printf("Cannot increment field reference of type %s\n", fieldType->GetFullName().c_str());
                    }
                }
                return;
            }
            else if(refType->IsPassedByReference())
            {
                // Plain old reference.
                AddRef(builder, value);
                return;
            }
        }

        printf("TODO: Add references of type %s\n", type->GetFullName().c_str());
    }

    void FunctionBlock::Release(llvm::IRBuilder<> &builder, llvm::Value *value)
    {
        // Get the release functions.
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Constant *release = vm->GetRelRef(module);

        // Get the i8* type and create a (i8*)null.
        llvm::PointerType *int8PtrType = llvm::Type::getInt8PtrTy(GetLlvmContext());

        // Release value reference.
        value = AttachPosition(builder.CreatePointerCast(value, int8PtrType));
        AttachPosition(builder.CreateCall(release, value));
    }

    void FunctionBlock::Release(llvm::IRBuilder<> &builder, llvm::Value *value, const ChelaType *type)
    {
        if(type->IsStructure())
        {
            // Release the references.
            const Structure *building = static_cast<const Structure*> (type);
            for(size_t i = 0; i < building->GetValueRefCount(); ++i)
            {
                // Get the reference field.
                const Field *field = building->GetValueRefField(i);
                const ChelaType *fieldType = field->GetType();

                // Load the field value.
                size_t fieldIndex = field->GetStructureIndex();
                llvm::Value *fieldValue = AttachPosition(builder.CreateExtractValue(value, fieldIndex));

                // Release structure fields.
                if(fieldType->IsStructure())
                {
                    Release(builder, fieldValue, fieldType);
                }
                else if(fieldType->IsReference())
                {
                    // Release the reference.
                    Release(builder, fieldValue);
                }
                else
                {
                    printf("Cannot release field of type %s\n", fieldType->GetFullName().c_str());
                }
            }

            return;
        }
        else if(type->IsReference())
        {
            const ChelaType *refType = DeReferenceType(type);
            if(refType->IsStructure())
            {
                const Structure *building = static_cast<const Structure*> (refType);
                for(size_t i = 0; i < building->GetValueRefCount(); ++i)
                {
                    // Get the reference field.
                    const Field *field = building->GetValueRefField(i);
                    const ChelaType *fieldType = field->GetType();

                    // Get the field pointer.
                    size_t fieldIndex = field->GetStructureIndex();
                    llvm::Value *fieldPtr = AttachPosition(builder.CreateStructGEP(value, fieldIndex));

                    // Release structure fields.
                    if(fieldType->IsStructure())
                    {
                        Release(builder, fieldPtr, ReferenceType::Create(fieldType));
                    }
                    else if(fieldType->IsReference())
                    {
                        // Release the reference.
                        llvm::Value *fieldValue = AttachPosition(builder.CreateLoad(fieldPtr));
                        Release(builder, fieldValue);
                    }
                    else
                    {
                        printf("Cannot release field of type %s\n", fieldType->GetFullName().c_str());
                    }
                }
                return;
            }
            else if(refType->IsPassedByReference())
            {
                // Plain old reference.
                Release(builder, value);
                return;
            }
        }

        printf("TODO: Release references of type %s\n", type->GetFullName().c_str());
    }

    void FunctionBlock::RefSet(llvm::IRBuilder<> &builder, llvm::Value *value, llvm::Value *variable, const ChelaType *type)
    {
        // Check if the type is reference counted.
        bool counted = false;
        if(type->IsReference())
        {
            const ChelaType *referencedType = DeReferenceType(type);
            counted = referencedType->IsPassedByReference();
        }

        // Is this a structure with sub references.
        bool subReferences = false;
        const Structure *building = NULL;
        if(type->IsStructure())
        {
            building = static_cast<const Structure*> (type);
            subReferences = building->GetValueRefCount() > 0;
        }

        // Increment the new value reference.
        if(counted || subReferences)
            AddRef(builder, value, type);

        // Release the old value.
        if(subReferences)
        {
            Release(builder, variable, ReferenceType::Create(type));
        }
        else if(counted)
        {
            llvm::Value *oldValue = AttachPosition(builder.CreateLoad(variable));
            Release(builder, oldValue);
        }

        // Store the new value.
        AttachPosition(builder.CreateStore(value, variable));
    }

    void FunctionBlock::RefSetInter(llvm::IRBuilder<> &builder, llvm::Value *value, llvm::Value *variable, const ChelaType *type)
    {
        // Check if the type is reference counted.
        bool counted = false;
        if(type->IsReference())
        {
            const ChelaType *referencedType = DeReferenceType(type);
            counted = referencedType->IsPassedByReference();
        }

        // Does the type have sub references?
        bool subReferences = false;
        const Structure *building = NULL;
        if(type->IsStructure())
        {
            building = static_cast<const Structure*> (type);
            subReferences = building->GetValueRefCount() > 0;
        }

        // Release the old value.
        llvm::Value *oldValue = NULL;
        if(counted)
        {
            oldValue = AttachPosition(builder.CreateLoad(variable));
            Release(builder, oldValue);
        }

        // Release sub references.
        if(subReferences)
        {
            // Load the old value.
            if(!oldValue)
                oldValue = AttachPosition(builder.CreateLoad(variable));

            // Release the references.
            for(size_t i = 0; i < building->GetValueRefCount(); ++i)
            {
                // Get the reference field.
                const Field *field = building->GetValueRefField(i);

                // Load the field value.
                size_t fieldIndex = field->GetStructureIndex();
                llvm::Value *fieldValue = AttachPosition(builder.CreateExtractValue(oldValue, fieldIndex));

                // Release the reference.
                Release(builder, fieldValue);
            }
        }

        // Store the new value.
        AttachPosition(builder.CreateStore(value, variable));
    }

    inline void FunctionBlock::ImplicitDeRef(llvm::IRBuilder<> &builder, StackValue &stackValue)
    {
        if(stackValue.type->IsReference())
        {
            const ChelaType *refType = DeReferenceType(stackValue.type);
            if(refType->IsFirstClass())
            {
                // TODO: Handle null references.
                stackValue.value = builder.CreateLoad(stackValue.value);
                stackValue.type = refType;
            }
        }
    }

    llvm::Value *FunctionBlock::CreateCall(llvm::IRBuilder<> &builder, llvm::Value *calledFunction,
            const FunctionType *calledType, int n, llvm::Value *retPtr, llvm::Value *first, const ChelaType *firstType)
    {
        int protoN = calledType->GetArgumentCount();

        // Count the implicit argument.
        int actualArgs = n;
        if(first)
            ++actualArgs;

        if(actualArgs < protoN)
            Error("Fewer arguments than expected");
        else if(actualArgs > protoN && !calledType->IsVariable())
            Error("More arguments than expected");

        // Count the return pointer.
        int implicit = 0;
        if(retPtr)
        {
            ++actualArgs;
            implicit = 1;
        }

        // Parse the arguments.
        std::vector<llvm::Value*> args;
        args.resize(actualArgs);
        for(int i = 0; i < n; ++i)
        {
            // Pop the argument.
            StackValue arg = Pop();

            // Store the argument.
            size_t argIndex = args.size() - i - 1;
            const ChelaType *argType = InstanceType(calledType->GetArgument(argIndex - implicit));
            args[argIndex] = MakeCompatible(builder, arg.value, arg.type, argType);
        }

        // Add the return pointer.
        int index = 0;
        if(retPtr)
            args[index++] = retPtr;

        // Add the first implicit argument.
        if(first)
        {
            const ChelaType *firstArgType = InstanceType(calledType->GetArgument(0));
            args[index++] = MakeCompatible(builder, first, firstType, firstArgType);
        }

        // Import the called function.
        llvm::Value *imported = TryImportFunction(module, calledFunction);

        // Compute the calling convention.
        VirtualMachine *vm = GetVM();
        llvm::CallingConv::ID cc;
        switch(calledType->GetFunctionFlags() & MFL_LanguageMask)
        {
        default:
        case MFL_Native:
        case MFL_Kernel:
        case MFL_Runtime:
        case MFL_Cdecl:
            cc = llvm::CallingConv::C;
            break;
        case MFL_StdCall:
            if(vm->IsX86())
                cc = llvm::CallingConv::X86_StdCall;
            else
                cc = llvm::CallingConv::C;
            break;
        case MFL_ApiCall:
            // stdcall in x86 windows, cdecl in every other arch.
            if(vm->IsX86() && vm->IsWindows())
                cc = llvm::CallingConv::X86_StdCall;
            else
                cc = llvm::CallingConv::C;
            break;
        }

        // Perform the call.
        llvm::Value *ret;
        if(parent->IsHandlingExceptions())
        {
            // Create the continue block.
            llvm::BasicBlock *icont = CreateBlock("icont");
            llvm::BasicBlock *unwindDest;
            if(exceptionContext != NULL)
                unwindDest = exceptionContext->landingPad;
            else
                unwindDest = parent->GetTopLandingPad();

            // Create the call.
            llvm::InvokeInst *inv = builder.CreateInvoke(imported, icont, unwindDest, args);
            inv->setCallingConv(cc);
            ret = AttachPosition(inv);

            // Move to the continue block.
            builder.SetInsertPoint(icont);
        }
        else
        {
            // Simple call.
            llvm::CallInst *call = builder.CreateCall(imported, args);
            call->setCallingConv(cc);
            ret = AttachPosition(call);
        }

        // Return the ret value.
        return ret;
    }

    void FunctionBlock::CreateCheckCast(llvm::IRBuilder<> &builder, llvm::Value *original, llvm::Value *casted, llvm::Value *typeinfo)
    {
        VirtualMachine *vm = module->GetVirtualMachine();
        if(parent->IsHandlingExceptions())
        {
            // Create the continue block.
            llvm::BasicBlock *icont = CreateBlock("icont");
            llvm::BasicBlock *unwindDest;
            if(exceptionContext != NULL)
                unwindDest = exceptionContext->landingPad;
            else
                unwindDest = parent->GetTopLandingPad();

            // Prepare the arguments.
            std::vector<llvm::Value *> args;
            args.push_back(original);
            args.push_back(casted);
            args.push_back(typeinfo);

            // Perform the cast check.
            AttachPosition(builder.CreateInvoke(vm->GetCheckCast(module), icont, unwindDest, args));

            // Move to the continue block.
            builder.SetInsertPoint(icont);
        }
        else
        {
            AttachPosition(builder.CreateCall3(vm->GetCheckCast(module), original, casted, typeinfo));
        }
    }

    void FunctionBlock::CreateBoundCheck(llvm::IRBuilder<> &builder, llvm::Value *array, llvm::Value *index)
    {
        // Null check the array.
        CreateNullCheck(builder, array);
    }

    llvm::Value *FunctionBlock::Box(llvm::IRBuilder<> &builder, llvm::Value *unboxedValue,
                                    const ChelaType *unboxedType, const Structure *building)
    {
        // The unboxed must be a primitive, or the same structure
        if(!unboxedType->IsFirstClass())
        {
            if(unboxedType->IsReference())
                unboxedType = DeReferenceType(unboxedType);

            // Some first class uses implicit references.
            if(unboxedType->IsFirstClass())
                unboxedValue = AttachPosition(builder.CreateLoad(unboxedValue));
            else if(unboxedType != building)
                Error("Expected a structure or a first class to box.");
        }

        // Create the object
        VirtualMachine *vm = module->GetVirtualMachine();
        const ChelaType *boxType = BoxedType::Create(building);
        const ChelaType *referenceType = ReferenceType::Create(boxType);
        llvm::Constant *size = llvm::ConstantExpr::getSizeOf(boxType->GetTargetType());
        size = llvm::ConstantExpr::getIntegerCast(size, ChelaType::GetSizeType(vm)->GetTargetType(), false);

        // Get the class type info.
        llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(building->GetTypeInfo(module), builder.getInt8PtrTy()));

        // Perform managed allocation.
        
        llvm::Value *object = AttachPosition(builder.CreateCall2(vm->GetManagedAlloc(module), size, typeinfo));

        // Cast the void*.
        object = AttachPosition(builder.CreatePointerCast(object, referenceType->GetTargetType()));

        // Get the structure pointer.
        llvm::Value *structPointer = builder.CreateStructGEP(object, building->GetBoxIndex());

        // Set the value type.
        if(unboxedType->IsFirstClass())
        {
            // Find the value field.
            Field *valueField = building->GetField("__value");
            if(!valueField)
                valueField = building->GetField("m_value");
            if(!valueField)
                Error("Cannot perform boxing, struct doesn't have __value or m_value.");

            // Set the value.
            llvm::Value *fieldPtr = builder.CreateStructGEP(structPointer, valueField->GetStructureIndex());
            AttachPosition(builder.CreateStore(MakeCompatible(builder, unboxedValue, unboxedType, valueField->GetType()), fieldPtr));
        }
        else
        {
            // Just copy the complete structure.
            AttachPosition(builder.CreateStore(MakeCompatible(builder, unboxedValue, unboxedType, building), structPointer));
        }

        // Return the boxed object.
        return object;
    }

    llvm::Value *FunctionBlock::Unbox(llvm::IRBuilder<> &builder, llvm::Value *boxedValue,
                                      const ChelaType *boxedType, const Structure *building)
    {
        // Common data.
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::PointerType *int8PtrTy = builder.getInt8PtrTy();

        // The boxed object must be a reference.
        if(!boxedType->IsReference())
            Error("Expected object or box reference.");

        // De-reference the type.
        const ChelaType *refType = DeReferenceType(boxedType);
        if(!refType)
            Error("Trying to unbox a null reference.");
            
        // Make sure its a box.
        if(!refType->IsBoxed() && refType != vm->GetObjectClass())
            Error("Expected object or box reference.");

        // Get the box pointer.
        llvm::Value *boxPointer = NULL;
        if(refType->IsBoxed())
        {
            // Check the box object.
            const BoxedType *boxedType = static_cast<const BoxedType*> (refType);
            if(boxedType->GetValueType() != building)
                Error("Incompatible boxed value.");
            boxPointer = boxedValue;
        }
        else
        {
            // Cast into the box object.
            llvm::Constant *cast = vm->GetObjectDownCast(module);
            llvm::Value *value = AttachPosition(builder.CreatePointerCast(boxedValue, int8PtrTy));
            llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(building->GetTypeInfo(module), int8PtrTy));
            llvm::Value *casted = AttachPosition(builder.CreateCall2(cast, value, typeinfo));
            CreateCheckCast(builder, value, casted, typeinfo);
            boxPointer = AttachPosition(builder.CreatePointerCast(casted, llvm::PointerType::getUnqual(building->GetBoxedType())));
        }

        // Get the struct pointer.
        llvm::Value *structPointer = builder.CreateStructGEP(boxPointer, building->GetBoxIndex());

        // Return the structure reference.
        return structPointer;
    }

    void FunctionBlock::CastInstruction(llvm::IRBuilder<> &builder, llvm::Value *argument,
                                        const ChelaType *argType, const ChelaType *targetType, bool generic)
    {
        // Store the raw argument and target types
        //const ChelaType *rawArgType = argType;
        const ChelaType *rawTargetType = targetType;

        // Get the virtual machine and some common types.
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::PointerType *int8PtrTy = builder.getInt8PtrTy();

        // Check nop
        if(argType == targetType)
        {
            Push(argument, rawTargetType);
            return;
        }

        // Perform the correct cast kind.
        if(argType->IsInteger() && targetType->IsInteger())
        {
            // Integer-Integer cast
            llvm::Value *res = NULL;
            if(argType->GetSize() <= targetType->GetSize())
            {
                // Increase size.
                if(!targetType->IsUnsigned() && !argType->IsUnsigned())
                    res = builder.CreateSExtOrBitCast(argument, targetType->GetTargetType());
                else
                    res = builder.CreateZExtOrBitCast(argument, targetType->GetTargetType());
            }
            else
            {
                // Decrease size.
                res = builder.CreateTrunc(argument, targetType->GetTargetType());
            }

            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsFloatingPoint() && targetType->IsFloatingPoint())
        {
            // Floating point cast.
            llvm::Value *res = builder.CreateFPCast(argument, targetType->GetTargetType());
            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsInteger() && targetType->IsFloatingPoint())
        {
            // Integer -> floating point.
            llvm::Value *res = NULL;
            if(argType->IsUnsigned())
                res = builder.CreateUIToFP(argument, targetType->GetTargetType());
            else
                res = builder.CreateSIToFP(argument, targetType->GetTargetType());

            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsFloatingPoint() && targetType->IsInteger())
        {
            // Read the target type.
            llvm::Value *res = NULL;
            if(targetType->IsUnsigned())
                res = builder.CreateFPToUI(argument, targetType->GetTargetType());
            else
                res = builder.CreateFPToSI(argument, targetType->GetTargetType());

            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsPointer() && targetType->IsSizeInteger())
        {
            llvm::Value *res = builder.CreatePtrToInt(argument, targetType->GetTargetType());
            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsSizeInteger() && targetType->IsPointer())
        {
            llvm::Value *res = builder.CreateIntToPtr(argument, targetType->GetTargetType());
            AttachPosition(res);
            Push(res, targetType);
        }
        else if((argType->IsPointer() || argType == ChelaType::GetNullType(vm))
                 && targetType->IsPointer())
        {
            llvm::Value *res = builder.CreatePointerCast(argument, targetType->GetTargetType());
            AttachPosition(res);
            Push(res, targetType);
        }
        else if(argType->IsReference() && targetType->IsReference())
        {
            // Reference-reference cast.
            if(argType != ChelaType::GetNullType(vm))
            {
                const ChelaType *rawTargetType = targetType;

                // De-Reference types.
                argType = DeReferenceType(argType);
                targetType = DeReferenceType(targetType);

                // Object-Array casting are special.
                if(targetType->IsArray())
                {
                    const ArrayType *targetArrayType = static_cast<const ArrayType*> (targetType);
                    const ChelaType *targetElement = targetArrayType->GetValueType();

                    // Array-Array casts.
                    if(argType->IsArray())
                    {
                        // Cast the source type.
                        const ArrayType *sourceArrayType = static_cast<const ArrayType*> (argType);
                        const ChelaType *sourceElement = sourceArrayType->GetValueType();

                        // Check for readonly constraint.
                        // TODO: Allow under unsafe code
                        if(!targetArrayType->IsReadOnly() && sourceArrayType->IsReadOnly())
                            Error("Cannot convert a readonly array into a writable one");

                        // The dimensions must match.
                        if(targetArrayType->GetDimensions() != sourceArrayType->GetDimensions())
                            Error("Source and target array dimensions don't match.");

                        // Handle simple <-> boxed.
                        if(sourceElement != targetElement &&
                           (sourceElement->IsFirstClass() || sourceElement->IsStructure()) &&
                           (targetElement->IsFirstClass() || targetElement->IsStructure()))
                        {
                            // Use the associated primitive.
                            // TODO: Support enums?
                            if(sourceElement->IsStructure())
                                sourceElement = vm->GetAssociatedPrimitive((Structure*) sourceElement);
                            if(targetElement->IsStructure())
                                targetElement = vm->GetAssociatedPrimitive((Structure*) targetElement);

                            // If there are equals, just cast the array pointer.
                            if(sourceElement == targetElement)
                            {
                                argument = AttachPosition(builder.CreatePointerCast(argument, rawTargetType->GetTargetType()));
                                Push(argument, rawTargetType);
                                return;

                            }
                        }
                        // If the element type is the same, pass through.
                        else if(sourceElement == targetElement)
                        {
                            Push(argument, rawTargetType);
                            return;
                        }

                        // Unsupported cast.
                        Error("Unsupported generic array -> array casting.");
                    }
                    else
                    {
                        // Object to array casting are tricky.
                        Error("Unimplemented object -> array casting.");
                    }
                }

                // Use the array class when the target is not an array.
                if(argType->IsArray())
                {
                    argType = vm->GetArrayClass();
                    const ChelaType *classRefType = ReferenceType::Create(argType);
                    argument = AttachPosition(builder.CreatePointerCast(argument, classRefType->GetTargetType()));
                }
    
                // Check the referenced types.
                if(!argType->IsInterface() && !targetType->IsInterface() &&
                   argType->IsClass() != targetType->IsClass() &&
                   argType->IsStructure() != targetType->IsStructure() &&
                   argType->IsBoxed() != targetType->IsClass())
                {
                    // Try to box structures in generic casting.
                    if(generic && argType->IsStructure())
                    {
                        llvm::Value *loadedStruct = AttachPosition(builder.CreateLoad(argument));
                        CastInstruction(builder, loadedStruct, argType, rawTargetType, true);
                    }
                    else
                        Error("Incompatible reference casting.");
                }
    
                // Remove the box.
                if(argType->IsBoxed())
                {
                    const BoxedType *boxType = static_cast<const BoxedType*> (argType);
                    argType = boxType->GetValueType();
                }
    
                if(argType->IsClass() || argType->IsInterface() || argType->IsStructure())
                {
                    const Structure *argBuilding = static_cast<const Structure*> (argType);
                    const Structure *targetBuilding = static_cast<const Structure*> (targetType);
    
                    if(!argBuilding->IsInterface() && !targetBuilding->IsInterface())
                    {
                        // Only up or downcasting must be posible.
                        bool downcast = false;
                        if(!argBuilding->IsDerivedFrom(targetBuilding))
                        {
                            // Check for downcasting
                            if(targetBuilding->IsDerivedFrom(argBuilding))
                            {
                                // Cannot perform downcasting with structures.
                                if(targetBuilding->IsStructure())
                                    Error("Cannot perform reference downcasting with structures.");
    
                                // Use rtti to perform the cast.
                                downcast = true;
                                llvm::Constant *cast = vm->GetObjectDownCast(module);
                                llvm::Value *value = AttachPosition(builder.CreatePointerCast(argument, int8PtrTy));
                                llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(targetBuilding->GetTypeInfo(module), int8PtrTy));
                                llvm::Value *casted = AttachPosition(builder.CreateCall2(cast, value, typeinfo));
                                if(checked)
                                    CreateCheckCast(builder, value, casted, typeinfo);
                                llvm::Value *res = builder.CreatePointerCast(casted, rawTargetType->GetTargetType());
                                Push(AttachPosition(res), rawTargetType);
                            }
                            else
                            {
                                Error("Target type " + targetBuilding->GetFullName() + " is not a related class/structure of " +
                                    argBuilding->GetFullName() + " in " + parent->GetFullName());
                            }
                        }
    
                        // Standard pointer casting.
                        if(!downcast)
                            Push(builder.CreatePointerCast(argument, rawTargetType->GetTargetType()), rawTargetType);
                    }
                    else
                    {
                        // Interface casting.
                        if(targetBuilding->IsInterface())
                        {
                            // Move to parent.
                            if(argBuilding->Implements(targetBuilding))
                            {
                                // Interface upcast.
                                int itableIndex = argBuilding->GetITableIndex(targetBuilding);
                                if(itableIndex < 0)
                                    Error("Invalid itable index.");
    
                                // Get the itable and cast it.
                                llvm::Value *itable = builder.CreateConstGEP2_32(argument, 0, itableIndex);
                                llvm::Value *res = AttachPosition(builder.CreatePointerCast(itable, rawTargetType->GetTargetType()));
                                Push(AttachPosition(res), rawTargetType);
                            }
                            else
                            {
                                // Interface crosscast.
                                llvm::Constant *cast = vm->GetIfaceCrossCast(module);
                                llvm::Value *value = AttachPosition(builder.CreatePointerCast(argument, int8PtrTy));
                                llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(targetBuilding->GetTypeInfo(module), int8PtrTy));
                                llvm::Value *casted = AttachPosition(builder.CreateCall2(cast, value, typeinfo));
                                if(checked)
                                    CreateCheckCast(builder, value, casted, typeinfo);
                                llvm::Value *res = builder.CreatePointerCast(casted, rawTargetType->GetTargetType());
                                Push(AttachPosition(res), rawTargetType);
                            }
                        }
                        else
                        {
                             // Interface downcast.
                             llvm::Constant *cast = vm->GetIfaceDownCast(module);
                             llvm::Value *value = AttachPosition(builder.CreatePointerCast(argument, int8PtrTy));
                             llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(targetBuilding->GetTypeInfo(module), int8PtrTy));
                             llvm::Value *casted = AttachPosition(builder.CreateCall2(cast, value, typeinfo));
                             if(checked)
                                 CreateCheckCast(builder, value, casted, typeinfo);
                             llvm::Value *res = builder.CreatePointerCast(casted, rawTargetType->GetTargetType());
                             Push(AttachPosition(res), rawTargetType);
                        }
                    }
                }
                else if(argType->IsFirstClass() || argType->IsStructure())
                {
                    // Some first class and structure objects are delayed loaded.
                    llvm::Value *loaded = AttachPosition(builder.CreateLoad(argument));

                    // Now perform the actual cast.
                    CastInstruction(builder, loaded, argType, rawTargetType, generic);
                }
                else
                    Error("Invalid reference casting " + argType->GetFullName() + "->" + targetType->GetFullName());
            }
            else
            {
                // Just push the null reference.
                llvm::Value *res = builder.CreatePointerCast(argument, rawTargetType->GetTargetType());
                Push(AttachPosition(res), rawTargetType);
            }
        }
        else if(generic)
        {
            // Support boxing-unboxing.
            if((argType->IsFirstClass() || argType->IsStructure()) && targetType->IsReference())
            {
                // Get the argument type associated structure.
                const Structure *assoc = NULL;
                if(argType->IsStructure())
                    assoc = static_cast<const Structure*> (argType);
                else
                    assoc = vm->GetAssociatedClass(argType);

                // Make sure there's an associated.
                if(!assoc)
                    Error("Unsupported associated type.");

                // Cast the target type.
                const ChelaType *referencedType = DeReferenceType(targetType);
                if(!referencedType->IsStructure() && referencedType->IsClass() &&
                    referencedType->IsInterface())
                    Error("Unsupported target type.");
                const Structure *targetBuilding = static_cast<const Structure *> (referencedType);

                // Make sure the target type is implemented.
                // TODO: support enums.
                if(targetType != assoc && !assoc->IsDerivedFrom(targetBuilding) &&
                   !assoc->Implements(targetBuilding))
                   Error("Incompatible cast.");

                // Box the primitive type.
                const ChelaType *boxedType = BoxedType::Create(assoc);
                const ChelaType *referenceType = ReferenceType::Create(boxedType);
                llvm::Value *boxed = Box(builder, argument, argType, assoc);

                // Cast the boxed type.
                CastInstruction(builder, boxed, referenceType, targetType, true);
            }
            else if(argType->IsReference() && (targetType->IsStructure() || targetType->IsFirstClass()))
            {
                // Get the target structure.
                const Structure *building;
                if(targetType->IsStructure())
                    building = static_cast<const Structure*> (targetType);
                else
                    building = vm->GetAssociatedClass(targetType);;

                // De reference the argument.
                const ChelaType *refType = DeReferenceType(argType);

                // If the reference type is structure, read it or try to cast.
                llvm::Value *casted = NULL;
                const ChelaType *castedType = NULL;
                if(refType->IsStructure())
                {
                    // If its the same structure, delay load it.
                    if(refType == targetType)
                    {
                        casted = argument;
                        castedType = argType;
                    }
                    else
                        Error("unimplemente structure-structure cast.");
                }
                else
                {
                    // Class-interface, use unboxing.
                    // TODO: Try to detect early the error.
                    casted = Unbox(builder, argument, argType, building);
                    castedType = ReferenceType::Create(building);
                }

                // The structure can reference can be used verbatim
                if(targetType->IsStructure())
                {
                    // Push the casted object.
                    Push(casted, castedType);
                }
                else
                {
                    // Get the value field.
                    Field *valueField = building->GetField("__value");
                    if(!valueField)
                        valueField = building->GetField("m_value");
                    if(!valueField)
                        Error("couldn't get value field in " + building->GetFullName());

                    // Read the field value.
                    llvm::Value *fieldPtr = AttachPosition(builder.CreateStructGEP(casted, valueField->GetStructureIndex()));

                    // Avoid loading some types.
                    if(targetType->IsVector())
                    {
                        // Use the field pointer.
                        casted = fieldPtr;

                        // Push the field reference.
                        Push(casted, ReferenceType::Create(targetType));
                    }
                    else
                    {
                        // Load the boxed value.
                        casted = AttachPosition(builder.CreateLoad(fieldPtr));

                        // Push the casted value.
                        Push(casted, targetType);
                    }
                }
            }
            else
                Error("Failed to perform generic cast " + argType->GetFullName() + "->" + targetType->GetFullName());
        }
        else
            Error("Failed to perform cast " + argType->GetFullName() + "->" + targetType->GetFullName() + " in " + parent->GetFullName());
    }

    void FunctionBlock::CreateNullCheck(llvm::IRBuilder<> &builder, llvm::Value *value)
    {
        VirtualMachine *vm = module->GetVirtualMachine();

        // Only check pointers.
        llvm::Type *valueTy = value->getType();
        if(!valueTy->isPointerTy())
            return;
        llvm::PointerType *pointerTy = static_cast<llvm::PointerType*> (valueTy);

        // Don't perform null check in functions that aren't handling exceptions.
        if(!parent->IsHandlingExceptions())
            return;

        // Create the continue block.
        llvm::BasicBlock *throwBlock = CreateBlock("nullThrow");
        llvm::BasicBlock *unreachable = CreateBlock("nullUn");
        llvm::BasicBlock *merge = CreateBlock("nullm");
        llvm::BasicBlock *unwindDest;
        if(exceptionContext != NULL)
            unwindDest = exceptionContext->landingPad;
        else
            unwindDest = parent->GetTopLandingPad();

        // Check for null.
        llvm::Value *cmpRes = builder.CreateICmpEQ(value, llvm::ConstantPointerNull::get(pointerTy));
        AttachPosition(builder.CreateCondBr(cmpRes, throwBlock, merge));

        // Throw the exception.
        builder.SetInsertPoint(throwBlock);
        AttachPosition(builder.CreateInvoke(vm->GetThrowNull(module), unreachable, unwindDest));

        // Create the unreacheable block.
        builder.SetInsertPoint(unreachable);
        builder.CreateUnreachable();

        // Move to the merge block.
        builder.SetInsertPoint(merge);
    }

    void FunctionBlock::Define(llvm::IRBuilder<> &builder)
    {
        // Ignore generated/generating.
        if(generated)
            return;
            
        // Set the generated  flag to prevent cycles.
        generated = true;

        // Clear the stack.
        stack.clear();

        // Generate the predecessors and calculate the stack.
        bool first = true;
        BlockSet::iterator it = predecessors.begin();
        for(; it != predecessors.end(); it++)
        {
            FunctionBlock *block = *it;
            block->Define(builder);
            
            if(first)
            {
                // Insert the phi nodes here.
                builder.SetInsertPoint(GetBlock());

                // Create the phi nodes.
                for(size_t i = 0; i < block->stack.size(); i++)
                {
                    const StackValue &iv = block->stack[i];
                    llvm::PHINode *phi = builder.CreatePHI(iv.type->GetTargetType(), predecessors.size(), "sphi");
                    phi->addIncoming(iv.value, block->GetLastBlock());
                    stack.push_back(StackValue(phi, iv.type));
                }
                
                first = false;
            }
            else
            {
                // Make sure the stack matches.
                if(block->stack.size() != stack.size())
                    Error("Unbalanced stack.");
                    
                // Link the phi nodes.
                for(size_t i = 0; i < block->stack.size(); i++)
                {
                    const StackValue &iv = block->stack[i];
                    StackValue &v = stack[i];
                    if(v.type != iv.type)
                        Error("Incompatible stack value from different incoming flows.");
                    
                    llvm::PHINode *phi = (llvm::PHINode*)v.value;
                    phi->addIncoming(iv.value, block->GetLastBlock());
                }
            }
        }

        // Generate the instructions.
        builder.SetInsertPoint(GetBlock());

        // Some useful values.
        //llvm::Module *targetModule = module->GetTargetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        Class *stringClass = vm->GetStringClass();
        const ChelaType *stringType = ReferenceType::Create(stringClass);
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(GetLlvmContext());
        llvm::Type *sizeTy = ChelaType::GetSizeType(vm)->GetTargetType();

        // Set the exception caught flag.
        if(isCatchBlock)
        {
            llvm::Value *caughtFlag = parent->GetExceptionCaught();
            builder.CreateStore(builder.getInt1(true), caughtFlag);

            // Compute the exception type.
            const ChelaType *exceptionClass = module->GetType(exceptionType);
            if(!exceptionClass->IsClass())
                Error("only class instances can be exceptions.");
            const ChelaType *refType = ReferenceType::Create(exceptionClass);

            // Load the exception.
            llvm::Constant *eh_read = vm->GetEhRead(module);
            llvm::Value *exceptionStruct = builder.CreateLoad(parent->GetExceptionLocal());
            llvm::Value *exception = builder.CreateCall(eh_read, exceptionStruct);
            exception = builder.CreatePointerCast(exception, refType->GetTargetType());

            // Create an intermediate for the implicit reference.
            llvm::Value *inter = parent->CreateIntermediate(refType, true);
            RefSetInter(builder, exception, inter, refType);

            // Push the exception
            Push(exception, refType);
        }

        // Prepare debug emition.
        PrepareDebugEmition();

        // Instruction flags.
        checked = false;

        //printf("gen [%zu]%s sc %zu preds %zu len = %d\n", blockId, parent->GetFullName().c_str(), stack.size(), predecessors.size(), rawblockSize);
        InstructionReader reader(instructions, rawblockSize);
        StackValue a1, a2, a3;
        const FunctionType *parentType = (const FunctionType*)parent->GetType();
        for(size_t pc = 0; !reader.End(); ++pc)
        {
            // Read the opcode.
            int op = reader.ReadOpCode();

            // Begin instruction.
            BeginDebugInstruction(pc);

            //printf("op %s\n", InstructionTable[op].mnemonic);
            switch(op)
            {
            case OpCode::LoadArg:
                {
                    FunctionArgument *arg = parent->GetArgument(reader.ReadUI8());
                    Push(arg->GetValue(), arg->GetType());
                }
                break;
            case OpCode::LoadLocal:
                {
                    // Get the variable data.
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI8());
                    const ChelaType *varType = TryDeConstType(local->GetType());

                    // Check permissions.
                    if(varType->IsUnsafe())
                        Unsafe(op);

                    // Load the variable.
                    if(varType->IsStructure() || varType->IsVector())
                    {
                        // Don't load the complete structure..
                        varType = ReferenceType::Create(varType);
                        Push(local->GetValue(), varType);
                    }
                    else
                    {
                        llvm::Value *loaded = AttachPosition(builder.CreateLoad(local->GetValue()));
                        Push(loaded, varType);
                    }
                }
                break;
            case OpCode::LoadLocalS:
                {
                    // Get the variable data.
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI16());
                    const ChelaType *varType = TryDeConstType(local->GetType());

                    // Check permissions.
                    if(varType->IsUnsafe())
                        Unsafe(op);

                    // Load the variable.
                    if(varType->IsStructure() || varType->IsVector())
                    {
                        // Don't load the complete structure.
                        varType = ReferenceType::Create(varType);
                        Push(local->GetValue(), varType);
                    }
                    else
                    {
                        llvm::Value *loaded = AttachPosition(builder.CreateLoad(local->GetValue()));
                        Push(loaded, varType);
                    }
                }
                break;
            case OpCode::StoreLocal:
                {
                    // Load the assignment value.
                    a1 = Pop();

                    // Get the variable data.
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI8());
                    const ChelaType *localType = local->GetType();;

                    // Check the permissions
                    if(localType->IsUnsafe())
                        Unsafe(op);

                    // Perform implicit castings.
                    llvm::Value *value = MakeCompatible(builder, a1.value, a1.type, localType);

                    // Use add/release for ref counting.
                    if(local->IsRefCounted())
                    {
                        RefSet(builder, value, local->GetValue(), localType);
                    }
                    else
                    {
                        // Use plain store for not ref counted locals
                        AttachPosition(builder.CreateStore(value, local->GetValue()));
                    }
                }
                break;
            case OpCode::StoreLocalS:
                {
                    // Load the assignment value.
                    a1 = Pop();

                    // Get the variable data.
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI16());
                    const ChelaType *localType = local->GetType();;

                    // Check the permissions
                    if(localType->IsUnsafe())
                        Unsafe(op);

                    // Perform implicit castings.
                    llvm::Value *value = MakeCompatible(builder, a1.value, a1.type, localType);

                     // Use add/release for ref counting.
                    if(local->IsRefCounted())
                    {
                        RefSet(builder, value, local->GetValue(), localType);
                    }
                    else
                    {
                        // Use plain store for not ref counted locals
                        AttachPosition(builder.CreateStore(value, local->GetValue()));
                    }
                }
                break;
            case OpCode::LoadField:
                {
                    // Read the field and the object.
                    uint32_t fieldId = reader.ReadFieldId();
                    a1 = Pop();

                    // Read the field and the building.
                    const Structure *building;
                    Field *field = LoadFieldDescription(parent, a1.type, fieldId, building);

                    // Check the permissions.
                    if(field->IsUnsafe())
                        Unsafe(op);
                    
                    // Get the field structure index.
                    int slot = field->GetStructureIndex();

                    // Get the field pointer
                    llvm::Value *fieldPointer = builder.CreateConstGEP2_32(a1.value, 0, slot);

                    // Delayed loading of structures and vectors.
                    const ChelaType *fieldType = field->GetType();
                    if(fieldType->IsStructure() || fieldType->IsVector())
                    {
                        // Don't load the complete structure field.
                        fieldType = ReferenceType::Create(fieldType);
                        Push(fieldPointer, fieldType);
                    }
                    else
                    {
                        // Load the field.
                        Push(AttachPosition(builder.CreateLoad(fieldPointer)),
                             TryDeConstType(fieldType));
                    }
                }
                break;
            case OpCode::StoreField:
                {
                    // Read the field and the object.
                    uint32_t fieldId = reader.ReadFieldId();
                    a2 = Pop();
                    a1 = Pop();

                    // Read the field and the building.
                    const Structure *building;
                    Field *field = LoadFieldDescription(parent, a1.type, fieldId, building);

                    // Check the permissions.
                    if(field->IsUnsafe())
                        Unsafe(op);

                    // Get the field.
                    const ChelaType *fieldType = field->GetType();
                    if(fieldType->IsConstant())
                        throw ModuleException("tried to modify a constant.");
                    
                    // Get the field structure index.
                    int slot = field->GetStructureIndex();

                    // Get the field pointer
                    llvm::Value *fieldPointer = builder.CreateConstGEP2_32(a1.value, 0, slot);

                    // Perform implicit cast.
                    llvm::Value *value = MakeCompatible(builder, a2.value, a2.type, fieldType);

                    // Modify the field.
                    if(field->IsRefCounted())
                    {
                        // Perform reference counting.
                        RefSet(builder, value, fieldPointer, field->GetType());
                    }
                    else
                    {
                        // Store the field value.
                        AttachPosition(builder.CreateStore(value, fieldPointer));
                    }
                }
                break;
            case OpCode::LoadArraySlot:
                {
                    // Read the array type and the indices.
                    std::vector<llvm::Value*> indices;
                    const ChelaType *arrayType = GetType(reader.ReadTypeId());
                    LoadArrayIndices(builder, arrayType, indices);

                    // Read the object.
                    a1 = Pop();

                    // Check the permissions.
                    if(arrayType->IsUnsafe())
                        Unsafe(op);

                    // Make sure its an array of the same type specified.
                    if(a1.type->IsReference())
                    {
                        if(DeReferenceType(a1.type) != arrayType)
                            throw ModuleException("Expected the same array type as specified in the instruction.");
                    }
                    else if(a1.type != arrayType)
                        throw ModuleException("Expected the same pointer type as specified in the instruction.");

                    // Get the element type and pointer.
                    const ChelaType *elementType;
                    llvm::Value *elementPointer;
                    if(a1.type->IsPointer())
                    {
                        // Get the pointed type.
                        const PointerType *pointerType = static_cast<const PointerType*> (a1.type);
                        elementType = pointerType->GetPointedType();
                        
                        // Calculate the element pointer.
                        elementPointer = builder.CreateGEP(a1.value, indices[0]);
                    }
                    else
                    {
                        elementPointer = GetArrayElement(builder, a1.value, a1.type, indices, &elementType);
                    }

                    const ChelaType *varType = TryDeConstType(elementType);
                    if(varType->IsStructure() || varType->IsVector())
                    {
                        // Don't load the complete structure.
                        varType = ReferenceType::Create(varType);
                        Push(elementPointer, varType);
                    }
                    else
                    {
                        Push(AttachPosition(builder.CreateLoad(elementPointer)), varType);
                    }
                }
                break;
            case OpCode::StoreArraySlot:
                {
                    // Read the new value object.
                    a3 = Pop();

                    // Read the array type and the indices.
                    std::vector<llvm::Value*> indices;
                    const ChelaType *arrayType = GetType(reader.ReadTypeId());
                    LoadArrayIndices(builder, arrayType, indices);

                    // Read the object.
                    a1 = Pop();

                    // Check the permissions.
                    if(arrayType->IsUnsafe())
                        Unsafe(op);

                    // Make sure its an array of the same type specified.
                    if(a1.type->IsReference())
                    {
                        if(DeReferenceType(a1.type) != arrayType)
                            throw ModuleException("Expected the same array type as specified in the instruction.");
                    }
                    else if(a1.type != arrayType)
                        throw ModuleException("Expected the same pointer type as specified in the instruction.");

                    // Get the element type and pointer.
                    const ChelaType *elementType;
                    llvm::Value *elementPointer;
                    if(a1.type->IsPointer())
                    {
                        // Get the pointed type.
                        const PointerType *pointerType = static_cast<const PointerType*> (a1.type);
                        elementType = pointerType->GetPointedType();

                        // Calculate the element pointer.
                        elementPointer = builder.CreateGEP(a1.value, indices[0]);

                        // Set the element.
                        builder.CreateStore(MakeCompatible(builder, a3.value, a3.type, elementType), elementPointer);
                    }
                    else
                    {
                        elementPointer = GetArrayElement(builder, a1.value, a1.type, indices, &elementType);

                        // Set the element.
                        llvm::Value *value = MakeCompatible(builder, a3.value, a3.type, elementType);
                        RefSet(builder, value, elementPointer, elementType);
                    }
                }
                break;
            case OpCode::LoadValue:
                {
                    // Read the pointer.
                    a1 = Pop();

                    // Check the permissions.
                    if(a1.type->IsUnsafe())
                        Unsafe(op);

                    // Make sure its a pointer.
                    const ChelaType *pointerBase = a1.type;
                    if(!pointerBase->IsPointer() && !pointerBase->IsReference())
                        throw ModuleException("expected pointer/reference");

                    // Get the pointed type.
                    const ChelaType *pointedType = NULL;
                    if(pointerBase->IsPointer())
                    {
                        const PointerType *pointerType = static_cast<const PointerType*> (pointerBase);
                        pointedType = pointerType->GetPointedType();
                    }
                    else
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (pointerBase);
                        pointedType = refType->GetReferencedType();
                    }

                    // Delayed loading of structure.
                    if(pointedType->IsStructure() && pointerBase->IsReference())
                    {
                        Push(a1.value, a1.type);
                    }
                    else
                    {
                        // Load the value.
                        llvm::Value *loaded = AttachPosition(builder.CreateLoad(a1.value));
                        Push(loaded, TryDeConstType(pointedType));
                    }
                }
                break;
            case OpCode::StoreValue:
                {
                    // Read the pointer and the value.
                    a2 = Pop();
                    a1 = Pop();

                    // Check the permissions.
                    if(a1.type->IsUnsafe())
                        Unsafe(op);
                    if(a2.type->IsUnsafe())
                        Unsafe(op);

                    // Make sure its a pointer.
                    const ChelaType *pointerBase = a1.type;
                    if(!pointerBase->IsPointer() && !pointerBase->IsReference())
                        throw ModuleException("expected pointer/reference");

                    // Get the pointed type.
                    const ChelaType *pointedType = NULL;
                    if(pointerBase->IsPointer())
                    {
                        Unsafe(op);
                        const PointerType *pointerType = static_cast<const PointerType*> (pointerBase);
                        pointedType = pointerType->GetPointedType();
                    }
                    else
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (pointerBase);
                        pointedType = refType->GetReferencedType();
                    }

                    // Avoid modifing a constant.
                    if(pointedType->IsConstant())
                        throw ModuleException("tried to modify a constant.");

                    // Store the value.
                    // FIXME: Is better using RefSet?
                    llvm::Value *value = MakeCompatible(builder, a2.value, a2.type, pointedType);
                    AttachPosition(builder.CreateStore(value, a1.value));
                }
                break;
            case OpCode::LoadGlobal:
                {
                    // Find the global.
                    Field *global = GetField(reader.ReadUI32());

                    // Check the permissions.
                    if(global->IsUnsafe())
                        Unsafe(op);

                    // Check that its a global variable.
                    if(!global->IsStatic())
                        throw ModuleException("Global variables must be static.");

                    // Load the global.
                    const ChelaType *varType = global->GetType();
                    llvm::Value *pointer = module->ImportGlobal(global->GetGlobalVariable());
                    if(varType->IsStructure() || varType->IsVector())
                    {
                        // Don't load the complete structure.
                        varType = ReferenceType::Create(varType);
                        Push(pointer, varType);
                    }
                    else
                    {
                        Push(AttachPosition(builder.CreateLoad(pointer)), TryDeConstType(global->GetType()));
                    }
                }
                break;
            case OpCode::StoreGlobal:
                {
                    // Read the global id.
                    uint32_t globalId = reader.ReadUI32();
                    a1 = Pop();

                    // Find the global.
                    Field *global = GetField(globalId);

                    // Check the permissions.
                    if(global->IsUnsafe())
                        Unsafe(op);

                    // Check that its a global variable.
                    if(!global->IsStatic())
                        throw ModuleException("Global variables must be static.");

                    // Get the field.
                    const ChelaType *fieldType = global->GetType();
                    if(fieldType->IsConstant())
                        throw ModuleException("Tried to modify a constant.");

                    // Perform implicit cast.
                    llvm::Value *value = MakeCompatible(builder, a1.value, a1.type, global->GetType());

                    // Modify the field.
                    llvm::Value *pointer = module->ImportGlobal(global->GetGlobalVariable());
                    if(global->IsRefCounted())
                    {
                        // Perform reference counting.
                        RefSet(builder, value, pointer, global->GetType());
                    }
                    else
                    {
                        // Store the field value.
                        AttachPosition(builder.CreateStore(value, pointer));
                    }
                }
                break;
            case OpCode::LoadSwizzle:
                {
                    // Read the swizzle kind.
                    int numcomps = reader.ReadUI8();
                    int mask = reader.ReadUI8();

                    // Read the vector or reference.
                    a1 = Pop();
                    if(a1.type->IsReference())
                    {
                        if(!a1.type->IsReference())
                            throw ModuleException("Expected vector reference.");
                        a1.type = DeReferenceType(a1.type);
                        if(!a1.type->IsVector())
                            throw ModuleException("Expected vector reference.");
                        a1.value = builder.CreateLoad(a1.value);
                    }
                    else if(!a1.type->IsVector())
                        throw ModuleException("Expected vector or vector reference.");

                    // Read the vector type.
                    const VectorType *vectorType = static_cast<const VectorType*> (a1.type);

                    // Make sure the number of components is acceptable.
                    int vectorComponents = vectorType->GetNumComponents();
                    if(numcomps < 1 || numcomps > 4)
                        throw ModuleException("Invalid swizzle number of components.");

                    // Perform the swizzle.
                    const ChelaType *primitiveType = vectorType->GetPrimitiveType();
                    const ChelaType *retType = NULL;
                    llvm::Value *ret = NULL;
                    if(numcomps > 1)
                    {
                         retType = VectorType::Create(primitiveType, numcomps);
                         ret = llvm::UndefValue::get(retType->GetTargetType());
                         for(int i = 0; i < numcomps; ++i)
                         {
                             // Get and check the index
                             int elementIndex = mask & 3;
                             if(elementIndex >= vectorComponents)
                                 throw ModuleException("Invalid swizzle element index");

                             // Load the element..
                             llvm::Value *element = builder.CreateExtractElement(a1.value, builder.getInt32(elementIndex));
                             element = AttachPosition(element);

                             // Store it.
                             ret = builder.CreateInsertElement(ret, element, builder.getInt32(i));
                             ret = AttachPosition(ret);
                             mask >>= 2;
                         }
                    }
                    else
                    {
                        // Get and check the index
                        int elementIndex = mask & 3;
                        if(elementIndex >= vectorComponents)
                            throw ModuleException("Invalid swizzle element index");

                        // Just load the element.
                        retType = primitiveType;
                        ret = builder.CreateExtractElement(a1.value, builder.getInt32(mask & 3));
                        ret = AttachPosition(ret);
                    }

                    // Return the result.
                    Push(ret, retType);
                }
                break;
            case OpCode::StoreSwizzle:
                {
                    // Read the arguments.
                    a2 = Pop();
                    a1 = Pop();

                    // Read the swizzle kind.
                    int numcomps = reader.ReadUI8();
                    int mask = reader.ReadUI8();

                    // Read the vector reference.

                    if(!a1.type->IsReference())
                        throw ModuleException("Expected vector reference.");

                    // Read the vector type.
                    const ChelaType *referencedType = DeReferenceType(a1.type);
                    if(!referencedType->IsVector())
                        throw ModuleException("Expected vector reference.");
                    const VectorType *vectorType = static_cast<const VectorType*> (referencedType);
                    const ChelaType *primitiveType = vectorType->GetPrimitiveType();

                    // Read the value.
                    ImplicitDeRef(builder, a2);

                    int valueComps = 1;
                    if(a2.type->IsVector())
                    {
                        // Use the vector components;
                        const VectorType *valueVector = static_cast<const VectorType*> (a2.type);
                        valueComps = valueVector->GetNumComponents();

                        // Check equal primitie.
                        CheckEqualType(op, primitiveType, valueVector->GetPrimitiveType());
                    }

                    // Make sure the number of components is acceptable.
                    int vectorComponents = vectorType->GetNumComponents();
                    if(numcomps < 1 || numcomps > vectorComponents || numcomps > valueComps)
                        throw ModuleException("Invalid swizzle number of components.");

                    // Make sure the swizzle is settable.
                    int c1 = mask & 3;
                    int c2 = (mask>>2) & 3;
                    int c3 = (mask>>4) & 3;
                    int c4 = (mask>>6) & 3;
                    bool settable = false;
                    switch(numcomps)
                    {
                    case 1:
                        settable = true;
                        break;
                    case 2:
                        settable = c1 != c2;
                        break;
                    case 3:
                        settable = c1 != c2 && c2 != c3;
                        break;
                    case 4:
                        settable = c1 != c2 && c2 != c3 && c3 != c4;
                        break;
                    default:
                        throw ModuleException("Invalid swizzle size.");
                    }

                    // Check the settable flag.
                    if(!settable)
                        throw ModuleException("Invalid settable swizzle mask.");

                    // Perform the swizzle.
                    llvm::Value *result = builder.CreateLoad(a1.value);
                    for(int i = 0; i < numcomps; ++i)
                    {
                        // Get and check the index
                        int elementIndex = mask & 3;
                        if(elementIndex >= vectorComponents)
                            throw ModuleException("Invalid swizzle element index");

                        // Load the element.
                        llvm::Value *element = NULL;
                        if(valueComps == 1)
                            element = a2.value;
                        else
                        {
                            element = builder.CreateExtractElement(a2.value, builder.getInt32(i));
                            AttachPosition(element);
                        }

                        // Store the element
                        result = builder.CreateInsertElement(result, element, builder.getInt32(elementIndex));
                        AttachPosition(result);
                        mask >>= 2;
                    }

                    // Store the new vector value.
                    AttachPosition(builder.CreateStore(result, a1.value));
                }
                break;
            case OpCode::LoadLocalAddr:
                {
                    Unsafe(op);
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI16());
                    Push(local->GetValue(), PointerType::Create(local->GetType()));
                }
                break;
            case OpCode::LoadLocalRef:
                {
                    // Get the local data.
                    FunctionLocal *local = parent->GetLocal(reader.ReadUI16());
                    const ChelaType *localType = local->GetType();

                    // Check the permissions.
                    if(localType->IsUnsafe())
                        Unsafe(op);

                    // Store the local reference.
                    Push(local->GetValue(), ReferenceType::Create(localType));
                }
                break;
            case OpCode::LoadFieldAddr:
                {
                    // Unsafe instruction.
                    Unsafe(op);

                    // Read the field and the object.
                    uint32_t fieldId = reader.ReadFieldId();
                    a1 = Pop();

                    // Read the field and the building.
                    const Structure *building;
                    Field *field = LoadFieldDescription(parent, a1.type, fieldId, building);

                    // Get the field index.
                    int slot = field->GetStructureIndex();

                    // Get the field pointer
                    llvm::Value *fieldPointer = builder.CreateConstGEP2_32(a1.value, 0, slot);
                    Push(fieldPointer, PointerType::Create(field->GetType()));
                }
                break;
            case OpCode::LoadFieldRef:
                {
                    // Read the field and the object.
                    uint32_t fieldId = reader.ReadFieldId();
                    a1 = Pop();

                    // Read the field and the building.
                    const Structure *building;
                    Field *field = LoadFieldDescription(parent, a1.type, fieldId, building);

                    // Check the permissions.
                    if(field->IsUnsafe())
                        Unsafe(op);

                    // Get the field index.
                    int slot = field->GetStructureIndex();

                    // Get the field pointer
                    llvm::Value *fieldPointer = builder.CreateConstGEP2_32(a1.value, 0, slot);
                    Push(fieldPointer, ReferenceType::Create(field->GetType()));
                }
                break;
            case OpCode::LoadGlobalAddr:
                {
                    // Unsafe instruction.
                    Unsafe(op);

                    // Read the global id.
                    uint32_t globalId = reader.ReadUI32();

                    // Find the global.
                    Field *global = GetField(globalId);

                    // Check that its a global variable.
                    if(!global->IsStatic())
                        throw ModuleException("Global variables must be static.");

                    // Cast the global.
                    llvm::Value *pointer = module->ImportGlobal(global->GetGlobalVariable());
                    Push(pointer, PointerType::Create(global->GetType()));
                }
                break;
            case OpCode::LoadGlobalRef:
                {
                    // Read the global id.
                    uint32_t globalId = reader.ReadUI32();

                    // Find the global.
                    Field *global = GetField(globalId);

                    // Check the permissions.
                    if(global->IsUnsafe())
                        Unsafe(op);

                    // Check that its a global variable.
                    if(!global->IsStatic())
                        throw ModuleException("Global variables must be static.");

                    // Cast the global.
                    llvm::Value *pointer = module->ImportGlobal(global->GetGlobalVariable());
                    Push(pointer, ReferenceType::Create(global->GetType()));
                }
                break;
            case OpCode::LoadArraySlotAddr:
                {
                    // Unsafe instruction.
                    Unsafe(op);

                    // Read the array type and the indices.
                    std::vector<llvm::Value*> indices;
                    const ChelaType *arrayType = GetType(reader.ReadTypeId());
                    LoadArrayIndices(builder, arrayType, indices);

                    // Read the object.
                    a1 = Pop();

                    // Make sure its an array of the same type specified.
                    if(a1.type->IsReference())
                    {
                        if(DeReferenceType(a1.type) != arrayType)
                            throw ModuleException("Expected the same array type as specified in the instruction.");
                    }
                    else if(a1.type != arrayType)
                        throw ModuleException("Expected the same pointer type as specified in the instruction.");

                    // Get the element type and pointer.
                    const ChelaType *elementType;
                    llvm::Value *elementPointer;
                    if(a1.type->IsPointer())
                    {
                        // Get the pointed type.
                        const PointerType *pointerType = static_cast<const PointerType*> (a1.type);
                        elementType = pointerType->GetPointedType();

                        // Calculate the element pointer.
                        elementPointer = builder.CreateGEP(a1.value, indices[0]);
                    }
                    else
                    {
                        elementPointer = GetArrayElement(builder, a1.value, a1.type, indices, &elementType);
                    }

                    Push(elementPointer, PointerType::Create(elementType));
                }
                break;
            case OpCode::LoadArraySlotRef:
                {
                    // Read the array type and the indices.
                    std::vector<llvm::Value*> indices;
                    const ChelaType *arrayType = GetType(reader.ReadTypeId());
                    LoadArrayIndices(builder, arrayType, indices);

                    // Read the object.
                    a1 = Pop();

                    // Make sure its an array of the same type specified.
                    if(a1.type->IsReference())
                    {
                        if(DeReferenceType(a1.type) != arrayType)
                            throw ModuleException("Expected the same array type as specified in the instruction.");
                    }
                    else if(a1.type != arrayType)
                        throw ModuleException("Expected the same pointer type as specified in the instruction.");

                    // Get the element type and pointer.
                    const ChelaType *elementType;
                    llvm::Value *elementPointer;
                    if(a1.type->IsPointer())
                    {
                        // Get the pointed type.
                        const PointerType *pointerType = static_cast<const PointerType*> (a1.type);
                        elementType = pointerType->GetPointedType();
                        
                        // Calculate the element pointer.
                        elementPointer = builder.CreateGEP(a1.value, indices[0]);
                    }
                    else
                    {
                        elementPointer = GetArrayElement(builder, a1.value, a1.type, indices, &elementType);
                    }

                    // Check the permissions.
                    if(arrayType->IsUnsafe())
                        Unsafe(op);
                    if(elementType->IsUnsafe())
                        Unsafe(op);

                    // Push the element reference.
                    Push(elementPointer, ReferenceType::Create(elementType));
                }
                break;
            case OpCode::LoadFunctionAddr:
                {
                    // Always unsafe.
                    Unsafe(op);

                    // Get the function.
                    Function *targetFunction = GetFunction(reader.ReadFunctionId());

                    // Add a dependency to the function.
                    parent->AddDependency(targetFunction);

                    // Don't invoke contract/abstract functions.
                    if(targetFunction->IsContract() || targetFunction->IsAbstract())
                        throw ModuleException("Cannot get abstract function address.");

                    // Get his type.
                    const FunctionType *functionType = static_cast<const FunctionType*> (targetFunction->GetType());

                    // Create the pointer type.
                    const PointerType *pointerType = PointerType::Create(functionType);

                    // Load the function address.
                    Push(targetFunction->ImportFunction(module), pointerType);
                }
                break;
            case OpCode::LoadBool:
                {
                    uint8_t value = reader.ReadUI8();
                    const ChelaType *type = ChelaType::GetBoolType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadChar:
                {
                    uint8_t value = reader.ReadUI16();
                    const ChelaType *type = ChelaType::GetCharType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadUInt8:
                {
                    uint8_t value = reader.ReadUI8();
                    const ChelaType *type = ChelaType::GetByteType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadInt8:
                {
                    int8_t value = reader.ReadI8();
                    const ChelaType *type = ChelaType::GetSByteType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, true), type);
                }
                break;
            case OpCode::LoadUInt16:
                {
                    uint16_t value = reader.ReadUI16();
                    const ChelaType *type = ChelaType::GetUShortType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadInt16:
                {
                    int16_t value = reader.ReadI16();
                    const ChelaType *type = ChelaType::GetShortType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, true), type);
                }
                break;
            case OpCode::LoadUInt32:
                {
                    uint32_t value = reader.ReadUI32();
                    const ChelaType *type = ChelaType::GetUIntType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadInt32:
                {
                    int32_t value = reader.ReadI32();
                    const ChelaType *type = ChelaType::GetIntType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, true), type);
                }
                break;
            case OpCode::LoadUInt64:
                {
                    uint64_t value = reader.ReadUI64();
                    const ChelaType *type = ChelaType::GetULongType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, false), type);
                }
                break;
            case OpCode::LoadInt64:
                {
                    int64_t value = reader.ReadI64();
                    const ChelaType *type = ChelaType::GetLongType(vm);
                    Push(llvm::ConstantInt::get(type->GetTargetType(), value, true), type);
                }
                break;
            case OpCode::LoadFp32:
                {
                    float value = reader.ReadFP32();
                    const ChelaType *type = ChelaType::GetFloatType(vm);
                    Push(llvm::ConstantFP::get(type->GetTargetType(), value), type);
                }
                break;
            case OpCode::LoadFp64:
                {
                    double value = reader.ReadFP64();
                    const ChelaType *type = ChelaType::GetDoubleType(vm);
                    Push(llvm::ConstantFP::get(type->GetTargetType(), value), type);
                }
                break;
            case OpCode::LoadNull:
                {
                    const ChelaType *type = ChelaType::GetNullType(vm);
                    Push(llvm::ConstantPointerNull::get(llvm::Type::getInt32PtrTy(GetLlvmContext())), type);
                }
                break;
            case OpCode::LoadString:
                {
                    const std::string &str = declaringModule->GetString(reader.ReadStringId());
                    llvm::Constant *stringValue = module->CompileString(str);
                    Push(stringValue, stringType);
                }
                break;
            case OpCode::LoadCString:
                {
                    const std::string &str = declaringModule->GetString(reader.ReadStringId());
                    llvm::Constant *stringValue = module->CompileCString(str);
                    Push(stringValue, ChelaType::GetCStringType(vm));
                }
                break;
            case OpCode::LoadDefault:
                {
                    // Read the type.
                    const ChelaType *valueType = GetType(reader.ReadTypeId());

                    // Check permissions;
                    if(valueType->IsUnsafe())
                        Unsafe(op);

                    // The type cannot be passed by reference.
                    if(valueType->IsPassedByReference())
                        throw ModuleException("cannot create default value of type " + valueType->GetFullName());

                    // Create the default value.
                    llvm::Constant *value = llvm::Constant::getNullValue(valueType->GetTargetType());
                    Push(value, valueType);
                }
                break;
            case OpCode::Add:
                {
                    a2 = Pop();
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    if(a1.type->IsPointer() || a2.type->IsPointer())
                    {
                        // Pointer are unsafe.
                        Unsafe(op);

                        // Pointer arithmetic.
                        if(a1.type->IsPointer() && a2.type->IsPointer())
                            throw ModuleException("Cannot add two pointers.");

                        // Discriminate the pointer and the offset.
                        StackValue *pointer, *offset;
                        if(a1.type->IsPointer())
                        {
                            pointer = &a1;
                            offset = &a2;
                        }
                        else
                        {
                            pointer = &a2;
                            offset = &a1;
                        }

                        // Make sure the value is an integer.
                        if(!offset->type->IsInteger())
                            throw ModuleException("Pointer arithment addition expects integer offset.");

                        llvm::Value *result;
                        //if(offset->type->IsUnsigned())
                        {
                            // Use GEP for unsigned offset.
                            result = AttachPosition(builder.CreateGEP(pointer->value, offset->value));
                        }
                        /*else
                        {
                            // Get the pointed type.
                            llvm::PointerType *pointerType = (llvm::PointerType*)pointer->type->GetTargetType();
                            llvm::Type *pointedType = DePointerType(pointer->type)->GetTargetType();

                            // Cast the pointer into integer.
                            llvm::Type *i64Type = builder.getInt64Ty();
                            llvm::Value *intPtr = builder.CreatePtrToInt(pointer->value, i64Type);

                            // Compute the offset and element size.
                            llvm::Constant *elementSize = llvm::ConstantExpr::getSizeOf(pointedType);
                            llvm::Value *offsetValue = builder.CreateIntCast(offset->value, i64Type, !offset->type->IsUnsigned());

                            // offset = offset*sizeof(T).
                            offsetValue = builder.CreateMul(offsetValue, elementSize);

                            // intPtr += offset
                            intPtr = builder.CreateAdd(intPtr, offsetValue);

                            // Cast into a pointer again.
                            result = builder.CreateIntToPtr(intPtr, pointerType);
                        }*/
                        Push(result, pointer->type);
                    }
                    else
                    {
                        CheckEqualType(op, a1.type, a2.type);
                        llvm::Value *res;
                        if(a1.type->IsInteger())
                            res = AttachPosition(builder.CreateAdd(a1.value, a2.value, "add"));
                        else if(a1.type->IsFloatingPoint())
                            res = AttachPosition(builder.CreateFAdd(a1.value, a2.value, "fadd"));
                        else
                            throw ModuleException("Expected integer/float values.");
                        Push(res, a1.type);
                    }
                }
                break;
            case OpCode::Sub:
                {
                    a2 = Pop();
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    if(a1.type->IsPointer() && a2.type->IsPointer())
                    {
                        // Pointer operations are unsafe.
                        Unsafe(op);

                        // Pointer difference.
                        throw ModuleException("Unimplemented pointer difference.");
                    }
                    else if(a1.type->IsPointer() || a2.type->IsPointer())
                    {
                        // Pointer operations are unsafe.
                        Unsafe(op);

                        // The left side must be the pointer.
                        if(!a1.type->IsPointer())
                            throw ModuleException("Integer - Pointer unsupported.");

                        StackValue *pointer = &a1;
                        StackValue *offset = &a2;

                        // Make sure the offset is an integer.
                        if(!offset->type->IsInteger())
                            throw ModuleException("Pointer arithment addition expects integer offset.");

                        // Get the pointed type.
                        llvm::PointerType *pointerType = (llvm::PointerType*)pointer->type->GetTargetType();
                        llvm::Type *pointedType = DePointerType(pointer->type)->GetTargetType();

                        // Cast the pointer into integer.
                        llvm::Type *i64Type = builder.getInt64Ty();
                        llvm::Value *intPtr = AttachPosition(builder.CreatePtrToInt(pointer->value, i64Type));

                        // Compute the offset and element size.
                        llvm::Constant *elementSize = llvm::ConstantExpr::getSizeOf(pointedType);
                        llvm::Value *offsetValue = AttachPosition(builder.CreateIntCast(offset->value, i64Type, !offset->type->IsUnsigned()));

                        // offset = offset*sizeof(T).
                        offsetValue = AttachPosition(builder.CreateMul(offsetValue, elementSize));

                        // intPtr += offset
                        intPtr = AttachPosition(builder.CreateSub(intPtr, offsetValue));

                        // Cast into a pointer again.
                        llvm::Value *result = AttachPosition(builder.CreateIntToPtr(intPtr, pointerType));
                        Push(result, pointer->type);
                    }
                    else
                    {
                        CheckEqualType(op, a1.type, a2.type);
                        llvm::Value *res;
                        if(a1.type->IsInteger())
                            res = AttachPosition(builder.CreateSub(a1.value, a2.value, "sub"));
                        else if(a1.type->IsFloatingPoint())
                            res = AttachPosition(builder.CreateFSub(a1.value, a2.value, "fsub"));
                        else
                            throw ModuleException("Expected integer/float values.");
                        Push(res, a1.type);
                    }
                }
                break;
            case OpCode::Mul:
                {
                    a2 = Pop();
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    // Check for scalar vector multiplication.
                    bool primitive = a1.type->IsPrimitive() || a2.type->IsPrimitive();
                    bool vector = a1.type->IsVector() || a2.type->IsVector();
                    if(primitive && vector)
                    {
                        // Scalar-vector multiplication.
                        if(a1.type->IsPrimitive())
                        {
                            // Check vector-scalar compatibility.
                            const VectorType *vectorType = static_cast<const VectorType*> (a2.type);
                            CheckEqualType(op, a1.type, vectorType->GetPrimitiveType());

                            // Expand the primitive into a vector.
                            llvm::Value *expanded = llvm::UndefValue::get(vectorType->GetTargetType());
                            for(int i = 0; i < vectorType->GetNumComponents(); ++i)
                                expanded = AttachPosition(builder.CreateInsertElement(expanded, a1.value, builder.getInt32(i)));

                            // Replace the primitive.
                            a1.value = expanded;
                            a1.type = vectorType;
                        }
                        else
                        {
                            // Check vector-scalar compatibility.
                            const VectorType *vectorType = static_cast<const VectorType*> (a1.type);
                            CheckEqualType(op, a2.type, vectorType->GetPrimitiveType());

                            // Expand the primitive into a vector.
                            llvm::Value *expanded = llvm::UndefValue::get(vectorType->GetTargetType());
                            for(int i = 0; i < vectorType->GetNumComponents(); ++i)
                                expanded = AttachPosition(builder.CreateInsertElement(expanded, a2.value, builder.getInt32(i)));

                            // Replace the primitive.
                            a2.value = expanded;
                            a2.type = vectorType;
                        }
                    }
                    else
                    {
                        CheckEqualType(op, a1.type, a2.type);
                    }

                    llvm::Value *res;
                    if(a1.type->IsInteger())
                        res = AttachPosition(builder.CreateMul(a1.value, a2.value, "mul"));
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFMul(a1.value, a2.value, "fmul"));
                    else
                        throw ModuleException("Expected integer/float values.");
                    Push(res, a1.type);
                }
                break;
            case OpCode::MatMul:
                {
                    a2 = Pop();
                    a1 = Pop();
                
                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    // Check the first argument.
                    const ChelaType *firstPrim = NULL;
                    int firstRows = 1;
                    int firstCols = 1;
                    if(a1.type->IsMatrix())
                    {
                        const MatrixType *matrixType = static_cast<const MatrixType*> (a1.type);
                        firstPrim = matrixType->GetPrimitiveType();
                        firstRows = matrixType->GetNumRows();
                        firstCols = matrixType->GetNumColumns();
                    }
                    else if(a1.type->IsVector())
                    {
                        const VectorType *vectorType = static_cast<const VectorType*> (a1.type);
                        firstPrim = vectorType->GetPrimitiveType();
                        firstCols = vectorType->GetNumComponents();
                    }
                    else
                        throw ModuleException("Expected matrix or vector.");

                    // Check the second argument.
                    const ChelaType *secondPrim = NULL;
                    int secondRows = 1;
                    int secondCols = 1;
                    if(a2.type->IsMatrix())
                    {
                        const MatrixType *matrixType = static_cast<const MatrixType*> (a2.type);
                        secondPrim = matrixType->GetPrimitiveType();
                        secondRows = matrixType->GetNumRows();
                        secondCols = matrixType->GetNumColumns();
                    }
                    else if(a2.type->IsVector())
                    {
                        const VectorType *vectorType = static_cast<const VectorType*> (a2.type);
                        secondPrim = vectorType->GetPrimitiveType();
                        secondRows = vectorType->GetNumComponents();
                    }
                    else
                        throw ModuleException("Expected matrix or vector.");

                    // Make sure the primitives types are equal.
                    if(firstPrim != secondPrim)
                        throw ModuleException("Incompatible matrices/vectors.");

                    // Make sure the first number of columns is equal to the second number of rows.
                    if(firstCols != secondRows)
                        throw ModuleException("Incompatible matrix dimensions.");

                    // Extract the first matrix rows.
                    llvm::Value *nullMat1 = llvm::UndefValue::get(a1.type->GetTargetType());
                    std::vector<llvm::Constant*> maskIndices;
                    std::vector<llvm::Value*> rows;
                    for(int i = 0; i < firstRows; ++i)
                    {
                        // Create the row mask.
                        maskIndices.clear();
                        for(int j = 0; j < firstCols; ++j)
                            maskIndices.push_back(builder.getInt32(i*firstCols + j));
                        llvm::Value *mask = llvm::ConstantVector::get(maskIndices);

                        // Extract the row.
                        llvm::Value *row = builder.CreateShuffleVector(a1.value, nullMat1, mask);
                        rows.push_back(row);
                    }

                    // Extract the second matrix columns.
                    llvm::Value *nullMat2 = llvm::UndefValue::get(a2.type->GetTargetType());
                    std::vector<llvm::Value*> columns;
                    for(int i = 0; i < secondCols; ++i)
                    {
                        // Create the column mask.
                        maskIndices.clear();
                        for(int j = 0; j < secondRows; ++j)
                            maskIndices.push_back(builder.getInt32(j*secondCols + i));
                        llvm::Value *mask = llvm::ConstantVector::get(maskIndices);

                        // Extract the column.
                        llvm::Value *column = builder.CreateShuffleVector(a2.value, nullMat2, mask);
                        columns.push_back(column);
                    }

                    // Initialize result value.
                    const ChelaType *resultType = GetMatOrVec(firstPrim, firstRows, secondCols);
                    llvm::Value *result = llvm::UndefValue::get(resultType->GetTargetType());
                    bool isInt = firstPrim->IsInteger();

                    // Perform the matrix-matrix multiplication.
                    for(int i = 0; i < firstRows; ++i)
                    {
                        for(int j = 0; j < secondCols; ++j)
                        {
                            // Multiply row by column.
                            llvm::Value *multiplied = NULL;
                            if(isInt)
                                multiplied = builder.CreateMul(rows[i], columns[j]);
                            else
                                multiplied = builder.CreateFMul(rows[i], columns[j]);


                            // Add the elements.
                            llvm::Value *cell = builder.CreateExtractElement(multiplied, builder.getInt32(0));
                            if(isInt)
                            {
                                for(int k = 1; k < firstCols; ++k)
                                    cell = builder.CreateAdd(cell,
                                        builder.CreateExtractElement(multiplied, builder.getInt32(k)));
                            }
                            else
                            {
                                for(int k = 1; k < firstCols; ++k)
                                    cell = builder.CreateFAdd(cell,
                                        builder.CreateExtractElement(multiplied, builder.getInt32(k)));
                            }

                            // Store the cell in the result.
                            result = builder.CreateInsertElement(result, cell, builder.getInt32(i*secondCols + j));
                        }
                    }

                    // Return the matrix.
                    Push(result, resultType);
                }
                break;
            case OpCode::Div:
                {
                    a2 = Pop();
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    // Check for vector-scalar divition.
                    if(a2.type->IsPrimitive() && a1.type->IsVector())
                    {
                        // Check vector-scalar compatibility.
                        const VectorType *vectorType = static_cast<const VectorType*> (a1.type);
                        CheckEqualType(op, a2.type, vectorType->GetPrimitiveType());

                        // Expand the primitive into a vector.
                        llvm::Value *expanded = llvm::UndefValue::get(vectorType->GetTargetType());
                        for(int i = 0; i < vectorType->GetNumComponents(); ++i)
                            expanded = AttachPosition(builder.CreateInsertElement(expanded, a2.value, builder.getInt32(i)));

                        // Replace the primitive.
                        a2.value = expanded;
                        a2.type = vectorType;
                    }
                    else
                    {
                        CheckEqualType(op, a1.type, a2.type);
                    }

                    llvm::Value *res;
                    if(a1.type->IsInteger())
                    {
                        if(a1.type->IsUnsigned())
                            res = AttachPosition(builder.CreateUDiv(a1.value, a2.value, "udiv"));
                        else
                            res = AttachPosition(builder.CreateSDiv(a1.value, a2.value, "idiv"));
                    }
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFDiv(a1.value, a2.value, "fdiv"));
                    else
                        throw ModuleException("Expected integer/float values.");
                    Push(res, a1.type);
                }
                break;
            case OpCode::Mod:
                {
                    a2 = Pop();
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);
                    ImplicitDeRef(builder, a2);

                    // Check for vector-scalar divition.
                    if(a2.type->IsPrimitive() && a1.type->IsVector())
                    {
                        // Check vector-scalar compatibility.
                        const VectorType *vectorType = static_cast<const VectorType*> (a1.type);
                        CheckEqualType(op, a2.type, vectorType->GetPrimitiveType());

                        // Expand the primitive into a vector.
                        llvm::Value *expanded = llvm::UndefValue::get(vectorType->GetTargetType());
                        for(int i = 0; i < vectorType->GetNumComponents(); ++i)
                            expanded = AttachPosition(builder.CreateInsertElement(expanded, a2.value, builder.getInt32(i)));

                        // Replace the primitive.
                        a2.value = expanded;
                        a2.type = vectorType;
                    }
                    else
                    {
                        CheckEqualType(op, a1.type, a2.type);
                    }
                    
                    llvm::Value *res;
                    if(a1.type->IsInteger())
                    {
                        if(a1.type->IsUnsigned())
                            res = AttachPosition(builder.CreateURem(a1.value, a2.value, "udiv"));
                        else
                            res = AttachPosition(builder.CreateSRem(a1.value, a2.value, "idiv"));
                    }
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFRem(a1.value, a2.value, "fdiv"));
                    else
                        throw ModuleException("Expected integer/float values.");
                    Push(res, a1.type);
                }
                break;
            case OpCode::Neg:
                {
                    a1 = Pop();

                    // Handle first class implicit reference.
                    ImplicitDeRef(builder, a1);

                    llvm::Value *res;
                    if(a1.type->IsInteger())
                        res = AttachPosition(builder.CreateNeg(a1.value, "neg"));
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFNeg(a1.value, "fneg"));
                    else
                        throw ModuleException("Expected integer/float values.");
                    Push(res, a1.type);
                }
                break;
            case OpCode::Not:
                {
                    a1 = Pop();
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");

                    llvm::Value *res = AttachPosition(builder.CreateNot(a1.value, "not"));
                    Push(res, a1.type);
                }
                break;
            case OpCode::And:
                {
                    a2 = Pop();
                    a1 = Pop();
                    CheckEqualType(op, a1.type, a2.type);
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");

                    llvm::Value *res = AttachPosition(builder.CreateAnd(a1.value, a2.value, "and"));
                    Push(res, a1.type);
                }
                break;
            case OpCode::Or:
                {
                    a2 = Pop();
                    a1 = Pop();
                    CheckEqualType(op, a1.type, a2.type);
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");

                    llvm::Value *res = AttachPosition(builder.CreateOr(a1.value, a2.value, "or"));
                    Push(res, a1.type);
                }
                break;
            case OpCode::Xor:
                {
                    a2 = Pop();
                    a1 = Pop();
                    CheckEqualType(op, a1.type, a2.type);
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");

                    llvm::Value *res = AttachPosition(builder.CreateXor(a1.value, a2.value, "xor"));
                    Push(res, a1.type);
                }
                break;
            case OpCode::ShLeft:
                {
                    a2 = Pop();
                    a1 = Pop();
                    CheckEqualType(op, a1.type, a2.type);
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");

                    llvm::Value *res = AttachPosition(builder.CreateShl(a1.value, a2.value, "shl"));
                    Push(res, a1.type);
                }
                break;
            case OpCode::ShRight:
                {
                    a2 = Pop();
                    a1 = Pop();
                    CheckEqualType(op, a1.type, a2.type);
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer values.");
                    else if(a1.type->IsUnsigned())
                    {
                        int numbits = a2.type->GetSize()*8;
                        llvm::Type* bitsType = a2.type->GetTargetType();
                        llvm::Value *defined = AttachPosition(builder.CreateICmpULT(a2.value, llvm::ConstantInt::get(bitsType, numbits)));
                        llvm::Value *shifted = AttachPosition(builder.CreateLShr(a1.value, a2.value, "lshr"));
                        llvm::Value *zero = llvm::ConstantInt::get(a1.type->GetTargetType(), 0);
                        Push(AttachPosition(builder.CreateSelect(defined, shifted, zero)), a1.type);
                    }
                    else
                        Push(AttachPosition(builder.CreateAShr(a1.value, a2.value, "ashr")), a1.type);
                }
                break;
            case OpCode::CmpZ:
                {
                    a1 = Pop();
                    llvm::Value *res;
                    if(a1.type->IsInteger())
                        res = AttachPosition(builder.CreateICmpEQ(a1.value, llvm::ConstantInt::get(a1.type->GetTargetType(), 0, !a1.type->IsUnsigned())));
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFCmpOEQ(a1.value, llvm::ConstantFP::get(a1.type->GetTargetType(), 0)));
                    else if(a1.type->IsPointer() || a1.type->IsReference())
                        res = AttachPosition(builder.CreateIsNull(a1.value));
                    else
                        throw ModuleException("Expected integer/float/pointer/reference values.");
                    Push(res, ChelaType::GetBoolType(vm));
                }
                break;
            case OpCode::CmpNZ:
                {
                    a1 = Pop();
                    llvm::Value *res;
                    if(a1.type->IsInteger())
                        res = AttachPosition(builder.CreateICmpNE(a1.value, llvm::ConstantInt::get(a1.type->GetTargetType(), 0, !a1.type->IsUnsigned())));
                    else if(a1.type->IsFloatingPoint())
                        res = AttachPosition(builder.CreateFCmpONE(a1.value, llvm::ConstantFP::get(a1.type->GetTargetType(), 0)));
                    else if(a1.type->IsPointer() || a1.type->IsReference())
                        res = AttachPosition(builder.CreateIsNotNull(a1.value));
                    else
                        throw ModuleException("Expected integer/float/pointer/reference values.");
                    Push(res, ChelaType::GetBoolType(vm));
                }
                break;
            case OpCode::CmpEQ:
            case OpCode::CmpNE:
            case OpCode::CmpLT:
            case OpCode::CmpGT:
            case OpCode::CmpLE:
            case OpCode::CmpGE:
                {
                    a2 = Pop();
                    a1 = Pop();
                    Push(DoCompare(builder, op, a1, a2), ChelaType::GetBoolType(vm));
                }                
                break;
            case OpCode::IntCast:
                {
                    a1 = Pop();
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer for integer cast.");

                    CastInstruction(builder, a1.value, a1.type, ParseIntTarget(reader.ReadI8()));
                }
                break;
            case OpCode::FPCast:
                {
                    // Read the target type.
                    const ChelaType *target = ParseFloatTarget(reader.ReadUI8());
                    a1 = Pop();

                    if(!a1.type->IsFloatingPoint())
                        throw ModuleException("Expected floating point operand.");

                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::IntToFP:
                {
                    // Read the target type.
                    const ChelaType *target = ParseFloatTarget(reader.ReadUI8());
                    a1 = Pop();

                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer operand.");
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::FPToInt:
                {
                    // Read the target type.
                    const ChelaType *target = ParseIntTarget(reader.ReadI8());                    
                    a1 = Pop();
                    
                    if(!a1.type->IsFloatingPoint())
                        throw ModuleException("Expected floating point operand.");
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::PtrToSize:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);
                    
                    // The operand must be a pointer.
                    if(!a1.type->IsPointer())
                        throw ModuleException("Expected pointer operand.");
                    CastInstruction(builder, a1.value, a1.type, ChelaType::GetSizeType(vm));
                }
                break;
            case OpCode::SizeToPtr:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);

                    // Read the pointer type.
                    const ChelaType *target = module->GetType(reader.ReadTypeId()); 
                    if(a1.type != ChelaType::GetSizeType(vm) && a1.type != ChelaType::GetPtrDiffType(vm))
                        throw ModuleException("Expected size operand.");
                    if(!target->IsPointer())
                        throw ModuleException("Expected pointer type.");

                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::RefToSize:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);

                    // The operand must be a reference.
                    if(!a1.type->IsReference())
                        throw ModuleException("Expected reference operand.");

                    CastInstruction(builder, a1.value, a1.type, ChelaType::GetSizeType(vm));
                }
                break;
            case OpCode::RefToPtr:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);

                    // Read the pointer type.
                    const ChelaType *target = module->GetType(reader.ReadTypeId());
                    if(!a1.type->IsReference())
                        throw ModuleException("Expected reference operand.");
                    if(!target->IsPointer())
                        throw ModuleException("Expected pointer target.");
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::PtrToRef:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);

                    // Read the target type.
                    const ChelaType *target = module->GetType(reader.ReadTypeId());
                    if(!a1.type->IsPointer())
                        throw ModuleException("Expected pointer operand.");
                    if(!target->IsReference())
                        throw ModuleException("Expected reference target.");
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::PtrCast:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);

                    // Read the pointer type.
                    const ChelaType *target = GetType(reader.ReadTypeId());
                    if(!a1.type->IsPointer() || !target->IsPointer())
                        throw ModuleException("Expected pointer operand and target");
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::RefCast:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Read the reference type.
                    const ChelaType *target = GetType(reader.ReadTypeId());
                    if(!a1.type->IsReference() || !target->IsReference())
                        throw ModuleException("Expected reference operand and target");

                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::BitCast:
                {
                    // Read the operand.
                    a1 = Pop();

                    // Read the source and target type.
                    const ChelaType *sourceType = a1.type;
                    const ChelaType *targetType = GetType(reader.ReadTypeId());

                    // Check permissions.
                    if(sourceType->IsUnsafe() || targetType->IsUnsafe())
                        Unsafe(op);

                    // Source and target type sizes must match.
                    if(sourceType->GetSize() != targetType->GetSize())
                        throw ModuleException("Cannot perform bitcast with types of different sizes.");

                    // Check if the source and target are pointers.
                    bool sourcePointer = sourceType->IsPointer() || sourceType->IsReference();
                    bool targetPointer = targetType->IsPointer() || targetType->IsReference();

                    // Perform the cast.
                    llvm::Value *casted = NULL;
                    if(sourcePointer && targetType->IsInteger())
                    {
                        // Check permissions.
                        Unsafe(op);

                        // Check size compatibility.
                        if(!targetType->IsSizeInteger())
                            throw ModuleException("Cannot convert a pointer into a non-pointer capable integer.");
                        casted = builder.CreatePtrToInt(a1.value, targetType->GetTargetType());
                    }
                    else if(sourceType->IsInteger() && targetPointer)
                    {
                        // Check permissions.
                        Unsafe(op);

                        // Check size compatibility.
                        if(!sourceType->IsSizeInteger())
                            throw ModuleException("Cannot convert a non-pointer capable integer into a pointer.");
                        casted = builder.CreateIntToPtr(a1.value, targetType->GetTargetType());
                    }
                    else
                    {
                        casted = builder.CreateBitCast(a1.value, targetType->GetTargetType());
                    }

                    // Push the casted value.
                    Push(casted, targetType);
                }
                break;
            case OpCode::Cast:
                {
                    a1 = Pop();

                    // Read the target type.
                    const ChelaType *target = GetType(reader.ReadTypeId());

                    // Check permissions.
                    if(a1.type->IsUnsafe() || target->IsUnsafe())
                        Unsafe(op);

                    // Perform the cast.
                    CastInstruction(builder, a1.value, a1.type, target);
                }
                break;
            case OpCode::GCast:
                {
                    a1 = Pop();

                    // Read the target type.
                    const ChelaType *target = GetType(reader.ReadTypeId());

                    // Check permissions.
                    if(a1.type->IsUnsafe() || target->IsUnsafe())
                        Unsafe(op);

                    // Perform the cast.
                    CastInstruction(builder, a1.value, a1.type, target, true);
                }
                break;
            case OpCode::IsA:
                {
                    a1 = Pop();

                    // Read the reference type.
                    const ChelaType *targetType = GetType(reader.ReadTypeId());

                    // De-ref the value type.
                    const ChelaType *valueType = a1.type;
                    const ChelaType *sourceType = valueType;
                    if(valueType->IsReference())
                        sourceType = DeReferenceType(valueType);

                    // Use the associated types.
                    if(sourceType->IsFirstClass())
                        sourceType = vm->GetAssociatedClass(targetType);
                    if(targetType->IsFirstClass())
                        targetType = vm->GetAssociatedClass(targetType);

                    // Make sure the source type is acceptable
                    if(!sourceType->IsClass() && !sourceType->IsStructure() && !sourceType->IsInterface())
                        Error("Invalid compare type for a reference.");

                    // Make sure the target type is acceptable
                    if(!targetType->IsClass() && !targetType->IsStructure() && !targetType->IsInterface())
                        Error("Invalid compare type for a reference.");

                    // Cast the target type
                    const Structure *sourceStructure = static_cast<const Structure*> (sourceType);
                    const Structure *compare = static_cast<const Structure*> (targetType);

                    // Try to perform the check in compile time.
                    bool res = false;
                    if(sourceType == compare)
                        res = true;
                    else if(compare->IsClass())
                        res = sourceStructure->IsDerivedFrom(compare);
                    else if(compare->IsInterface())
                        res = sourceStructure->Implements(compare);

                    // Push the afirmative results, and any results if the
                    // source type is sealed or a structure.
                    if(res || sourceStructure->IsStructure() || sourceStructure->IsSealed())
                    {
                        Push(builder.getInt1(res), ChelaType::GetBoolType(vm));
                    }
                    else
                    {
                        // Cast the type info and value.
                        llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(compare->GetTypeInfo(module), int8PtrTy));
                        llvm::Value *value = AttachPosition(builder.CreatePointerCast(a1.value, int8PtrTy));
    
                        // Perform the comparison.
                        llvm::Constant *isRef = vm->GetIsRefType(module);
                        llvm::Value *res = builder.CreateCall2(isRef, value, typeinfo);
                        AttachPosition(res);
    
                        // Return the result.
                        Push(res, ChelaType::GetBoolType(vm));
                    }
                }
                break;
            case OpCode::NewObject:
                {
                    // Get the object type.
                    const ChelaType *objectType = GetType(reader.ReadTypeId());
                    if(!objectType->IsClass())
                        throw ModuleException("NewObject only can be used with class types.");

                    // Cannot create abstract objects.
                    if(objectType->IsAbstract())
                        throw ModuleException("Cannot create abstract objects of type " + objectType->GetFullName());

                    // Check object permissions.
                    if(objectType->IsUnsafe())
                        Unsafe(op);

                    // Get the constructor id and the number of arguments.
                    uint32_t ctorId = reader.ReadFunctionId();
                    if(ctorId == 0)
                        throw ModuleException("Expected constructor to create struct/class instance.");

                    // Read the number of argument.                    
                    int n = reader.ReadUI8();
                    if(ctorId == 0 && n != 0)
                        throw ModuleException("Unexpected construction arguments for primitives/pointers.");
                    
                    // Create the object
                    const ChelaType *referenceType;
                    llvm::Value *object;
                    llvm::Constant *size = llvm::ConstantExpr::getSizeOf(objectType->GetTargetType());
                    size = llvm::ConstantExpr::getIntegerCast(size, sizeTy, false);
                    referenceType = ReferenceType::Create(objectType);

                    // Get the class type info.
                    const Class *clazz = static_cast<const Class*> (objectType);
                    llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(clazz->GetTypeInfo(module), int8PtrTy));

                    // Perform managed allocation.
                    object = AttachPosition(builder.CreateCall2(vm->GetManagedAlloc(module), size, typeinfo));

                    // Cast the void*.
                    object = AttachPosition(builder.CreatePointerCast(object, referenceType->GetTargetType()));
                    
                    // Invoke constructor.
                    if(objectType->IsClass())
                    {
                        // Cast the structure.
                        const Structure *building = static_cast<const Structure*> (objectType);
                        
                        // Get the constructor.
                        Function *ctor = GetFunction(ctorId);
                        if(!ctor->IsConstructor() || ctor->IsStatic())
                            throw ModuleException("Invalid constructor.");

                        // Make sure the constructor is in the class.
                        if(ctor->GetParent() != building)
                            throw ModuleException("Cannot use a constructor of other class.");

                        // Check constructor permissions.
                        if(ctor->IsUnsafe())
                            Unsafe(op);

                        // Get his type.
                        const FunctionType *ctorType = static_cast<const FunctionType*> (ctor->GetType());
                        
                        // The return type must be void.
                        if(ctorType->GetReturnType() != ChelaType::GetVoidType(vm))
                            throw ModuleException("Constructor return type must be void.");

                        // Add a dependency to the constructor.
                        parent->AddDependency(ctor);

                        // Call the constructor.
                        CreateCall(builder, ctor->GetTarget(), ctorType, n, NULL, object, ReferenceType::Create(objectType));
                    }

                    // Create an intermediate for the object.
                    llvm::Value *inter = parent->CreateIntermediate(referenceType, true);
                    RefSetInter(builder, object, inter, referenceType);

                    // Return the object.
                    Push(object, referenceType);
                }
                break;
            case OpCode::NewStruct:
                {
                    // Get the value type.
                    const ChelaType *valueType = module->GetType(reader.ReadTypeId());
                    if(!valueType->IsStructure())
                        throw ModuleException("Expected struct type.");

                    // Check object permissions.
                    if(valueType->IsUnsafe())
                        Unsafe(op);

                    // Get the constructor id and the number of arguments.
                    uint32_t ctorId = reader.ReadFunctionId();
                    if(ctorId == 0)
                        throw ModuleException("Expected constructor to create struct instance.");

                    // Read the number of argument.
                    int n = reader.ReadUI8();
                    if(ctorId == 0 && n != 0)
                        throw ModuleException("Unexpected construction arguments for primitives/pointers.");

                    // Cast the structure.
                    const Structure *building = static_cast<const Structure*> (valueType);

                    // Create the object in the stack.
                    const ChelaType *referenceType = ReferenceType::Create(valueType);
                    llvm::Value *object = parent->CreateIntermediate(valueType, building->HasSubReferences());

                    // Get the constructor.
                    Function *ctor = GetFunction(ctorId);
                    if(!ctor->IsConstructor() || ctor->IsStatic() ||
                        ctor->GetParent() != building)
                            throw ModuleException("Invalid structure constructor.");

                    // Check constructor permissions.
                    if(ctor->IsUnsafe())
                        Unsafe(op);

                    // Get his type.
                    const FunctionType *ctorType = static_cast<const FunctionType*> (ctor->GetType());

                    // The return type must be void.
                    if(ctorType->GetReturnType() != ChelaType::GetVoidType(vm))
                        throw ModuleException("Constructor return type must be void.");

                    // Add a dependency to the constructor.
                    parent->AddDependency(ctor);

                    // Release previous references.
                    if(building->HasSubReferences())
                        Release(builder, object, referenceType);

                    // Call the constructor.
                    CreateCall(builder, ctor->GetTarget(), ctorType, n, NULL, object, referenceType);

                    // Return the object.
                    Push(object, referenceType);
                }
                break;
            case OpCode::NewVector:
                {
                    // Get the object type.
                    const ChelaType *objectType = module->GetType(reader.ReadTypeId());
                    if(!objectType->IsVector())
                        throw ModuleException("Expected vector type.");

                    // Cast it.
                    const VectorType *vectorType = static_cast<const VectorType*> (objectType);
                    const ChelaType *componentType = vectorType->GetPrimitiveType();

                    // Read each component.
                    std::vector<llvm::Value*> components;
                    size_t numcomponents = vectorType->GetNumComponents();
                    while(components.size() < numcomponents)
                    {
                        // Read each component.
                        a1 = Pop();

                        // Handle implicit references.
                        if(a1.type->IsReference())
                        {
                            a1.type = DeReferenceType(a1.type);
                            if(a1.type->IsFirstClass())
                                a1.value = AttachPosition(builder.CreateLoad(a1.value));
                        }

                        if(a1.type->IsVector())
                        {
                            // Make sure the vector is acceptable.
                            const VectorType* argVectorType = static_cast<const VectorType*> (a1.type);
                            const ChelaType *argComponentType = argVectorType->GetPrimitiveType();
                            CheckEqualType(op, argComponentType, componentType);

                            // Append each element of the vector.
                            size_t numargs = argVectorType->GetNumComponents();
                            for(size_t i = 0; i < numargs; ++i)
                            {
                                llvm::Value *element = builder.CreateExtractElement(a1.value, builder.getInt32(numargs - i - 1));
                                components.push_back(AttachPosition(element));
                            }
                        }
                        else if(a1.type->IsPrimitive())
                        {
                            // Make sure the primitive is acceptable.
                            CheckEqualType(op, a1.type, componentType);

                            // Store the component.
                            components.push_back(a1.value);
                        }
                        else
                            throw ModuleException("Expected vector or primitive value.");
                    }

                    // Make sure the number of components is correct.
                    if(components.size() != numcomponents)
                        throw ModuleException("Invalid number of components.");

                    // Create the vector.
                    llvm::Value *vector = llvm::UndefValue::get(vectorType->GetTargetType());
                    for(size_t i = numcomponents; i > 0; --i)
                    {
                        vector = builder.CreateInsertElement(vector, components[i-1], builder.getInt32(numcomponents - i));
                        AttachPosition(vector);
                    }

                    // Return the vector.
                    Push(vector, vectorType);
                }
                break;
            case OpCode::NewMatrix:
                {
                    // Get the object type.
                    const ChelaType *objectType = module->GetType(reader.ReadTypeId());
                    if(!objectType->IsMatrix())
                        throw ModuleException("Expected matrix type.");

                    // Cast it.
                    const MatrixType *matrixType = static_cast<const MatrixType*> (objectType);
                    const ChelaType *componentType = matrixType->GetPrimitiveType();

                    // Read each component.
                    std::vector<llvm::Value*> components;
                    size_t numcomponents = matrixType->GetNumColumns() * matrixType->GetNumRows();
                    while(components.size() < numcomponents)
                    {
                        // Read each component.
                        a1 = Pop();

                        // Handle implicit references.
                        if(a1.type->IsReference())
                        {
                            a1.type = DeReferenceType(a1.type);
                            if(a1.type->IsFirstClass())
                                a1.value = AttachPosition(builder.CreateLoad(a1.value));
                        }

                        if(a1.type->IsVector())
                        {
                            // Make sure the vector is acceptable.
                            const VectorType* argVectorType = static_cast<const VectorType*> (a1.type);
                            const ChelaType *argComponentType = argVectorType->GetPrimitiveType();
                            CheckEqualType(op, argComponentType, componentType);

                            // Append each element of the vector.
                            size_t numargs = argVectorType->GetNumComponents();
                            for(size_t i = 0; i < numargs; ++i)
                            {
                                llvm::Value *element = builder.CreateExtractElement(a1.value, builder.getInt32(numargs - i - 1));
                                components.push_back(AttachPosition(element));
                            }
                        }
                        else if(a1.type->IsPrimitive())
                        {
                            // Make sure the primitive is acceptable.
                            CheckEqualType(op, a1.type, componentType);

                            // Store the component.
                            components.push_back(a1.value);
                        }
                        else
                            throw ModuleException("Expected vector or primitive value.");
                    }

                    // Make sure the number of components is correct.
                    if(components.size() != numcomponents)
                        throw ModuleException("Invalid number of components.");

                    // Create the vector.
                    llvm::Value *matrix = llvm::UndefValue::get(matrixType->GetTargetType());
                    for(size_t i = numcomponents; i > 0; --i)
                    {
                        matrix = builder.CreateInsertElement(matrix, components[i-1], builder.getInt32(numcomponents - i));
                        AttachPosition(matrix);
                    }

                    // Return the vector.
                    Push(matrix, matrixType);
                }
                break;
            case OpCode::NewDelegate:
                {
                    // Get the delegate type.
                    const ChelaType *objectType = GetType(reader.ReadTypeId());
                    if(!objectType->IsClass())
                        throw ModuleException("Expected delegate class type.");

                    // Cannot create abstract objects.
                    if(objectType->IsAbstract())
                        throw ModuleException("Cannot create abstract objects.");

                    // Get the delegated function id and the number of arguments.
                    uint32_t delegatedId = reader.ReadFunctionId();
                    if(delegatedId == 0)
                        throw ModuleException("Expected delegated function.");

                    // Get the delegated function.
                    Function *delegatedFunction = GetFunction(delegatedId);

                    // Check permisions.
                    if(objectType->IsUnsafe() || delegatedFunction->IsUnsafe())
                        Unsafe(op);

                    // Get the implicit self.
                    llvm::Value *implicitSelf = NULL;
                    const ChelaType *implicitSelfType = NULL;
                    if(!delegatedFunction->IsStatic())
                    {
                        // Read the self argument.
                        a1 = Pop();

                        // Get the delegated function type.
                        const FunctionType *delegatedType = delegatedFunction->GetFunctionType();
                        if(delegatedType->GetArgumentCount() == 0)
                            throw ModuleException("Invalid non-static function.");

                        // Check the compatiblity.
                        CheckCompatible(delegatedType->GetArgument(0), a1.type);

                        // Store the implicit self.
                        implicitSelf = a1.value;
                        implicitSelfType = a1.type;
                    }

                    // Cast the delegate type, and make sure is actually a delegate.
                    const Class *delegateType = static_cast<const Class*> (objectType);
                    Class *delegateClass = vm->GetDelegateClass();
                    if(!delegateType->IsDerivedFrom(delegateClass))
                        throw ModuleException("Cannot create a delegate without a valid delegate class.");

                    // Get the invoke member.
                    FunctionGroup *invokeGroup = delegateType->GetFunctionGroup("Invoke");
                    if(!invokeGroup)
                        throw ModuleException("Invalid delegate type, doesn't have Invoke function group.");

                    // Get the first no static function of the group.
                    Function *invokeFunction = NULL;
                    for(size_t i = 0; i < invokeGroup->GetFunctionCount(); ++i)
                    {
                        Function *groupMember = invokeGroup->GetFunction(i);
                        if(!groupMember->IsStatic())
                        {
                            invokeFunction = groupMember;
                            break;
                        }
                    }

                    if(!invokeFunction)
                        throw ModuleException("Invalid delegate type, doesn't have no static Invoke function.");

                    // Make sure that the delegate and delegated function are compatible.
                    CheckDelegateCompatible(invokeFunction, delegatedFunction);

                    // Get the class type info.
                    llvm::Value *typeinfo = AttachPosition(builder.CreatePointerCast(delegateType->GetTypeInfo(module), int8PtrTy));

                    // Create the delegate
                    const ChelaType *referenceType = ReferenceType::Create(objectType);
                    llvm::Constant *size = llvm::ConstantExpr::getSizeOf(objectType->GetTargetType());
                    size = llvm::ConstantExpr::getIntegerCast(size, sizeTy, false);

                    // Perform managed allocation.
                    llvm::Value *object = AttachPosition(builder.CreateCall2(vm->GetManagedAlloc(module), size, typeinfo));
                    object = AttachPosition(builder.CreatePointerCast(object, referenceType->GetTargetType()));

                    // Set the instance reference.
                    if(implicitSelf != NULL)
                    {
                        // Get the instance field.
                        Field *instanceField = delegateType->GetField("instance");
                        if(!instanceField)
                            throw ModuleException("Delegate doesn't have instance field.");

                        // Get the instance field pointer.
                        llvm::Value *instanceFieldPtr = builder.CreateStructGEP(object, instanceField->GetStructureIndex());

                        // Set it.
                        AddRef(builder, implicitSelf);
                        implicitSelf = MakeCompatible(builder, implicitSelf, implicitSelfType, instanceField->GetType());
                        AttachPosition(builder.CreateStore(implicitSelf, instanceFieldPtr));
                    }

                    // Set the function field.
                    Field *functionField = delegateType->GetField("function");
                    if(!functionField)
                        throw ModuleException("Delegate doesn't have function field.");

                    // Get the function field pointer and set it.
                    llvm::Value *functionFieldPtr = builder.CreateStructGEP(object, functionField->GetStructureIndex());
                    llvm::Value *functionPtr = AttachPosition(builder.CreatePointerCast(delegatedFunction->GetTarget(), functionField->GetType()->GetTargetType()));
                    AttachPosition(builder.CreateStore(functionPtr, functionFieldPtr));

                    // Create an intermediate for the object.
                    llvm::Value *inter = parent->CreateIntermediate(referenceType, true);
                    RefSetInter(builder, object, inter, referenceType);

                    // Return the delegate.
                    Push(object, referenceType);
                }
                break;
            case OpCode::Box:
                {
                    // Get the object.
                    a1 = Pop();

                    // Read the box type.
                    const ChelaType *valueType = GetType(reader.ReadTypeId());
                    if(!valueType->IsStructure())
                        throw ModuleException("Expected structure type.");
                    const Structure *building = (const Structure*)valueType;
                    const ChelaType *boxedType = BoxedType::Create(building);
                    const ChelaType *referenceType = ReferenceType::Create(boxedType);

                    // Box the value.
                    llvm::Value *object = Box(builder, a1.value, a1.type, building);

                    // Create an intermediate for the object.
                    llvm::Value *inter = parent->CreateIntermediate(referenceType, true);
                    RefSetInter(builder, object, inter, referenceType);

                    // Return the object.
                    Push(object, referenceType);
                }
                break;
            case OpCode::PrimBox:
                {
                    // Get the object.
                    a1 = Pop();

                    // Read the box type.
                    const ChelaType *valueType = GetType(reader.ReadTypeId());
                    if(!valueType->IsStructure())
                        throw ModuleException("Expected structure type.");
                    const Structure *building = (const Structure*)valueType;

                    // a1 must be a primitive or a reference to a primitive.
                    const ChelaType *unboxedType = a1.type;
                    if(!unboxedType->IsFirstClass())
                    {
                        // Some first class uses implicit references.
                        if(unboxedType->IsReference())
                            unboxedType = DeReferenceType(unboxedType);

                        // Load referenced first class.
                        if(unboxedType->IsFirstClass())
                        {
                            // TODO: Check null references.
                            a1.value = builder.CreateLoad(a1.value);
                            a1.type = unboxedType;
                        }
                        else
                        {   throw ModuleException("Expected first class type.");
                        }
                    }

                    // Allocate the structure in the stack.
                    llvm::Value *structPointer = parent->CreateIntermediate(building, false);

                    // Find the value field.
                    Field *valueField = building->GetField("__value");
                    if(!valueField)
                        valueField = building->GetField("m_value");
                    if(!valueField)
                        throw ModuleException("Cannot perform boxing, struct doesn't have __value or m_value.");

                    // Set the value.
                    llvm::Value *fieldPtr = builder.CreateStructGEP(structPointer, valueField->GetStructureIndex());
                    AttachPosition(builder.CreateStore(MakeCompatible(builder, a1.value, a1.type, valueField->GetType()), fieldPtr));

                    // Return the object.
                    Push(structPointer, ReferenceType::Create(building));
                }
                break;
            case OpCode::Unbox:
                {
                    // Get the object.
                    a1 = Pop();

                    // Read the box type.
                    const ChelaType *valueType = GetType(reader.ReadTypeId());
                    if(!valueType->IsStructure())
                        throw ModuleException("Expected structure type.");
                    const Structure *building = (const Structure*)valueType;
                    const ChelaType *refType = ReferenceType::Create(building);

                    // Unbox the value.
                    llvm::Value *unboxed = Unbox(builder, a1.value, a1.type, building);

                    // Push the structure reference.
                    Push(unboxed, refType);
                }
                break;
            case OpCode::ExtractPrim:
                {
                    // Get the object.
                    a1 = Pop();

                    // Make sure its a structure reference.
                    if(!a1.type->IsReference() && !a1.type->IsStructure())
                        throw ModuleException("Expected structure or structure reference.");

                    const ReferenceType *refType = NULL;
                    const Structure *building = NULL;
                    if(a1.type->IsStructure())
                    {
                        // Cast the value type.
                        building = static_cast<const Structure*> (a1.type);
                    }
                    else
                    {
                        // Make sure is a reference to a structure.
                        refType = static_cast<const ReferenceType*> (a1.type);
                        const ChelaType *referencedType = refType->GetReferencedType();
                        if(!referencedType->IsStructure())
                            throw ModuleException("Expected structure reference.");

                        // Cast the referenced type.
                        building = static_cast<const Structure*> (referencedType);
                    }

                    // Find the value field.
                    Field *valueField = building->GetField("__value");
                    if(!valueField)
                        valueField = building->GetField("m_value");
                    if(!valueField)
                        throw ModuleException("Cannot extract primitive value from a structure without __value or m_value field.");

                    // Extract the primitive.
                    llvm::Value *value = NULL;
                    if(refType)
                    {
                        // Get the field pointer.
                        llvm::Value *fieldPtr = builder.CreateStructGEP(a1.value, valueField->GetStructureIndex());

                        // Load the field.
                        value = AttachPosition(builder.CreateLoad(fieldPtr));
                    }
                    else
                    {
                        // Extract the value.
                        value = AttachPosition(builder.CreateExtractValue(a1.value, valueField->GetStructureIndex()));
                    }

                    // Return the result.
                    const ChelaType *fieldType = valueField->GetType();
                    Push(value, fieldType);
                }
                break;
            case OpCode::SizeOf:
                {
                    // Read the type to extract the size.
                    const ChelaType *valueType = GetType(reader.ReadTypeId());
                    llvm::Value *size = llvm::ConstantExpr::getSizeOf(valueType->GetTargetType());
                    size = builder.CreateIntCast(size, sizeTy, false);
                    Push(size, ChelaType::GetSizeType(vm));
                }
                break;
            case OpCode::TypeOf:
                {
                    // Read the type.
                    const ChelaType *type = GetType(reader.ReadTypeId());

                    // Get the Type type
                    Class *typeClass = vm->GetTypeClass();
                    const ChelaType *refType = ReferenceType::Create(typeClass);

                    // Return the type info.
                    llvm::Value *typeinfo = builder.CreatePointerCast(module->GetTypeInfo(type), refType->GetTargetType());
                    Push(AttachPosition(typeinfo), refType);
                }
                break;
            case OpCode::DeleteObject:
                {
                    // Get the object.
                    a1 = Pop();

                    // Check permissions.
                    Unsafe(op);
                    
                    // His type must be a pointer.
                    if(!a1.type->IsPointer())
                        throw ModuleException("Only pointers can be deleted.");
                        
                    // Cast into void*
                    llvm::Value *pointer = AttachPosition(
                        builder.CreatePointerCast(a1.value,
                            ChelaType::GetVoidPointerType(vm)->GetTargetType()));
                    
                    // Create the call.
                    AttachPosition(builder.CreateCall(vm->GetUnmanagedFree(module), pointer));
                }
                break;
            case OpCode::NewArray:
                {
                    // Read the array type.
                    const ChelaType *rawArrayType = GetType(reader.ReadTypeId());
                    if(!rawArrayType->IsArray())
                        throw ModuleException("Expected array type.");
                    const ArrayType *arrayType = static_cast<const ArrayType*> (rawArrayType);

                    // Check permissions.
                    if(arrayType->IsUnsafe())
                        Unsafe(op);

                    // Get the element type.
                    const ChelaType *elementType = arrayType->GetActualValueType();

                    // Read the lengths.
                    llvm::Type *lengthTy = builder.getInt64Ty();
                    std::vector<llvm::Value*> lengths;
                    lengths.resize(arrayType->GetDimensions());
                    for(int i = 0; i < arrayType->GetDimensions(); ++i)
                    {
                        // Read the length.
                        a1 = Pop();

                        // Make sure its an integer type.
                        if(!a1.type->IsInteger() || !a1.type->IsPrimitive())
                            throw ModuleException("Array size must be integer instead of " + a1.type->GetFullName());
                        //TODO: Support integer vectors.
                        llvm::Value *castedLength = builder.CreateIntCast(a1.value, lengthTy, false);
                        AttachPosition(castedLength);
                        lengths[lengths.size() - i - 1] = castedLength;
                    }

                    // Compute the total length.
                    llvm::Value *totalLength = lengths[0];
                    for(size_t i = 1; i < lengths.size(); ++i)
                        totalLength = builder.CreateMul(totalLength, lengths[i]);

                    // Compute the array structure size.
                    llvm::StructType *arrayStructTy = llvm::cast<llvm::StructType> (arrayType->GetTargetType());
                    llvm::Constant *structSize = llvm::ConstantExpr::getOffsetOf(arrayStructTy, 2);
                    llvm::Constant *elementSize = llvm::ConstantExpr::getSizeOf(elementType->GetTargetType());
                    llvm::Value *elementsSize = AttachPosition(builder.CreateMul(totalLength, elementSize));
                    llvm::Value *allocSize = AttachPosition(builder.CreateAdd(structSize, elementsSize));
                    allocSize = builder.CreateIntCast(allocSize, sizeTy, false);

                    // Get the array type info.
                    llvm::Value *typeInfo = module->GetTypeInfo(arrayType);
                    typeInfo = AttachPosition(builder.CreatePointerCast(typeInfo, int8PtrTy));

                    // Allocate the array.
                    const ChelaType *arrayRefType = ReferenceType::Create(arrayType);
                    llvm::Value *array = AttachPosition(builder.CreateCall2(vm->GetManagedAlloc(module), allocSize, typeInfo));
                    array = AttachPosition(builder.CreatePointerCast(array, arrayRefType->GetTargetType()));

                    // Set the array lengths.
                    llvm::Value *lengthArrayPtr = AttachPosition(builder.CreateStructGEP(array, 1));
                    for(size_t i = 0; i < lengths.size(); ++i)
                    {
                        llvm::Value *lengthPtr = AttachPosition(builder.CreateConstInBoundsGEP2_32(lengthArrayPtr, 0, i));
                        llvm::Value *length = AttachPosition(builder.CreateIntCast(lengths[i], builder.getInt32Ty(), false));
                        AttachPosition(builder.CreateStore(length, lengthPtr));
                    }

                    // Create an intermediate for the object.
                    llvm::Value *inter = parent->CreateIntermediate(arrayRefType, true);
                    RefSetInter(builder, array, inter, arrayRefType);

                    // Push the array.
                    Push(array, arrayRefType);
                }
                break;
            case OpCode::NewRawObject:
                {
                    // Check permissions.
                    Unsafe(op);

                    // Get the object type.
                    const ChelaType *objectType = GetType(reader.ReadTypeId());
                    if(!objectType->IsPrimitive() && !objectType->IsPointer() && !objectType->IsStructure())
                        throw ModuleException("Expected primitive/pointer/struct type.");

                    // Cannot create abstract objects.
                    if(objectType->IsAbstract())
                        throw ModuleException("Cannot create abstract objects.");

                    // Get the constructor id and the number of arguments.
                    uint32_t ctorId = reader.ReadFunctionId();
                    if(objectType->IsStructure())
                    {
                        if(ctorId == 0)
                            throw ModuleException("Expected constructor to create a struct instance.");
                    }
                    else if(ctorId != 0)
                        throw ModuleException("Primitives/pointers hasn't constructors.");

                    // Read the number of argument.                 
                    int n = reader.ReadUI8();
                    if(ctorId == 0 && n != 0)
                        throw ModuleException("Unexpected construction arguments for primitives/pointers.");
                    
                    const ChelaType *referenceType = PointerType::Create(objectType);

                    // Perform unmanaged allocation.
                    llvm::Constant *size = llvm::ConstantExpr::getSizeOf(objectType->GetTargetType());
                    size = llvm::ConstantExpr::getIntegerCast(size, sizeTy, false);
                    llvm::Value *object = AttachPosition(builder.CreateCall(vm->GetUnmanagedAlloc(module), size));
                    
                    // Cast the void*.
                    object = AttachPosition(builder.CreatePointerCast(object, referenceType->GetTargetType()));
                    
                    // Create an intermediate for the object.
                    llvm::Value *inter = parent->CreateIntermediate(referenceType, true);
                    RefSetInter(builder, object, inter, referenceType);

                    // Return the object.
                    Push(object, referenceType);
                }
                break;
            case OpCode::NewStackArray:
                {
                    // Check permissions.
                    Unsafe(op);

                    // Get the number of elements.
                    a1 = Pop();

                    // Make sure the number of elements is integer.
                    if(!a1.type->IsInteger())
                        throw ModuleException("Array size must be integer.");

                    // Read the element type.
                    const ChelaType *elementType = GetType(reader.ReadTypeId());
                    if(!elementType->IsPrimitive() && !elementType->IsPointer() && !elementType->IsStructure())
                        throw ModuleException("Expected primitive/pointer/struct type.");
                    const ChelaType *arrayType = PointerType::Create(elementType);

                    // Compute the array size.
                    llvm::Value *numelements = builder.CreateIntCast(a1.value, builder.getInt64Ty(), !a1.type->IsUnsigned());

                    // Allocate the array.
                    llvm::Value *array = AttachPosition(builder.CreateAlloca(elementType->GetTargetType(), numelements));
                    array = AttachPosition(builder.CreatePointerCast(array, arrayType->GetTargetType()));

                    // Push the array.
                    Push(array, arrayType);
                }
                break;
            case OpCode::NewRawArray:
                {
                    // Check permissions.
                    Unsafe(op);

                    // Get the number of elements.
                    a1 = Pop();

                    // Make sure the number of elements is integer.
                    if(!a1.type->IsInteger())
                        throw ModuleException("Array size must be integer.");

                    // Read the element type.
                    const ChelaType *elementType = GetType(reader.ReadTypeId());
                    if(!elementType->IsPrimitive() && !elementType->IsPointer() && !elementType->IsStructure())
                        throw ModuleException("Expected primitive/pointer/struct type.");
                    const ChelaType *arrayType = PointerType::Create(elementType);

                    // Compute the array size.
                    llvm::Value *numelements = builder.CreateIntCast(a1.value, sizeTy, !a1.type->IsUnsigned());
                    llvm::Constant *elementSize = llvm::ConstantExpr::getSizeOf(elementType->GetTargetType());
                    elementSize = llvm::ConstantExpr::getIntegerCast(elementSize, sizeTy, false);

                    // Allocate the array.
                    llvm::Value *array = AttachPosition(builder.CreateCall2(vm->GetUnmanagedAllocArray(module), elementSize, numelements));
                    array = AttachPosition(builder.CreatePointerCast(array, arrayType->GetTargetType()));

                    // Push the array.
                    Push(array, arrayType);
                }
                break;
            case OpCode::DeleteRawArray:
                {
                    // Check permissions.
                    Unsafe(op);

                    // Get the object.
                    a1 = Pop();
                    
                    // His type must be a pointer.
                    if(!a1.type->IsPointer())
                        throw ModuleException("Only pointers can be deleted.");

                    // TODO: Invoke destructors.
                                        
                    // Cast into void*
                    llvm::Value *pointer = AttachPosition(
                        builder.CreatePointerCast(a1.value,
                            ChelaType::GetVoidPointerType(vm)->GetTargetType()));

                    // Create the call.
                    AttachPosition(builder.CreateCall(vm->GetUnmanagedFreeArray(module), pointer));
                }
                break;
            case OpCode::Throw:
                {
                    // Use only in functions that are handling exception.
                    if(!parent->IsHandlingExceptions())
                        throw ModuleException("throw cannot be used here.");

                    a1 = Pop();

                    // Check the exception type.
                    if(!a1.type->IsReference())
                        throw ModuleException("Expected reference operand for throw.");
                    const ReferenceType *refType = static_cast<const ReferenceType*> (a1.type);

                    // It must be a class instance.
                    const ChelaType *objectType = refType->GetReferencedType();
                    if(!objectType->IsClass())
                        throw ModuleException("Only class instances can be thrown.");

                    // Cast the reference.
                    llvm::Value *exception = AttachPosition(builder.CreatePointerCast(a1.value, builder.getInt8PtrTy()));

                    // Create the continue block.
                    llvm::BasicBlock *icont = CreateBlock("icont");

                    // Select the unwind destination.
                    llvm::BasicBlock *unwindDest;
                    if(exceptionContext != NULL)
                        unwindDest = exceptionContext->landingPad;
                    else
                        unwindDest = parent->GetTopLandingPad();

                    // Throw the exception.
                    llvm::InvokeInst *throwInst = builder.CreateInvoke(vm->GetEhThrow(module), icont, unwindDest, exception);
                    throwInst->setDoesNotReturn();
                    AttachPosition(throwInst);

                    // Make the continue block unreachable.
                    builder.SetInsertPoint(icont);
                    builder.CreateUnreachable();

                    // Ignore the rest of this block.
                    return;
                }
                break;
            case OpCode::Jmp:
                {
                    FunctionBlock *target = parent->GetBlock(reader.ReadBlock());
                    AttachPosition(builder.CreateBr(target->GetBlock()));

                    // Ignore the rest of this block.
                    return;
                }
                break;
            case OpCode::Br:
                {
                    FunctionBlock *trueTarget = parent->GetBlock(reader.ReadBlock());
                    FunctionBlock *falseTarget = parent->GetBlock(reader.ReadBlock());
                    a1 = Pop();
                    if(a1.type != ChelaType::GetBoolType(vm))
                        throw ModuleException("Expected boolean value.");
                    AttachPosition(builder.CreateCondBr(a1.value, trueTarget->GetBlock(), falseTarget->GetBlock()));

                    // Ignore the rest of this block.
                    return;
                }
                break;
            case OpCode::Switch:
                {
                    a1 = Pop();
                    // Read the jump table size.
                    int numcases = reader.ReadUI16() - 1;

                    // Read the default case.
                    reader.ReadUI32(); // Ignore default constant.
                    FunctionBlock *defaultTarget = parent->GetBlock(reader.ReadBlock());

                    // Check the argument type.
                    if(!a1.type->IsInteger())
                        throw ModuleException("Expected integer value.");

                    // Check the sign.
                    bool sign = !a1.type->IsUnsigned();

                    // Compute the common type.
                    const ChelaType *commonType = sign ? ChelaType::GetIntType(vm) : ChelaType::GetUIntType(vm);
                    llvm::IntegerType *targetType = (llvm::IntegerType*)commonType->GetTargetType();
                    llvm::Value *selection = a1.value;
                    if(a1.type != commonType)
                        selection = builder.CreateIntCast(selection, targetType, sign);

                    // Create the switch instruction.
                    llvm::SwitchInst *switchInst = builder.CreateSwitch(selection, defaultTarget->GetBlock(), numcases);
                    AttachPosition(switchInst);

                    // Add all of the cases.
                    std::set<uint32_t> cases;
                    for(int i = 0; i < numcases; i++)
                    {
                        // Read the constant.
                        llvm::ConstantInt *constant;
                        uint32_t caseConstant;
                        if(sign)
                        {
                            int32_t signedCase = reader.ReadI32();
                            caseConstant = signedCase;
                            constant = llvm::ConstantInt::get(targetType, signedCase, true);
                        }
                        else
                        {
                            caseConstant = reader.ReadUI32();
                            constant = llvm::ConstantInt::get(targetType, caseConstant, false);
                        }

                        // Don't allow duplicated cases.
                        if(!cases.insert(caseConstant).second)
                            throw ModuleException("switch with duplicated constants in " + parent->GetFullName());

                        // Read the case block.
                        FunctionBlock *caseBlock = parent->GetBlock(reader.ReadBlock());

                        // Append the case into the switch.
                        switchInst->addCase(constant, caseBlock->GetBlock());
                    }

                    // Ignore the rest of this block.
                    return;
                }
                break;
            case OpCode::JumpResume:
                {
                    // Use only in functions that are handling exception.
                    if(!parent->IsHandlingExceptions())
                        throw ModuleException("jmprs cannot be used here.");

                    // Load the exception caught flag.
                    llvm::Value *caught = AttachPosition(builder.CreateLoad(parent->GetExceptionCaught()));
                    llvm::Value *exception = AttachPosition(builder.CreateLoad(parent->GetExceptionLocal()));

                    // Check if propagating.
                    llvm::Value *notPropagating = AttachPosition(builder.CreateOr(caught, builder.CreateIsNull(exception)));

                    // Compute the not resume dest.
                    llvm::BasicBlock *resumeDest;
                    if(exceptionContext != NULL)
                        resumeDest = exceptionContext->selectionPad;
                    else
                        resumeDest = parent->GetTopCleanup();

                    // Branch to the correct destination.
                    FunctionBlock *target = parent->GetBlock(reader.ReadBlock());
                    AttachPosition(builder.CreateCondBr(notPropagating, target->GetBlock(), resumeDest));

                    // Ignore the rest of this block.
                    return;
                }
                break;
            case OpCode::Call:
                {
                    Member *target = GetFunction(reader.ReadFunctionId());

                    // Get the called function.
                    Function *calledFunction = static_cast<Function*> (target);

                    // Check permissions.
                    if(calledFunction->IsUnsafe())
                        Unsafe(op);

                    // Add a dependency to the function.
                    parent->AddDependency(calledFunction);

                    // Don't invoke contract/abstract functions.
                    if(calledFunction->IsContract() || calledFunction->IsAbstract())
                        throw ModuleException("Cannot call directly abstract functions.");
                        
                    // Get his type.
                    const FunctionType *calledType = static_cast<const FunctionType*> (calledFunction->GetType());

                    // Read the number of arguments.
                    int n = reader.ReadUI8();

                    // Check the return type.
                    const ChelaType *retType = calledType->GetReturnType();

                    // Check for implicit references.
                    bool needInter = retType->IsComplexStructure() || retType->HasSubReferences();
                    if(!needInter && retType->IsReference())
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (retType);
                        const ChelaType *objectType = refType->GetReferencedType();
                        needInter = objectType->IsPassedByReference();
                    }

                    // Create and intermediate for the return type.
                    llvm::Value *retInter = NULL;
                    if(needInter)
                        retInter = parent->CreateIntermediate(retType, true);

                    // Create the return pointer.
                    llvm::Value *retPtr = retType->IsComplexStructure() ? retInter : NULL;

                    // Perform the call.
                    llvm::Value *ret = CreateCall(builder, calledFunction->GetTarget(), calledType, n, retPtr);

                    // Push the return value.
                    if(retType != ChelaType::GetVoidType(vm))
                    {
                        if(needInter)
                        {
                            // Set the intermediate.
                            if(!retType->IsStructure())
                                RefSetInter(builder, ret, retInter, retType);

                            // Push the intermediate.
                            if(retType->IsStructure())
                                Push(retInter, ReferenceType::Create(retType));
                            else
                                Push(ret, retType);
                        }
                        else
                        {
                            Push(ret, retType);
                        }
                    }
                }
                break;
            case OpCode::CallVirtual:
                {
                    // Get the target function
                    Member *target = GetFunction(reader.ReadFunctionId());

                    // Get the called function.
                    Function *functionDecl = static_cast<Function*> (target);

                    // Check permissions.
                    if(functionDecl->IsUnsafe())
                        Unsafe(op);

                    // Read the vslot.
                    uint16_t vslot = functionDecl->GetVSlot();

                    // Read the number of arguments.
                    size_t n = reader.ReadUI8();

                    // Read the object.
                    if(stack.size() < n)
                        throw ModuleException("Operand stack overflow");
                    a1 = stack[stack.size() - n];
                    llvm::Value *selfPtr = a1.value;
                    const ChelaType *selfType = a1.type;

                    // Read the structure.
                    const Structure *building;
                    if(selfType->IsReference())
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (a1.type);
                        const ChelaType *referencedType = refType->GetReferencedType();
                        if(referencedType->IsArray())
                            referencedType = vm->GetArrayClass();
                        if(!referencedType->IsClass() && !referencedType->IsInterface() && !referencedType->IsBoxed())
                            throw ModuleException("Only classes/interface are passed by reference in vcalls.");

                        if(referencedType->IsBoxed())
                        {
                            const BoxedType *boxed = static_cast<const BoxedType*> (referencedType);
                            referencedType = boxed->GetValueType();
                            if(!referencedType->IsStructure())
                                throw ModuleException("Invalid boxed type, bug in virtual machine or hacking attempt.");
                        }

                        building = static_cast<const Structure*> (referencedType);
                    }
                    else
                    {
                        throw ModuleException("Expected reference.");
                    }
                    
                    // Get the virtual method.
                    Function *vmethod = building->GetVMethod(vslot);

                    // Get his type.
                    const FunctionType *calledType = static_cast<const FunctionType*> (vmethod->GetType());

                    // TODO: Make sure the vmethod is actually in the building.

                    // The vtable is the first slot, always.
                    llvm::Value *vtablePtr = builder.CreateConstGEP2_32(a1.value, 0, 0);
                    llvm::Value *vtable = AttachPosition(builder.CreateLoad(vtablePtr));
                    
                    // Read the vtable slot.
                    // TODO: Perform safety checks.
                    llvm::Value *vtableSlotPtr = builder.CreateConstGEP2_32(vtable, 0, vslot + 2);
                    llvm::Value *vtableSlot = AttachPosition(builder.CreateLoad(vtableSlotPtr));

                    // Offset the self pointer.
                    if(building->IsInterface())
                    {
                        llvm::Value *selfOffsetPtr = builder.CreateStructGEP(vtable, 1);
                        llvm::Value *selfOffset = builder.CreateLoad(selfOffsetPtr);
                        selfPtr = builder.CreatePointerCast(a1.value, int8PtrTy);
                        selfPtr = builder.CreateGEP(selfPtr, builder.CreateNeg(selfOffset));

                        selfType = InstanceType(calledType->GetArgument(0));
                        selfPtr = builder.CreatePointerCast(selfPtr, selfType->GetTargetType());
                    }

                    // Check the return type.
                    const ChelaType *retType = calledType->GetReturnType();

                    // Check for implicit references.
                    bool needInter = retType->IsComplexStructure() || retType->HasSubReferences();
                    if(!needInter && retType->IsReference())
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (retType);
                        const ChelaType *objectType = refType->GetReferencedType();
                        needInter = objectType->IsPassedByReference();
                    }

                    // Create and intermediate for the return type.
                    llvm::Value *retInter = NULL;
                    if(needInter)
                        retInter = parent->CreateIntermediate(retType, true);

                    // Create the return pointer.
                    llvm::Value *retPtr = retType->IsComplexStructure() ? retInter : NULL;

                    // Perform the call.
                    llvm::Value *ret = CreateCall(builder, vtableSlot, calledType, n-1, retPtr, selfPtr, selfType);

                    // Remove the self pointer
                    Pop();

                    // Push the return value.
                    if(retType != ChelaType::GetVoidType(vm))
                    {
                        if(needInter)
                        {
                            // Set the intermediate.
                            if(!retType->IsComplexStructure())
                                RefSetInter(builder, ret, retInter, retType);

                            // Push the intermediate.
                            if(retType->IsStructure())
                                Push(retInter, ReferenceType::Create(retType));
                            else
                                Push(ret, retType);
                        }
                        else
                        {
                            Push(ret, retType);
                        }
                    }
                }
                break;
            case OpCode::CallIndirect:
                {
                    // Read the number of arguments.
                    size_t n = reader.ReadUI8();

                    // Read the function object.
                    if(stack.size() < n)
                        throw ModuleException("Operand stack overflow");
                    a1 = stack[stack.size() - n - 1];

                    // Read the structure.
                    llvm::Value *functionPointer;
                    const FunctionType *functionType;
                    if(a1.type->IsReference())
                    {
                        throw ModuleException("Unimplemented delegates.");
                    }
                    else if(a1.type->IsPointer())
                    {
                        // Make sure its a function pointer.
                        const ChelaType *pointedType = DePointerType(a1.type);
                        if(!pointedType->IsFunction())
                            throw ModuleException("Expected function pointer.");

                        // Treat as a function pointer.
                        functionType = static_cast<const FunctionType*> (pointedType);
                        functionPointer = a1.value;
                    }
                    else
                    {
                        throw ModuleException("Expected delegate reference or function pointer.");
                    }

                    // Check permissions.
                    if(a1.type->IsUnsafe())
                        Unsafe(op);

                    // Check the return type.
                    const ChelaType *retType = functionType->GetReturnType();

                    // Check for implicit references.
                    bool needInter = retType->IsComplexStructure() || retType->HasSubReferences();
                    if(!needInter && retType->IsReference())
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (retType);
                        const ChelaType *objectType = refType->GetReferencedType();
                        needInter = objectType->IsPassedByReference();
                    }

                    // Create and intermediate for the return type.
                    llvm::Value *retInter = NULL;
                    if(needInter)
                        retInter = parent->CreateIntermediate(retType, true);

                    // Create the return pointer.
                    llvm::Value *retPtr = retType->IsComplexStructure() ? retInter : NULL;

                    // Perform the call.
                    llvm::Value *ret = CreateCall(builder, functionPointer, functionType, n, retPtr);

                    // Pop the function object.
                    Pop();

                    // Push the return value.
                    if(retType != ChelaType::GetVoidType(vm))
                    {
                        if(needInter)
                        {
                            // Set the intermediate.
                            if(!retType->IsComplexStructure())
                                RefSetInter(builder, ret, retInter, retType);

                            // Push the intermediate.
                            if(retType->IsStructure())
                                Push(retInter, ReferenceType::Create(retType));
                            else
                                Push(ret, retType);
                        }
                        else
                        {
                            Push(ret, retType);
                        }
                    }
                }
                break;
            case OpCode::BindKernel:
                {
                    // Read the delegate type, and the function.
                    const ChelaType *delegateType = GetType(reader.ReadTypeId());
                    Function *target = GetFunction(reader.ReadFunctionId());

                    // Check permissions.
                    if(delegateType->IsUnsafe() || target->IsUnsafe())
                        Unsafe(op);

                    // The delegate type must be class.
                    if(!delegateType->IsClass())
                        throw ModuleException("Delegate type must be a class.");

                    // TODO: Make sure the delegate is derived from Chela.Lang.Delegate

                    // Give reference layer to the delegate type.
                    delegateType = ReferenceType::Create(delegateType);

                    // Add a dependency to the function.
                    parent->AddDependency(target);

                    // Make sure its an entry point kernel.
                    if(!target->IsKernel())
                        throw ModuleException("Expected a kernel function.");

                    // Get his type.
                    const FunctionType *calledType = target->GetKernelBinderType();
                    if(!calledType)
                        throw ModuleException("Expected an entry point kernel.");

                    // Use the arguments in the function type.
                    int n = calledType->GetArgumentCount();

                    // Get the return type.
                    const ChelaType *retType = calledType->GetReturnType();

                    // The return type is always a delegate.
                    llvm::Value *retInter = parent->CreateIntermediate(retType, true);

                    // Perform the call.
                    llvm::Value *ret = CreateCall(builder, target->GetKernelBinder(), calledType, n, NULL);

                    // Set the intermediate.
                    if(!retType->IsStructure())
                        RefSetInter(builder, ret, retInter, retType);

                    // Cast the delegate.
                    if(retType != delegateType)
                        CastInstruction(builder, ret, retType, delegateType);
                    else
                        Push(ret, delegateType);
                }
                break;
            case OpCode::Ret:
                {
                    a1 = Pop();
                    const ChelaType *returnType = parentType->GetReturnType();
    
                    // Increase the reference value before returning.
                    const ChelaType *retValueType = a1.type;
                    if(retValueType->IsReference())
                    {
                        const ReferenceType *refType = static_cast<const ReferenceType*> (a1.type);
                        const ChelaType *objectType = refType->GetReferencedType();
                        if(objectType->IsReference() || objectType->IsPassedByReference())
                            AddRef(builder, a1.value);
                        else if(objectType->HasSubReferences())
                        {
                            // Increase sub references.
                            const Structure *building = static_cast<const Structure*> (objectType);
                            size_t numrefs = building->GetValueRefCount();
                            for(size_t i = 0; i < numrefs; ++i)
                            {
                                // Get the reference field.
                                const Field *field = building->GetValueRefField(i);

                                // Load the field value.
                                size_t fieldIndex = field->GetStructureIndex();
                                llvm::Value *fieldPtr = builder.CreateStructGEP(a1.value, fieldIndex);
                                llvm::Value *fieldValue = AttachPosition(builder.CreateLoad(fieldPtr));

                                // Add the reference.
                                AddRef(builder, fieldValue);
                            }
                        }
                    }
                    else if(retValueType->HasSubReferences() && !parent->GetReturnPointer())
                    {
                        // Increase sub references.
                        const Structure *building = static_cast<const Structure*> (retValueType);
                        size_t numrefs = building->GetValueRefCount();
                        for(size_t i = 0; i < numrefs; ++i)
                        {
                            // Get the reference field.
                            const Field *field = building->GetValueRefField(i);

                            // Load the field value.
                            size_t fieldIndex = field->GetStructureIndex();
                            llvm::Value *fieldValue = AttachPosition(builder.CreateExtractValue(a1.value, fieldIndex));

                            // Add the reference.
                            AddRef(builder, fieldValue);
                        }
                    }
    
                    Return(builder, MakeCompatible(builder, a1.value, a1.type, returnType));
                }
                break;
            case OpCode::RetVoid:
                Return(builder, NULL);
                break;
            case OpCode::Checked:
                checked = true;
                break;
            case OpCode::Push:
                {
                    uint8_t off = reader.ReadUI8();
                    if(stack.empty() || off >= stack.size())
                        throw ModuleException("Stack overflow.");

                    // Get the element to push.
                    StackValue &toPush = stack[stack.size() - off - 1];

                    // Check the element permissions.
                    if(toPush.type->IsUnsafe())
                        Unsafe(op);

                    // Push element.
                    stack.push_back(toPush);
                }
                break;
            case OpCode::Pop:
                a1 = Pop();
                break;
            case OpCode::Dup:
                {
                    uint8_t n = reader.ReadUI8();
                    size_t oldSize = stack.size();
                    if(n > oldSize)
                        throw ModuleException("Stack overflow.");

                    // Push element by element.
                    for(size_t i = oldSize - n; i < oldSize; i++)
                    {
                        // Get the element to push.
                        StackValue &toPush = stack[i];
    
                        // Check the element permissions.
                        if(toPush.type->IsUnsafe())
                            Unsafe(op);
    
                        // Push element.
                        stack.push_back(toPush);
                    }
                }
                break;
            case OpCode::Dup1:
                // Make sure there's at least one element in the stack.
                if(stack.empty())
                    throw ModuleException("Stack overflow.");

                // Check the element permissions.
                if(stack.back().type->IsUnsafe())
                    Unsafe(op);

                // Push the element.
                stack.push_back(stack.back());
                break;
            case OpCode::Dup2:
                {
                    size_t oldSize = stack.size();
                    if(oldSize < 2)
                        throw ModuleException("Stack overflow.");

                    // Check the elements permissions.
                    if(stack[oldSize-2].type->IsUnsafe() ||
                       stack[oldSize-1].type->IsUnsafe())
                        Unsafe(op);

                    // Push the elements.
                    stack.push_back(stack[oldSize-2]);
                    stack.push_back(stack[oldSize-1]);
                }
                break;
            case OpCode::Remove:
                {
                    uint8_t off = reader.ReadUI8();
                    if(stack.empty() || off >= stack.size())
                        throw ModuleException("Stack overflow.");

                    // Remove element.
                    size_t index = stack.size() - off - 1;
                    stack.erase(stack.begin() + index);
                }
                break;
            default:
                // Ignore unimplemented opcodes.
                if(op < OpCode::Invalid)
                    printf("Warning ignoring opcode '%s'.\n", InstructionTable[op].mnemonic);
                reader.Next();
            }

            // Reset the instruction flags.
            checked = false;
        }
    }

    void FunctionBlock::Return(llvm::IRBuilder<> &builder, llvm::Value *retValue)
    {
        // Store the return value and set the finish cleanup flag.
        this->returnValue = retValue;
        finishCleanup = true;
    }

    void FunctionBlock::Finish(llvm::IRBuilder<> &builder)
    {
        if(!finishCleanup)
            return;

        // Set the insert point.
        builder.SetInsertPoint(GetLastBlock());

        // Return the structure value.
        llvm::Value *returnPointer = parent->GetReturnPointer();
        if(returnPointer)
        {
            const ChelaType *retType = parent->GetFunctionType()->GetReturnType();
            RefSet(builder, returnValue, returnPointer, retType);
        }

        // Perform cleanup.
        parent->PerformCleanup();

        // Perform return.
        if(returnValue && !returnPointer)
            AttachPosition(builder.CreateRet(returnValue));
        else
            AttachPosition(builder.CreateRetVoid());
    }

    void FunctionBlock::Push(const StackValue &value)
    {
        stack.push_back(value);
    }

    void FunctionBlock::Push(llvm::Value *value, const ChelaType *type)
    {
        stack.push_back(StackValue(value, type));
    }
    
    FunctionBlock::StackValue FunctionBlock::Pop()
    {
        if(stack.empty())
            throw ModuleException("Stack overflow.");
        StackValue ret = stack.back();
        stack.pop_back();
        return ret;
    }

    void FunctionBlock::PrepareDebugEmition()
    {
        // Get the module debug information.
        DebugInformation *debugInfo = module->GetDebugInformation();
        if(!debugInfo)
            return;

        // Get the function top lexical scope.
        llvm::MDNode *topLexicalScope = parent->GetTopLexicalScope();
        if(!topLexicalScope)
            return;

        // Get the debug function.
        FunctionDebugInfo *functionDebug = debugInfo->GetFunctionDebug(parent->GetMemberId());
        if(!functionDebug)
            return;

        // Get the debug block.
        blockDebugInfo = functionDebug->GetBlock(blockId);
    }

    void FunctionBlock::BeginDebugInstruction(size_t index)
    {
        // The block debug info must be available.
        //printf("block debug %p\n", blockDebugInfo);
        if(!blockDebugInfo)
            return;

        // Check if the instruction is in the current range.
        if(currentDebugRange != NULL &&
            index >= currentDebugRange->GetStart() &&
            index <= currentDebugRange->GetEnd())
            return;

        // Reset the debug range.
        currentDebugRange = NULL;

        // Find a debug range with the instruction.
        size_t numranges = blockDebugInfo->GetRangeCount();
        for(size_t i = 0; i < numranges; ++i)
        {
            // Select the range is the instruction is there.
            const BasicBlockRangePosition *range = &blockDebugInfo->GetRange(i);
            if(index >= range->GetStart() &&
               index <= range->GetEnd())
            {
                currentDebugRange = range;
                return;
            }
        }
    }

    llvm::Value *FunctionBlock::AttachPosition(llvm::Value *value)
    {
        // Check the current debug range
        if(currentDebugRange == NULL)
            return value;

        // Cast the value into an instruction.
        llvm::Instruction *inst = llvm::dyn_cast<llvm::Instruction> (value);
        if(!inst)
            return value;

        // TODO: Use a more apropiate scope.
        llvm::DIDescriptor scope = parent->GetTopLexicalScope();

        // Attach the debug location.
        const SourcePosition &position = currentDebugRange->GetPosition();
        inst->setDebugLoc(llvm::DebugLoc::get(position.GetLine(), position.GetColumn(), scope));
        return value;
    }

    void FunctionBlock::CalculateDependencies()
    {
        InstructionReader reader(instructions, rawblockSize);
        while(!reader.End())
        {
            int op = reader.ReadOpCode();
            
            // Only interested in jmp, br and jmprs.
            if(op == OpCode::Jmp || op == OpCode::Br || op == OpCode::JumpResume)
            {
                // Get the jump target.
                FunctionBlock *target = parent->GetBlock(reader.ReadBlock());
                
                // Store it as a successor.
                successor.insert(target);
                
                // Visit the target only once.
                bool visitTarget = target->predecessors.empty();
                target->predecessors.insert(this);
                if(visitTarget)
                    target->CalculateDependencies();
                    
                // Visit the false target.
                if(op == OpCode::Br)
                {
                    // Get the false target.
                    target = parent->GetBlock(reader.ReadBlock());

                    // Store it as a successor.
                    successor.insert(target);
                    
                    // Visit the false target only once.
                    visitTarget = target->predecessors.empty();
                    target->predecessors.insert(this);
                    if(visitTarget)
                        target->CalculateDependencies();
                }
            }
            else if(op == OpCode::Switch)
            {
                // Read the number of targets.
                size_t numtargets = reader.ReadUI16();

                // Visit each one of the targets.
                for(size_t i = 0; i < numtargets; ++i)
                {
                    // Ignore the constant.
                    reader.ReadUI32();

                    // Read the target.
                    FunctionBlock *target = parent->GetBlock(reader.ReadBlock());

                    // Store it as a successor.
                    successor.insert(target);

                    // Visit the target only once.
                    bool visitTarget = target->predecessors.empty();
                    target->predecessors.insert(this);
                    if(visitTarget)
                        target->CalculateDependencies();
                }
            }
            else
            {
                reader.Next();
            }            
        }
    }

    FunctionBlock *FunctionBlock::InstanceGeneric(Function *newParent, const GenericInstance *instance)
    {
        // Create the new block.
        FunctionBlock *block = new FunctionBlock(newParent, blockId);

        // Copy the instructions.
        block->blockDataOffset = blockDataOffset;
        block->rawblockSize = rawblockSize;
        block->instructionCount = instructionCount;
        block->instructions = instructions;

        // Return the block.
        return block;
    }
}
