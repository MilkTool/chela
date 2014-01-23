#ifndef _CHELAVM_TYPE_INSTANCE_HPP
#define _CHELAVM_TYPE_INSTANCE_HPP

#include "ChelaVm/Module.hpp"
#include "ChelaVm/ModuleReader.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/GenericInstance.hpp"

namespace ChelaVm
{
    class TypeInstance: public ChelaType
    {
    public:
        TypeInstance(Module *module);
        ~TypeInstance();

        virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;

        virtual bool IsAnonymous() const;
        virtual bool IsTypeInstance() const;
        virtual bool IsGenericType() const;
        virtual bool IsGeneric() const;

        Member *GetFactory() const;
        Structure *GetOriginal() const;
        Structure *GetImplementation();
        const Structure *GetImplementation() const;
        const GenericInstance &GetGenericInstance() const;

        Member *GetInstancedMember(Member *templateMember) const;

        virtual llvm::Type *GetTargetType() const;

        virtual void DeclarePass();
        virtual void DefinitionPass();

        static TypeInstance *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        virtual void Read(ModuleReader &reader, const MemberHeader &header);

        Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);
        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;

    private:
        void ReadData() const;
        void Preload() const;

        mutable Member *factoryMember;
        mutable Structure *originalTemplate;
        mutable Structure *originalInstance;
        mutable Structure *implementation;
        mutable GenericInstance instance;
        mutable size_t rawDataSize;
        mutable uint8_t *rawData;
        mutable bool readedData;

    };
};

#endif //_CHELAVM_TYPE_INSTANCE_HPP
