#include "ChelaVm/FunctionGroup.hpp"

namespace ChelaVm
{
    FunctionGroup::FunctionGroup(Module *module)
        : Member(module)
    {
        isInstance = false;
    }
    
    FunctionGroup::~FunctionGroup()
    {
    }
    
    std::string FunctionGroup::GetName() const
    {
        return name;
    }
    
    bool FunctionGroup::IsFunctionGroup() const
    {
        return true;
    }
    
    void FunctionGroup::UpdateParent(Member *parent)
    {
        // Update my parent.
        Member::UpdateParent(parent);
        
        // Update my children parent.
        for(size_t i = 0; i < functions.size(); i++)
        {
            Function *child = functions[i];
            if(!child)
                continue;

            child->UpdateParent(parent);
        }
    }
    
    size_t FunctionGroup::GetFunctionCount() const
    {
        return functions.size();
    }
    
    Function *FunctionGroup::GetFunction(size_t index)
    {
        if(index >= functions.size())
            throw ModuleException("Invalid function index.");
        return functions[index];
    }
    
    uint32_t FunctionGroup::GetFunctionId(size_t index)
    {
        if(index >= functions.size())
            throw ModuleException("Invalid function index.");
        return functions[index]->GetMemberId();
    }

    FunctionGroup *FunctionGroup::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        FunctionGroup *res = new FunctionGroup(module);

        // Store the group name.
        res->name = module->GetString(header.memberName);

        // Skip the group data.
        reader.Skip(header.memberSize);
        return res;
    }

    void FunctionGroup::ReadStructure(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the group members.
        Member *parent = GetParent();
        Module *module = GetModule();
        int count = header.memberSize/4;
        for(int i = 0; i < count; i++)
        {
            // Store the function in the group.
            uint32_t id;
            reader >> id;
            Function *function = module->GetFunction(id);
            functions.push_back(function);

            // Update the function parent.
            if(function && parent)
                function->UpdateParent(parent);
        }
    }

    void FunctionGroup::DeclarePass()
    {
        if(!isInstance)
            return;

        for(size_t i = 0; i < functions.size(); ++i)
        {
            Function *function = functions[i];
            if(function)
                function->DeclarePass();
        }
    }

    void FunctionGroup::DefinitionPass()
    {
        if(!isInstance)
            return;

        for(size_t i = 0; i < functions.size(); ++i)
        {
            Function *function = functions[i];
            if(function)
                function->DefinitionPass();
        }
    }

    Member *FunctionGroup::InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        // Create the new group.
        FunctionGroup *group = new FunctionGroup(implementingModule);
        group->name = name;
        group->isInstance = true;
        group->UpdateParent(factory);

        // Instance the functions.
        for(size_t i = 0; i < functions.size(); ++i)
        {
            Function *function = functions[i];
            if(!function)
                continue;

            // Instance the function,
            Member *instanced = function->InstanceMember(factory, implementingModule, instance);
            group->functions.push_back(static_cast<Function*> (instanced));
        }

        return group;
    }

};
