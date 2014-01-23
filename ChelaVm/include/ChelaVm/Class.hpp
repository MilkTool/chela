#ifndef CHELAVM_CLASS_HPP
#define CHELAVM_CLASS_HPP

#include "Structure.hpp"

namespace ChelaVm
{
    // Keep this sync with the garbage collector.
    const unsigned int ConstantRefData = 1;

	class Class: public Structure
	{
	public:
		Class(Module *module);
		~Class();
		
		virtual bool IsStructure() const;
		virtual bool IsClass() const;
        virtual bool IsInterface() const;
        virtual bool IsPassedByReference() const;

        static Class *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        
    protected:
        virtual Structure *CreateBuilding(Module *module) const;
	};
};

#endif //CHELAVM_CLASS_HPP
