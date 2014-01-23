#include "ChelaVm/MemberInstance.hpp"
#include "ChelaVm/ModuleFile.hpp"
#include "ChelaVm/Module.hpp"
#include "ChelaVm/TypeInstance.hpp"

namespace ChelaVm
{
    MemberInstance::MemberInstance(Module *module)
        : Member(module)
    {
        actualMember = NULL;
    }

    MemberInstance::~MemberInstance()
    {
    }

    bool MemberInstance::IsAnonymous() const
    {
        return true;
    }

    bool MemberInstance::IsGeneric() const
    {
        return factoryMember->IsGeneric();
    }

    bool MemberInstance::IsMemberInstance() const
    {
        return true;
    }

    Member *MemberInstance::GetTemplateMember() const
    {
        return templateMember;
    }

    Member *MemberInstance::GetActualMember() const
    {
        // Return the cached member,
        if(actualMember != NULL)
            return actualMember;

        // The factory must be a type instance.
        if(!factoryMember->IsTypeInstance())
            throw ModuleException("Factory member must be a type instance.");

        // Request the actual member.
        TypeInstance *type = static_cast<TypeInstance*> (factoryMember);
        actualMember = type->GetInstancedMember(templateMember);
        return actualMember;
    }

    Member *MemberInstance::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the member instance.
        MemberInstance *res = new MemberInstance(module);

        // Store the name and the flags.
        //res->name = header.memberName;
        //res->flags = (MemberFlags)header.memberFlags;

        // Skip the data.
        reader.Skip(header.memberSize);

        // Return the member.
        return res;

    }

    void MemberInstance::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the factory and template id.
        uint32_t templateId, factoryId;
        reader >> templateId >> factoryId;

        // Get the template and the factory.
        Module *module = GetModule();
        templateMember = module->GetMember(templateId);
        factoryMember = module->GetMember(factoryId);
        if(!templateMember || !factoryMember)
        {
            printf("tmpl %d fact %d\n", templateId, factoryId);
            throw ModuleException("Invalid member instance.");
        }
    }

    Member *MemberInstance::InstanceMember(Member */*factory*/, Module *implementingModule, const GenericInstance *instance)
    {
        // Only instance if I'm generic.
        if(!IsGeneric())
            return this;

        // Instance the factory.
        Member *factory = factoryMember->InstanceMember(implementingModule, instance);
        if(factory->IsTypeInstance())
        {
            // Request the actual member.
            TypeInstance *type = static_cast<TypeInstance*> (factory);
            return type->GetInstancedMember(templateMember);
        }
        else if(factory->IsStructure() || factory->IsClass() || factory->IsInterface())
        {
            Structure *building = static_cast<Structure*> (factory);
            building->DeclarePass();
            return building->GetInstancedMember(templateMember);
        }
        else
            throw ModuleException("Invalid factory member must be a type instance.");


    }
}
