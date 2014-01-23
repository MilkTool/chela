#ifndef CHELAVM_FUNCTION_GROUP_HPP
#define CHELAVM_FUNCTION_GROUP_HPP

#include "ChelaVm/Function.hpp"

namespace ChelaVm
{
    class FunctionGroup: public Member
    {
    public:
        FunctionGroup(Module *module);
        ~FunctionGroup();

        virtual std::string GetName() const;

        virtual void UpdateParent(Member *parent);

        virtual bool IsFunctionGroup() const;

        size_t GetFunctionCount() const;
        Function *GetFunction(size_t index);
        uint32_t GetFunctionId(size_t index);

        static FunctionGroup *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        void ReadStructure(ModuleReader &reader, const MemberHeader &header);

        virtual void DeclarePass();
        virtual void DefinitionPass();
        virtual Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);

    private:
        std::string name;
        std::vector<Function *> functions;
        bool isInstance;

    };
}; // namespace ChelaVm

#endif //CHELAVM_FUNCTION_GROUP_HPP
