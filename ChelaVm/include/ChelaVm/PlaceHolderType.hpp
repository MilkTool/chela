#ifndef _CHELAVM_PLACEHOLDER_TYPE_HPP
#define _CHELAVM_PLACEHOLDER_TYPE_HPP

#include "ChelaVm/Module.hpp"
#include "ChelaVm/Types.hpp"

namespace ChelaVm
{
    class Module;
    class PlaceHolderType: public ChelaType
    {
    public:
        PlaceHolderType(Module *module, const std::string &name, uint32_t id, bool valueType,
                        const std::vector<Structure*> &bases);

        virtual std::string GetName() const;
        virtual std::string GetMangledName() const;
        virtual std::string GetFullName() const;

        virtual bool IsGenericType() const;
        virtual bool IsPlaceHolder() const;
        virtual bool IsValueType() const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;

        virtual llvm::Type *GetTargetType() const;
        
        size_t GetBaseCount() const;
        Structure *GetBase(size_t base) const;

    private:
        std::string name;
        uint32_t id;
        bool valueType;
        std::vector<Structure*> bases;
    };
}

#endif //_CHELAVM_PLACEHOLDER_TYPE_HPP
