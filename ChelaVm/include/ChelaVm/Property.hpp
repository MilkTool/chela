#ifndef CHELAVM_PROPERTY_HPP
#define CHELAVM_PROPERTY_HPP

#include "Member.hpp"
#include "Module.hpp"
#include "Function.hpp"

namespace ChelaVm
{
    class Property: public Member
    {
    public:
        Property(Module *module);
        ~Property();

        virtual std::string GetName() const;
        virtual MemberFlags GetFlags() const;
        const ChelaType *GetType() const;
        Function *GetGetAccessor() const;
        Function *GetSetAccessor() const;

        virtual bool IsGeneric() const;
        virtual bool IsProperty() const;

        virtual void DeclarePass();
        virtual void DefinitionPass();

        static Property *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        void Read(ModuleReader &reader, const MemberHeader &header);

        virtual llvm::GlobalVariable *GetMemberInfo();

        Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);

    private:
        std::string name;
        MemberFlags flags;
        const ChelaType *type;
        Function *getAccessor;
        Function *setAccessor;
        std::vector<const ChelaType*> indices;

        llvm::GlobalVariable *propertyInfo;
        bool createdMemberInfo;
        const GenericInstance *genericInstance;
    };
};

#endif //CHELAVM_PROPERTY_HPP
