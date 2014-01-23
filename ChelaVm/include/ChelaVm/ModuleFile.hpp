#ifndef _CHELAVM_MODULE_FILE_HPP_
#define _CHELAVM_MODULE_FILE_HPP_

#include <inttypes.h>
#include "ChelaVm/ModuleReader.hpp"

namespace ChelaVm
{
	extern const char *ModuleSignature;
	const unsigned int ModuleFormatVersionMajor = 0;
	const unsigned int ModuleFormatVersionMinor = 1;
	enum TypeKind
	{
		TK_BUILT = 0,
		TK_STRUCT,
		TK_CLASS,
        TK_INTERFACE,
		TK_FUNCTION,
		TK_REFERENCE,
		TK_POINTER,
        TK_CONSTANT,
        TK_ARRAY,
        TK_VECTOR,
        TK_MATRIX,
        TK_PLACEHOLDER,
        TK_INSTANCE,
	};

    enum ModuleType
    {
        MT_LIBRARY = 0,
        MT_EXECUTABLE,
    };
		
	struct ModuleHeader
	{
		uint8_t signature[8];
		uint32_t moduleSize;
		uint32_t formatVersionMajor;
		uint32_t formatVersionMinor;
        uint32_t moduleType;
        uint32_t entryPoint;
		uint32_t moduleRefTableEntries;
		uint32_t moduleRefTableOffset;
		uint32_t memberTableOffset;
		uint32_t memberTableSize;
		uint32_t anonTypeTableOffset;
		uint32_t anonTypeTableSize;
		uint32_t typeTableEntries;
		uint32_t typeTableOffset;
		uint32_t stringTableEntries;
		uint32_t stringTableOffset;
        uint32_t libTableOffset;
        uint32_t libTableEntries;
        uint32_t debugInfoSize;
        uint32_t debugInfoOffset;
        uint32_t resourceDataSize;
        uint32_t resourceDataOffset;

		friend ModuleReader &operator>>(ModuleReader &r, ModuleHeader &h)
		{
			r.Read(h.signature, 8);
			r >> h.moduleSize;
			r >> h.formatVersionMajor >> h.formatVersionMinor;
            r >> h.moduleType >> h.entryPoint;
			r >> h.moduleRefTableEntries >> h.moduleRefTableOffset;
			r >> h.memberTableOffset >> h.memberTableSize;
			r >> h.anonTypeTableOffset >> h.anonTypeTableSize;
			r >> h.typeTableEntries >> h.typeTableOffset;
			r >> h.stringTableEntries >> h.stringTableOffset;
            r >> h.libTableOffset >> h.libTableEntries;
            r >> h.debugInfoSize >> h.debugInfoOffset;
            r >> h.resourceDataSize >> h.resourceDataOffset;
			
			return r;
		};
	};
	
	struct ModuleReference
	{
		uint32_t moduleName;
		uint16_t versionMajor;
		uint16_t versionMinor;
		uint16_t versionMicro;
		
		friend ModuleReader &operator>>(ModuleReader &r, ModuleReference &h)
		{
			r >> h.moduleName;
			r >> h.versionMajor >> h.versionMinor >> h.versionMicro;
			return r;
		}
	};
	
	struct AnonTypeHeader
	{
		uint8_t typeKind;
		uint32_t recordSize;
		
		friend ModuleReader &operator>>(ModuleReader &r, AnonTypeHeader &h)
		{
			r >> h.typeKind >> h.recordSize;
			return r;
		}
	};
	
	struct TypeReference
	{
		uint32_t typeName;
		uint8_t typeKind;
		uint32_t memberId;
		
		friend ModuleReader &operator>>(ModuleReader &r, TypeReference &h)
		{
			r >> h.typeName  >> h.typeKind;
			r >> h.memberId;
			return r;
		}
	};
	
	struct MemberHeader
	{
		enum MemberHeaderType
		{
            Namespace = 0,
            Structure,
            Class,
            Interface,
            Function,
            Field,
            FunctionGroup,
            Property,
            TypeName,
            TypeInstance,
            TypeGroup,
            Event,
            Reference,
            FunctionInstance,
            MemberInstance,
		};

		uint8_t memberType;
		uint32_t memberName;
		uint32_t memberFlags;
		uint32_t memberSize;
        uint8_t memberAttributes;
		
		friend ModuleReader &operator>>(ModuleReader &r, MemberHeader &h)
		{
			r >> h.memberType >> h.memberName >> h.memberFlags >> h.memberSize >> h.memberAttributes;
			return r;
		}
	};
	
	struct FunctionHeader
	{
		uint32_t functionType;
        uint16_t numargs;
		uint16_t numlocals;
		uint16_t numblocks;
        uint8_t numexceptions;
		int16_t vslot;
		
		friend ModuleReader &operator>>(ModuleReader &r, FunctionHeader &h)
		{
			r >> h.functionType >> h.numargs >> h.numlocals >> h.numblocks >> h.numexceptions >> h.vslot;
			return r;
		}
	};

    struct ExceptionContextHeader
    {
        int8_t parentId;
        uint16_t numblocks;
        uint8_t numhandlers;
        int32_t cleanup;

        friend ModuleReader &operator>>(ModuleReader &r, ExceptionContextHeader &h)
        {
            r >> h.parentId >> h.numblocks >> h.numhandlers >> h.cleanup;
            return r;
        }
    };

    enum ConstantValueType
    {
        CVT_Byte = 0,
        CVT_SByte,
        CVT_Char,
        CVT_Short,
        CVT_UShort,
        CVT_Int,
        CVT_UInt,
        CVT_Long,
        CVT_ULong,
        CVT_Float,
        CVT_Double,
        CVT_Bool,
        CVT_Size,
        CVT_String,
        CVT_Null,
        CVT_Unknown,
    };
};

#endif //_CHELAVM_MODULE_FILE_HPP_
