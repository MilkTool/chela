#ifndef CHELA_TYPES_HPP
#define CHELA_TYPES_HPP

#include <stdlib.h>
#include <set>
#include <string>
#include <vector>
#include "llvm/Metadata.h"
#include "llvm/DerivedTypes.h"
#include "llvm/LLVMContext.h"
#include "llvm/Analysis/DebugInfo.h"
#include "ChelaVm/Member.hpp"

namespace ChelaVm
{
	enum PrimitiveTypeId
	{
		PTI_Void = 0,
		PTI_UInt8,
		PTI_Int8,
		PTI_UInt16,
		PTI_Int16,
		PTI_UInt32,
		PTI_Int32,
		PTI_UInt64,
		PTI_Int64,
		PTI_Fp32,
		PTI_Fp64,
		PTI_Bool,
		PTI_Size,
        PTI_PtrDiff,
		PTI_Char,
	};

    enum ReferenceTypeFlow
    {
        RTF_InOut = 0,
        RTF_Out,
        RTF_In,
    };

    class DebugInformation;
	class IntegerType;
	class FloatType;
	class StringType;
	class VoidType;
	class PointerType;
    class GenericInstance;
    class VirtualMachine;
	class ChelaType: public Member
	{
	public:
		ChelaType(Module *module);
        ChelaType(VirtualMachine *vm);
		~ChelaType();

		virtual std::string GetName() const;

		virtual bool IsType() const;
		virtual bool IsNamespace() const;
		virtual bool IsStructure() const;
		virtual bool IsClass() const;
        virtual bool IsInterface() const;
        virtual bool IsGeneric() const;

        virtual bool IsVector() const;
        virtual bool IsMatrix() const;
        virtual bool IsFirstClass() const;
		virtual bool IsPrimitive() const;
		virtual bool IsMetaType() const;
		virtual bool IsNumber() const;
        virtual bool IsSizeInteger() const;
		virtual bool IsInteger() const;
		virtual bool IsFloat() const;
		virtual bool IsDouble() const;
		virtual bool IsFloatingPoint() const;
		virtual bool IsUnsigned() const;
		virtual bool IsVoid() const;
        virtual bool IsConstant() const;
		virtual bool IsPointer() const;
        virtual bool IsArray() const;
        virtual bool IsBoxed() const;
        virtual bool IsPassedByReference() const;
        virtual bool IsComplexStructure() const;
		virtual bool IsReference() const;
		virtual bool IsFunction() const;
        virtual bool IsPlaceHolder() const;
        virtual bool IsGenericType() const;
		virtual size_t GetSize() const;
        virtual size_t GetAlign() const;

        virtual bool HasSubReferences() const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
        virtual Member *InstanceMember(Module *implementingModule, const GenericInstance *instance);

		virtual llvm::Type *GetTargetType() const;
        virtual llvm::DIType GetDebugType(DebugInformation *context) const;
        virtual llvm::DIDescriptor GetDebugNode(DebugInformation *context) const;
		
		// Primitive types.
		static const ChelaType *GetVoidType(VirtualMachine *vm);
        static const ChelaType *GetConstVoidType(VirtualMachine *vm);
		static const ChelaType *GetBoolType(VirtualMachine *vm);
		static const ChelaType *GetByteType(VirtualMachine *vm);
		static const ChelaType *GetSByteType(VirtualMachine *vm);
		static const ChelaType *GetCharType(VirtualMachine *vm);
		static const ChelaType *GetShortType(VirtualMachine *vm);
		static const ChelaType *GetUShortType(VirtualMachine *vm);
		static const ChelaType *GetIntType(VirtualMachine *vm);
		static const ChelaType *GetUIntType(VirtualMachine *vm);
		static const ChelaType *GetLongType(VirtualMachine *vm);
		static const ChelaType *GetULongType(VirtualMachine *vm);
		static const ChelaType *GetFloatType(VirtualMachine *vm);
		static const ChelaType *GetDoubleType(VirtualMachine *vm);
		static const ChelaType *GetSizeType(VirtualMachine *vm);
        static const ChelaType *GetPtrDiffType(VirtualMachine *vm);
        static const ChelaType *GetCStringType(VirtualMachine *vm);
		static const ChelaType *GetNullType(VirtualMachine *vm);
		
		// Some pointers.
		static const PointerType *GetVoidPointerType(VirtualMachine *vm);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;
    };

	class IntegerType: public ChelaType
	{
	public:
		~IntegerType();

		virtual std::string GetName() const;
        virtual std::string GetMangledName() const;

		virtual bool IsPrimitive() const;
		virtual bool IsNumber() const;
        virtual bool IsSizeInteger() const;
		virtual bool IsInteger() const;
		virtual bool IsFloat() const;
		virtual bool IsDouble() const;
		virtual bool IsFloatingPoint() const;
		virtual bool IsUnsigned() const;
		virtual size_t GetSize() const;
        virtual size_t GetAlign() const;
		virtual llvm::Type *GetTargetType() const;

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

	private:
		friend class _VirtualMachineDataImpl;
		IntegerType(VirtualMachine *vm, const std::string &name, const std::string &mangled, bool sign,
                    size_t size, size_t align, llvm::Type *target = NULL);

		std::string name;
        std::string mangledName;
		bool sign;
		size_t size;
        size_t align;
		llvm::Type *target;
	};

	class FloatType: public ChelaType
	{
	public:
		~FloatType();

		virtual std::string GetName() const;
        virtual std::string GetMangledName() const;

		virtual bool IsPrimitive() const;
		virtual bool IsNumber() const;
		virtual bool IsInteger() const;
		virtual bool IsFloat() const;
		virtual bool IsDouble() const;
		virtual bool IsFloatingPoint() const;
		virtual bool IsUnsigned() const;
		virtual size_t GetSize() const;
        virtual size_t GetAlign() const;
		virtual llvm::Type *GetTargetType() const;

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

	private:
		friend class _VirtualMachineDataImpl;
		FloatType(VirtualMachine *vm, const std::string &name, const std::string &mangled, bool isDouble, llvm::Type *target = NULL);

		std::string name;
        std::string mangledName;

		bool isDouble;
		llvm::Type *target;
	};

	class VoidType: public ChelaType
	{
	public:
        VoidType(VirtualMachine *vm);

		virtual std::string GetName() const;
        virtual std::string GetMangledName() const;

		virtual bool IsVoid() const;
		virtual llvm::Type *GetTargetType() const;

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;
	};

	class FunctionType: public ChelaType
	{
	public:
		typedef std::vector<const ChelaType*> Arguments;
		typedef Arguments::const_iterator arg_iterator;

		~FunctionType();

		virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;
		virtual llvm::Type *GetTargetType() const;
        llvm::FunctionType *GetTargetFunctionType() const;

		virtual bool IsFunction() const;
        virtual bool IsGenericType() const;
        virtual bool IsUnsafe() const;
		bool IsVariable() const;
        int GetFunctionFlags() const;

		const ChelaType *GetReturnType() const;

        size_t GetArgumentCount() const;
        const ChelaType *GetArgument(size_t index) const;

		size_t arg_size() const;
		arg_iterator arg_begin() const;
		arg_iterator arg_end() const;

		bool operator<(const FunctionType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;

		static const FunctionType *Create(const ChelaType* ret, const Arguments &args, bool variable = false, int flags = 0);
		static const FunctionType *Create(const ChelaType* ret, bool variable = false, int flags = 0);
		friend bool MatchFunctionType(const FunctionType *a, const FunctionType *b, size_t skiparg);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

	private:
		struct ValueLess
		{
			bool operator()(const FunctionType *a, const FunctionType *b)
			{
				return (*a) < (*b);
			}
		};
		typedef std::set<const FunctionType*, ValueLess> FunctionTypes;

		FunctionType(const ChelaType *returnType, const Arguments &arguments,
						bool variable, int flags);

		const ChelaType *returnType;
		Arguments arguments;
		bool variable;
        int flags;
		static FunctionTypes functionTypes;
		mutable llvm::Type *target;
	};

	inline bool MatchFunctionType(const FunctionType *a, const FunctionType *b, size_t skiparg)
	{
		if(skiparg == 0)
			return a == b;
			
		if(a->arguments.size() < skiparg || b->arguments.size() < skiparg ||
			a->arguments.size() != b->arguments.size())
			return false;
			
		if(a->returnType != b->returnType)
			return false;
			
		for(size_t i = skiparg; i < a->arguments.size(); i++)
		{
			if(a->arguments[i] != b->arguments[i])
				return false;
		}
		
		return true;
	}

	class MetaType: public ChelaType
	{
	public:
		~MetaType();

		virtual bool IsMetaType() const;
        virtual bool IsGenericType() const;
		const ChelaType *GetActualType() const;

		bool operator<(const MetaType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;

		static const MetaType *Create(const ChelaType *type);

	private:
		MetaType(const ChelaType *type);

		struct ValueLess
		{
			bool operator()(const MetaType *a, const MetaType *b)
			{
				return (*a) < (*b);
			}
		};
		typedef std::set<const MetaType*, ValueLess> MetaTypes;

		const ChelaType *actualType;
		static MetaTypes metaTypes;
	};

    class VectorType: public ChelaType
    {
    public:
        ~VectorType();

        virtual std::string GetName() const;
        virtual std::string GetMangledName() const;

        virtual llvm::Type *GetTargetType() const;

        virtual bool IsVector() const;
        virtual bool IsNumber() const;
        virtual bool IsInteger() const;
        virtual bool IsFloatingPoint() const;
        virtual bool IsFloat() const;
        virtual bool IsDouble() const;
        virtual bool IsGenericType() const;

        virtual size_t GetSize() const;
        virtual size_t GetAlign() const;

        int GetNumComponents() const;
        const ChelaType *GetPrimitiveType() const;

        bool operator<(const VectorType &o) const;

        static const VectorType *Create(const ChelaType *type, int numcomponents);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

    private:
        VectorType(const ChelaType *type, int numcomponents);

        struct ValueLess
        {
            bool operator()(const VectorType *a, const VectorType *b)
            {
                return (*a) < (*b);
            }
        };
        typedef std::set<const VectorType*, ValueLess> VectorTypes;

        const ChelaType *primitiveType;
        int numcomponents;
        static VectorTypes vectorTypes;
        mutable llvm::Type *target;
    };

    class MatrixType: public ChelaType
    {
    public:
        ~MatrixType();

        virtual std::string GetName() const;
        virtual std::string GetMangledName() const;

        virtual llvm::Type *GetTargetType() const;

        virtual bool IsMatrix() const;
        virtual bool IsNumber() const;
        virtual bool IsInteger() const;
        virtual bool IsFloatingPoint() const;
        virtual bool IsFloat() const;
        virtual bool IsDouble() const;
        virtual bool IsGenericType() const;

        virtual size_t GetSize() const;

        int GetNumRows() const;
        int GetNumColumns() const;
        const ChelaType *GetPrimitiveType() const;

        bool operator<(const MatrixType &o) const;

        static const MatrixType *Create(const ChelaType *type, int numrows, int numcolumns);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

    private:
        MatrixType(const ChelaType *type, int numrows, int numcolumns);

        struct ValueLess
        {
            bool operator()(const MatrixType *a, const MatrixType *b)
            {
                return (*a) < (*b);
            }
        };
        typedef std::set<const MatrixType*, ValueLess> MatrixTypes;

        const ChelaType *primitiveType;
        int numrows, numcolumns;
        static MatrixTypes matrixTypes;
        mutable llvm::Type *target;
    };

	class PointerType: public ChelaType
	{
	public:
		~PointerType();

		virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;
		virtual llvm::Type *GetTargetType() const;

		virtual bool IsPointer() const;
        virtual bool IsGenericType() const;
        virtual bool IsUnsafe() const;
		virtual size_t GetSize() const;
		const ChelaType *GetPointedType() const;

		bool operator<(const PointerType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
		static const PointerType *Create(const ChelaType *type);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

	private:
		PointerType(const ChelaType *type);

		struct ValueLess
		{
			bool operator()(const PointerType *a, const PointerType *b)
			{
				return (*a) < (*b);
			}
		};
		typedef std::set<const PointerType*, ValueLess> PointerTypes;

		const ChelaType *pointedType;
		static PointerTypes pointerTypes;
		mutable llvm::Type *target;
	};

	class ReferenceType: public ChelaType
	{
	public:
		~ReferenceType();

		virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;
		virtual llvm::Type *GetTargetType() const;

		virtual bool IsReference() const;
        virtual bool IsOutReference() const;
        virtual bool IsStreamReference() const;
        virtual bool IsGenericType() const;
        virtual bool IsUnsafe() const;
		virtual size_t GetSize() const;
		const ChelaType *GetReferencedType() const;
        ReferenceTypeFlow GetFlow() const;

		bool operator<(const ReferenceType &o) const;

        const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
		static const ReferenceType *Create(const ChelaType *type, ReferenceTypeFlow flow, bool streamReference);
        static const ReferenceType *Create(const ChelaType *type);
        static const ReferenceType *CreateNullReference(VirtualMachine *vm);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

	private:
 		ReferenceType(VirtualMachine *vm, const ChelaType *type, ReferenceTypeFlow flow, bool streamReference);
         static const ReferenceType *Create(VirtualMachine *vm, const ChelaType *type, ReferenceTypeFlow flow, bool streamReference);

		struct ValueLess
		{
			bool operator()(const ReferenceType *a, const ReferenceType *b)
			{
				return (*a) < (*b);
			}
		};
		typedef std::set<const ReferenceType*, ValueLess> ReferenceTypes;

		const ChelaType *referencedType;
        ReferenceTypeFlow flow;
        bool streamReference;
		static ReferenceTypes referenceTypes;
		mutable llvm::Type *target;
	};

    class ConstantType: public ChelaType
    {
    public:
        ~ConstantType();

        virtual std::string GetName() const;
        virtual std::string GetMangledName() const;
        virtual llvm::Type *GetTargetType() const;

        virtual bool IsConstant() const;
        virtual bool IsGenericType() const;
        virtual bool IsUnsafe() const;
        virtual bool IsReadOnly() const;
        virtual size_t GetSize() const;
        const ChelaType *GetValueType() const;

        bool operator<(const ConstantType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
        static const ConstantType *Create(const ChelaType *type);

    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;
        
    private:
        ConstantType(const ChelaType *type);

        struct ValueLess
        {
            bool operator()(const ConstantType *a, const ConstantType *b)
            {
                return (*a) < (*b);
            }
        };
        typedef std::set<const ConstantType*, ValueLess> ConstantTypes;

        const ChelaType *valueType;
        static ConstantTypes constantTypes;
        mutable llvm::Type *target;
    };

    class VirtualMachine;
    class Class;
    class ArrayType: public ChelaType
    {
    public:
        ~ArrayType();

        virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;

        virtual llvm::Type *GetTargetType() const;

        virtual bool IsArray() const;
        virtual bool IsPassedByReference() const;
        virtual bool IsGenericType() const;
        virtual bool IsUnsafe() const;
        virtual size_t GetSize() const;
        const ChelaType *GetValueType() const;
        const ChelaType *GetActualValueType() const;

        int GetDimensions() const;
        bool IsReadOnly() const;

        bool operator<(const ArrayType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
        static const ArrayType *Create(const ChelaType *type, int dimensions, bool readOnly);
        
    protected:
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

    private:
        ArrayType(const ChelaType *type, int dimensions, bool readOnly);

        struct ValueLess
        {
            bool operator()(const ArrayType *a, const ArrayType *b)
            {
                return (*a) < (*b);
            }
        };
        typedef std::set<const ArrayType*, ValueLess> ArrayTypes;

        const ChelaType *valueType;
        int dimensions;
        bool readOnly;
        static ArrayTypes arrayTypes;
        mutable llvm::Type *target;
    };

    class Structure;
    class BoxedType: public ChelaType
    {
    public:
        ~BoxedType();

        virtual std::string GetName() const;
        virtual std::string GetMangledName() const;
        virtual llvm::Type *GetTargetType() const;

        virtual bool IsBoxed() const;
        virtual bool IsPassedByReference() const;
        virtual bool IsGenericType() const;
        virtual size_t GetSize() const;
        const Structure *GetValueType() const;

        bool operator<(const BoxedType &o) const;

        virtual const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
        static const BoxedType *Create(const Structure *valueType);

    private:
        BoxedType(const Structure *valueType);

        struct ValueLess
        {
            bool operator()(const BoxedType *a, const BoxedType *b)
            {
                return (*a) < (*b);
            }
        };
        typedef std::set<const BoxedType*, ValueLess> BoxedTypes;

        const Structure *valueType;
        static BoxedTypes boxedTypes;
        mutable llvm::Type *target;
    };

	inline const ChelaType *DeReferenceType(const ChelaType *type)
	{
		const ReferenceType *refType = static_cast<const ReferenceType*> (type);
		return refType->GetReferencedType();
	}
	
	inline const ChelaType *TryDeReferenceType(const ChelaType *type)
	{
		if(type->IsReference())
			return DeReferenceType(type);
		return type;
	}

	inline const ChelaType *DePointerType(const ChelaType *type)
	{
		const PointerType *pointerType = static_cast<const PointerType*> (type);
		return pointerType->GetPointedType();
	}
	
	inline const ChelaType *TryDePointerType(const ChelaType *type)
	{
		if(type->IsPointer())
			return DePointerType(type);
		return type;
	}

    inline const ChelaType *DeConstType(const ChelaType *type)
    {
        const ConstantType *constantType = static_cast<const ConstantType*> (type);
        return constantType->GetValueType();
    }

    inline const ChelaType *TryDeConstType(const ChelaType *type)
    {
        if(type->IsConstant())
            return DeConstType(type);
        return type;
    }

    inline const ChelaType *GetFloatVectorTy(VirtualMachine *vm, int numcomps)
    {
        return VectorType::Create(ChelaType::GetFloatType(vm), numcomps);
    }

    inline const ChelaType *GetDoubleVectorTy(VirtualMachine *vm, int numcomps)
    {
        return VectorType::Create(ChelaType::GetDoubleType(vm), numcomps);
    }

    inline const ChelaType *GetIntVectorTy(VirtualMachine *vm, int numcomps)
    {
        return VectorType::Create(ChelaType::GetIntType(vm), numcomps);
    }

    inline const ChelaType *GetBoolVectorTy(VirtualMachine *vm, int numcomps)
    {
        return VectorType::Create(ChelaType::GetBoolType(vm), numcomps);
    }

    inline const ChelaType *GetMatOrVec(const ChelaType *primType, int rows, int cols)
    {
        if(rows == 1 && cols == 1)
            return primType;
        else if(rows == 1)
            return VectorType::Create(primType, cols);
        else if(cols == 1)
            return VectorType::Create(primType, rows);
        else
            return MatrixType::Create(primType, rows, cols);
    }

    inline const ChelaType *GetFloatMatrixTy(VirtualMachine *vm, int numcols, int numrows)
    {
        return GetMatOrVec(ChelaType::GetFloatType(vm), numcols, numrows);
    }

    inline const ChelaType *GetDoubleMatrixTy(VirtualMachine *vm, int numcols, int numrows)
    {
        return GetMatOrVec(ChelaType::GetDoubleType(vm), numcols, numrows);
    }
};

#endif //CHELA_TYPES_HPP
