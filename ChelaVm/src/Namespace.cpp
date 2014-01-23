#include "ChelaVm/Namespace.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "llvm/Support/Dwarf.h"

using namespace llvm::dwarf;
using llvm::LLVMDebugVersion;

namespace ChelaVm
{
	Namespace::Namespace(Module *module)
		: Member(module)
	{
		name = 0;
	}
	
	Namespace::~Namespace()
	{
	}
	
	std::string Namespace::GetName() const
	{
        Module *module = GetModule();
		return module->GetString(name);
	}

    MemberFlags Namespace::GetFlags() const
    {
        return MFL_Public;
    }
	
	bool Namespace::IsNamespace() const
	{
		return true;
	}

    Namespace *Namespace::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the namespace.
        Namespace *res = new Namespace(module);

        // Read the name.
        res->name = header.memberName;

        // Skip the member data.
        reader.Skip(header.memberSize);
        return res;
    }
    
    void Namespace::ReadStructure(ModuleReader &reader, const MemberHeader &header)
	{
		// Skip the attributes.
        SkipAttributes(reader, header.memberAttributes);

		// Read the namespace members.
        Module *module = GetModule();
		int count = header.memberSize/4;
		for(int i = 0; i < count; i++)
		{
            // Read the member id.
			uint32_t id;
			reader >> id;

            // Get the member.
            Member *member = module->GetMember(id);
			members.push_back(member);

            // Update my child parent.
            if(member)
                member->UpdateParent(this);
		}
	}

    llvm::DIDescriptor Namespace::GetDebugNode(DebugInformation *context) const
    {
        // Return the cached debug info.
        llvm::DIDescriptor debugNode = context->GetMemberNode(this);
        if(debugNode)
            return debugNode;

        // Don't create a node for unnamed namespaces.
        if(GetName().empty())
        {
            // Use the parent node.
            debugNode = Member::GetDebugNode(context);
            return debugNode;
        }

        // Use the parent debug node.
        debugNode = Member::GetDebugNode(context);

        // Create the debug node.
        debugNode = context->GetDebugBuilder()
            .createNameSpace(debugNode, GetName(), context->GetModuleFileDescriptor(), 0);
        context->RegisterMemberNode(this, debugNode);
        return debugNode;
    }
};
