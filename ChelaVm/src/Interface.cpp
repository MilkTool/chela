#include "ChelaVm/Interface.hpp"

namespace ChelaVm
{
    Interface::Interface(Module *module)
        : Structure(module)
    {
    }
    
    Interface::~Interface()
    {
    }
        
    bool Interface::IsStructure() const
    {
        return false;
    }
    
    bool Interface::IsClass() const
    {
        return false;
    }

    bool Interface::IsInterface() const
    {
        return true;
    }

    bool Interface::IsPassedByReference() const
    {
        return true;
    }

    Interface *Interface::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        Interface *res = new Interface(module);

        // Store the name and the member flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Skip the member data.
        reader.Skip(header.memberSize);
        return res;
    }

    Structure *Interface::CreateBuilding(Module *module) const
    {
        return new Interface(module);
    }
};

