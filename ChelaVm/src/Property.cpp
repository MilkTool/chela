#include "ChelaVm/Class.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/Property.hpp"

namespace ChelaVm
{
    Property::Property(Module *module)
        : Member(module)
    {
        propertyInfo = NULL;
        getAccessor = NULL;
        setAccessor = NULL;
        createdMemberInfo = false;
        genericInstance = NULL;
    }

    Property::~Property()
    {
    }

    std::string Property::GetName() const
    {
        return name;
    }

    MemberFlags Property::GetFlags() const
    {
        return flags;
    }

    const ChelaType *Property::GetType() const
    {
        return type;
    }

    Function *Property::GetGetAccessor() const
    {
        return getAccessor;
    }

    Function *Property::GetSetAccessor() const
    {
        return setAccessor;
    }

    bool Property::IsGeneric() const
    {
        if(genericInstance != NULL && genericInstance->IsGeneric())
            return true;

        return IsParentGeneric() ||
            type->IsGeneric();
    }

    bool Property::IsProperty() const
    {
        return true;
    }

    void Property::DeclarePass()
    {
        // Declare the get accessor.
        if(getAccessor != NULL)
            getAccessor->DeclarePass();

        // Declare the set accessor.
        if(setAccessor != NULL)
            setAccessor->DeclarePass();
    }

    Property *Property::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        Property *res = new Property(module);

        // Store the member name and flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Skip the member data.
        reader.Skip(header.memberSize);
        return res;
    }

    void Property::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Get the module.
        Module *module = GetModule();

        // Read the member attributes.
        ReadAttributes(reader, header.memberAttributes);

        // Read the type, and the number of indices.
        uint8_t numindices;
        uint32_t typeId;
        reader >> typeId >> numindices;

        // Read the indices
        for(size_t i = 0; i < numindices; ++i)
        {
            uint32_t indexTypeId;
            reader >> indexTypeId;
            indices.push_back(module->GetType(indexTypeId));
        }

        // Read the get and set accessor.
        uint32_t getId, setId;
        reader >> getId >> setId;

        // Get the type, getter and setter.
        type = module->GetType(typeId);
        if(getId != 0)
            getAccessor = module->GetFunction(getId);
        if(setId != 0)
            setAccessor = module->GetFunction(setId);
    }

    llvm::GlobalVariable *Property::GetMemberInfo()
    {
        if(!propertyInfo)
        {
            // Only create the property info variable when reflection is enabled.
            Module *module = GetModule();
            if(!module->HasReflection())
                return NULL;

            // Get the property info class.
            VirtualMachine *vm = module->GetVirtualMachine();
            Class *propInfoClass = vm->GetPropertyInfoClass();
            llvm::Module *targetModule = module->GetTargetModule();
            propertyInfo = new llvm::GlobalVariable(*targetModule, propInfoClass->GetTargetType(),
                                    false, ComputeMetadataLinkage(), NULL, GetMangledName() + "_propinfo_");
        }

        return propertyInfo;
    }

    void Property::DefinitionPass()
    {
        // Only create once the member info.
        if(createdMemberInfo)
            return;
        createdMemberInfo = true;

        // Only create the reflection info in modules that support it.    
        Module *module = GetModule();
        if(!module->HasReflection())
            return;

        // Get the property info class.
        VirtualMachine *vm = module->GetVirtualMachine();
        Class *propInfoClass = vm->GetPropertyInfoClass();

        // Create the property info value.
        ConstantStructurePtr propInfoValue(propInfoClass->CreateConstant(module));

        // Store the MemberInfo attributes.
        SetMemberInfoData(propInfoValue);

        // Set the "getMethod" field.
        if(getAccessor)
            propInfoValue->SetField("getMethod", getAccessor->GetMemberInfo());

        // Set the "setMethod" field.
        if(setAccessor)
            propInfoValue->SetField("setMethod", setAccessor->GetMemberInfo());

        // Set the property type field.
        propInfoValue->SetField("propertyType", GetReflectedType(GetType()));

        // Set the field info.
        GetMemberInfo()->setInitializer(propInfoValue->Finish());

        // Define the custom attributes.
        DefineAttributes();
    }

    Member *Property::InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        // Create the instanced property.
        Property *instanced = new Property(implementingModule);

        // Use the same name and flags.
        instanced->name = name;
        instanced->flags = flags;
        instanced->genericInstance = instance;
        instanced->UpdateParent(factory);
        
        // Instance the property type.
        instanced->type = type->InstanceGeneric(instance);

        // Instance the getter.
        if(getAccessor)
            instanced->getAccessor = getAccessor->InstanceGeneric(factory, implementingModule, instance);

        // Instance the setter.
        if(setAccessor)
            instanced->setAccessor = setAccessor->InstanceGeneric(factory, implementingModule, instance);

        // Return the property.
        return instanced;
    }
};
