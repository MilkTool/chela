#include "ChelaVm/Field.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/AttributeConstant.hpp"
#include "llvm/Constants.h"

namespace ChelaVm
{
    Field::Field(Module *module)
        : Member(module), initData(module)
    {
        flags = MFL_Default;
        isRefCounted = false;
        isGeneric = false;
        structureIndex = 0;
        globalVariable = NULL;
        fieldInfo = NULL;
        declared = false;
        type = NULL;
        fieldTemplate = NULL;
    }
    
    Field::~Field()
    {
    }

    std::string Field::GetName() const
    {
        return name;
    }

    MemberFlags Field::GetFlags() const
    {
        return flags;
    }

    const ChelaType *Field::GetType() const
    {
        return type;
    }

    void Field::SetType(const ChelaType *type)
    {
        this->type = type;
    }
    
    bool Field::IsField() const
    {
        return true;
    }

    bool Field::IsGeneric() const
    {
        isGeneric = isGeneric || type->IsGenericType() ||
            IsParentGeneric();
        return isGeneric;
    }
    
    void Field::DeclarePass()
    {
        // Declare just one.
        if(IsGeneric() || declared)
            return;
        declared = true;

        // Get the module and the virtual machine.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();

        // Check for TLS.
        bool threadLocal = GetCustomAttribute(vm->GetThreadStaticAttribute());
        AttributeConstant *chelaIntrinsic = GetCustomAttribute(vm->GetChelaIntrinsicAttribute());

        // Check safetyness.
        const ChelaType *fieldType = GetType();
        if(!IsUnsafe() && fieldType->IsUnsafe())
            Error("safe field with unsafe type.");

        // Field cannot be void.
        if(fieldType == ChelaType::GetVoidType(vm) ||
            fieldType == ChelaType::GetConstVoidType(vm))
            Error("variables cannot be void.");

        // Check for ref counting.
        bool isConstant = fieldType->IsConstant();
        if(fieldType->IsReference())
        {
            // Get the actual type.
            const ReferenceType *refType = static_cast<const ReferenceType*> (fieldType);
            const ChelaType *objectType = refType->GetReferencedType();

            // Only class/interfaces are referenced counter.
            if(objectType->IsPassedByReference())
            {
                isRefCounted = true;
            }
        }
        else
        {
            // Use sub-references.
            isRefCounted = fieldType->HasSubReferences();
        }

        // De-const.
        if(isConstant)
        {
            const ConstantType *constType = static_cast<const ConstantType*> (fieldType);
            fieldType = constType->GetValueType();
        }

        if(!IsStatic())
        {
            if(threadLocal)
                Warning("Thread local storage is only available in static variables.");
            return;
        }

        // Compute the global linkage.
        llvm::Function::LinkageTypes linkage = ComputeLinkage();

        // Get the variable type.
        llvm::Type *variableType = fieldType->GetTargetType();

        // Create the variable.
        globalVariable =
            new llvm::GlobalVariable(variableType,
                isConstant, linkage, 0, GetMangledName());
        globalVariable->setThreadLocal(threadLocal);
        //globalVariable->setVisibility(llvm::GlobalValue::ProtectedVisibility);
        //globalVariable->dump();

        // Use the value type for enumerations.
        const ChelaType *valueType = fieldType;
        if(fieldType->IsStructure() && isConstant)
        {
            // Make sure the field structure is a enumeration.
            Class *enumClass = vm->GetEnumClass();
            const Structure *fieldStruct = static_cast<const Structure*> (fieldType);
            if(!fieldStruct->IsDerivedFrom(enumClass))
                Error("cannot have constant structure.");

            // Get the enumeration value field.
            Field *valueField = fieldStruct->GetField("__value");
            if(!valueField)
                valueField = fieldStruct->GetField("m_value");
            if(!valueField)
                Error("invalid enumeration definition.");
            valueType = valueField->GetType();
            if(!valueType->IsPrimitive())
                Error("enumeration type must be primitive.");
        }

        // Create initializer.
        llvm::Constant *initializer = initData.GetTargetConstant();
        if(initializer != NULL)
        {
            if(valueType != initData.GetChelaType())
                Error("incompatible field initialization type.");

            // Wrap the value in a structure.
            if(fieldType != valueType)
            {
                std::vector<llvm::Constant*> layout;
                layout.push_back(initializer);
                llvm::StructType *structTy = static_cast<llvm::StructType*> (fieldType->GetTargetType());
                initializer = llvm::ConstantStruct::get(structTy, layout);
            }
        }
        else if(chelaIntrinsic && chelaIntrinsic->GetArgumentCount() > 0)
        {
            // This is used for platform specific constants.
            ConstantValue *name = chelaIntrinsic->GetArgument(0);
            if(name != NULL && name->GetType() == CVT_String)
            {
                // Get the intrinsic value.
                initializer = GetIntrinsicConstant(name->GetStringValue(), valueType->GetTargetType());

                // Wrap the value in a structure.
                if(fieldType != valueType)
                {
                    std::vector<llvm::Constant*> layout;
                    layout.push_back(initializer);
                    llvm::StructType *structTy = static_cast<llvm::StructType*> (fieldType->GetTargetType());
                    initializer = llvm::ConstantStruct::get(structTy, layout);
                }
            }
            else
            {
                // Initialize with null.
                initializer = llvm::Constant::getNullValue(variableType);
            }
        }
        else
        {
            initializer = llvm::Constant::getNullValue(variableType);
        }

        // Set the initializer.
        globalVariable->setInitializer(initializer);

        // Register it with the module.
        llvm::Module *mod = module->GetTargetModule();
        mod->getGlobalList().push_back(globalVariable);

        // Create the debug information.
        GetDebugNode(module->GetDebugInformation());
    }

    llvm::GlobalVariable *Field::GetGlobalVariable() const
    {
        return globalVariable;
    }

    bool Field::IsRefCounted()
    {
        return isRefCounted;
    }

    void Field::SetRefCounted(bool refCounted)
    {
        this->isRefCounted = refCounted;
    }

    uint32_t Field::GetStructureIndex() const
    {
        return structureIndex;
    }

    void Field::SetStructureIndex(uint32_t index)
    {
        structureIndex = index;
    }

    Field *Field::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        Field *res = new Field(module);

        // Store the name and the flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Skip the data.
        reader.Skip(header.memberSize);
        return res;
    }
    
    void Field::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the member attributes.
        ReadAttributes(reader, header.memberAttributes);
        
        // Read the type id and slot.
        uint32_t typeId, slot;
        reader >> typeId >> slot;

        // Get the type.
        type = GetModule()->GetType(typeId);

        // Read the initializer.
        size_t initDataSize = header.memberSize - 8;
        if(initDataSize > 0)
            initData.Read(reader);
    }

    llvm::DIDescriptor Field::GetDebugNode(DebugInformation *context) const
    {
        // Don't create debug information for no static fields.
        if(!context || !IsStatic())
            return llvm::DIDescriptor();

        // Return the cached debug node.
        llvm::DIDescriptor debugNode = context->GetMemberNode(this);
        if(debugNode)
            return debugNode;

        // Get the debug information.
        DebugInformation *debugInfo = GetDeclaringModule()->GetDebugInformation();
        if(!debugInfo)
            return debugNode;

        // Get the field debug info.
        FieldDebugInfo *fieldInfo = debugInfo->GetFieldDebug(GetMemberId());
        if(!fieldInfo)
            return debugNode;

        // Get the debug info builder.
        llvm::DIBuilder &diBuilder = debugInfo->GetDebugBuilder();

        // Create the debug node.
        const SourcePosition &position = fieldInfo->GetPosition();
        debugNode = diBuilder.createGlobalVariable(GetName(),
                debugInfo->GetFileDescriptor(position.GetFileName()),
                position.GetLine(), type->GetDebugType(debugInfo), false, globalVariable);
        context->RegisterMemberNode(this, debugNode);


        // Return the created debug node.
        return debugNode;
    }

    llvm::GlobalVariable *Field::GetMemberInfo()
    {
        if(!fieldInfo)
        {
            // Get the module and check if reflection is enabled.
            Module *module = GetModule();
            if(!module->HasReflection())
                return NULL;

            // Get the target field data.
            VirtualMachine *vm = module->GetVirtualMachine();
            Class *fieldInfoClass = vm->GetFieldInfoClass();
            llvm::Module *targetModule = module->GetTargetModule();

            // Create the field info
            fieldInfo = new llvm::GlobalVariable(*targetModule, fieldInfoClass->GetTargetType(),
                                    false, ComputeMetadataLinkage(), NULL, GetMangledName() + "_fieldinfo_");
        }

        return fieldInfo;
    }

    void Field::DefinitionPass()
    {
        // Only generate field info if reflection is enabled.
        Module *module = GetModule();
        if(!module->HasReflection())
            return;

        // Get the field info class.
        VirtualMachine *vm = module->GetVirtualMachine();
        Class *fieldInfoClass = vm->GetFieldInfoClass();

        // Create the field info value.
        ConstantStructurePtr fieldInfoValue(fieldInfoClass->CreateConstant(module));

        // Store the MemberInfo attributes.
        SetMemberInfoData(fieldInfoValue);

        // Set the field type.
        fieldInfoValue->SetField("type", GetReflectedType(type));

        // Set the field pointer.
        if(IsStatic() && !IsGeneric())
        {
            // Store the global variable.
            fieldInfoValue->SetField("fieldPointer", globalVariable);
        }
        else if(!IsGeneric())
        {
            // Get the parent.
            Member *parent = GetParent();
            if(!parent->IsStructure() && !parent->IsClass())
                throw ModuleException("No static fields must be in a structure or a class.");
            Structure *building = static_cast<Structure*> (parent);

            // Get the field offset.
            llvm::Type *intType = llvm::Type::getInt32Ty(GetLlvmContext());
            std::vector<llvm::Constant*> fieldPtrIdx;
            fieldPtrIdx.push_back(llvm::ConstantInt::get(intType, 0));

            // Add the box index
            if(!building->IsClass())
                fieldPtrIdx.push_back(llvm::ConstantInt::get(intType, building->GetBoxIndex()));

            // Add the field index.
            fieldPtrIdx.push_back(llvm::ConstantInt::get(intType, structureIndex));

            // Get the field offset.
            llvm::Constant *fieldOffset = llvm::ConstantExpr::getGetElementPtr(
                llvm::Constant::getNullValue(llvm::PointerType::getUnqual(building->GetBoxedType())),
                fieldPtrIdx);

            // Set the field pointer equal to the field offset.
            fieldInfoValue->SetField("fieldPointer", fieldOffset);
        }

        // Set the field info.
        GetMemberInfo()->setInitializer(fieldInfoValue->Finish());

        // Define the custom attributes.
        DefineAttributes();
    }

    Member *Field::InstanceMember(Member *factory, Module *module, const GenericInstance *instance)
    {
        // Don't instance if I'm not generic
        if(!IsGeneric())
            return this;

        // Used the normalized instance.
        instance = instance->Normalize();

        // Use the correct template.
        Field *templateFld = fieldTemplate ? fieldTemplate : this;

        // Find an existing implementation in the using module.
        Field *impl = module->FindGenericImplementation(templateFld, instance);
        if(impl)
            return impl;

        // Create the instanced field.
        Field *field = new Field(module);
        field->UpdateParent(factory);
        field->fieldTemplate = templateFld;

        // Store the name, slot and flags.
        field->name = name;
        field->flags = flags;
        field->structureIndex = structureIndex;

        // Instance the field type.
        field->type = type->InstanceGeneric(instance);

        // Register the instanced field.
        module->RegisterGenericImplementation(templateFld, field, instance);

        // Return the instanced field.
        return field;
    }


    Member *Field::GetTemplateMember() const
    {
        return fieldTemplate;
    }

    llvm::Constant *Field::GetIntrinsicConstant(const std::string &name, llvm::Type *valueType)
    {
        VirtualMachine *vm = GetVM();

        if(name == "Eh.Dwarf.ExceptionRegister" && valueType->isIntegerTy())
        {
            // TODO: Find this value from the llvm target data.
            if(vm->IsX86() || vm->IsAmd64())
                return llvm::ConstantInt::get(valueType, 0);
            Error("Unsupported architecture for Eh.Dwarf");
        }
        else if(name == "Eh.Dwarf.SelectorRegister" && valueType->isIntegerTy())
        {
            // TODO: Find this value from the llvm target data.
            if(vm->IsX86())
                return llvm::ConstantInt::get(valueType, 2);
            else if(vm->IsAmd64())
                return llvm::ConstantInt::get(valueType, 1);
            Error("Unsupported architecture for Eh.Dwarf");
        }

        return llvm::Constant::getNullValue(valueType);
    }
};
