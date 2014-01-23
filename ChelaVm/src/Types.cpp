#include <stdexcept>
#include "ChelaVm/Types.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/Dwarf.h"
#include "llvm/Support/raw_ostream.h"

using namespace llvm::dwarf;
using llvm::LLVMDebugVersion;

namespace ChelaVm
{
    // ChelaType
    ChelaType::ChelaType(Module *module)
        : Member(module)
    {
    }

    ChelaType::ChelaType(VirtualMachine *vm)
        : Member(vm)
    {
    }

    ChelaType::~ChelaType()
    {
    }

    std::string ChelaType::GetName() const
    {
        return "<unknown-type>";
    }

    bool ChelaType::IsVector() const
    {
        return false;
    }

    bool ChelaType::IsMatrix() const
    {
        return false;
    }

    bool ChelaType::IsPrimitive() const
    {
        return false;
    }

    bool ChelaType::IsFirstClass() const
    {
        return IsMatrix() || IsVector() || IsPrimitive();
    }

    bool ChelaType::IsMetaType() const
    {
        return false;
    }

    bool ChelaType::IsNumber() const
    {
        return false;
    }

    bool ChelaType::IsInteger() const
    {
        return false;
    }

    bool ChelaType::IsSizeInteger() const
    {
        return false;
    }

    bool ChelaType::IsFloat() const
    {
        return false;
    }

    bool ChelaType::IsDouble() const
    {
        return false;
    }

    bool ChelaType::IsFloatingPoint() const
    {
        return false;
    }

    bool ChelaType::IsUnsigned() const
    {
        return false;
    }

    bool ChelaType::IsVoid() const
    {
        return false;
    }

    bool ChelaType::IsConstant() const
    {
        return false;
    }

    bool ChelaType::IsPointer() const
    {
        return false;
    }

    bool ChelaType::IsArray() const
    {
        return false;
    }

    bool ChelaType::IsBoxed() const
    {
        return false;
    }

    bool ChelaType::IsPassedByReference() const
    {
        return false;
    }

    bool ChelaType::IsComplexStructure() const
    {
        return false;
    }

    bool ChelaType::IsReference() const
    {
        return false;
    }

    bool ChelaType::IsFunction() const
    {
        return false;
    }

    bool ChelaType::IsStructure() const
    {
        return false;
    }

    bool ChelaType::IsClass() const
    {
        return false;
    }

    bool ChelaType::IsInterface() const
    {
        return false;
    }

    bool ChelaType::IsNamespace() const
    {
        return false;
    }

    bool ChelaType::IsPlaceHolder() const
    {
        return false;
    }

    bool ChelaType::IsGenericType() const
    {
        return false;
    }

    bool ChelaType::IsGeneric() const
    {
        return IsGenericType();
    }

    bool ChelaType::IsType() const
    {
        return true;
    }

    llvm::Type *ChelaType::GetTargetType() const
    {
        return NULL;
    }

    llvm::DIType ChelaType::GetDebugType(DebugInformation *context) const
    {
        // Read the cached debug type
        llvm::DIType result = context->GetCachedDebugType(this);
        if(result && result.getContext())
            return result;

        // Create and register the debug type.
        result = CreateDebugType(context);
        context->RegisterDebugType(this, result);
        return result;
    }

    llvm::DIDescriptor ChelaType::GetDebugNode(DebugInformation *context) const
    {
        return GetDebugType(context);
    }

    llvm::DIType ChelaType::CreateDebugType(DebugInformation *context) const
    {
        return llvm::DIType();
    }

    size_t ChelaType::GetSize() const
    {
        return 4; // Default size.
    }

    size_t ChelaType::GetAlign() const
    {
        return 4; // Default align.
    }

    bool ChelaType::HasSubReferences() const
    {
        return false;
    }

    const ChelaType *ChelaType::InstanceGeneric(const GenericInstance *instance) const
    {
        return this;
    }

    Member *ChelaType::InstanceMember(Module *implementingModule, const GenericInstance *instance)
    {
        return (Member*) InstanceGeneric(instance);
    }

    const ChelaType *ChelaType::GetVoidType(VirtualMachine *vm)
    {
        return vm->GetVoidType();
    }

    const ChelaType *ChelaType::GetConstVoidType(VirtualMachine *vm)
    {
        return vm->GetConstVoidType();
    }

    const ChelaType *ChelaType::GetBoolType(VirtualMachine *vm)
    {
        return vm->GetBoolType();
    }

    const ChelaType *ChelaType::GetByteType(VirtualMachine *vm)
    {
        return vm->GetByteType();
    }

    const ChelaType *ChelaType::GetSByteType(VirtualMachine *vm)
    {
        return vm->GetSByteType();
    }

    const ChelaType *ChelaType::GetCharType(VirtualMachine *vm)
    {
        return vm->GetCharType();
    }

    const ChelaType *ChelaType::GetShortType(VirtualMachine *vm)
    {
        return vm->GetShortType();
    }

    const ChelaType *ChelaType::GetUShortType(VirtualMachine *vm)
    {
        return vm->GetUShortType();
    }

    const ChelaType *ChelaType::GetIntType(VirtualMachine *vm)
    {
        return vm->GetIntType();
    }

    const ChelaType *ChelaType::GetUIntType(VirtualMachine *vm)
    {
        return vm->GetUIntType();
    }

    const ChelaType *ChelaType::GetLongType(VirtualMachine *vm)
    {
        return vm->GetLongType();
    }

    const ChelaType *ChelaType::GetULongType(VirtualMachine *vm)
    {
        return vm->GetULongType();
    }

    const ChelaType *ChelaType::GetFloatType(VirtualMachine *vm)
    {
        return vm->GetFloatType();
    }

    const ChelaType *ChelaType::GetDoubleType(VirtualMachine *vm)
    {
        return vm->GetDoubleType();
    }

    const ChelaType *ChelaType::GetSizeType(VirtualMachine *vm)
    {
        return vm->GetSizeType();
    }

    const ChelaType *ChelaType::GetPtrDiffType(VirtualMachine *vm)
    {
        return vm->GetPtrDiffType();
    }

    const ChelaType *ChelaType::GetCStringType(VirtualMachine *vm)
    {
        return vm->GetCStringType();
    }

    const ChelaType *ChelaType::GetNullType(VirtualMachine *vm)
    {
        return vm->GetNullType();
    }
    
    const PointerType *ChelaType::GetVoidPointerType(VirtualMachine *vm)
    {
        return vm->GetVoidPointerType();
    }

    // IntegerType
    IntegerType::IntegerType(VirtualMachine *vm, const std::string &name, const std::string &mangled, bool sign,
                size_t size, size_t align, llvm::Type *target)
        : ChelaType(vm), name(name), mangledName(mangled), sign(sign),
          size(size), align(align), target(target)
    {
        if(target == NULL)
            this->target = llvm::Type::getIntNTy(GetLlvmContext(), size*8);
    }

    IntegerType::~IntegerType()
    {
    }

    std::string IntegerType::GetName() const
    {
        return name;
    }

    std::string IntegerType::GetMangledName() const
    {
        return mangledName;
    }

    bool IntegerType::IsPrimitive() const
    {
        return true;
    }

    bool IntegerType::IsNumber() const
    {
        return true;
    }

    bool IntegerType::IsInteger() const
    {
        return true;
    }

    bool IntegerType::IsSizeInteger() const
    {
        return this == ChelaType::GetSizeType(GetVM()) ||
            this == ChelaType::GetPtrDiffType(GetVM());
    }

    bool IntegerType::IsFloat() const
    {
        return false;
    }

    bool IntegerType::IsDouble() const
    {
        return false;
    }

    bool IntegerType::IsFloatingPoint() const
    {
        return false;
    }

    bool IntegerType::IsUnsigned() const
    {
        return !sign;
    }

    size_t IntegerType::GetSize() const
    {
        return size;
    }

    size_t IntegerType::GetAlign() const
    {
        return align;
    }
    
    llvm::Type *IntegerType::GetTargetType() const
    {
        return target;
    }

    llvm::DIType IntegerType::CreateDebugType(DebugInformation *context) const
    {
        // Add the encoding.
        int encoding;
        if(this == ChelaType::GetBoolType(GetVM()))
            encoding = DW_ATE_boolean;
        else if(this == ChelaType::GetCharType(GetVM()))
            encoding = DW_ATE_unsigned_char;
        else if(IsUnsigned())
            encoding = DW_ATE_unsigned;
        else
            encoding = DW_ATE_signed;
        return context->GetDebugBuilder().createBasicType(name, GetSize()*8, GetAlign()*8, encoding);
    }

    // FloatType
    FloatType::FloatType(VirtualMachine *vm, const std::string &name, const std::string &mangled, bool isDouble, llvm::Type *target)
        : ChelaType(vm), name(name), mangledName(mangled), isDouble(isDouble), target(target)
    {
        if(target == NULL)
        {
            if(isDouble)
                this->target = llvm::Type::getDoubleTy(GetLlvmContext());
            else
                this->target = llvm::Type::getFloatTy(GetLlvmContext());
        }
    }

    FloatType::~FloatType()
    {
    }

    std::string FloatType::GetName() const
    {
        return name;
    }

    std::string FloatType::GetMangledName() const
    {
        return mangledName;
    }

    bool FloatType::IsPrimitive() const
    {
        return true;
    }

    bool FloatType::IsNumber() const
    {
        return true;
    }

    bool FloatType::IsInteger() const
    {
        return false;
    }

    bool FloatType::IsFloat() const
    {
        return !isDouble;
    }

    bool FloatType::IsDouble() const
    {
        return isDouble;
    }

    bool FloatType::IsFloatingPoint() const
    {
        return true;
    }

    bool FloatType::IsUnsigned() const
    {
        return false;
    }

    size_t FloatType::GetSize() const
    {
        return isDouble ? 8 : 4;
    }

    size_t FloatType::GetAlign() const
    {
        return isDouble ? 8 : 4;
    }
    
    llvm::Type *FloatType::GetTargetType() const
    {
        return target;
    }

    llvm::DIType FloatType::CreateDebugType(DebugInformation *context) const
    {
        return context->GetDebugBuilder().createBasicType(name, GetSize()*8, GetAlign()*8, DW_ATE_float);
    }

    // VoidType
    VoidType::VoidType(VirtualMachine *vm)
        : ChelaType(vm)
    {
    }

    std::string VoidType::GetName() const
    {
        return "void";
    }

    std::string VoidType::GetMangledName() const
    {
        return "V";
    }

    bool VoidType::IsVoid() const
    {
        return true;
    }
    
    llvm::Type *VoidType::GetTargetType() const
    {
        return llvm::Type::getVoidTy(GetLlvmContext());
    }

    llvm::DIType VoidType::CreateDebugType(DebugInformation *context) const
    {
        return llvm::DIType();
    }

    // FunctionType
    FunctionType::FunctionTypes FunctionType::functionTypes;

    FunctionType::FunctionType(const ChelaType *returnType, const Arguments &arguments,
                                bool variable, int flags)
        : ChelaType(returnType->GetVM()), returnType(returnType), arguments(arguments),
          variable(variable), flags(flags)
    {
        target = NULL;
    }

    FunctionType::~FunctionType()
    {
    }

    std::string FunctionType::GetName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the return type.
        out << returnType->GetName() << '(';

        // Add the parameters.
        size_t numargs = arguments.size();
        for(size_t i = 0; i < numargs; i++)
        {
            if(i > 0)
                out << ',';

            if(IsVariable() && i + 1 == numargs)
                out << "params ";

            out << arguments[i]->GetName().c_str();
        }
        out << ')';

        // Return the name.
        return out.str();
    }

    std::string FunctionType::GetFullName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the return type.
        out << returnType->GetFullName() << '(';

        // Add the parameters.
        size_t numargs = arguments.size();
        for(size_t i = 0; i < numargs; i++)
        {
            if(i > 0)
                out << ',';

            if(IsVariable() && i + 1 == numargs)
                out << "params ";

            out << arguments[i]->GetFullName().c_str();
        }
        out << ')';

        // Return the name.
        return out.str();
    }

    std::string FunctionType::GetMangledName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << 'G';

        // Add the variable arguments flag.
        if(IsVariable())
            out << 'E';

        // Add the argument count and the return type.
        out << GetArgumentCount() + 1 << returnType->GetMangledName();

        // Add each one of the arguments.
        size_t numargs = arguments.size();
        for(size_t i = 0; i < numargs; i++)
            out << arguments[i]->GetMangledName();

        return out.str();
    }

    llvm::Type *FunctionType::GetTargetType() const
    {
        if(!target)
        {
            // Create the target type.
            std::vector<llvm::Type*> args;

            // Add pointer to return type if its a complex type.
            llvm::Type *retType = returnType->GetTargetType();
            if(returnType->IsComplexStructure())
            {
                args.push_back(llvm::PointerType::getUnqual(retType));
                retType = GetVoidType(GetVM())->GetTargetType();
            }

            // Add the others arguments.
            for(size_t i = 0; i < arguments.size(); i++)
                args.push_back(arguments[i]->GetTargetType());
            target = llvm::FunctionType::get(retType,
                args, false);
        }

        return target;
    }

    llvm::FunctionType *FunctionType::GetTargetFunctionType() const
    {
        return llvm::cast<llvm::FunctionType> (GetTargetType());
    }
    
    bool FunctionType::IsVariable() const
    {
        return variable;
    }

    int FunctionType::GetFunctionFlags() const
    {
        return flags;
    }

    bool FunctionType::IsFunction() const
    {
        return true;
    }

    bool FunctionType::IsGenericType() const
    {
        if(returnType->IsGenericType())
            return true;

        for(size_t i = 0; i < arguments.size(); ++i)
            if(arguments[i]->IsGenericType())
                return true;
        return false;
    }

    bool FunctionType::IsUnsafe() const
    {
        if(returnType->IsUnsafe())
            return true;
        for(size_t i = 0; i < arguments.size(); ++i)
            if(arguments[i]->IsGenericType())
                return true;
        return false;
    }

    const ChelaType *FunctionType::GetReturnType() const
    {
        return returnType;
    }

    size_t FunctionType::GetArgumentCount() const
    {
        return arguments.size();
    }

    const ChelaType *FunctionType::GetArgument(size_t index) const
    {
        return arguments[index];
    }

    size_t FunctionType::arg_size() const
    {
        return arguments.size();
    }

    FunctionType::arg_iterator FunctionType::arg_begin() const
    {
        return arguments.begin();
    }

    FunctionType::arg_iterator FunctionType::arg_end() const
    {
        return arguments.end();
    }

    bool FunctionType::operator<(const FunctionType &o) const
    {
        if(returnType < o.returnType)
            return true;
        else if(returnType > o.returnType)
            return false;

        if(arguments.size() < o.arguments.size())
            return true;
        else if(arguments.size() > o.arguments.size())
            return false;

        if(variable < o.variable)
            return true;
        else if(variable > o.variable)
            return false;
        
        if(flags < o.flags)
            return true;
        else if(flags > o.flags)
            return false;

        arg_iterator it = arg_begin();
        arg_iterator it2 = o.arg_begin();
        for(; it != arg_end(); it++, it2++)
        {
            const ChelaType *l = *it;
            const ChelaType *r = *it2;
            
            if(l < r)
                return true;
            else if(l > r)
                return false;
        }

        // Equal.
        return false;
    }

    const ChelaType *FunctionType::InstanceGeneric(const GenericInstance *instance) const
    {
        // Instance the return type.
        const ChelaType *returnType = this->returnType->InstanceGeneric(instance);

        // Instance the arguments
        Arguments args;
        for(size_t i = 0; i < arguments.size(); ++i)
        {
            const ChelaType *arg = arguments[i];
            args.push_back(arg->InstanceGeneric(instance));
        }

        // Create the function with the replaced arguments.
        return Create(returnType, args, variable);
    }

    const FunctionType *FunctionType::Create(const ChelaType* ret, const Arguments &args, bool variable, int flags)
    {
        // First create a new function.
        FunctionType *newFunction = new FunctionType(ret, args, variable, flags & 0xFFFFFF00);
        // Removed variable argument data from the flag.
        
        // Find for an existing one.
        FunctionTypes::const_iterator it = functionTypes.find(newFunction);
        if(it != functionTypes.end())
        {
            delete newFunction;
            return *it;
        }

        functionTypes.insert(newFunction);
        return newFunction;
    }

    const FunctionType *FunctionType::Create(const ChelaType* ret, bool variable, int flags)
    {
        Arguments args;
        return Create(ret, args, variable, flags);
    }

    llvm::DIType FunctionType::CreateDebugType(DebugInformation *context) const
    {
        // Create the function type members array.
        std::vector<llvm::Value*> elements;
        elements.push_back(GetReturnType()->GetDebugType(context));
        for(size_t i = 0; i < arguments.size(); ++i)
            elements.push_back(arguments[i]->GetDebugType(context));

        // Create the function type.
        llvm::DIBuilder &builder = context->GetDebugBuilder();
        llvm::DIArray members = builder.getOrCreateArray(elements);
        return builder.createSubroutineType(context->GetModuleFileDescriptor(), members);
    }

    // MetaTypes
    MetaType::MetaTypes MetaType::metaTypes;

    MetaType::MetaType(const ChelaType *type)
        : ChelaType(type->GetVM()), actualType(type)
    {
    }

    MetaType::~MetaType()
    {
    }

    bool MetaType::IsMetaType() const
    {
        return true;
    }

    bool MetaType::IsGenericType() const
    {
        return actualType->IsGenericType();
    }

    const ChelaType *MetaType::GetActualType() const
    {
        return actualType;
    }

    bool MetaType::operator<(const MetaType &o) const
    {
        return actualType < o.actualType;
    }

    const ChelaType *MetaType::InstanceGeneric(const GenericInstance *instance) const
    {
        return Create(actualType->InstanceGeneric(instance));
    }

    const MetaType *MetaType::Create(const ChelaType *type)
    {
        // Create the meta type.
        MetaType *res = new MetaType(type);

        // Find an existing one.
        MetaTypes::const_iterator it = metaTypes.find(res);
        if(it != metaTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new meta type and return it.
        metaTypes.insert(res);
        return res;
    }

    // VectorType
    VectorType::VectorTypes VectorType::vectorTypes;

    VectorType::VectorType(const ChelaType *type, int numcomponents)
        : ChelaType(type->GetVM()), primitiveType(type), numcomponents(numcomponents)
    {
        target = NULL;
    }

    VectorType::~VectorType()
    {
    }

    std::string VectorType::GetName() const
    {
        // Create the buffer.
        llvm::SmallString<32> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the name.
        out << primitiveType->GetName() << '*' << numcomponents;
        return out.str();
    }

    std::string VectorType::GetMangledName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the name.
        out << 'v' << numcomponents << primitiveType->GetMangledName();
        return out.str();
    }
    
    llvm::Type *VectorType::GetTargetType() const
    {
        if(!target)
            target = llvm::VectorType::get(primitiveType->GetTargetType(), numcomponents);

        return target;
    }

    llvm::DIType VectorType::CreateDebugType(DebugInformation *context) const
    {
        // Create the subscripts.
        llvm::DIBuilder &builder = context->GetDebugBuilder();
        llvm::Value *subscripts[] = {
            builder.getOrCreateSubrange(0, numcomponents),
        };

        // Create the vector type
        return builder.createVectorType(GetSize()*8, GetAlign()*8,
            primitiveType->GetDebugType(context),
            builder.getOrCreateArray(subscripts));
    }

    bool VectorType::IsVector() const
    {
        return true;
    }

    bool VectorType::IsNumber() const
    {
        return primitiveType->IsNumber();
    }

    bool VectorType::IsInteger() const
    {
        return primitiveType->IsInteger();
    }

    bool VectorType::IsFloatingPoint() const
    {
        return primitiveType->IsFloatingPoint();
    }

    bool VectorType::IsFloat() const
    {
        return primitiveType->IsFloat();
    }

    bool VectorType::IsDouble() const
    {
        return primitiveType->IsDouble();
    }

    bool VectorType::IsGenericType() const
    {
        return primitiveType->IsGenericType();
    }

    size_t VectorType::GetSize() const
    {
        return primitiveType->GetSize()*numcomponents;
    }

    size_t VectorType::GetAlign() const
    {
        // TODO: Round to the closest power of two.
        return GetSize();
    }

    int VectorType::GetNumComponents() const
    {
        return numcomponents;
    }

    const ChelaType *VectorType::GetPrimitiveType() const
    {
        return primitiveType;
    }

    bool VectorType::operator<(const VectorType &o) const
    {
        if(primitiveType == o.primitiveType)
            return numcomponents < o.numcomponents;
        else
            return primitiveType < o.primitiveType;
    }

    const VectorType *VectorType::Create(const ChelaType *type, int numcomponents)
    {
        // Create the pointer type.
        VectorType *res = new VectorType(type, numcomponents);

        // Find an existing one.
        VectorTypes::const_iterator it = vectorTypes.find(res);
        if(it != vectorTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new vector type and return it.
        vectorTypes.insert(res);
        return res;
    }

    // MatrixType
    MatrixType::MatrixTypes MatrixType::matrixTypes;

    MatrixType::MatrixType(const ChelaType *type, int numrows, int numcolumns)
        : ChelaType(type->GetVM()), primitiveType(type), numrows(numrows), numcolumns(numcolumns)
    {
        target = NULL;
    }

    MatrixType::~MatrixType()
    {
    }

    std::string MatrixType::GetName() const
    {
        // Create the buffer.
        llvm::SmallString<32> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the name.
        out << primitiveType->GetName() << '*' << numrows << 'x' << numcolumns;
        return out.str();
    }

    std::string MatrixType::GetMangledName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Create the name.
        out << 'm' << numrows << '_' << numcolumns << primitiveType->GetMangledName();
        return out.str();
    }
    
    llvm::Type *MatrixType::GetTargetType() const
    {
        if(!target)
        {
            target = llvm::VectorType::get(primitiveType->GetTargetType(), numrows*numcolumns);
        }

        return target;
    }

    llvm::DIType MatrixType::CreateDebugType(DebugInformation *context) const
    {
        // Create the subscripts.
        llvm::DIBuilder &builder = context->GetDebugBuilder();
        llvm::Value *subscripts[] = {
            builder.getOrCreateSubrange(0, numrows),
            builder.getOrCreateSubrange(0, numcolumns),
        };

        // Create the vector type
        return builder.createVectorType(GetSize()*8, GetAlign()*8,
            primitiveType->GetDebugType(context),
            builder.getOrCreateArray(subscripts));
    }

    bool MatrixType::IsMatrix() const
    {
        return true;
    }

    bool MatrixType::IsNumber() const
    {
        return primitiveType->IsNumber();
    }

    bool MatrixType::IsInteger() const
    {
        return primitiveType->IsInteger();
    }

    bool MatrixType::IsFloatingPoint() const
    {
        return primitiveType->IsFloatingPoint();
    }

    bool MatrixType::IsFloat() const
    {
        return primitiveType->IsFloat();
    }

    bool MatrixType::IsDouble() const
    {
        return primitiveType->IsDouble();
    }

    bool MatrixType::IsGenericType() const
    {
        return primitiveType->IsGenericType();
    }

    size_t MatrixType::GetSize() const
    {
        return primitiveType->GetSize()*numrows*numcolumns;
    }

    int MatrixType::GetNumRows() const
    {
        return numrows;
    }

    int MatrixType::GetNumColumns() const
    {
        return numcolumns;
    }

    const ChelaType *MatrixType::GetPrimitiveType() const
    {
        return primitiveType;
    }

    bool MatrixType::operator<(const MatrixType &o) const
    {
        if(primitiveType == o.primitiveType)
        {
            if(numrows == o.numrows)
                return numcolumns < o.numcolumns;
            return numrows < o.numrows;
        }
        else
            return primitiveType < o.primitiveType;
    }

    const MatrixType *MatrixType::Create(const ChelaType *type, int numrows, int numcolumns)
    {
        // Create the pointer type.
        MatrixType *res = new MatrixType(type, numrows, numcolumns);

        // Find an existing one.
        MatrixTypes::const_iterator it = matrixTypes.find(res);
        if(it != matrixTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new vector type and return it.
        matrixTypes.insert(res);
        return res;
    }

    // PointerType
    PointerType::PointerTypes PointerType::pointerTypes;

    PointerType::PointerType(const ChelaType *type)
        : ChelaType(type->GetVM()), pointedType(type)
    {
        target = NULL;
    }

    PointerType::~PointerType()
    {
    }

    std::string PointerType::GetName() const
    {
        return pointedType->GetName() + "*";
    }

    std::string PointerType::GetFullName() const
    {
        return pointedType->GetFullName() + "*";
    }

    std::string PointerType::GetMangledName() const
    {
        return "p" + pointedType->GetMangledName();
    }
    
    llvm::Type *PointerType::GetTargetType() const
    {
        if(!target)
        {
            if(pointedType->IsConstant())
            {
                const ChelaType *constVoid = ConstantType::Create(ChelaType::GetVoidType(GetVM()));
                if(pointedType != constVoid)
                    target = llvm::PointerType::getUnqual(pointedType->GetTargetType());
                else
                    target = llvm::Type::getInt8PtrTy(GetLlvmContext());
            }
            else
            {
                if(pointedType != ChelaType::GetVoidType(GetVM()))
                    target = llvm::PointerType::getUnqual(pointedType->GetTargetType());
                else
                    target = llvm::Type::getInt8PtrTy(GetLlvmContext());
            }
        }

        return target;
    }

    bool PointerType::IsPointer() const
    {
        return true;
    }

    bool PointerType::IsGenericType() const
    {
        return pointedType->IsGenericType();
    }

    bool PointerType::IsUnsafe() const
    {
        return true;
    }

    size_t PointerType::GetSize() const
    {
        return sizeof(void*);
    }

    const ChelaType *PointerType::GetPointedType() const
    {
        return pointedType;
    }

    bool PointerType::operator<(const PointerType &o) const
    {
        return pointedType < o.pointedType;
    }

    const ChelaType *PointerType::InstanceGeneric(const GenericInstance *instance) const
    {
        return Create(pointedType->InstanceGeneric(instance));
    }

    const PointerType *PointerType::Create(const ChelaType *type)
    {
        // Create the pointer type.
        PointerType *res = new PointerType(type);

        // Find an existing one.
        PointerTypes::const_iterator it = pointerTypes.find(res);
        if(it != pointerTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new meta type and return it.
        pointerTypes.insert(res);
        return res;
    }

    llvm::DIType PointerType::CreateDebugType(DebugInformation *context) const
    {
        Module *module = context->GetModule();
        const llvm::TargetData *targetData = module->GetTargetData();
        return context->GetDebugBuilder()
                .createPointerType(pointedType->GetDebugType(context),
                    targetData->getPointerSizeInBits(),
                    targetData->getPointerABIAlignment()*8);
    }

    // ReferenceType
    ReferenceType::ReferenceTypes ReferenceType::referenceTypes;

    ReferenceType::ReferenceType(VirtualMachine *vm, const ChelaType *type, ReferenceTypeFlow flow, bool streamReference)
        : ChelaType(vm), referencedType(type), flow(flow), streamReference(streamReference)
    {
        target = NULL;
    }

    ReferenceType::~ReferenceType()
    {
    }

    std::string ReferenceType::GetName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the reference flow.
        switch(flow)
        {
        case RTF_Out:
            out << "out ";
            break;
        case RTF_In:
            out << "in ";
            break;
        case RTF_InOut:
        default:
            break;
        }

        // Add the referenced type.
        if(referencedType)
            out << referencedType->GetName();
        else
            out << "<unknown>";

        // Add the operator.
        if(streamReference)
            out << "$&";
        else
            out << "&";
        return out.str();
    }

    std::string ReferenceType::GetFullName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the reference flow.
        switch(flow)
        {
        case RTF_Out:
            out << "out ";
            break;
        case RTF_In:
            out << "in ";
            break;
        case RTF_InOut:
        default:
            break;
        }

        // Add the referenced type.
        if(referencedType)
            out << referencedType->GetFullName();
        else
            out << "<unknown>";

        // Add the reference kind.
        if(streamReference)
            out << "$&";
        else
            out << "&";
        return out.str();
    }

    std::string ReferenceType::GetMangledName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << 'R';

        switch(flow)
        {
        case RTF_In:
            out << 'i';
            break;
        case RTF_Out:
            out << 'o';
            break;
        case RTF_InOut:
            break;
        }

        // Add the referenced type.
        if(referencedType)
            out << referencedType->GetMangledName();
        else
            out << "RN"; // Reference to null.

        return out.str();
    }
    
    llvm::Type *ReferenceType::GetTargetType() const
    {
        if(!target)
            target =
                llvm::PointerType::getUnqual(referencedType ? referencedType->GetTargetType() :
                                     llvm::Type::getInt8PtrTy(GetLlvmContext()));

        return target;
    }

    bool ReferenceType::IsReference() const
    {
        return true;
    }

    bool ReferenceType::IsOutReference() const
    {
        return flow == RTF_Out;
    }

    bool ReferenceType::IsStreamReference() const
    {
        return streamReference;
    }

    bool ReferenceType::IsGenericType() const
    {
        return referencedType->IsGenericType();
    }

    bool ReferenceType::IsUnsafe() const
    {
        if(!referencedType)
            return false;
        return referencedType->IsUnsafe();
    }

    size_t ReferenceType::GetSize() const
    {
        return sizeof(void*);
    }

    const ChelaType *ReferenceType::GetReferencedType() const
    {
        return referencedType;
    }

    bool ReferenceType::operator<(const ReferenceType &o) const
    {
        if(flow != o.flow)
            return flow < o.flow;
        if(streamReference != o.streamReference)
            return streamReference < o.streamReference;
        return referencedType < o.referencedType;
    }

    const ChelaType *ReferenceType::InstanceGeneric(const GenericInstance *instance) const
    {
        return Create(referencedType->InstanceGeneric(instance), flow, streamReference);
    }

    const ReferenceType *ReferenceType::Create(VirtualMachine *vm, const ChelaType *type, ReferenceTypeFlow flow, bool streamRef)
    {
        // Create the reference type.
        ReferenceType *res = new ReferenceType(vm, type, flow, streamRef);

        // Find an existing one.
        ReferenceTypes::const_iterator it = referenceTypes.find(res);
        if(it != referenceTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new meta type and return it.
        referenceTypes.insert(res);
        return res;
    }

    const ReferenceType *ReferenceType::Create(const ChelaType *type, ReferenceTypeFlow flow, bool streamRef)
    {
        return Create(type->GetVM(), type, flow, streamRef);
    }

    const ReferenceType *ReferenceType::CreateNullReference(VirtualMachine *vm)
    {
        return Create(vm, NULL, RTF_InOut, false);
    }

    const ReferenceType *ReferenceType::Create(const ChelaType *type)
    {
        return Create(type, RTF_InOut, false);
    }

    llvm::DIType ReferenceType::CreateDebugType(DebugInformation *context) const
    {
        Module *module = context->GetModule();
        const llvm::TargetData *targetData = module->GetTargetData();
        return context->GetDebugBuilder()
                .createPointerType(referencedType->GetDebugType(context),
                    targetData->getPointerSizeInBits(),
                    targetData->getPointerABIAlignment()*8);

    }

    // ConstantType
    ConstantType::ConstantTypes ConstantType::constantTypes;

    ConstantType::ConstantType(const ChelaType *type)
        : ChelaType(type->GetVM()), valueType(type)
    {
        target = NULL;
    }

    ConstantType::~ConstantType()
    {
    }

    std::string ConstantType::GetName() const
    {
        if(valueType->IsPointer())
            return valueType->GetName() + "* const";
        else
            return "const " + valueType->GetName();
    }

    std::string ConstantType::GetMangledName() const
    {
        return "C" + valueType->GetMangledName();
    }
    
    llvm::Type *ConstantType::GetTargetType() const
    {
        if(!target)
            target = valueType->GetTargetType();
        return target;
    }

    bool ConstantType::IsConstant() const
    {
        return true;
    }

    bool ConstantType::IsGenericType() const
    {
        return valueType->IsGenericType();
    }

    bool ConstantType::IsUnsafe() const
    {
        return valueType->IsUnsafe();
    }

    bool ConstantType::IsReadOnly() const
    {
        return true;
    }

    size_t ConstantType::GetSize() const
    {
        return sizeof(void*);
    }

    const ChelaType *ConstantType::GetValueType() const
    {
        return valueType;
    }

    bool ConstantType::operator<(const ConstantType &o) const
    {
        return valueType < o.valueType;
    }

    const ChelaType *ConstantType::InstanceGeneric(const GenericInstance *instance) const
    {
        return Create(valueType->InstanceGeneric(instance));
    }

    const ConstantType *ConstantType::Create(const ChelaType *type)
    {
        // Create the constant type.
        ConstantType *res = new ConstantType(type);

        // Find an existing one.
        ConstantTypes::const_iterator it = constantTypes.find(res);
        if(it != constantTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new constant type and return it.
        constantTypes.insert(res);
        return res;
    }

    llvm::DIType ConstantType::CreateDebugType(DebugInformation *context) const
    {
        return context->GetDebugBuilder()
            .createQualifiedType(DW_TAG_const_type, valueType->GetDebugType(context));
    }

    // ArrayType
    ArrayType::ArrayTypes ArrayType::arrayTypes;

    ArrayType::ArrayType(const ChelaType *type, int dimensions, bool readOnly)
        : ChelaType(type->GetVM()), valueType(type), dimensions(dimensions), readOnly(readOnly)
    {
        target = NULL;
    }

    ArrayType::~ArrayType()
    {
    }

    std::string ArrayType::GetName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the readonly.
        if(readOnly)
            out << "readonly ";

        // Add the value type.
        out << valueType->GetName();

        // Add the dimensions.
        out << '[' << dimensions << ']';
        return out.str();
    }

    std::string ArrayType::GetFullName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the readonly.
        if(readOnly)
            out << "readonly ";

        // Add the value type.
        out << valueType->GetFullName();

        // Add the dimensions.
        out << '[' << dimensions << ']';
        return out.str();
    }

    std::string ArrayType::GetMangledName() const
    {
        // Create the buffer.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the prefix.
        out << 'a';

        // Add the readonly flag.
        if(readOnly)
            out << 'r';

        // Add the dimensions and the value type.
        out << dimensions << valueType->GetMangledName();
        return out.str();
    }

    llvm::Type *ArrayType::GetTargetType() const
    {
        if(!target)
        {
            // Build the array type.
            std::vector<llvm::Type*> arrayLayout;

            // Add the array class data.
            arrayLayout.push_back(GetVM()->GetArrayClass()->GetTargetType());

            // Add the dimensions array.
            llvm::Type *dimensionType = llvm::Type::getInt32Ty(GetLlvmContext());
            llvm::Type *dimensionsType =
                llvm::ArrayType::get(dimensionType, dimensions);
            arrayLayout.push_back(dimensionsType);

            // Add the elements array.
            llvm::Type *arrayType =
                llvm::ArrayType::get(GetActualValueType()->GetTargetType(), 0);
            arrayLayout.push_back(arrayType);

            // Create the target type.
            target = llvm::StructType::get(GetLlvmContext(), arrayLayout, false);
        }

        return target;
    }

    llvm::DIType ArrayType::CreateDebugType(DebugInformation *context) const
    {
        return GetVM()->GetArrayClass()->GetDebugType(context);
        /*// Create the elements
        llvm::DIBuilder &builder = context->GetDebugBuilder();
        llvm::Value *elements[] = {

        };*/
    }

    bool ArrayType::IsArray() const
    {
        return true;
    }

    bool ArrayType::IsPassedByReference() const
    {
        return true;
    }

    bool ArrayType::IsGenericType() const
    {
        return valueType->IsGenericType();
    }

    bool ArrayType::IsUnsafe() const
    {
        return valueType->IsUnsafe();
    }

    size_t ArrayType::GetSize() const
    {
        return sizeof(void*);
    }

    const ChelaType *ArrayType::GetValueType() const
    {
        return valueType;
    }

    const ChelaType *ArrayType::GetActualValueType() const
    {
        if(valueType->IsPassedByReference())
            return ReferenceType::Create(valueType);
        return valueType;
    }

    int ArrayType::GetDimensions() const
    {
        return dimensions;
    }

    bool ArrayType::IsReadOnly() const
    {
        return readOnly;
    }

    bool ArrayType::operator<(const ArrayType &o) const
    {
        if(dimensions != o.dimensions)
            return dimensions < o.dimensions;
        if(readOnly != o.readOnly)
            return readOnly < o.readOnly;
        return valueType < o.valueType;
    }

    const ChelaType *ArrayType::InstanceGeneric(const GenericInstance *instance) const
    {
        return Create(valueType->InstanceGeneric(instance), dimensions, readOnly);
    }

    const ArrayType *ArrayType::Create(const ChelaType *type, int dimensions, bool readOnly)
    {
        // Implicit references are used in arrays.
        // This is required by generics.
        if(type->IsReference())
            type = DeReferenceType(type);
        if(!type)
            throw ModuleException("Expected an element type not null to create an array type.");

        // Don't allow 0 dimensions.
        if(dimensions <= 0)
            dimensions = 1;

        // Create the constant type.
        ArrayType *res = new ArrayType(type, dimensions, readOnly);

        // Find an existing one.
        ArrayTypes::const_iterator it = arrayTypes.find(res);
        if(it != arrayTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new constant type and return it.
        arrayTypes.insert(res);
        return res;
    }

    // BoxedType
    BoxedType::BoxedTypes BoxedType::boxedTypes;
    
    BoxedType::BoxedType(const Structure *valueType)
        : ChelaType(valueType->GetVM())
    {
        this->valueType = valueType;
        target = NULL;
    }

    BoxedType::~BoxedType()
    {
    }

    std::string BoxedType::GetName() const
    {
        return "[" + valueType->GetName() + "]";
    }

    std::string BoxedType::GetMangledName() const
    {
        return  "J" + valueType->GetMangledName();
    }

    llvm::Type *BoxedType::GetTargetType() const
    {
        if(target == NULL)
            target = valueType->GetBoxedType();

        return target;
    }

    bool BoxedType::IsBoxed() const
    {
        return true;
    }

    bool BoxedType::IsPassedByReference() const
    {
        return true;
    }

    bool BoxedType::IsGenericType() const
    {
        return valueType->IsGenericType();
    }

    size_t BoxedType::GetSize() const
    {
        return 0;
    }

    const Structure *BoxedType::GetValueType() const
    {
        return valueType;
    }

    bool BoxedType::operator<(const BoxedType &o) const
    {
        return valueType < o.valueType;
    }

    const ChelaType *BoxedType::InstanceGeneric(const GenericInstance *instance) const
    {
        // Hack
        Structure *building = const_cast<Structure*> (valueType);
        return Create(static_cast<const Structure*> (building->InstanceGeneric(instance->GetModule(), instance)));
    }

    const BoxedType *BoxedType::Create(const Structure *valueType)
    {
        // Create the constant type.
        BoxedType *res = new BoxedType(valueType);

        // Find an existing one.
        BoxedTypes::const_iterator it = boxedTypes.find(res);
        if(it != boxedTypes.end())
        {
            delete res;
            return *it;
        }

        // Store the new constant type and return it.
        boxedTypes.insert(res);
        return res;
    }
}
