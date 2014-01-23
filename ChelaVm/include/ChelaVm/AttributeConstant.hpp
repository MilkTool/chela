#include <vector>
#include "ConstantValue.hpp"
#include "llvm/Support/IRBuilder.h"

namespace ChelaVm
{
    // Property-value pair.
    class AttributePropertyValue
    {
    public:
        AttributePropertyValue(Module *module);
        ~AttributePropertyValue();

        Member *GetVariable();
        ConstantValue &GetValue();

        void Read(ModuleReader &reader);

    private:
        Module *module;
        uint32_t variableId;
        ConstantValue value;
    };

    // Attribute instance data.
    class Class;
    class Function;
    class AttributeConstant
    {
    public:
        AttributeConstant(Module *module);
        ~AttributeConstant();

        Class *GetClass();
        Function *GetConstructor();

        size_t GetArgumentCount();
        ConstantValue *GetArgument(size_t id);

        void Read(ModuleReader &reader);

        llvm::GlobalVariable *DeclareVariable();
        void DefineAttribute();

        void Construct(llvm::IRBuilder<> &builder);

    private:
        llvm::Constant *CheckArgument(const ChelaType *argType, const ChelaType *valueType, llvm::Constant *value);

        Module *module;
        uint32_t attributeClassId;
        uint32_t attributeCtorId;
        std::vector<ConstantValue> arguments;
        std::vector<AttributePropertyValue> propertyValues;
        llvm::GlobalVariable *attributeVariable;
        bool defined;
    };
}
