#include "ChelaVm/Class.hpp"

namespace ChelaVm
{
	Class::Class(Module *module)
		: Structure(module)
	{
	}
	
	Class::~Class()
	{
	}
		
	bool Class::IsStructure() const
	{
		return false;
	}
	
	bool Class::IsClass() const
	{
		return true;
	}

    bool Class::IsInterface() const
    {
        return false;
    }

    bool Class::IsPassedByReference() const
    {
        return true;
    }

    Class *Class::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        Class *res = new Class(module);

        // Store the name and the member flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Skip the member data.
        reader.Skip(header.memberSize);
        return res;
    }

    Structure *Class::CreateBuilding(Module *module) const
    {
        return new Class(module);
    }
};
