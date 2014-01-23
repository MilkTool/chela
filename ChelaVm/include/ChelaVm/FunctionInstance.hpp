#ifndef _CHELAVM_FUNCTION_INSTANCE_HPP
#define _CHELAVM_FUNCTION_INSTANCE_HPP

#include "ChelaVm/Module.hpp"
#include "ChelaVm/ModuleReader.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/GenericInstance.hpp"

namespace ChelaVm
{
    class FunctionInstance: public Member
    {
    public:
        FunctionInstance(Module *module);
        ~FunctionInstance();

        virtual bool IsAnonymous() const;
        virtual bool IsFunctionInstance() const;

        Function *GetFunction();
        Function *GetImplementation();
        const GenericInstance &GetGenericInstance() const;

        virtual void DeclarePass();
        virtual void DefinitionPass();

        static FunctionInstance *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        virtual void Read(ModuleReader &reader, const MemberHeader &header);

    private:
        Function *function;
        Function *implementation;
        GenericInstance instance;
    };
}

#endif //_CHELAVM_FUNCTION_INSTANCE_HPP

