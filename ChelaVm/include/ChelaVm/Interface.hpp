#ifndef CHELAVM_INTERFACE_HPP
#define CHELAVM_INTERFACE_HPP

#include "Structure.hpp"

namespace ChelaVm
{
    class Interface: public Structure
    {
    public:
        Interface(Module *module);
        ~Interface();

        virtual bool IsStructure() const;
        virtual bool IsClass() const;
        virtual bool IsInterface() const;
        virtual bool IsPassedByReference() const;

        static Interface *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        
    protected:
        virtual Structure *CreateBuilding(Module *module) const;
    };
};

#endif //CHELAVM_INTERFACE_HPP
