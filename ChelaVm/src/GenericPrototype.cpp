#include "ChelaVm/GenericPrototype.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    GenericPrototype::GenericPrototype(Module *module)
        : module(module)
    {
    }

    GenericPrototype::~GenericPrototype()
    {
    }

    std::string GenericPrototype::GetFullName() const
    {
        // Return empty string if there aren't plceholders.
        if(placeHolders.empty())
            return std::string();

        // The buffer to create the string.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << '<';

        // Add each one of the place holders.
        for(size_t i = 0; i < placeHolders.size(); ++i)
        {
            out << placeHolders[i]->GetFullName();
            if(i + 1 < placeHolders.size())
                out << ',';
        }

        // Add the suffix.
        out << '>';

        return out.str();
    }

    std::string GenericPrototype::GetMangledName() const
    {
        // Return empty string for now.
        return std::string();
    }

    size_t GenericPrototype::GetPlaceHolderCount() const
    {
        return placeHolders.size();
    }

    const PlaceHolderType *GenericPrototype::GetPlaceHolder(size_t index) const
    {
        if(index >= placeHolders.size())
            throw ModuleException("Invalid generic placeholder index.");
        return placeHolders[index];
    }

    void GenericPrototype::AppendPrototype(const GenericPrototype *other)
    {
        if(!other || other == this)
            return;
            
        for(size_t i = 0; i < other->placeHolders.size(); ++i)
            placeHolders.push_back(other->placeHolders[i]);
    }

    void GenericPrototype::Read(ModuleReader &reader)
    {
        // Read the count.
        uint8_t count;
        reader >> count;

        // Read the place holder types.
        placeHolders.reserve(count);
        for(size_t i = 0; i < count; ++i)
        {
            // Read the placeholder id
            uint32_t placeHolderId;
            reader >> placeHolderId;

            // Get the place holder.
            const ChelaType *type = module->GetType(placeHolderId);
            if(!type->IsPlaceHolder())
                throw ModuleException("expected place holder type.");

            // Cast and store the placeholder.
            const PlaceHolderType *placeHolder = static_cast<const PlaceHolderType*> (type);
            placeHolders.push_back(placeHolder);
        }
    }

    void GenericPrototype::Skip(ModuleReader &reader)
    {
        // Read the count.
        uint8_t count;
        reader >> count;

        // Ignore the placeholders.
        reader.Skip(count*4);
    }
}
