#ifndef CHELAVM_NAMESPACE_HPP
#define CHELAVM_NAMESPACE_HPP

#include <vector>
#include "ChelaVm/Member.hpp"
#include "ChelaVm/Module.hpp"

namespace ChelaVm
{
	class Namespace: public Member
	{
	public:
		Namespace(Module *module);
		~Namespace();

		virtual std::string GetName() const;
        virtual MemberFlags GetFlags() const;
		
		virtual bool IsNamespace() const;

        static Namespace *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        virtual void ReadStructure(ModuleReader &reader, const MemberHeader &header);

        virtual llvm::DIDescriptor GetDebugNode(DebugInformation *context) const;

	private:
		uint32_t name;
		std::vector<Member*> members;
	};
}

#endif //CHELAVM_NAMESPACE_HPP
