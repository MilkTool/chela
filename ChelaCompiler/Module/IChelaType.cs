using System;
namespace Chela.Compiler.Module
{
	public interface IChelaType
	{
		string GetName();
		string GetFullName();
        string GetDisplayName();

        bool IsAbstract();
        bool IsFirstClass();
		bool IsPrimitive();
		bool IsMetaType();
		bool IsNumber();
		bool IsInteger();
		bool IsFloat();
		bool IsDouble();
		bool IsFloatingPoint();
		bool IsUnsigned();
		bool IsVoid();
        bool IsConstant();
		bool IsPointer();
        bool IsArray();
        bool IsVector();
        bool IsMatrix();
		bool IsReference();
        bool IsOutReference();
        bool IsStreamReference();
		bool IsNamespace();
        bool IsTypeInstance();
		bool IsInterface();
		bool IsStructure();
		bool IsClass();
        bool IsPassedByReference();
		bool IsFunction();
		bool IsFunctionGroup();
        bool IsIncompleteType();
        bool IsPlaceHolderType();
        bool IsGenericType();
        bool IsGenericInstance();
        bool IsGenericImplicit();
        bool IsTypeGroup();
        bool IsReadOnly();
        bool IsUnsafe();

        IChelaType InstanceGeneric(GenericInstance args, ChelaModule instModule);

		uint GetSize();
        uint GetComponentSize();
		ChelaModule GetModule();
	}
}

