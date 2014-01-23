#include "ChelaVm/TypeInstance.hpp"
#include "ChelaVm/ModuleFile.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    TypeInstance::TypeInstance(Module *module)
        : ChelaType(module), instance(module)
    {
        factoryMember = NULL;
        originalTemplate = NULL;
        originalInstance = NULL;
        implementation = NULL;
        readedData = false;
        rawData = NULL;
        rawDataSize = 0;
    }

    TypeInstance::~TypeInstance()
    {
        delete [] rawData;
    }

    std::string TypeInstance::GetName() const
    {
        return originalTemplate->GetName() + instance.GetFullName();
    }

    std::string TypeInstance::GetFullName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the factory member.
        if(factoryMember)
            out << factoryMember->GetFullName() << '.';

        // Add the template name.
        out << originalTemplate->GetName();

        // Add the instance data.
        out << instance.GetFullName();
        return out.str();
    }

    std::string TypeInstance::GetMangledName() const
    {
        const Structure *impl = GetImplementation();
        if(impl != NULL)
            return GetImplementation()->GetMangledName();

        return originalTemplate->GetMangledName() + instance.GetMangledName();
    }

    bool TypeInstance::IsAnonymous() const
    {
        return true;
    }

    bool TypeInstance::IsTypeInstance() const
    {
        return true;
    }

    bool TypeInstance::IsGenericType() const
    {
        // Handle type instances dependening in another type-instances.
        if(!originalTemplate)
        {
            ReadData();
            if(!originalTemplate)
                Error("Cyclic type instance readed.");
        }

        Member *factory = GetFactory();
        if(factory && factory->IsGeneric())
            return true;
        return instance.IsGeneric();
    }

    bool TypeInstance::IsGeneric() const
    {
        return IsGenericType();
    }

    void TypeInstance::Preload() const
    {
        // Preload the instance of the template.
        if(!implementation)
        {
            Module *module = GetModule();
            implementation = GetOriginal()->PreloadGeneric(GetFactory(), module, &instance);
        }
    }

    Member *TypeInstance::GetFactory() const
    {
        if(factoryMember && factoryMember->IsTypeInstance())
        {
            TypeInstance *factoryInstance = static_cast<TypeInstance*> (factoryMember);
            return factoryInstance->GetImplementation();
        }

        if(originalTemplate)
            return originalTemplate->GetParent();
        return NULL;
    }

    Structure *TypeInstance::GetOriginal() const
    {
        if(!originalInstance)
        {
            if(factoryMember && factoryMember->IsTypeInstance())
            {
                TypeInstance *factoryInstance = static_cast<TypeInstance*> (factoryMember);
                originalInstance = static_cast<Structure*> (factoryInstance->GetInstancedMember(originalTemplate->GetTemplateMember()));
                if(!originalInstance)
                    Error("Failed to find the instanced version of member " + originalTemplate->GetFullName());
            }
            else
            {
                originalInstance = originalTemplate;
            }
        }

        return originalInstance;
    }

    Structure *TypeInstance::GetImplementation()
    {
        Preload();
        return implementation;
    }

    const Structure *TypeInstance::GetImplementation() const
    {
        Preload();
        return implementation;
    }

    const GenericInstance &TypeInstance::GetGenericInstance() const
    {
        return instance;
    }

    Member *TypeInstance::GetInstancedMember(Member *templateMember) const
    {
        // Here the implementation must be declared.
        const_cast<TypeInstance*> (this)->DeclarePass();
        return GetImplementation()->GetInstancedMember(templateMember);
    }

    llvm::Type *TypeInstance::GetTargetType() const
    {
        if(IsGeneric())
            return NULL;

        return GetImplementation()->GetTargetType();
    }

    void TypeInstance::DeclarePass()
    {
        if(IsGeneric())
            return;

        // Check the prototype.
        instance.CheckPrototype();

        // Make sure the original is declared.
        GetOriginal()->DeclarePass();

        // Declare the implementation.
        GetImplementation()->DeclarePass();
    }

    void TypeInstance::DefinitionPass()
    {
        if(IsGeneric())
            return;

        // Define the implementation.
        GetImplementation()->DefinitionPass();
    }

    TypeInstance *TypeInstance::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the type instance.
        TypeInstance *res = new TypeInstance(module);

        // Store the raw data for delayed loading.
        res->rawDataSize = header.memberSize;
        res->rawData = new uint8_t[res->rawDataSize];
        reader.Read(res->rawData, res->rawDataSize);

        return res;
    }

    void TypeInstance::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Skip the data and read using prereaded data.
        reader.Skip(header.memberSize);
        ReadData();
    }

    void TypeInstance::ReadData() const
    {
        // Only read once.
        if(readedData)
            return;
        readedData = true;

        // Use the already readed data.
        ModuleReader reader(rawDataSize, rawData);

        // Read the template id.
        uint32_t templateId, factoryId;
        reader >> templateId >> factoryId;

        // Get the original structure.
        Module *module = GetModule();
        Member *member = module->GetMember(templateId);
        if(!member || (!member->IsStructure() && !member->IsClass() && !member->IsInterface()))
            throw ModuleException("expected structure/class/interface for type instance.");
        originalTemplate = static_cast<Structure*> (member);

        // Read the factory member.
        if(factoryId)
            factoryMember = module->GetMember(factoryId);

        // Read the generic instance
        instance.Read(reader);

        // Use the original generic prototype.
        instance.SetPrototype(originalTemplate->GetGenericPrototype());
    }

    Member *TypeInstance::InstanceMember(Member */*factory*/, Module *implementingModule, const GenericInstance *instance)
    {
        // Instance the instance.
        GenericInstance newInstance(implementingModule);
        newInstance.InstanceFrom(this->instance, instance);

        // Try to instance my factory.
        Member *oldFactory = GetFactory();
        Member *newFactory = oldFactory;
        Structure *newOriginal = NULL;
        if(oldFactory != NULL && oldFactory->IsGeneric())
        {
            // Instance the factory.
            Member *newFactory = oldFactory->InstanceMember(oldFactory->GetParent(), implementingModule, instance);
            if(!newFactory->IsStructure() && !newFactory->IsClass() && !newFactory->IsInterface())
                Error("Invalid kind of type factory.");

            // Find the new original.
            Structure *newFactoryStruct = static_cast<Structure*> (newFactory);
            newOriginal = static_cast<Structure*> (newFactoryStruct->GetInstancedMember(originalTemplate->GetTemplateMember()));
            if(!newOriginal)
                Error("Failed to get member instance of factory instance: " + newFactory->GetFullName());
        }
        else
        {
            // Use the already computed original.
            newOriginal = GetOriginal();
        }

        // Instance the type.
        return newOriginal->PreloadGeneric(newFactory, implementingModule, &newInstance);
    }

    const ChelaType *TypeInstance::InstanceGeneric(const GenericInstance *instanceData) const
    {
        // Use the instance member.
        Module *newModule = instanceData->GetModule();
        return static_cast<ChelaType*> (const_cast<TypeInstance*> (this)->InstanceMember(NULL, newModule, instanceData));
    }
}
