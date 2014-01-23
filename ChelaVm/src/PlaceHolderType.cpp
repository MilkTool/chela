#include "ChelaVm/PlaceHolderType.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    PlaceHolderType::PlaceHolderType(Module *module, const std::string &name, uint32_t id, bool valueType,
                        const std::vector<Structure*> &bases)
        : ChelaType(module), name(name), id(id), valueType(valueType), bases(bases)
    {
    }

    std::string PlaceHolderType::GetName() const
    {
        return name;
    }

    std::string PlaceHolderType::GetMangledName() const
    {
        // The buffer to construct the mangled name.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the mangled name.
        out << GetModule()->GetMangledName() << 'h' << id;
        return out.str();
    }

    std::string PlaceHolderType::GetFullName() const
    {
        // The buffer to construct the mangled name.
        llvm::SmallString<16> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the full name name.
        out << '<' << id << '>';
        return out.str();
    }

    bool PlaceHolderType::IsGenericType() const
    {
        return true;
    }

    bool PlaceHolderType::IsPlaceHolder() const
    {
        return true;
    }

    bool PlaceHolderType::IsValueType() const
    {
        return valueType;
    }

    const ChelaType *PlaceHolderType::InstanceGeneric(const GenericInstance *instance) const
    {
        // Find myself in the prototype.
        const GenericPrototype *prototype = instance->GetPrototype();
        for(size_t i = 0; i < prototype->GetPlaceHolderCount(); ++i)
        {
            if(prototype->GetPlaceHolder(i) == this)
            {
                const ChelaType *type = instance->GetArgument(i);
                if(type->IsPassedByReference())
                    return ReferenceType::Create(type);
                else
                    return type;
            }
        }

        // Not found.
        return this;
    }

    llvm::Type *PlaceHolderType::GetTargetType() const
    {
        Module *module = GetModule();
        return llvm::Type::getInt32Ty(module->GetLlvmContext());
    }

    size_t PlaceHolderType::GetBaseCount() const
    {
        return bases.size();
    }

    Structure *PlaceHolderType::GetBase(size_t base) const
    {
        return bases[base];
    }
}
