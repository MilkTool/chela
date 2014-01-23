#ifndef _CONSTANT_VALUE_HPP
#define _CONSTANT_VALUE_HPP

#include "Module.hpp"
#include "ModuleReader.hpp"

namespace ChelaVm
{
    class ConstantValue
    {
    public:
        ConstantValue(Module *module);
        ~ConstantValue();

        ConstantValueType GetType() const;
        const ChelaType *GetChelaType() const;

        llvm::Constant *GetTargetConstant();

        const std::string &GetStringValue() const;

        void Read(ModuleReader &reader);

    private:
        Module *module;
        ConstantValueType type;

        union
        {
            uint64_t ulongValue;
            int64_t longValue;
            float singleValue;
            double doubleValue;
            bool boolValue;
            uint32_t stringValue;
        };
    };
}

#endif //_CONSTANT_VALUE_HPP
