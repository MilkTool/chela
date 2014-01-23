#include "ChelaVm/Closure.hpp"
#include "ChelaVm/VirtualMachine.hpp"

namespace ChelaVm
{
    Closure::Closure(Module *module)
        : Class(module)
    {
        parentClosure = NULL;
        name = "closure";
    }

    Closure::~Closure()
    {
    }

    bool Closure::IsClosure() const
    {
        return true;
    }

    Field *Closure::GetParentClosure()
    {
        return parentClosure;
    }

    size_t Closure::GetLocalCount()
    {
        return locals.size();
    }

    Field *Closure::GetLocal(size_t id)
    {
        if(id >= locals.size())
            throw ModuleException("Invalid closure local id.");
        return locals[id];
    }

    void Closure::Read(ModuleReader &reader)
    {
        // Use Chela.Lang.Closure as a base class.
        Module *module = GetModule();
        baseStructure = module->GetVirtualMachine()->GetClosureClass();

        // Read the parent closure type.
        uint32_t parentTypeId;
        reader >> parentTypeId;
        const ChelaType *parentType = module->GetType(parentTypeId);
        if(!parentType->IsVoid())
        {
            // Create the parent field.
            Field *parentField = new Field(module);
            parentField->SetType(parentType);
            parentField->UpdateParent(this);
            parentClosure = parentField;

            // Store the parent as a member.
            members.push_back(parentClosure);
        }

        // Read the actual locals.
        uint8_t localCount;
        reader >> localCount;
        for(size_t i = 0; i < localCount; ++i)
        {
            // Read the local type
            uint32_t localTypeId;
            reader >> localTypeId;
            const ChelaType *localType = module->GetType(localTypeId);

            // Create the local field.
            Field *local = new Field(module);
            local->SetType(localType);
            local->UpdateParent(this);
            locals.push_back(local);
        }

        // Store the locals as members.
        for(size_t i = 0; i < locals.size(); ++i)
            members.push_back(locals[i]);
    }

    Closure *Closure::Create(Module *module, Closure *parent, std::vector<const ChelaType*> &locals)
    {
        // Create the closure.
        Closure *res = new Closure(module);

        // Use Chela.Lang.Closure as a base class.
        Class *baseClass = module->GetVirtualMachine()->GetClosureClass();
        res->baseStructure = baseClass;
        res->vmethods.resize(baseClass->GetVMethodCount());

        // Set the parent closure.
        if(parent)
        {
            // Create the parent field.
            Field *parentField = new Field(module);
            parentField->SetType(ReferenceType::Create(parent));
            parentField->UpdateParent(res);

            // Store the parent field.
            res->parentClosure = parentField;
            res->members.push_back(parentField);
        }

        // Create the locals fields.
        for(size_t i = 0; i < locals.size(); ++i)
        {
            // Create the local field.
            Field *local = new Field(module);
            local->SetType(locals[i]);
            local->UpdateParent(res);

            // Store the field.
            res->locals.push_back(local);
            res->members.push_back(local);
        }

        // Return the created closure.
        return res;
    }
}
