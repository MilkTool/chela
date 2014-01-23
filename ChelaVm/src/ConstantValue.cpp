#include "ChelaVm/ConstantValue.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "llvm/Constants.h"

namespace ChelaVm
{
    ConstantValue::ConstantValue(Module *module)
        : module(module)
    {
        type = CVT_Unknown;
    }

    ConstantValue::~ConstantValue()
    {
    }

    ConstantValueType ConstantValue::GetType() const
    {
        return type;
    }

    const ChelaType *ConstantValue::GetChelaType() const
    {
        VirtualMachine *vm = module->GetVirtualMachine();

        switch(type)
        {
        case CVT_Byte: return ChelaType::GetByteType(vm);
        case CVT_SByte: return ChelaType::GetSByteType(vm);
        case CVT_Char: return ChelaType::GetCharType(vm);
        case CVT_Short: return ChelaType::GetShortType(vm);
        case CVT_UShort: return ChelaType::GetUShortType(vm);
        case CVT_Int: return ChelaType::GetIntType(vm);
        case CVT_UInt: return ChelaType::GetUIntType(vm);
        case CVT_Long: return ChelaType::GetLongType(vm);
        case CVT_ULong: return ChelaType::GetULongType(vm);
        case CVT_Float: return ChelaType::GetFloatType(vm);
        case CVT_Double: return ChelaType::GetDoubleType(vm);
        case CVT_Bool: return ChelaType::GetBoolType(vm);
        case CVT_Size: return ChelaType::GetSizeType(vm);
        case CVT_String: return ReferenceType::Create(vm->GetStringClass());
        case CVT_Null: return ChelaType::GetNullType(vm);
        case CVT_Unknown:
        default:
            return NULL;
        }
    }

    llvm::Constant *ConstantValue::GetTargetConstant()
    {
        llvm::LLVMContext &ctx = module->GetLlvmContext();
        VirtualMachine *vm = module->GetVirtualMachine();

        switch(type)
        {
        case CVT_Byte: return llvm::ConstantInt::get(llvm::Type::getInt8Ty(ctx), ulongValue, false);
        case CVT_SByte: return llvm::ConstantInt::get(llvm::Type::getInt8Ty(ctx), ulongValue, true);
        case CVT_Char: return llvm::ConstantInt::get(llvm::Type::getInt16Ty(ctx), ulongValue, true);
        case CVT_Short: return llvm::ConstantInt::get(llvm::Type::getInt16Ty(ctx), ulongValue, false);
        case CVT_UShort: return llvm::ConstantInt::get(llvm::Type::getInt16Ty(ctx), ulongValue, true);
        case CVT_Int: return llvm::ConstantInt::get(llvm::Type::getInt32Ty(ctx), ulongValue, false);
        case CVT_UInt: return llvm::ConstantInt::get(llvm::Type::getInt32Ty(ctx), ulongValue, true);
        case CVT_Long: return llvm::ConstantInt::get(llvm::Type::getInt64Ty(ctx), ulongValue, false);
        case CVT_ULong: return llvm::ConstantInt::get(llvm::Type::getInt64Ty(ctx), ulongValue, true);
        case CVT_Float: return llvm::ConstantFP::get(llvm::Type::getFloatTy(ctx), singleValue);
        case CVT_Double: return llvm::ConstantFP::get(llvm::Type::getDoubleTy(ctx), doubleValue);
        case CVT_Bool: return llvm::ConstantInt::get(llvm::Type::getInt1Ty(ctx), ulongValue);
        case CVT_Size: return llvm::ConstantInt::get(ChelaType::GetSizeType(vm)->GetTargetType(), ulongValue);
        case CVT_String: return module->CompileString(ulongValue);
        case CVT_Null: return NULL;
        case CVT_Unknown:
        default:
            return NULL;
        }
    }

    const std::string &ConstantValue::GetStringValue() const
    {
        static std::string empty;
        if(type != CVT_String)
            return empty;
        return module->GetString(stringValue);
    }

    void ConstantValue::Read(ModuleReader &reader)
    {
        // Read first the type.
        uint8_t btype;
        reader >> btype;
        type = (ConstantValueType)btype;

        // Now read the constant value.
        switch(type)
        {
        case CVT_Byte:
            {
                uint8_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_SByte:
            {
                int8_t tv;
                reader >> tv;
                longValue = tv;
            }
            break;
        case CVT_Short:
            {
                int16_t tv;
                reader >> tv;
                longValue = tv;
            }
            break;
        case CVT_Char:
        case CVT_UShort:
            {
                uint16_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_Int:
            {
                int32_t tv;
                reader >> tv;
                longValue = tv;
            }
            break;
        case CVT_UInt:
            {
                uint32_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_Long:
            {
                int64_t tv;
                reader >> tv;
                longValue = tv;
            }
            break;
        case CVT_ULong:
            {
                uint64_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_Float:
            reader >> singleValue;
            break;
        case CVT_Double:
            reader >> doubleValue;
            break;
        case CVT_Bool:
            {
                uint8_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_Size:
            {
                uint64_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_String:
            {
                uint32_t tv;
                reader >> tv;
                ulongValue = tv;
            }
            break;
        case CVT_Null:
            break;
        case CVT_Unknown:
            throw ModuleException("cannot read constant of unknown type.");
            break;
        }
    }
}
