#ifndef _CHELAVM_GENERIC_PROTOTYPE_HPP
#define _CHELAVM_GENERIC_PROTOTYPE_HPP

#include "ChelaVm/Module.hpp"
#include "ChelaVm/ModuleReader.hpp"
#include "ChelaVm/PlaceHolderType.hpp"

namespace ChelaVm
{
    class GenericPrototype
    {
    public:
        GenericPrototype(Module *module);
        ~GenericPrototype();

        std::string GetFullName() const;
        std::string GetMangledName() const;

        size_t GetPlaceHolderCount() const;
        const PlaceHolderType *GetPlaceHolder(size_t index) const;

        void AppendPrototype(const GenericPrototype *other);

        void Read(ModuleReader &reader);
        static void Skip(ModuleReader &reader);

    private:
        Module *module;
        std::vector<const PlaceHolderType*> placeHolders;
    };
}

#endif //_CHELAVM_GENERIC_PROTOTYPE_HPP
