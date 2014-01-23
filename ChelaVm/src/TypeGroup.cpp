#include "ChelaVm/TypeGroup.hpp"
#include "ChelaVm/ModuleFile.hpp"

namespace ChelaVm
{
    TypeGroup::TypeGroup(Module *module)
        : Member(module)
    {
        isInstance = false;
    }

    TypeGroup::~TypeGroup()
    {
    }

    std::string TypeGroup::GetName() const
    {
        return name;
    }

    void TypeGroup::UpdateParent(Member *parent)
    {
        // Update my parent.
        Member::UpdateParent(parent);

        // Update my children parent.
        for(size_t i = 0; i < buildings.size(); i++)
        {
            Structure *child = buildings[i];
            if(!child)
                continue;

            child->UpdateParent(parent);
        }
    }

    bool TypeGroup::IsTypeGroup() const
    {
        return true;
    }

    size_t TypeGroup::GetTypeCount() const
    {
        return buildings.size();
    }

    Structure *TypeGroup::GetType(size_t index)
    {
        if(index >= buildings.size())
            throw ModuleException("Invalid type index.");
        return buildings[index];
    }

    uint32_t TypeGroup::GetTypeId(size_t index)
    {
        if(index >= buildings.size())
            throw ModuleException("Invalid type index.");
        return buildings[index]->GetMemberId();
    }

    TypeGroup *TypeGroup::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        TypeGroup *res = new TypeGroup(module);

        // Store the group name.
        res->name = module->GetString(header.memberName) + " <TG> ";

        // Skip the group data.
        reader.Skip(header.memberSize);
        return res;
    }

    void TypeGroup::ReadStructure(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the group members.
        Member *parent = GetParent();
        Module *module = GetModule();
        int count = header.memberSize/4;
        for(int i = 0; i < count; i++)
        {
            uint32_t id;
            reader >> id;

            // Get the member and check it..
            Member *member = module->GetMember(id);
            if(!member->IsClass() && !member->IsStructure() && !member->IsInterface())
                throw ModuleException("expected class/structure/interface member in type group.");

            // Store the member.
            buildings.push_back(static_cast<Structure*> (member));

            // Update his parent.
            if(member && parent)
                member->UpdateParent(parent);
        }
    }

    void TypeGroup::DeclarePass()
    {
        if(!isInstance)
            return;

        for(size_t i = 0; i < buildings.size(); ++i)
        {
            Structure *building = buildings[i];
            if(building)
                building->DeclarePass();
        }
    }

    void TypeGroup::DefinitionPass()
    {
        if(!isInstance)
            return;

        for(size_t i = 0; i < buildings.size(); ++i)
        {
            Structure *building = buildings[i];
            if(building)
                building->DefinitionPass();
        }
    }

    Member *TypeGroup::InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        // TODO: Correct this please.
        Error("Unimplemented TypeGroup instance.");
        return NULL;
    }
}

