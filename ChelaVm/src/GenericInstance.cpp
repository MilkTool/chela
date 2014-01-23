#include "ChelaVm/GenericInstance.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    GenericInstance::GenericInstance(Module *module)
        : module(module)
    {
        prototype = NULL;
    }

    GenericInstance::~GenericInstance()
    {
    }

    GenericInstance *GenericInstance::Clone() const
    {
        GenericInstance *ret = new GenericInstance(module);
        ret->prototype = prototype;
        ret->arguments = arguments;
        return ret;
    }

    void GenericInstance::NormalizeTypes()
    {
        // Normalize each argument
        for(size_t i = 0; i < arguments.size(); ++i)
        {
            // Get the argument.
            const ChelaType *arg = arguments[i];

            // Remove unnecessary references.
            if(arg->IsReference())
            {
                arg = DeReferenceType(arg);
                // Place holder type adds it back automatically.
                if(arg->IsPassedByReference())
                    arguments[i] = arg;
            }
        }
    }

    GenericInstance *GenericInstance::Normalize() const
    {
        // Clone myself.
        GenericInstance *normalized = Clone();
        normalized->NormalizeTypes();
        return normalized;
    }

    GenericInstance *GenericInstance::Normalize(const GenericPrototype *generic) const
    {
        // Create the generic instance.
        GenericInstance *ret = new GenericInstance(module);
        ret->prototype = generic;

        // Instance each member of the prototype.
        if(prototype != generic)
        {
            for(size_t i = 0; i < generic->GetPlaceHolderCount(); ++i)
                ret->arguments.push_back(generic->GetPlaceHolder(i)->InstanceGeneric(this));
        }
        else
        {
            ret->arguments = arguments;
        }

        // Normalize and return.
        ret->NormalizeTypes();
        return ret;
    }

    Module *GenericInstance::GetModule() const
    {
        return module;
    }

    std::string GenericInstance::GetFullName() const
    {
        // Return empty string when no arguments.
        if(arguments.empty())
            return std::string();

        // The buffer to create the string.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << '<';

        // Add each type name.
        for(size_t i = 0; i < arguments.size(); ++i)
        {
            out << arguments[i]->GetFullName();
            if(i + 1 < arguments.size())
                out << ',';
        }

        // Add the suffix.
        out << '>';

        return out.str();
    }

    std::string GenericInstance::GetDebugName() const
    {
        return prototype->GetFullName() + " <- " + GetFullName();
    }

    std::string GenericInstance::GetMangledName() const
    {
        // Return empty string when no arguments.
        if(arguments.empty())
            return std::string();

        // The buffer to create the string.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << 't' << (int)arguments.size();

        // Add each type name.
        for(size_t i = 0; i < arguments.size(); ++i)
            out << arguments[i]->GetMangledName();

        return out.str();
    }

    const GenericPrototype *GenericInstance::GetPrototype() const
    {
        return prototype;
    }

    void GenericInstance::SetPrototype(const GenericPrototype *prototype)
    {
        this->prototype = prototype;
    }

    size_t GenericInstance::GetArgumentCount() const
    {
        return arguments.size();
    }

    const ChelaType *GenericInstance::GetArgument(size_t index) const
    {
        return arguments[index];
    }

    void GenericInstance::CheckPrototype()
    {
        // Argument count must match.
        if(arguments.size() != prototype->GetPlaceHolderCount())
            throw ModuleException("generic argument count doesn't match.");

        // TODO: Check for primitive classes.
    }

    void GenericInstance::Read(ModuleReader &reader)
    {
        // Read the argument count.
        uint8_t argCount;
        reader >> argCount;

        for(size_t i = 0; i < argCount; ++i)
        {
            // Read the argument.
            uint32_t argument;
            reader >> argument;

            // Store it.
            arguments.push_back(module->GetType(argument));
        }
    }

    bool GenericInstance::operator<(const GenericInstance &o) const
    {
        if(arguments.size() == o.arguments.size())
        {
            // Check for each argument.
            for(size_t i = 0; i < arguments.size(); ++i)
            {
                // Add the pass by reference.
                const ChelaType *left = arguments[i];
                //if(left->IsPassedByReference())
                //    left = ReferenceType::Create(left);

                // Add the pass by reference.
                const ChelaType *right = o.arguments[i];
                //if(right->IsPassedByReference())
                //    right = ReferenceType::Create(right);

                // Check differences.
                if(left != right) // Found a difference.
                    return left < right;
            }
        }

        // Check the arguments size.
        return arguments.size() < o.arguments.size();
    }

    bool GenericInstance_LessThan(const GenericInstance *a, const GenericInstance *b)
    {
        return *a < *b;
    }

    bool GenericInstance::IsGeneric() const
    {
        // HACK: Type instances aren't readed in the correct order.
        if(!prototype)
            return true;

        // The number of arguments must match.
        if(prototype->GetPlaceHolderCount() != GetArgumentCount())
            return true;

        // Check if at least one argument is generic.
        for(size_t i = 0; i < arguments.size(); ++i)
            if(arguments[i]->IsGenericType())
                return true;

        return false;
    }

    void GenericInstance::InstanceFrom(const GenericInstance &original, const GenericInstance *instanceData)
    {
        // Instance the arguments.
        arguments.clear();
        for(size_t i = 0; i < original.arguments.size(); ++i)
            arguments.push_back(original.arguments[i]->InstanceGeneric(instanceData));

        // Use the original prototype.
        prototype = original.prototype;

        // Normalize my types.
        NormalizeTypes();
    }

    void GenericInstance::AppendInstance(const GenericInstance *instance)
    {
        // Ignore NULL instances.
        if(instance == NULL)
            return;

        // Append the instance arguments.
        for(size_t i = 0; i < instance->arguments.size(); ++i)
            arguments.push_back(instance->arguments[i]);
    }

    GenericInstance *GenericInstance::Create(Module *module, Member *tmpl, const ChelaType** types, size_t numtypes)
    {
        GenericInstance *res = new GenericInstance(module);
        res->prototype = tmpl->GetGenericPrototype();
        for(size_t i = 0; i < numtypes; ++i)
            res->arguments.push_back(types[i]);
        return res;
    }
}
