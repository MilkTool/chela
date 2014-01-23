#include "ChelaVm/Module.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/Namespace.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/Interface.hpp"
#include "ChelaVm/Field.hpp"
#include "ChelaVm/FileSystem.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionInstance.hpp"
#include "ChelaVm/FunctionGroup.hpp"
#include "ChelaVm/MemberInstance.hpp"
#include "ChelaVm/PlaceHolderType.hpp"
#include "ChelaVm/Property.hpp"
#include "ChelaVm/System.hpp"
#include "ChelaVm/TypeInstance.hpp"
#include "ChelaVm/TypeGroup.hpp"
#include "ChelaVm/AttributeConstant.hpp"
#include "ChelaVm/PEFormat.hpp"
#include "ChelaVm/ELFFormat.hpp"
#include "llvm/Analysis/Verifier.h"
#include "llvm/Analysis/Passes.h"
#include "llvm/Target/TargetData.h"
#include "llvm/Transforms/Scalar.h"
#include "llvm/Bitcode/ReaderWriter.h"
#include "llvm/Support/raw_ostream.h"
#include "llvm/Support/FormattedStream.h"
#include "llvm/Support/TargetRegistry.h"
#include "llvm/Support/TargetSelect.h"
#include "llvm/Support/ToolOutputFile.h"
#include "llvm/Support/Host.h"
#include "llvm/Analysis/Verifier.h"
#include "llvm/Target/TargetMachine.h"
#include <algorithm>
#include <memory>

namespace ChelaVm
{
    // TypeReferenceCache
    struct TypeReferenceCache
    {
        uint32_t typeName;
        TypeKind kind;
        uint32_t memberId;
        const ChelaType *cached;
        bool loading;
        
        TypeReferenceCache()
        {
            cached = NULL;
            loading = false;
        }
    };
    
    // AnonTypeCache
    struct AnonTypeCache
    {
        TypeKind kind;
        std::vector<uint32_t> components;
        bool variableArg; // Used by function types.
        const ChelaType *cached;
        bool loading;
        int extraData;
        int extraData2;
        int extraData3;
        
        AnonTypeCache()
        {
            cached = NULL;
            loading = false;
            extraData3 = extraData2 = extraData = 0;
        }
    };
    
    // Module
    Module::Module(VirtualMachine *virtualMachine, bool noreflection, bool debugging)
        : virtualMachine(virtualMachine), irBuilder(virtualMachine->getContext()),
          noreflection(noreflection), debugging(debugging)
    {
        targetModule = NULL;
        functionPassManager = NULL;
        interfaceImplStruct = NULL;
        debugInfo = NULL;
        invokedConstructors = false;
        declared = false;
        defined = false;
        definingGenericStructures = false;
        definingGenericFunctions = false;
        compiled = false;
        optimizationLevel = 0;
        moduleSize = 0;
        moduleData = NULL;
        moduleDataStartPointer = NULL;
        assemblyVariable = NULL;
        entryPoint = NULL;
        runtimeFlag = false;
        moduleReferences.push_back(this);
    }
    
    Module::~Module()
    {
        delete [] moduleData;
        delete debugInfo;
    }

    ModuleName &Module::GetName()
    {
        return name;
    }
    
    const ModuleName &Module::GetName() const
    {
        return name;
    }

    const std::string &Module::GetMangledName() const
    {
        if(mangledName.empty())
        {
            char temp[64];
            sprintf(temp, "_M%lu%s%d", (unsigned long)name.name.size(), name.name.c_str(), name.versionMajor);
            mangledName = temp;
        }

        return mangledName;
    }

    Namespace *Module::GetGlobalNamespace() const
    {
        return globalNamespace;
    }
    
    const std::string &Module::GetString(size_t id)
    {
        static std::string empty = "";
        if(id == 0)
            return empty;
        else if(id > stringTable.size())
            throw ModuleException("Invalid string id.");
        return stringTable[id-1];
    }
    
    Module *Module::GetReferencedModule(uint32_t id)
    {
        if(id >= moduleReferences.size())
            throw ModuleException("Invalid module reference id.");
        return moduleReferences[id];        
    }
    
    Member *Module::GetMember(uint32_t id)
    {
        if(id == 0 || id > memberTable.size())
            throw ModuleException("Invalid module member id.");
        return memberTable[id-1];
    }
    
    Member *Module::GetMember(const std::string &fullname)
    {
        MemberNameTable::iterator it = memberNameTable.find(fullname);
        if(it == memberNameTable.end())
            return NULL;
        return it->second;
    }

    Function *Module::GetFunction(uint32_t id)
    {
        Member *member = GetMember(id);
        if(!member) return NULL;
        if(!member->IsFunction() || member->IsType())
            throw ModuleException("expected function member.");
        return static_cast<Function *> (member);
    }

    Class *Module::GetClass(uint32_t id)
    {
        Member *member = GetMember(id);
        if(!member) return NULL;
        if(!member->IsClass()) throw ModuleException("expected class member.");
        return static_cast<Class *> (member);
    }

    const Class *Module::GetClassType(uint32_t id)
    {
        const ChelaType *type = GetType(id);
        if(!type) return NULL;
        if(!type->IsClass()) throw ModuleException("expected class type");
        return static_cast<const Class *> (type);
    }

    Structure *Module::GetStructure(uint32_t id)
    {
        Member *member = GetMember(id);
        if(!member) return NULL;
        if(!member->IsStructure()) throw ModuleException("expected structure member.");
        return static_cast<Structure *> (member);
    }

    Interface *Module::GetInterface(uint32_t id)
    {
        Member *member = GetMember(id);
        if(!member) return NULL;
        if(!member->IsInterface()) throw ModuleException("expected interface member.");
        return static_cast<Interface *> (member);
    }

    int Module::GetOptimizationLevel() const
    {
        return optimizationLevel;
    }

    void Module::SetOptimizationLevel(int level)
    {
        optimizationLevel = level;
    }

    // Libaries.
    void Module::AddLibrary(const std::string &library)
    {
        libReferences.push_back(library);
        targetModule->addLibrary(library);
    }

    // Library paths.
    void Module::AddLibraryPath(const std::string &libPath)
    {
        libPaths.push_back(libPath);
    }

    size_t Module::GetLibraryPathCount() const
    {
        return libPaths.size();
    }

    const std::string &Module::GetLibraryPath(size_t index) const
    {
        return libPaths[index];
    }
    
    // Module type.
    bool Module::IsSharedLibrary() const
    {
        return moduleType == MT_LIBRARY;
    }
    
    bool Module::IsStaticLibrary() const
    {
        return false;
    }
    
    bool Module::IsProgram() const
    {
        return moduleType == MT_EXECUTABLE;
    }

    // Type finding.
    const ChelaType *Module::GetType(uint32_t id)
    {
        // Get the virtual machine.
        VirtualMachine *vm = GetVirtualMachine();

        // Check for primitive types.
        if(id <= 0xFF)
        {
            // Return the primitive type.
            switch(id)
            {
            case PTI_Void:
                return ChelaType::GetVoidType(vm);
            case PTI_UInt8:
                return ChelaType::GetByteType(vm);
            case PTI_Int8:
                return ChelaType::GetSByteType(vm);
            case PTI_UInt16:
                return ChelaType::GetUShortType(vm);
            case PTI_Int16:
                return ChelaType::GetShortType(vm);
            case PTI_UInt32:
                return ChelaType::GetUIntType(vm);
            case PTI_Int32:
                return ChelaType::GetIntType(vm);
            case PTI_UInt64:
                return ChelaType::GetULongType(vm);
            case PTI_Int64:
                return ChelaType::GetLongType(vm);
            case PTI_Fp32:
                return ChelaType::GetFloatType(vm);
            case PTI_Fp64:
                return ChelaType::GetDoubleType(vm);
            case PTI_Bool:
                return ChelaType::GetBoolType(vm);
            case PTI_Size:
                return ChelaType::GetSizeType(vm);
            case PTI_PtrDiff:
                return ChelaType::GetPtrDiffType(vm);
            case PTI_Char:
                return ChelaType::GetCharType(vm);
            }
        }
        
        // Decrease the type id to perform a table lookup.
        id -= 0x100;
        if(id >= typeTable.size())
            throw ModuleException("Invalid type id.");
        
        // Get the type cache.
        TypeReferenceCache &cache = typeTable[id];
        
        // Return the cached type.
        if(cache.cached != NULL)
            return cache.cached;
        
        // Prevent circular references.
        if(cache.loading)
            throw ModuleException("Unsupported types circular references.");
        cache.loading = true;
        
        // Load the type.
        switch(cache.kind)
        {
        case TK_STRUCT:
        case TK_CLASS:
        case TK_INTERFACE:
            {
                // Find the struct or class.
                Member *type = GetMember(cache.memberId);
                
                // Make sure its an acceptable member
                if(!type->IsStructure() && !type->IsClass() && !type->IsInterface())
                    throw ModuleException("Invalid structure/class/interface reference.");
                    
                // Store it in the cache.
                cache.cached = (ChelaType*)type;
            }
            break;
        case TK_INSTANCE:
            {
                // Find the type instance member.
                Member *typeInstance = GetMember(cache.memberId);

                // Make sure its an acceptable member.
                if(!typeInstance->IsTypeInstance())
                    throw ModuleException("invalid type instance.");

                // Cast the type instance.
                TypeInstance *instance = static_cast<TypeInstance *> (typeInstance);

                // Store it in the cache.
                if(instance->IsGeneric())
                    cache.cached = instance;
                else
                    cache.cached = instance->GetImplementation();
            }
            break;
        case TK_FUNCTION:
        case TK_REFERENCE:
        case TK_POINTER:
        case TK_CONSTANT:
        case TK_ARRAY:
        case TK_VECTOR:
        case TK_MATRIX:
        case TK_PLACEHOLDER:
            {    
                // Load the anonymous type.
                cache.cached = LoadAnonType(cache.kind, cache.memberId);
            }
            break;
        default:
            throw ModuleException("Invalid type.");
            break;
        }

        // Unset the loading flag.
        cache.loading = false;
                
        // Return the type.
        return cache.cached;
    }
    
    const ChelaType *Module::LoadAnonType(TypeKind kind, uint32_t id)
    {
        // Check the bounds.
        if(id >= anonTypeTable.size())
            throw ModuleException("Invalid anonymous type id.");
        
        // Get the type cache.    
        AnonTypeCache &cache = anonTypeTable[id];
        
        // Make sure the type kind is the correct.
        if(kind != cache.kind)
            throw ModuleException("Incompatible anonymous type.");
        
        // Return the cached type, if available.
        if(cache.cached != NULL)
            return cache.cached;
        
        // Prevent circular references.
        if(cache.loading)
            throw ModuleException("Unsupported types circular references.");
        cache.loading = true;

        // Get the void and const void type.
        VirtualMachine *vm = GetVirtualMachine();
        const ChelaType *voidTy = ChelaType::GetVoidType(vm);
        const ChelaType *constVoidTy = ChelaType::GetConstVoidType(vm);
        
        // Load the anonymous type.
        switch(cache.kind)
        {
        case TK_FUNCTION:
            {
                // Store the arguments.
                std::vector<const ChelaType*> arguments;
                for(size_t i = 1; i < cache.components.size(); i++)
                {
                    const ChelaType *argType = GetType(cache.components[i]);
                    if(argType == voidTy || argType == constVoidTy)
                        throw ModuleException("Cannot have functions with void parameters");
                    arguments.push_back(argType);
                }

                // Create the function type.
                cache.cached = FunctionType::Create(GetType(cache.components[0]),
                                arguments, cache.variableArg, cache.extraData);
            }
            break;
        case TK_REFERENCE:
            // Only one component is accepted.
            if(cache.components.size() != 1)
                throw ModuleException("Invalid reference type description.");
            cache.cached = ReferenceType::Create(GetType(cache.components[0]), (ReferenceTypeFlow)cache.extraData, cache.extraData2);
            break;
        case TK_POINTER:
            // Only one component is accepted.
            if(cache.components.size() != 1)
                throw ModuleException("Invalid pointer type description.");
            cache.cached = PointerType::Create(GetType(cache.components[0]));
            break;
        case TK_CONSTANT:
            // Only one component is accepted.
            if(cache.components.size() != 1)
                throw ModuleException("Invalid constant type description.");
            cache.cached = ConstantType::Create(GetType(cache.components[0]));
            break;
        case TK_ARRAY:
            {
                // The element type cannot be void.
                const ChelaType *elementType = GetType(cache.components[0]);
                if(elementType == voidTy || elementType == constVoidTy)
                    throw ModuleException("Cannot have arrays of void.");

                // Only one component is accepted.
                if(cache.components.size() != 1)
                    throw ModuleException("Invalid array type description.");
                cache.cached = ArrayType::Create(elementType, cache.extraData, cache.extraData2);
            }
            break;
        case TK_VECTOR:
            // Only one component is accepted.
            if(cache.components.size() != 1)
                throw ModuleException("Invalid vector type description.");
            cache.cached = VectorType::Create(GetType(cache.components[0]), cache.extraData);
            break;
        case TK_MATRIX:
            // Only one component is accepted.
            if(cache.components.size() != 1)
                throw ModuleException("Invalid matrix type description.");
            cache.cached = MatrixType::Create(GetType(cache.components[0]), cache.extraData, cache.extraData2);
            break;
        case TK_PLACEHOLDER:
            {
                // Store the bases.
                std::vector<Structure*> bases;
                for(size_t i = 0; i < cache.components.size(); i++)
                {
                    Member *member = GetMember(cache.components[i]);
                    if(!member->IsClass() && !member->IsInterface())
                        throw ModuleException("Expected class/interface member.");
                    bases.push_back(static_cast<Structure*> (member));
                }

                // Create the place holder type.
                cache.cached = new PlaceHolderType(this, GetString(cache.extraData), cache.extraData2, cache.extraData3, bases);
            }
            break;
        default:
            throw ModuleException("Unsupported anonymous type kind.");
            break;
        }
        
        // Unset the loading flag.
        cache.loading = false;
        return cache.cached;        
    }

    inline void SkipAttributes(Module *mod, ModuleReader &reader, size_t count)
    {
        for(size_t i = 0; i < count; ++i)
        {
            AttributeConstant attr(mod);
            attr.Read(reader);
        }
    }

    void Module::LoadFromFile(ModuleReader &reader)
    {
        // Store the module position.
        size_t modulePosition = reader.GetPosition();

        // Get the module size.
        reader.Seek(0, SEEK_END);
        moduleSize = reader.GetPosition();
        reader.Seek(modulePosition, SEEK_SET);

        // Store the file name.
        name.filename = reader.GetFileName();

        // Guess the file type by his signature.
        uint8_t signature[8];
        reader.Read(signature, 8);
        if(strncmp((char*)signature, ModuleSignature, 8))
        {
            // Check for embedded modules
            if(!strncmp((char*)signature, "MZ", 2))
            {
                // This is a PE file.
                reader.Seek(modulePosition, SEEK_SET);
                moduleSize = PEFormat_GetEmbeddedModule(reader);
                modulePosition = reader.GetPosition();
            }
            else if(!strncmp((char*)signature, (char*)ElfMagic, 4))
            {
                // This is an ELF file.
                reader.Seek(modulePosition, SEEK_SET);
                moduleSize = ELFFormat_GetEmbeddedModule(reader);
                modulePosition = reader.GetPosition();
            }
            else
                throw ModuleException("Invalid module signature.");
        }

        // Set the reader base.
        reader.Seek(modulePosition, SEEK_SET);

        // Read the module data into memory.
        delete [] moduleData;
        moduleData = new uint8_t[moduleSize];
        reader.Read(moduleData, moduleSize);

        // Load the module content.
        LoadFromMemory();
    }

    void Module::LoadFromMemory(size_t moduleSize, uint8_t *moduleData)
    {
        // Copy the module data.
        this->moduleSize = moduleSize;
        this->moduleData = new uint8_t[moduleSize];
        memcpy(this->moduleData, moduleData, moduleSize);

        // Load the copied data.
        LoadFromMemory();
    }

    const uint8_t *Module::GetModuleData() const
    {
        return moduleData;
    }

    void Module::LoadFromMemory()
    {
        // Create the module reader.
        ModuleReader reader(moduleSize, moduleData);

        // Read the header.
        ModuleHeader header;
        reader >> header;

        // Check the module signature.
        if(strncmp((char*)header.signature, ModuleSignature, 8))
            throw ModuleException("Invalid module signature.");

        // Check format version.
        if(header.formatVersionMajor != ModuleFormatVersionMajor &&
           header.formatVersionMinor >= ModuleFormatVersionMinor)
           throw ModuleException("Unsupported module format.");

        // TODO: Validate the module.

        // Read the module type.
        moduleType = (ModuleType)header.moduleType;
        
        // Read the string table.
        reader.Seek(header.stringTableOffset, SEEK_SET);
        for(size_t i = 0; i < header.stringTableEntries; i++)
        {
            std::string str;
            reader >> str;
            stringTable.push_back(str);
        }

        // Get the context
        llvm::LLVMContext &context = GetLlvmContext();

        // Load the module references.
        reader.Seek(header.moduleRefTableOffset, SEEK_SET);
        for(size_t i = 0; i < header.moduleRefTableEntries; i++)
        {
            // Read the reference.
            ModuleReference ref;
            reader >> ref;

            // Convert it into a module name.
            ModuleName refName;
            refName.name = GetString(ref.moduleName);
            refName.versionMajor = ref.versionMajor;
            refName.versionMinor = ref.versionMinor;
            refName.versionMicro = ref.versionMicro;

            // The first reference is the description of myself.
            if(i == 0)
            {
                // TODO: Perform cryptographic validation.
                name = refName;
                targetModule = new llvm::Module(refName.name, context);
                continue;
            }

            // Load the module reference.
            Module *referenced = virtualMachine->LoadModule(refName, false, false, this);
            moduleReferences.push_back(referenced);
        }

        // Unnamed module.
        if(!targetModule)
            targetModule = new llvm::Module("unnamed", context);

        // Read the library table
        reader.Seek(header.libTableOffset, SEEK_SET);
        for(size_t i = 0; i < header.libTableEntries; ++i)
        {
            uint32_t libName;
            reader >> libName;
            AddLibrary(GetString(libName));
        }

        // Preload the members.
        reader.Seek(header.memberTableOffset, SEEK_SET);
        size_t end = header.memberTableOffset + header.memberTableSize;
        MemberHeader memberHeader;
        while(reader.GetPosition() < end)
        {
            // Is the last member a reference?
            bool reference = false;

            // Read the member header.
            reader >> memberHeader;

            // Dump preload name.
            //printf("preload member %zu %s\n", memberTable.size(),
            // GetString(memberHeader.memberName).c_str());

            // Skip the attributes.
            SkipAttributes(this, reader, memberHeader.memberAttributes);

            // Preload the member according to his type.
            switch(memberHeader.memberType)
            {
            case MemberHeader::Namespace:
                memberTable.push_back(Namespace::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Structure:
                memberTable.push_back(Structure::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Class:
                memberTable.push_back(Class::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Interface:
                memberTable.push_back(Interface::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Function:
                memberTable.push_back(Function::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::FunctionGroup:
                memberTable.push_back(FunctionGroup::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Field:
                memberTable.push_back(Field::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Property:
                memberTable.push_back(Property::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::FunctionInstance:
                memberTable.push_back(FunctionInstance::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::TypeInstance:
                memberTable.push_back(TypeInstance::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::TypeGroup:
                memberTable.push_back(TypeGroup::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::MemberInstance:
                memberTable.push_back(MemberInstance::PreloadMember(this, reader, memberHeader));
                break;
            case MemberHeader::Reference:
                {
                    // Set the reference flag.
                    reference = true;

                    // Read the module id.
                    uint8_t moduleId;
                    reader >> moduleId;

                    // Get the referenced module.
                    Module *refMod = GetReferencedModule(moduleId);

                    // Find the member there.
                    //printf("Foreign member %zu = %s\n", memberTable.size(), GetString(memberHeader.memberName).c_str());
                    Member *member = refMod->GetMember(GetString(memberHeader.memberName));
                    memberTable.push_back(member);
                }
                break;
            default:
                // Unknown member.
                memberTable.push_back(NULL);
                reader.Skip(memberHeader.memberSize);
                break;
            }

            // Set the member id.
            Member *last = memberTable.back();
            if(last && !reference)
                last->SetMemberId(memberTable.size());
        }

        // Load the members parent-child relationships
        reader.Seek(header.memberTableOffset, SEEK_SET);
        for(size_t i = 0; i < memberTable.size(); ++i)
        {
            // Get the preloaded member.
            Member *preloadedMember = memberTable[i];

            // Read his header.
            reader >> memberHeader;

            // Read his structure.
            if(preloadedMember != NULL &&
               memberHeader.memberType != MemberHeader::Reference)
            {
                preloadedMember->ReadStructure(reader, memberHeader);
            }
            else
            {
                // Skip the attributes.
                SkipAttributes(this, reader, memberHeader.memberAttributes);

                // Skip the member data.
                reader.Skip(memberHeader.memberSize);
            }
        }

        // Load the anonymous types.
        reader.Seek(header.anonTypeTableOffset, SEEK_SET);
        while(reader.GetPosition() - header.anonTypeTableOffset < header.anonTypeTableSize)
        {
            AnonTypeHeader aheader;
            reader >> aheader;
            switch(aheader.typeKind)
            {
            case TK_BUILT:
                throw ModuleException("built-in types cannot be in type table.");
                break;
            case TK_STRUCT:
            case TK_CLASS:
            case TK_INTERFACE:
            case TK_INSTANCE:
                throw ModuleException("unexpected struct/class/iface as 'anonymous' type table.");
                break;
            case TK_FUNCTION:
                {
                    // Calculate the number of components.
                    int args = (aheader.recordSize-4)/4;
                    
                    // Create the type cache.
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    
                    // Read variable argument flag.
                    uint32_t flags;
                    reader >> flags;
                    cache.variableArg = flags & 1;
                    cache.extraData = flags;
                    
                    // Read the components.
                    for(int i = 0; i < args; i++)
                    {
                        uint32_t comp;
                        reader >> comp;
                        cache.components.push_back(comp);
                    }
                    
                    // Function type.
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_REFERENCE:
                {
                    // Read the target type.
                    uint32_t target;
                    uint8_t flow, streamRef;
                    reader >> target >> flow >> streamRef;

                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.components.push_back(target);
                    cache.extraData = flow;
                    cache.extraData2 = streamRef;
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_POINTER:
            case TK_CONSTANT:
                {
                    // Read the target type.
                    uint32_t target;
                    reader >> target;
                    
                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.components.push_back(target);
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_ARRAY:
                {
                    // Read the target type.
                    uint32_t target;
                    uint8_t dimensions, readOnly;
                    reader >> target >> dimensions >> readOnly;

                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.components.push_back(target);
                    cache.extraData = dimensions;
                    cache.extraData2 = readOnly;
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_VECTOR:
                {
                    // Read the target type and the number of components.
                    uint32_t target;
                    uint8_t numcomponents;
                    reader >> target >> numcomponents;
                    
                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.components.push_back(target);
                    cache.extraData = numcomponents;
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_MATRIX:
                {
                    // Read the target type and the number of components.
                    uint32_t target;
                    uint8_t numrows, numcolumns;
                    reader >> target >> numrows >> numcolumns;
                    
                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.components.push_back(target);
                    cache.extraData = numrows;
                    cache.extraData2 = numcolumns;
                    anonTypeTable.push_back(cache);
                }
                break;
            case TK_PLACEHOLDER:
                {
                    // Read the name, value type and numbases.
                    uint32_t name, id;
                    uint8_t valueType, numbases;
                    reader >> name >> id >> valueType >> numbases;

                    // Create the anon cache;
                    AnonTypeCache cache;
                    cache.kind = (TypeKind)aheader.typeKind;
                    cache.extraData = name;
                    cache.extraData2 = id;
                    cache.extraData3 = valueType;

                    // Read the bases.
                    for(int i = 0; i < numbases; ++i)
                    {
                        uint32_t base;
                        reader >> base;
                        cache.components.push_back(base);
                    }

                    anonTypeTable.push_back(cache);
                }
                break;
            default:
                reader.Skip(aheader.recordSize);
                break;
            }
        }

        // Load the types.
        reader.Seek(header.typeTableOffset, SEEK_SET);
        for(size_t i = 0; i < header.typeTableEntries; i++)
        {
            // Read the type reference.
            TypeReference ref;
            reader >> ref;
            
            // Create the type reference cache version.
            TypeReferenceCache cache;
            cache.typeName = ref.typeName;
            cache.kind = (TypeKind)ref.typeKind;
            cache.memberId = ref.memberId;
            
            // Store the cache.
            typeTable.push_back(cache);
        }

        // Type instances, classes, structures and interfaces must be read
        // before anything else.
        reader.Seek(header.memberTableOffset, SEEK_SET);
        for(size_t i = 0; i < memberTable.size(); ++i)
        {
            // Get the preloaded member.
            Member *preloadedMember = memberTable[i];

            // Read his header.
            reader >> memberHeader;

            // Read the member data or ignore it.
            uint32_t mtype = memberHeader.memberType;
            if(preloadedMember != NULL &&
               (mtype == MemberHeader::TypeInstance ||
                mtype == MemberHeader::Structure ||
                mtype == MemberHeader::Class ||
                mtype == MemberHeader::Interface))
                preloadedMember->Read(reader, memberHeader);
            else
            {
                // Skip the attributes.
                SkipAttributes(this, reader, memberHeader.memberAttributes);

                // Skip the member data.
                reader.Skip(memberHeader.memberSize);
            }
        }

        // Now read the other members.
        reader.Seek(header.memberTableOffset, SEEK_SET);
        for(size_t i = 0; i < memberTable.size(); ++i)
        {
            // Get the preloaded member.
            Member *preloadedMember = memberTable[i];

            // Read his header.
            reader >> memberHeader;

            // Read the member data or ignore it.
            uint32_t mtype = memberHeader.memberType;
            if(preloadedMember != NULL &&
                mtype != MemberHeader::Reference &&
                mtype != MemberHeader::TypeInstance &&
                mtype != MemberHeader::Structure &&
                mtype != MemberHeader::Class &&
                mtype != MemberHeader::Interface)
                preloadedMember->Read(reader, memberHeader);
            else
            {
                // Skip the attributes.
                SkipAttributes(this, reader, memberHeader.memberAttributes);

                // Skip the member data.
                reader.Skip(memberHeader.memberSize);
            }
        }

        // Read the debug information.
        if(header.debugInfoSize != 0 && debugging)
        {
            reader.Seek(header.debugInfoOffset, SEEK_SET);
            debugInfo = new DebugInformation(this);
            debugInfo->Read(reader, header.debugInfoSize);
        }

        // Read the resource data.
        if(header.resourceDataSize != 0)
        {
            reader.Seek(header.resourceDataOffset, SEEK_SET);
            uint32_t numresources;
            reader >> numresources;
            for(size_t i = 0; i < numresources; ++i)
            {
                uint32_t name, offset, length;
                reader >> name >> offset >> length;
                resources.push_back(ResourceData(GetString(name), offset, length));
            }
        }

        // Notify loaded module.
        virtualMachine->NotifyLoadedModule(this);
        DeclareRuntimeFunctions();
        
        // The first member must be the global namespace.
        if(memberTable.size() == 0)
            throw ModuleException("Empty module.");
            
        Member *global = memberTable[0];
        if(!global->IsNamespace())
            throw ModuleException("The first module member must be the global namespace.");
        globalNamespace = (Namespace*)global;
        
        // Register the member names.
        for(size_t i = 0; i < memberTable.size(); i++)
        {
            Member *member = memberTable[i];
            if(!member || member->IsAnonymous())
                continue;

            // Get the member full name.
            std::string fullname = member->GetFullName();

            // Ignore members without name.
            if(fullname.empty())
                continue;

            //printf("register member %zu'%s'\n", i, member->GetFullName().c_str());
            memberNameTable.insert(std::make_pair(fullname, member));
        }

        // Get the entry point.
        if(header.entryPoint != 0)
            entryPoint = GetFunction(header.entryPoint);

        // Declare the object.
        DeclarePass();
    }
    
    VirtualMachine *Module::GetVirtualMachine()
    {
        return virtualMachine;
    }

    const llvm::TargetData *Module::GetTargetData()
    {
        return virtualMachine->GetTargetData();
    }

    llvm::Module *Module::GetTargetModule()
    {
        return targetModule;
    }
    
    llvm::FunctionPassManager *Module::GetFunctionPassManager()
    {
        if(!functionPassManager)
        {
            functionPassManager = new llvm::FunctionPassManager(targetModule);

            // Setup it.
            llvm::FunctionPassManager *fpm = functionPassManager;
            fpm->add(new llvm::TargetData(*virtualMachine->GetExecutionEngine()->getTargetData()));
            fpm->add(llvm::createPromoteMemoryToRegisterPass());
            /*if(optimizationLevel != 0)
            {
                  fpm->add(llvm::createBasicAliasAnalysisPass());
                  fpm->add(llvm::createInstructionCombiningPass());
                  fpm->add(llvm::createReassociatePass());
                  fpm->add(llvm::createGVNPass());
                  fpm->add(llvm::createCFGSimplificationPass());
            }*/
            fpm->doInitialization();
        }
        return functionPassManager;
    }

    llvm::LLVMContext &Module::GetLlvmContext()
    {
        return virtualMachine->getContext();
    }

    llvm::IRBuilder<> &Module::GetIRBuilder()
    {
        return irBuilder;
    }

    inline
    llvm::Function *DeclareFunction(llvm::Module& module,
                                   llvm::Type* retType,
                                   const std::vector<llvm::Type*>& theArgTypes,
                                   const std::string& functName,
                                   llvm::GlobalValue::LinkageTypes linkage)
    {
        llvm::FunctionType* functType = llvm::FunctionType::get(retType,
                                                                theArgTypes,
                                                                false);
        return llvm::Function::Create(functType, linkage, functName, &module);
    }

    void Module::DeclareRuntimeFunctions()
    {
        // _Unwind_Resume
        std::vector<llvm::Type*> argTypes;
        llvm::Function* funct = NULL;

        llvm::Type *retType = llvm::Type::getInt32Ty(GetLlvmContext());
        argTypes.push_back(llvm::Type::getInt8PtrTy(GetLlvmContext()));
        funct = DeclareFunction(*targetModule, retType, argTypes, "_Unwind_Resume", llvm::Function::ExternalLinkage);
        funct->addFnAttr(llvm::Attribute::NoReturn);
    }
    
    void Module::Compile()
    {
        // Compile only once.
        if(compiled)
            return;
        compiled = true;

        // Compile the referenced modules.
        //for(size_t i = 0; i < moduleReferences.size(); ++i)
        //    moduleReferences[i]->Compile();

        // Define the objects.
        DefinitionPass();

        // Verify the module.
        llvm::verifyModule(*targetModule);
    }
    
    llvm::Constant *Module::CompileString(uint32_t id)
    {
        return CompileString(GetString(id));
    }

    void Module::PrepareJit()
    {
        if(!defined)
            Compile();

        Member *member = GetMember("__chela_eh_personality__");
        if(!member)
            return;
        if(!member->IsFunction() || !member->IsStatic())
            throw ModuleException("__chela_eh_personality__ must be a static function");

        // Prepare the personality function.
        Function *personality = static_cast<Function*> (member);
        personality->DeclarePass();
        personality->DefinitionPass();
        personality->BuildDependencies();

        // Get a jit-ed version.
        llvm::ExecutionEngine *ee = virtualMachine->GetExecutionEngine();
        ee->getPointerToFunction(personality->GetTarget());
    }

    llvm::Constant *Module::CompileString(const std::string &str)
    {
        // Get the string class.
        VirtualMachine *vm = GetVirtualMachine();
        Class *stringClass = vm->GetStringClass();
        llvm::StructType *stringType = stringClass->GetStructType();

        // Declare the string.
        stringClass->DeclarePass();
                
        // Create the string value.
        ConstantStructurePtr stringValue(stringClass->CreateConstant(this));

        // Encode utf-16 string.
        std::vector<uint16_t> utf16;
        for(size_t i = 0; i < str.size(); ++i)
        {
            // Count the number of characters.
            int c = str[i];
            int numbytes = 0;
            for(int bc = 7; bc > 0; --bc)
            {
                if(c & 1<<bc)
                    ++numbytes;
                else
                    break;
            }

            // Read the first byte.
            uint32_t character = c & ((1 << (8 - numbytes)) - 1);

            // Decode the remaining bytes.
            if(numbytes > 0)
            {
                character <<= numbytes + 1;
    
                // Read the next bytes.
                for(int j = 1; j < numbytes; ++j)
                {
                    character |= str[i + j] & ((1 << 6) - 1);
                    if(j + 1 < numbytes)
                        character <<= 6;
                }
            }

            // Encode the character.
            if(character <= 0xFFFF)
            {
                utf16.push_back(character);
            }
            else
            {
                character -= 0x10000;
                utf16.push_back((character >> 10) + 0xD800);
                utf16.push_back((character & 0x3FF) + 0xDC00);
            }

            // Increase by the excess.
            if(numbytes > 0)
                i += numbytes - 1;
        }

        // Append a null terminator.
        utf16.push_back(0);

        // Create the string constant.
        llvm::Constant *stringConstant = llvm::ConstantDataArray::get(GetLlvmContext(), utf16);

        // Set the data
        stringConstant = new llvm::GlobalVariable(*targetModule, stringConstant->getType(),
            true, llvm::GlobalValue::PrivateLinkage, stringConstant, "_chstringData");
        stringValue->SetField("data", stringConstant);

        // Set the size field.
        llvm::Type *intType = llvm::Type::getInt32Ty(GetLlvmContext());
        stringValue->SetField("size", llvm::ConstantInt::get(intType, utf16.size()-1));

        // Create a global variable
        return new llvm::GlobalVariable(*targetModule, stringType, false,
            llvm::GlobalValue::PrivateLinkage, stringValue->Finish(), "_chstring");
    }

    llvm::Constant *Module::CompileCString(uint32_t id)
    {
        return CompileCString(GetString(id));
    }

    llvm::Constant *Module::CompileCString(const std::string &str)
    {
        // Get the context.
        VirtualMachine *vm = GetVirtualMachine();
        llvm::LLVMContext &ctx = GetLlvmContext();

        // Create the string constant.
        llvm::Constant *stringConstant = llvm::ConstantDataArray::getString(ctx, str);
        llvm::Constant *storage = new llvm::GlobalVariable(*targetModule, stringConstant->getType(), true, llvm::GlobalValue::PrivateLinkage, stringConstant, "_cstring");
        return llvm::ConstantExpr::getPointerCast(storage, ChelaType::GetCStringType(vm)->GetTargetType());
    }

    llvm::Constant *Module::ImportGlobal(llvm::GlobalVariable *original)
    {
        if(original->getParent() == targetModule)
            return original;

        // Find the old global.
        return targetModule->getOrInsertGlobal(original->getName(), original->getType()->getElementType());
        /*llvm::Constant *oldGlobal = targetModule->getGlobalVariable(original->getName(), false);
        if(oldGlobal)
            return oldGlobal;

        // Import the variable.
        llvm::GlobalValue::LinkageTypes linkage = original->getLinkage();
        if(GetVirtualMachine()->IsWindows() && original->hasExternalLinkage() && !original->isConstant())
            linkage = llvm::GlobalVariable::DLLImportLinkage;
        return new llvm::GlobalVariable(*targetModule, original->getType()->getElementType(),
            original->isConstant(), linkage, NULL, original->getName());*/
    }

    llvm::Constant *Module::ImportFunction(llvm::Function *original)
    {
        if(original->getParent() == targetModule)
            return original;
        return targetModule->getOrInsertFunction(original->getName(),
                       original->getFunctionType(), original->getAttributes());
    }

    llvm::StructType *Module::GetInterfaceImplStruct()
    {
        if(!interfaceImplStruct)
            BuildTypeInformationStructs();
        return interfaceImplStruct;
    }

    llvm::GlobalVariable *Module::GetAssemblyVariable()
    {
        if(!assemblyVariable)
        {
            Class *assemblyClass = virtualMachine->GetAssemblyClass();
            assemblyVariable =
                new llvm::GlobalVariable(*targetModule, assemblyClass->GetTargetType(),
                            false, llvm::GlobalVariable::ExternalLinkage, NULL, "_Chela_AssemblyData_");
            assemblyVariable->setVisibility(llvm::GlobalValue::ProtectedVisibility);
        }
        return assemblyVariable;
    }

    llvm::Constant *Module::GetArrayTypeInfo(const ChelaType *type)
    {
        // Find an existing definition.
        std::string arrayName = "_C" + type->GetMangledName() + "_typeinfo";
        llvm::Constant *definition = targetModule->getNamedGlobal(arrayName);
        if(definition != NULL)
            return definition;

        // Create the definition variable.
        VirtualMachine *vm = GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage, NULL,
                                arrayName);

        // Create new definition initializer.
        Class *arrayClass = virtualMachine->GetArrayClass();
        ConstantStructurePtr typeValue = arrayClass->GetTypeInfoData(this, typeInfo, arrayName);

        // Cast the array type.
        const ArrayType *arrayType = static_cast<const ArrayType*> (type);
        const ChelaType *elementType = arrayType->GetValueType();

        // Set the type kind.
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        typeValue->SetField("kind", llvm::ConstantInt::get(intType, TIK_Array));

        if(!arrayType->IsGenericType())
        {
            llvm::StructType *arrayStructType = llvm::cast<llvm::StructType> (arrayType->GetTargetType());

            // Set the dimensions.
            typeValue->SetField("dimensions", llvm::ConstantInt::get(intType, arrayType->GetDimensions()));

            // Set the size.
            typeValue->SetField("size", llvm::ConstantExpr::getSizeOf(arrayStructType));

            // Set the align.
            typeValue->SetField("align", llvm::ConstantExpr::getAlignOf(arrayStructType));

            // Set the size offset
            typeValue->SetField("sizeOffset", llvm::ConstantExpr::getOffsetOf(arrayStructType, 1));

            // Set the data offset
            typeValue->SetField("dataOffset", llvm::ConstantExpr::getOffsetOf(arrayStructType, 2));
        }

        // Create the sub-types array type.
        llvm::Type *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());
        llvm::ArrayType *subArrayType = llvm::ArrayType::get(int8PtrTy, 1);

        // Get and cast the element type.
        llvm::Constant *elementTypeInfo = GetTypeInfo(elementType);
        elementTypeInfo = llvm::ConstantExpr::getPointerCast(elementTypeInfo, int8PtrTy);

        // Create the sub-types array.
        llvm::Constant *arrayConstant = llvm::ConstantArray::get(subArrayType, llvm::makeArrayRef(&elementTypeInfo, 1));
        llvm::GlobalVariable *subTypeArray =
            new llvm::GlobalVariable(*targetModule, subArrayType,
                                true, llvm::GlobalValue::LinkOnceODRLinkage, arrayConstant,
                                arrayName + "_subtypes_");

        // Set the sub types.
        typeValue->SetField("numsubtypes", llvm::ConstantInt::get(intType, 1));
        typeValue->SetField("subtypes", subTypeArray);

        // Set the type info value.
        typeInfo->setInitializer(typeValue->Finish());

        return typeInfo;
    }

    llvm::Constant *Module::GetInstanceTypeInfo(const TypeInstance *type)
    {
        // Find an existing definition.
        std::string instanceName = type->GetMangledName() + "_typeinfo";
        llvm::Constant *definition = targetModule->getNamedGlobal(instanceName);
        if(definition != NULL)
            return definition;

        // Create new definition initializer.
        Class *typeClass = virtualMachine->GetTypeClass();
        ConstantStructurePtr typeValue(typeClass->CreateConstant(this));

        // TODO: Set type instance values.

        // Create the definition variable.
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage,
                                typeValue->Finish(), instanceName);
        return typeInfo;
    }

    llvm::Constant *Module::GetPlaceHolderTypeInfo(const ChelaType *type)
    {
        // Make sure its a placeholder.
        if(!type->IsPlaceHolder())
            throw ModuleException("expected placeholder type.");

        // Find an existing definition.
        std::string placeName = type->GetMangledName() + "_typeinfo";
        llvm::Constant *definition = targetModule->getNamedGlobal(placeName);
        if(definition != NULL)
            return definition;

        // Create new definition initializer.
        VirtualMachine *vm = GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        ConstantStructurePtr typeValue(typeClass->CreateConstant(this));

        // Set the type kind.
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        typeValue->SetField("kind", llvm::ConstantInt::get(intType, TIK_Placeholder));

        // Create the definition variable.
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage,
                                typeValue->Finish(), placeName);
        return typeInfo;
    }

    llvm::Constant *Module::GetPointerTypeInfo(const ChelaType *type)
    {
        // Remove constant references.
        if(type->IsConstant())
            type = DeConstType(type);

        // Build the reference type name.
        std::string pointerName = "_CP";
        pointerName += type->GetMangledName();
        pointerName += "_typeinfo";

        // Find an existing definition.
        llvm::Constant *definition = targetModule->getNamedGlobal(pointerName);
        if(definition != NULL)
            return definition;

        // Find the associated type info.
        llvm::Constant *valueTypeInfo = GetTypeInfo(type);

        // Create new definition initializer.
        VirtualMachine *vm = GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        ConstantStructurePtr typeValue(typeClass->CreateConstant(this));

        // Set the pointer type kind.
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        typeValue->SetField("kind", llvm::ConstantInt::get(intType, TIK_Pointer));

        // Use the pointed type as a base.
        typeValue->SetField("baseType", valueTypeInfo);

        // Create the definition variable.
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage,
                                typeValue->Finish(), pointerName);
        return typeInfo;
    }

    llvm::Constant *Module::GetReferenceTypeInfo(const ChelaType *type)
    {
        // Remove constant references.
        if(type->IsConstant())
            type = DeConstType(type);

        // Build the reference type name.
        std::string referenceName = "_C";
        referenceName += type->GetMangledName();
        referenceName += "_typeinfo";

        // Find an existing definition.
        llvm::Constant *definition = targetModule->getNamedGlobal(referenceName);
        if(definition != NULL)
            return definition;

        // Find the associated type info.
        llvm::Constant *valueTypeInfo = GetTypeInfo(type);

        // Create new definition initializer.
        VirtualMachine *vm = GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        ConstantStructurePtr typeValue(typeClass->CreateConstant(this));

        // Set the reference type kind.
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        typeValue->SetField("kind", llvm::ConstantInt::get(intType, TIK_Reference));

        // Use the referenced type as a base.
        typeValue->SetField("baseType", valueTypeInfo);

        // Create the definition variable.
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage,
                                typeValue->Finish(), referenceName);
        return typeInfo;
    }

    llvm::Constant *Module::GetFunctionTypeInfo(const ChelaType *type)
    {
        // Build the reference type name.
        std::string referenceName = "_C";
        referenceName += type->GetMangledName();
        referenceName += "_typeinfo";

        // Find an existing definition.
        llvm::Constant *definition = targetModule->getNamedGlobal(referenceName);
        if(definition != NULL)
            return definition;

        // Create new definition initializer.
        VirtualMachine *vm = GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        ConstantStructurePtr typeValue(typeClass->CreateConstant(this));

        // Set the reference type kind.
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        typeValue->SetField("kind", llvm::ConstantInt::get(intType, TIK_Function));

        // TODO: Store the return value, and the parameters in the composite type.

        // Create the definition variable.
        llvm::GlobalVariable *typeInfo =
            new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                                false, llvm::GlobalValue::LinkOnceODRLinkage,
                                typeValue->Finish(), referenceName);
        return typeInfo;
    }

    llvm::Constant *Module::GetTypeInfo(const ChelaType *type)
    {
        // The type cannot be null.
        if(type == NULL)
            throw ModuleException("expected a non NULL type.");

        // Get the plain type.
        if(type->IsConstant())
            type = DeConstType(type);

        // Remove reference.
        if(type->IsReference())
        {
            type = DeReferenceType(type);

            // Remove constant.
            if(type->IsConstant())
                type = DeConstType(type);

            // Check for ByRef types
            if(type->IsReference())
                return GetReferenceTypeInfo(DeReferenceType(type));
            else if(!type->IsPassedByReference())
                return GetReferenceTypeInfo(type);
        }

        // Handle array types.
        if(type->IsArray())
            return GetArrayTypeInfo(type);

        // Handle place holders.
        if(type->IsPlaceHolder())
            return GetPlaceHolderTypeInfo(type);

        // Handle pointers.
        if(type->IsPointer())
            return GetPointerTypeInfo(DePointerType(type));

        // Handle function types.
        if(type->IsFunction())
            return GetFunctionTypeInfo(type);

        // Handle type instances.
        if(type->IsTypeInstance())
        {
            const TypeInstance *instance = static_cast<const TypeInstance*> (type);
            if(instance->IsGenericType())
                return GetInstanceTypeInfo(instance);
            else
                return GetTypeInfo(instance->GetImplementation());
        }

        // Use the associated type.
        if(!type->IsStructure() && !type->IsClass() && !type->IsInterface())
        {
            Structure *assoc = virtualMachine->GetAssociatedClass(type);
            if(assoc == NULL)
                throw ModuleException("couldn't get associated type for " + type->GetFullName());
            type = assoc;
        }

        // Cast the type and use his type info.
        const Structure *building = static_cast<const Structure*> (type);
        return building->GetTypeInfo(this);
    }

    DebugInformation *Module::GetDebugInformation()
    {
        return debugInfo;
    }

    void Module::BuildTypeInformationStructs()
    {
        if(interfaceImplStruct)
            return;

        // Get the type info type.
        llvm::PointerType *typeInfoPtr = llvm::PointerType::getUnqual(virtualMachine->GetTypeClass()->GetTargetType());

        // Create interface structure
        llvm::Type *vtablePtr = llvm::Type::getInt8PtrTy(GetLlvmContext());
        interfaceImplStruct = llvm::StructType::create("__ifaceImpl__",
            typeInfoPtr,
            vtablePtr,
            ChelaType::GetUIntType(GetVirtualMachine())->GetTargetType(),
            NULL);

    }

    void Module::DeclarePass()
    {
        // Declare only once.
        if(declared)
            return;
        declared = true;

        // Notify the virtual machine
        virtualMachine->LoadRuntimeClasses();
        
        // Perform member declarations, first in types.
        for(size_t i = 0; i < memberTable.size(); i++)
        {
            Member *member = memberTable[i];
            if(member && member->IsType() && member->GetModule() == this)
                member->DeclarePass();
        }

        // Now in non-types
        for(size_t i = 0; i < memberTable.size(); i++)
        {
            Member *member = memberTable[i];
            if(member && !member->IsType() && member->GetModule() == this)
                member->DeclarePass();
        }
    }
    
    void Module::DefinitionPass()
    {
        // Define only once.
        if(defined)
            return;
        defined = true;

        // Perform member definitions.
        for(size_t i = 0; i < memberTable.size(); ++i)
        {
            Member *member = memberTable[i];
            if(member && member->GetModule() == this)
                member->DefinitionPass();
        }

        // Define structure implementations.
        definingGenericStructures = true;
        GenericStructureImpls::iterator sit = genericStructureImpls.begin();
        for(; sit != genericStructureImpls.end(); ++sit)
        {
            Structure *genericStruct = sit->second;
            if(genericStruct)
                genericStruct->DefinitionPass();
        }

        // Define function implementations.
        definingGenericFunctions = true;
        GenericFunctionImpls::iterator fit = genericFunctionImpls.begin();
        for(; fit != genericFunctionImpls.end(); ++fit)
        {
            Function *genericFunction = fit->second;
            if(genericFunction)
                genericFunction->DefinitionPass();
        }

        // Create the static construction list.
        CreateStaticConstructionList();

        // Create the assembly data.
        CreateAssemblyData();

        // Create the entry point.
        CreateEntryPoint();

        // Finish the debug information.
        if(debugInfo)
            debugInfo->FinishDebug();
    }
    
    void Module::Dump()
    {
        targetModule->dump();
    }

    void Module::RegisterStaticConstructor(Function *constructor)
    {
        staticConstructors.push_back(constructor);
    }

    void Module::RegisterAttributeConstant(AttributeConstant *attribute)
    {
        attributes.push_back(attribute);
    }

    void Module::RegisterTypeInfo(const std::string &name, llvm::GlobalVariable *typeInfo)
    {
        assemblyTypes.push_back(std::make_pair(name, typeInfo));
    }

    void Module::SetRuntimeFlag(bool flag)
    {
        runtimeFlag = flag;
    }

    bool Module::IsRuntime() const
    {
        return runtimeFlag;
    }

    bool Module::HasReflection() const
    {
        return !noreflection;
    }

    Field *Module::FindGenericImplementation(Field *declaration, const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Field> name(declaration, instance);

        // Find the implementation.
        GenericFieldImpls::iterator it = genericFieldImpls.find(name);
        if(it != genericFieldImpls.end())
            return it->second;

        // Not found.
        return NULL;
    }

    Function *Module::FindGenericImplementation(Function *declaration, const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Function> name(declaration, instance);

        // Find the implementation.
        GenericFunctionImpls::iterator it = genericFunctionImpls.find(name);
        if(it != genericFunctionImpls.end())
            return it->second;

        // Not found.
        return NULL;
    }

    Structure *Module::FindGenericImplementation(Structure *declaration,
                                                 const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Structure> name(declaration, instance);

        // Find the implementation.
        GenericStructureImpls::iterator it = genericStructureImpls.find(name);
        if(it != genericStructureImpls.end())
            return it->second;

        // Not found.
        return NULL;
    }

    void Module::RegisterGenericImplementation(Field *declaration, Field *implementation,
                                                    const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Field> name(declaration, instance);

        // Register the generic.
        genericFieldImpls.insert(std::make_pair(name, implementation));
    }

    void Module::RegisterGenericImplementation(Function *declaration, Function *implementation,
                                                    const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Function> name(declaration, instance);

        // Register the generic.
        genericFunctionImpls.insert(std::make_pair(name, implementation));

        // Define the function.
        if(definingGenericFunctions)
        {
            implementation->DeclarePass();
            implementation->DefinitionPass();
        }
    }

    void Module::RegisterGenericImplementation(Structure *declaration, Structure *implementation,
                                                     const GenericInstance *instance)
    {
        // Create the generic name.
        GenericImplName<Structure> name(declaration, instance);

        // Register the generic.
        genericStructureImpls.insert(std::make_pair(name, implementation));

        // Define the structure.
        if(definingGenericStructures)
        {
            implementation->DeclarePass();
            implementation->DefinitionPass();
        }
    }

    void Module::CreateStaticConstructionList()
    {
        // Don't create a list if not necessary.
        if(staticConstructors.empty() && attributes.empty() && !IsRuntime())
            return;

        // Create the function type.
        llvm::LLVMContext &context = targetModule->getContext();
        llvm::Type *i32Ty = llvm::Type::getInt32Ty(context);
        llvm::Type *voidTy = llvm::Type::getVoidTy(context);
        llvm::FunctionType *ctorType = llvm::FunctionType::get(voidTy, false);
        llvm::PointerType *ctorPtrType = llvm::PointerType::getUnqual(ctorType);

        // Create priority-constructor structure type.
        llvm::StructType *pairType = llvm::StructType::get(i32Ty, ctorPtrType, NULL);

        // Create the constructors list.
        std::vector<llvm::Constant*> ctorList;
        std::vector<llvm::Constant*> pairMembers;

        // Find the init memory member.
        Function *initMemory = virtualMachine->GetRuntimeFunction("__chela_init_memmgr__", true);

        // Add it to the constructor list.
        pairMembers.push_back(llvm::ConstantInt::get(i32Ty, 1));
        pairMembers.push_back(initMemory->ImportFunction(this));
        ctorList.push_back(
            llvm::ConstantStruct::get(pairType, pairMembers));

        // Give first priority to the attributes.
        if(!attributes.empty())
        {
            pairMembers.clear();
            pairMembers.push_back(llvm::ConstantInt::get(i32Ty, 10));
            pairMembers.push_back(CreateAttributeConstructor());
            ctorList.push_back(
                llvm::ConstantStruct::get(pairType, pairMembers));
        }

        // TODO: Sort the static constructor, giving priority.

        // Create a pair for each constructor.
        pairMembers.clear();
        pairMembers.push_back(llvm::ConstantInt::get(i32Ty, 20));
        pairMembers.push_back(NULL);
        for(size_t i = 0; i < staticConstructors.size(); ++i)
        {
            Function *ctor = staticConstructors[i];
            pairMembers[1] = ctor->GetTarget();
            ctorList.push_back(
                llvm::ConstantStruct::get(pairType, pairMembers));
        }

        // Create the constructor array type.
        llvm::ArrayType *ctorArrayType = llvm::ArrayType::get(pairType, ctorList.size());

        // Create the constructor array.
        llvm::Constant *ctorArray = llvm::ConstantArray::get(ctorArrayType, ctorList);

        // Use the global intrinsic llvm.global_ctors
        new llvm::GlobalVariable(*targetModule, ctorArrayType,
            true, llvm::GlobalVariable::AppendingLinkage,
            ctorArray, "llvm.global_ctors");

        // Add the global destructors.
        if(IsRuntime())
        {
            //TODO: shutdown the memory manager.
        }
    }

    llvm::Function *Module::CreateAttributeConstructor()
    {
        // Create the function type.
        llvm::LLVMContext &context = targetModule->getContext();
        llvm::Type *voidTy = llvm::Type::getVoidTy(context);
        llvm::FunctionType *ctorType = llvm::FunctionType::get(voidTy, false);

        // Create the constructor function
        llvm::Function *ctor =
            llvm::Function::Create(ctorType, llvm::Function::PrivateLinkage,
                "_pattr_ctor", targetModule);

        // Create the basic block and the ir builder.
        llvm::BasicBlock *bb = llvm::BasicBlock::Create(context, "top", ctor);
        llvm::IRBuilder<> builder(bb);

        // Construct the attributes.
        for(size_t i = 0; i < attributes.size(); ++i)
            attributes[i]->Construct(builder);

        // Finish the constructor.
        builder.CreateRetVoid();

        // Return the constructor.
        return ctor;
    }

    struct TypeInfoComparer
    {
        typedef std::pair<std::string, llvm::Constant*> pair;
        bool operator()(const pair &a, const pair &b)
        {
            return a.first < b.first;
        }
    };

    llvm::Constant *Module::EmbedModule()
    {
        // Only embed once.
        if(moduleDataStartPointer)
            return moduleDataStartPointer;

        // Create a constant with the module content.
        llvm::Constant *moduleDataConstant =
            llvm::ConstantDataArray::get(GetLlvmContext(), llvm::makeArrayRef(moduleData, moduleSize));

        // Create the module start variable.
        llvm::GlobalVariable *moduleStartGlobal =
            new llvm::GlobalVariable(*targetModule, moduleDataConstant->getType(), true,
                    llvm::GlobalVariable::ExternalLinkage, moduleDataConstant, "_ChelaModule_Start");
        moduleStartGlobal->setVisibility(llvm::GlobalVariable::HiddenVisibility);
        moduleStartGlobal->setSection(".cbm");

        // Cast the module begining.
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());
        moduleDataStartPointer = llvm::ConstantExpr::getPointerCast(moduleStartGlobal, int8PtrTy);

        // Return the pointer.
        return moduleDataStartPointer;
    }

    void Module::CreateAssemblyReflectionData(ConstantStructurePtr &value)
    {
        // Common types.
        llvm::Type *intType = llvm::Type::getInt32Ty(targetModule->getContext());
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());

        // Sort the type infos by name.
        std::sort(assemblyTypes.begin(), assemblyTypes.end(), TypeInfoComparer());

        // Cast the type infos to i8*
        std::vector<llvm::Constant*> typeInfoConstants;
        typeInfoConstants.reserve(assemblyTypes.size());
        for(size_t i = 0; i < assemblyTypes.size(); ++i)
            typeInfoConstants.push_back(llvm::ConstantExpr::getPointerCast(assemblyTypes[i].second, int8PtrTy));

        // Create the type info array.
        llvm::ArrayType *typeInfoArrayTy = llvm::ArrayType::get(int8PtrTy, typeInfoConstants.size());
        llvm::Constant *typeInfoArray = llvm::ConstantArray::get(typeInfoArrayTy, typeInfoConstants);
        llvm::GlobalVariable *typeInfos =
            new llvm::GlobalVariable(*targetModule, typeInfoArrayTy, true,
                            llvm::GlobalVariable::PrivateLinkage, typeInfoArray, "_Chela_Assembly_TypeInfos_");

        // Set the number of type infos.
        value->SetField("numtypes", llvm::ConstantInt::get(intType, assemblyTypes.size()));

        // Set the type infos.
        value->SetField("types", typeInfos);

        // Get the member informations.
        llvm::Constant *nullPtr = llvm::Constant::getNullValue(int8PtrTy);
        std::vector<llvm::Constant*> tableData;
        for(size_t i = 0; i < memberTable.size(); ++i)
        {
            Member *member = memberTable[i];
            llvm::Constant *memberInfo = nullPtr;
            if(member && (member->GetModule() == this || member->IsKernel()))
            {
                memberInfo = member->GetMemberInfo(this);
                if(memberInfo)
                    memberInfo = llvm::ConstantExpr::getPointerCast(memberInfo, int8PtrTy);
                else
                    memberInfo = nullPtr;
            }
            tableData.push_back(memberInfo);
        }

        // Create the member table.
        llvm::ArrayType *memberTableTy = llvm::ArrayType::get(int8PtrTy, tableData.size());
        llvm::GlobalVariable *memberTableGlobal =
            new llvm::GlobalVariable(*targetModule, memberTableTy, true,
                    llvm::GlobalVariable::PrivateLinkage,
                    llvm::ConstantArray::get(memberTableTy, tableData),
                    "_memberTable_");

        // Store the member table in the assembly.
        value->SetField("moduleMemberTableSize", llvm::ConstantInt::get(intType, tableData.size()));
        value->SetField("moduleMemberTable", memberTableGlobal);

        // Get the type table data.
        tableData.clear();
        for(size_t i = 0; i < typeTable.size(); ++i)
        {
            const ChelaType *type = GetType(i + 0x100);
            llvm::Constant *typeInfo = nullPtr;
            if(type)
            {
                typeInfo = GetTypeInfo(type);
                if(typeInfo)
                    typeInfo = llvm::ConstantExpr::getPointerCast(typeInfo, int8PtrTy);
                else
                    typeInfo = nullPtr;
            }
            tableData.push_back(typeInfo);
        }

        // Create the member table.
        llvm::ArrayType *typeTableTy = llvm::ArrayType::get(int8PtrTy, tableData.size());
        llvm::GlobalVariable *typeTableGlobal =
            new llvm::GlobalVariable(*targetModule, typeTableTy, true,
                    llvm::GlobalVariable::PrivateLinkage,
                    llvm::ConstantArray::get(typeTableTy, tableData),
                    "_typeTable_");

        // Store the type table in the assembly.
        value->SetField("moduleTypeTableSize", llvm::ConstantInt::get(intType, tableData.size()));
        value->SetField("moduleTypeTable", typeTableGlobal);

        // Get the string table data.
        tableData.clear();
        for(size_t i = 0; i < stringTable.size(); ++i)
            tableData.push_back(llvm::ConstantExpr::getPointerCast(CompileString(i+1), int8PtrTy));

        // Create the string table.
        /*llvm::ArrayType *stringTableTy = llvm::ArrayType::get(int8PtrTy, tableData.size());
        llvm::GlobalVariable *stringTableGlobal =

            new llvm::GlobalVariable(*targetModule, stringTableTy, true,
                    llvm::GlobalVariable::PrivateLinkage,
                    llvm::ConstantArray::get(stringTableTy, tableData),
                    "_stringTable_");

        // Store the string table in the assembly.

        value = SetStructField(assemblyClass, value, "moduleStringTableSize", llvm::ConstantInt::get(intType, tableData.size()));
        value = SetStructField(assemblyClass, value, "moduleStringTable", stringTableGlobal);*/
    }

    void Module::CreateAssemblyResourceData(ConstantStructurePtr &value)
    {
        // Common types.
        llvm::Type *intType = llvm::Type::getInt32Ty(targetModule->getContext());
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());

        // The beginning of the embedded module.
        llvm::Constant *moduleStart = moduleDataStartPointer;

        // Create the resource data.
        llvm::StructType *resourceDataTy =
            llvm::StructType::get(int8PtrTy, int8PtrTy, intType, NULL);
        std::vector<llvm::Constant*> resourceData;
        resourceData.reserve(resources.size());

        std::vector<llvm::Constant*> dataElements;
        dataElements.resize(3);
        for(size_t i = 0; i < resources.size(); ++i)
        {
            const ResourceData &data = resources[i];

            // Use the resource from the embedded module or embedd the resource.
            llvm::Constant *resourceBegin = NULL;
            if(moduleStart)
            {
                // Get the resource pointer.
                llvm::Constant *resourceOffset = llvm::ConstantInt::get(intType, data.offset);
                resourceBegin =
                    llvm::ConstantExpr::getGetElementPtr(moduleStart, llvm::makeArrayRef(&resourceOffset, 1));
            }
            else
            {
                // Create a constant with the resource content.
                llvm::Constant *resourceDataConstant =
                    llvm::ConstantDataArray::get(GetLlvmContext(), llvm::makeArrayRef(moduleData + data.offset, data.length));

                // Create the resource start variable.
                llvm::GlobalVariable *resourceDataGlobal =
                    new llvm::GlobalVariable(*targetModule, resourceDataConstant->getType(), true,
                        llvm::GlobalVariable::PrivateLinkage, resourceDataConstant, "_resource_");
                resourceBegin = llvm::ConstantExpr::getPointerCast(resourceDataGlobal, int8PtrTy);

            }

            // Store the resource meta data.
            dataElements[0] = llvm::ConstantExpr::getPointerCast(CompileString(data.name), int8PtrTy);
            dataElements[1] = resourceBegin;
            dataElements[2] = llvm::ConstantInt::get(intType, data.length);
            resourceData.push_back(llvm::ConstantStruct::get(resourceDataTy, dataElements));
        }

        // Create the resource private global.
        llvm::ArrayType *resourcesTy = llvm::ArrayType::get(resourceDataTy, resources.size());
        llvm::Constant *resourcesConstant = llvm::ConstantArray::get(resourcesTy, resourceData);
        llvm::GlobalVariable *resourcesGlobal =
            new llvm::GlobalVariable(*targetModule, resourcesTy, true,
                    llvm::GlobalVariable::PrivateLinkage, resourcesConstant, "_resources_");

        // Store the resources in the assembly.
        value->SetField("resources", resourcesGlobal);

        // Store the number of resources.
        value->SetField("numresources", llvm::ConstantInt::get(intType, resources.size()));

    }

    void Module::CreateAssemblyData()
    {
        // Get the assembly class.
        Class *assemblyClass = virtualMachine->GetAssemblyClass();

        // Create the assembly value
        ConstantStructurePtr value(assemblyClass->CreateConstant(this));

        // Always embed the module in libraries.
        if(HasReflection() || IsSharedLibrary() || IsStaticLibrary())
        {
            // Embed the module data.
            llvm::Constant *moduleStart = EmbedModule();
            value->SetField("moduleStart", moduleStart);

            // Create the module end variable.
            llvm::Type *intType = llvm::Type::getInt32Ty(targetModule->getContext());
            llvm::Constant *moduleSizeConstant = llvm::ConstantInt::get(intType, moduleSize);
            llvm::Constant *moduleEnd =
                llvm::ConstantExpr::getGetElementPtr(moduleStart, llvm::makeArrayRef(&moduleSizeConstant, 1));
            value->SetField("moduleEnd", moduleEnd);
        }

        // Create the reflection data.
        if(HasReflection())
            CreateAssemblyReflectionData(value);

        // Embed the resources.
        CreateAssemblyResourceData(value);


        // Set the assembly value.
        GetAssemblyVariable()->setInitializer(value->Finish());
    }

    void Module::CreateEntryPoint()
    {
        VirtualMachine *vm = GetVirtualMachine();
        llvm::LLVMContext &ctx = GetLlvmContext();

        if(!entryPoint)
            return;

        // The entry point must be static.
        if(!entryPoint->IsStatic())
            throw ModuleException("Invalid entry point.");

        // Get the entry point type.
        const FunctionType *entryType = entryPoint->GetFunctionType();

        // Only one argument is supported.
        if(entryType->GetArgumentCount() > 1)
            throw ModuleException("Invalid entry point.");

        // Check the return type.
        const ChelaType *retType = entryType->GetReturnType();
        if(retType != ChelaType::GetVoidType(vm) &&
           retType != ChelaType::GetIntType(vm))
           throw ModuleException("Invalid entry point return type.");

        // Check the argument.
        if(entryType->GetArgumentCount() == 1)
        {
            const ChelaType *argType = entryType->GetArgument(0);
            if(argType != ReferenceType::Create(ArrayType::Create(vm->GetStringClass(), 1, false)))
                throw ModuleException("Invalid entry point argument.");
        }

        // Get the entry point caller.
        bool args = entryType->GetArgumentCount();
        bool voidRet = retType == ChelaType::GetVoidType(vm);
        Function *caller = NULL;
        if(voidRet && args)
            caller = virtualMachine->GetRuntimeFunction("__chela_main_vs__", true);
        else if(voidRet && !args)
            caller = virtualMachine->GetRuntimeFunction("__chela_main_vv__", true);
        else if(!voidRet && args)
            caller = virtualMachine->GetRuntimeFunction("__chela_main_is__", true);
        else //if(!voidRet && !args)
            caller = virtualMachine->GetRuntimeFunction("__chela_main_iv__", true);
        const FunctionType *callerType = caller->GetFunctionType();


        // Create the main function type.
        std::vector<llvm::Type*> types;
        types.push_back(llvm::Type::getInt32Ty(ctx));
        types.push_back(llvm::PointerType::getUnqual(llvm::Type::getInt8PtrTy(ctx)));
        llvm::FunctionType *functionType =
            llvm::FunctionType::get(llvm::Type::getInt32Ty(ctx), types, false);

        // Create the main function.
        llvm::Function *mainFunction =
            llvm::Function::Create(functionType, llvm::Function::ExternalLinkage,
                "main", targetModule);

        // Create the main function block.
        llvm::BasicBlock *bb = llvm::BasicBlock::Create(ctx, "bb", mainFunction);

        // Create the IR builder.
        llvm::IRBuilder<> builder(ctx);
        builder.SetInsertPoint(bb);

        // Cast the entry point.
        llvm::Value *entryPointPtr = entryPoint->GetTarget();
        entryPointPtr = builder.CreatePointerCast(entryPointPtr, callerType->GetArgument(2)->GetTargetType());

        // Get the argc, argv
        llvm::Function::arg_iterator it = mainFunction->arg_begin();
        llvm::Value *argc = it++;
        llvm::Value *argv = it;

        // Call the caller.
        llvm::Value *ret = builder.CreateCall3(caller->ImportFunction(this), argc, argv, entryPointPtr);

        // Return.
        if(voidRet)
            builder.CreateRet(builder.getInt32(0));
        else
            builder.CreateRet(ret);

        // Verify the function.
        //mainFunction->dump();
        llvm::verifyFunction(*mainFunction);
    }

    bool Module::WriteBitcode(const std::string &fileName)
    {
        // Create the output file.
        std::string error;
        llvm::tool_output_file out(fileName.c_str(), error, llvm::raw_fd_ostream::F_Binary);

        // Write the bitcode.
        llvm::WriteBitcodeToFile(targetModule, out.os());

        // Keep the file.
        out.keep();
        return true;
    }

    bool Module::WriteNativeAssembler(const std::string &fileName)
    {
        // Create the output file.
        std::string error;
        llvm::tool_output_file out(fileName.c_str(), error);

        // Write the assembly.
        if(WriteFile(out.os(), true))
        {
            out.keep();
            return true;
        }

        return false;
    }

    bool Module::WriteObjectFile(const std::string &fileName)
    {
        // Check if object file directly supported.
        llvm::Triple triple(GetVirtualMachine()->GetOptions().Triple);
        bool writeDirect = triple.isOSBinFormatELF() || triple.isOSBinFormatCOFF();
        if(!writeDirect)
        {
            fprintf(stderr, "Unimplemented -c support\n");
            abort();
        }
        
        // Create the output file.
        std::string error;
        llvm::tool_output_file out(fileName.c_str(), error, llvm::raw_fd_ostream::F_Binary);

        // Write the object file.
        if(WriteFile(out.os(), false))
        {
            out.keep();
            return true;
        }
        
        return false;
    }

    bool Module::WriteFile(llvm::raw_ostream &stream, bool assembly)
    {
        // Emits the assembly/object code related with the module.
        // This is based in llc tool.

        // Get the llvm target.
        VirtualMachine *vm = GetVirtualMachine();
        const llvm::Target *theTarget = vm->GetTarget();
        
        // Set the relocation model.
        llvm::Reloc::Model relocModel = llvm::Reloc::Default;
        if(moduleType == MT_LIBRARY && !vm->IsWindows())
            relocModel = llvm::Reloc::PIC_;

        // Get the target machine.
        const VirtualMachineOptions &options = vm->GetOptions();
        std::auto_ptr<llvm::TargetMachine>
            target(theTarget->createTargetMachine(options.Triple, options.Cpu, options.FeatureString, options.TargetOptions, relocModel));
        if(!target.get())
            throw VirtualMachineException("Failed to create the the target machine.\n");

        // Build the passes to emit the assembly.
        llvm::PassManager pm;
        const llvm::TargetData *td = target->getTargetData();
        if(td)
            pm.add(new llvm::TargetData(*td));
        else
            pm.add(new llvm::TargetData(targetModule));

        // Emit verbose assembly.
        target->setAsmVerbosityDefault(true);

        {
            llvm::formatted_raw_ostream fos(stream);

            // Select the target file type.
            llvm::TargetMachine::CodeGenFileType fileType = assembly ? llvm::TargetMachine::CGFT_AssemblyFile : llvm::TargetMachine::CGFT_ObjectFile;

            // Select the optimization level.
            llvm::CodeGenOpt::Level optLevel;
            switch(optimizationLevel)
            {
            case 0:
                optLevel = llvm::CodeGenOpt::None;
                break;
            case 1:
                optLevel = llvm::CodeGenOpt::Less;
                break;
            case 2:
                optLevel = llvm::CodeGenOpt::Default;
                break;
            case 3:
            default:
                optLevel = llvm::CodeGenOpt::Aggressive;
                break;
            }

            // Ask the target to add backend passes as necessary.
            if (target->addPassesToEmitFile(pm, fos, fileType, optLevel))
            {
                // Failed to emit.
                fprintf(stderr, "Target doesn't support generation of assembly code.\n");
                return false;
            }

            pm.run(*targetModule);
        }

        // Sucessfully emitted.
        return true;
    }
    
    bool Module::WriteExportDefinition(llvm::raw_ostream &stream)
    {
        // Write the exports section.
        stream << "EXPORTS\n";
        
        // Export each one of the external or dllexport functions.
        llvm::Module::iterator it = targetModule->begin();
        for(; it != targetModule->end(); ++it)
        {
            llvm::Function *function = it;
            
            // Ignore declarations.
            if(function->isDeclaration())
                continue;
            
    
            if(function->hasExternalLinkage() || function->hasDLLExportLinkage() ||
                function->hasLinkOnceLinkage())
                stream << function->getName() << "\n";
        }
        
        // Export the global variables.
        llvm::Module::global_iterator git = targetModule->global_begin();
        for(; git != targetModule->global_end(); ++git)
        {
            llvm::GlobalVariable *global = git;
            
            // Ignore declarations.
            if(global->hasExternalLinkage() && !global->hasInitializer())
                continue;            
    
            if(global->hasExternalLinkage() || global->hasDLLExportLinkage() ||
                global->hasLinkOnceLinkage())
                stream << global->getName() <<  " DATA\n";
        }
        
        return true;
    }

    bool Module::WriteModule(const std::string &fileName)
    {
        // Create a temporal file for the object code.
        llvm::raw_ostream *stream;
        std::string objectName;
        
        // Write an object file with the module content.
        VirtualMachine *vm = GetVirtualMachine();
        CreateTemporal(vm->IsWindows() ? ".obj" : ".o", &stream, &objectName, false);

        // Write the object code.
        bool res = WriteFile(*stream, false);
        delete stream; // Close the stream.
        if(!res)
        {
            // Delete temporal file.
            DeleteFile(objectName);
            return false;
        }

        // Build the compiler command line.
        std::vector<std::string> commandLine;

        // Use the linker ld.
        commandLine.push_back(PROG_PREFIX "g++" PROG_SUFFIX);

        // Add the output file.
        commandLine.push_back("-o");
        commandLine.push_back(fileName);

        // Add the object file.
        commandLine.push_back(objectName);
        
        // Add the shared option.
        std::string defName;
        if(moduleType == MT_LIBRARY)
        {
            commandLine.push_back("-shared");

            // TODO: Add soname

            if(vm->IsWindows())
            {
                // Create the export definitions.
                llvm::raw_ostream *defStream;
                CreateTemporal(".def", &defStream, &defName, true);
                
                // Write the exports.
                res = WriteExportDefinition(*defStream);
                delete defStream;
                if(!res)
                {
                    // Delete temporal file.
                    DeleteFile(defName);
                    return false;
                }
                
                // Add the definition file to the command line.
                commandLine.push_back(defName);
            }
        }

        // Add the rpath
        commandLine.push_back("-Wl,-rpath,.");
        commandLine.push_back("-Wl,-rpath," + DirName(fileName));
        
#ifdef _WIN32
        // Reduce memory overheard.
        commandLine.push_back("-Wl,--no-keep-memory,--reduce-memory-overheads");
#endif

        // Windows specific options.
        if(vm->IsWindows())
        {
            // Disable stdcall warnings.
            commandLine.push_back("-Wl,--enable-stdcall-fixup");

            // Enable runtime pseudo-relocs.
            commandLine.push_back("-Wl,--enable-runtime-pseudo-reloc");
        }

        // Add library search paths.
        commandLine.push_back("-L.");
        for(size_t i = 0; i < libPaths.size(); ++i)
            commandLine.push_back("-L" + libPaths[i]);

        // Add module references.
        for(size_t i = 1; i < moduleReferences.size(); ++i)
        {
            Module *mod = moduleReferences[i];
            commandLine.push_back("-l" + mod->name.name);
        }

        // Add additional library references.
        for(size_t i = 0; i < libReferences.size(); ++i)
            commandLine.push_back("-l" + libReferences[i]);

        // Build the program/library.
        res = RunCommand(commandLine) == 0;

        // Delete the temporary files.
        DeleteFile(objectName);
        if(!defName.empty())
            DeleteFile(defName);

        // Return.
        return res;
    }

    int  Module::Run(int argc, const char *argv[])
    {
        // Get the execution engine.
        llvm::ExecutionEngine *ee = virtualMachine->GetExecutionEngine();

        // Prepare the jit.
        virtualMachine->PrepareJit();

        // Invoke global constructors.
        if(!invokedConstructors)
        {
            ee->runStaticConstructorsDestructors(false);
            invokedConstructors = true;
        }

        // Find the main function.
        llvm::Function *mainFunction = targetModule->getFunction("main");
        if(!mainFunction)
        {
            fprintf(stderr, "Failed to find the entry point.\n");
            return -1;
        }
            
        // Compile the main function.
        int (*cmain)(int, const char *[]);
        cmain = (int (*)(int, const char *[])) ee->getPointerToFunction(mainFunction);
        
        // Invoke it.
        return cmain(argc, argv);
    }

}
