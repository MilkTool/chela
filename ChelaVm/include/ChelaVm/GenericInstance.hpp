#ifndef CHELAVM_GENERIC_INSTANCE_HPP
#define CHELAVM_GENERIC_INSTANCE_HPP

#include "ChelaVm/Module.hpp"
#include "ChelaVm/ModuleReader.hpp"
#include "ChelaVm/GenericPrototype.hpp"

namespace ChelaVm
{
    class GenericInstance
    {
    public:
        GenericInstance(Module *module);
        ~GenericInstance();

        GenericInstance *Clone() const;
        GenericInstance *Normalize() const;
        GenericInstance *Normalize(const GenericPrototype *generic) const;

        Module *GetModule() const;
        std::string GetFullName() const;
        std::string GetMangledName() const;
        std::string GetDebugName() const;

        const GenericPrototype *GetPrototype() const;
        void SetPrototype(const GenericPrototype *prototype);

        size_t GetArgumentCount() const;
        const ChelaType *GetArgument(size_t index) const;

        void CheckPrototype();
        void Read(ModuleReader &reader);

        bool operator<(const GenericInstance &o) const;

        bool IsGeneric() const;
        void InstanceFrom(const GenericInstance &original, const GenericInstance *instanceData);
        void AppendInstance(const GenericInstance *instance);

        static GenericInstance *Create(Module *module, Member *tmpl, const ChelaType** types, size_t numtypes);
    private:
        void NormalizeTypes();

        Module *module;
        const GenericPrototype *prototype;
        std::vector<const ChelaType*> arguments;
    };
}

#endif //CHELAVM_GENERIC_INSTANCE_HPP
