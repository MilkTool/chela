#include <stdio.h>
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FileSystem.hpp"
#include "ChelaVm/TypeGroup.hpp"
#include "llvm/Support/Host.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Target/TargetOptions.h"
#include "llvm/Support/TargetRegistry.h"
#include "llvm/Support/TargetSelect.h"
#ifdef EXPLICIT_SYSLAYER
#include "SysLayer.hpp"
#endif

namespace ChelaVm
{
    class _VirtualMachineDataImpl
    {
    public:
        _VirtualMachineDataImpl(VirtualMachine *vm, llvm::LLVMContext &context, llvm::TargetData *target)
          : voidType(vm),
            boolType(vm, "bool", "x", false, 1, 1, llvm::Type::getInt1Ty(context)),
            charType(vm, "char", "c", false, 2, 2),
            byteType(vm, "byte", "B", false, 1, 1),
            sbyteType(vm, "sbyte", "b", true, 1, 1),
            shortType(vm, "short", "s", true, 2, 2),
            ushortType(vm, "ushort", "S", false, 2, 2),
            intType(vm, "int", "i", true, 4, 4),
            uintType(vm, "uint", "I", false, 4, 4),
            longType(vm, "long", "l", true, 8, target->getABIIntegerTypeAlignment(64)),
            ulongType(vm, "ulong", "L", false, 8, target->getABIIntegerTypeAlignment(64)),
            sizeType(vm, "size_t", "Z", false, target->getPointerSize(), target->getPointerABIAlignment()),
            ptrdiffType(vm, "ptrdiff_t", "z", true, target->getPointerSize(), target->getPointerABIAlignment()),
            floatType(vm, "float", "f", false),
            doubleType(vm, "double", "F", true),
            cstringType(NULL), constVoidType(NULL)
        {
        }

        VoidType voidType;
        IntegerType boolType;
        IntegerType charType;
        IntegerType byteType;
        IntegerType sbyteType;
        IntegerType shortType;
        IntegerType ushortType;
        IntegerType intType;
        IntegerType uintType;
        IntegerType longType;
        IntegerType ulongType;
        IntegerType sizeType;
        IntegerType ptrdiffType;
        FloatType floatType;
        FloatType doubleType;
        const ChelaType *cstringType;
        const ChelaType *constVoidType;
    };

	VirtualMachine::VirtualMachine(llvm::LLVMContext &context, const VirtualMachineOptions &opts)
     :  context(context), options(opts)
	{
#ifdef USE_ALL_TARGETS
        // Initialize all targets first
        llvm::InitializeAllTargets();
        llvm::InitializeAllTargetMCs();
        llvm::InitializeAllAsmPrinters();
        llvm::InitializeAllAsmParsers();
#else
        // Initialize the native target first
        llvm::InitializeNativeTarget();
        llvm::InitializeNativeTargetAsmPrinter();
#endif

        // Use the default target when no specified.
        if(options.Triple.empty())
            options.Triple = llvm::sys::getDefaultTargetTriple();
        triple.setTriple(options.Triple);

        // Lookup the the requested target.
        std::string error;
        target = llvm::TargetRegistry::lookupTarget(options.Triple, error);
        if(!target)
            throw VirtualMachineException("Failed to get target for " + options.Triple);

        // Create the target machine to get data about the target.
        std::auto_ptr<llvm::TargetMachine>
            targetMachine(target->createTargetMachine(options.Triple, options.Cpu, options.FeatureString, options.TargetOptions));
        if(!targetMachine.get())
            throw VirtualMachineException("Failed to create a target machine for " + options.Triple);

        // Copy the target data of the machine.
        targetData = new llvm::TargetData(*targetMachine->getTargetData());
        
        // Create the type data.
        dataImpl = new _VirtualMachineDataImpl(this, context, targetData);

        // Initialize to null some variables.
		executionEngine = NULL;
		runtimeModule = NULL;

        // Base classes.
		objectClass = NULL;
        typeClass = NULL;
		stringClass = NULL;
        closureClass = NULL;
        arrayClass = NULL;
        valueTypeClass = NULL;
        enumClass = NULL;
        delegateClass = NULL;

        // Special attributes.
        threadStaticAttribute = NULL;
        chelaIntrinsicAttribute = NULL;

        // Kernel data holders
        streamHolderClass = NULL;
        streamHolder1DClass = NULL;
        streamHolder2DClass = NULL;
        streamHolder3DClass = NULL;
        uniformHolderClass = NULL;

        // Reflection info.
        assemblyClass = NULL;
        constructorInfoClass = NULL;
        eventInfoClass = NULL;
        fieldInfoClass = NULL;
        memberInfoClass = NULL;
        methodInfoClass = NULL;
        parameterInfoClass = NULL;
        propertyInfoClass = NULL;

        // Memory management.
		unmanagedAlloc = NULL;
		unmanagedAllocArray = NULL;
		unmanagedFree = NULL;
		unmanagedFreeArray = NULL;
		managedAlloc = NULL;
        addReference = NULL;
        releaseReference = NULL;

        // Casting.
        objectDownCast = NULL;
        ifaceDownCast = NULL;
        ifaceCrossCast = NULL;
        checkCast = NULL;

        // Type checking.
        isRefType = NULL;

        // Exception handling.
        ehPersonality = NULL;
        ehThrow = NULL;
        ehRead = NULL;

        // Implicit checks.
        throwNull = NULL;
        throwBound = NULL;

        // Kernel binder helpers.
        computeIsCpu = NULL;
        computeCpuBind = NULL;
        computeBind = NULL;

        // Initialize the sys layer.
        InitSysLayer();
	}
	
	VirtualMachine::~VirtualMachine()
	{
		for(size_t i = 0; i < loadedModules.size(); i++)
			delete loadedModules[i];
	}
		
	Module* VirtualMachine::LoadModule(ModuleReader &reader, bool noreflection,
                                      bool debug, std::string *libPaths, size_t libPathCount)
	{
        // Create the module.
		Module *ret = new Module(this, noreflection, debug);

        // Add library paths.
        for(size_t i = 0; i < libPathCount; ++i)
            ret->AddLibraryPath(libPaths[i]);

        // Load from file.
		ret->LoadFromFile(reader);
		
		// Load the runtime data.
		if(loadedModules.size() == 1)
			LoadRuntimeData();
		else
			executionEngine->addModule(ret->GetTargetModule());		
		
		return ret;
	}
	
	Module* VirtualMachine::LoadModule(const std::string &filename, bool noreflection, bool debug,
                                       std::string *libPaths, size_t libPathCount)
	{
		ModuleReader reader(filename);
		return LoadModule(reader, noreflection, debug, libPaths, libPathCount);
	}

    inline bool FindModule(std::string baseName, std::string *fileName, Module *hint)
    {
        // Try first in the current directory.
        if(ExistsFile(baseName))
        {
            *fileName = baseName;
            return true;
        }

        // Now, use the hint.
        if(hint)
        {
            // Check in the same directory as the module.
            std::string modDir = DirName(hint->GetName().filename);
            *fileName = JoinPath(modDir, baseName);
            if(ExistsFile(*fileName))
                return true;

            // Check in the library search path.
            for(size_t i = 0; i < hint->GetLibraryPathCount(); ++i)
            {
                *fileName = JoinPath(hint->GetLibraryPath(i), baseName);
                if(ExistsFile(*fileName))
                    return true;
            }
        }

        return false;
    }

	Module* VirtualMachine::LoadModule(const ModuleName &name, bool noreflection, bool debug, Module *hint)
	{
        // Check for an already loaded module.
        // TODO: Add security.
        LoadedModulesByName::iterator it = loadedModulesByName.find(name.name);
        if(it != loadedModulesByName.end())
            return it->second;

        // Use the filename if present
        if(!name.filename.empty())
    		return LoadModule(name.filename, debug);

        // Try to load the module.
        std::string baseName = name.name + ".cbm";
        std::string fileName;
        if(FindModule(baseName, &fileName, hint))
            return LoadModule(fileName, noreflection, debug);

        // Try to load embedded module.
        // ELF module.
        baseName = "lib" + name.name + ".so";
        if(FindModule(baseName, &fileName, hint))
            return LoadModule(fileName, noreflection, debug);

        // PE module.
        baseName = name.name + ".dll";
        if(FindModule(baseName, &fileName, hint))
            return LoadModule(fileName, noreflection, debug);

        return NULL;
	}
	
	void VirtualMachine::CompileModules()
	{
		// Compile each one of the modules.
		for(size_t i = 0; i < loadedModules.size(); i++)
		{
			Module *mod = loadedModules[i];
			mod->Compile();
		}
	}

    void VirtualMachine::InitSysLayer()
    {
#ifdef EXPLICIT_SYSLAYER
        // Read the system layer functions.
        int sysMajor, sysMinor, numfunctions;
        _Chela_SysLayer_Version(&sysMajor, &sysMinor, &numfunctions);

        for(int i = 0; i < numfunctions; i++)
            sysLayerApi.insert(
                std::make_pair(_Chela_SysLayer_GetFunctionName(i),
                               _Chela_SysLayer_GetFunction(i)));
#endif
    }

    void VirtualMachine::NotifyLoadedModule(Module *module)
    {
        loadedModules.push_back(module);
        loadedModulesByName.insert(std::make_pair(module->GetName().name, module));
    }

    void VirtualMachine::PrepareJit()
    {
        // The first loaded module must be the runtime.
        loadedModules[0]->PrepareJit();
    }

    const void *VirtualMachine::GetSystemApi(const std::string &name)
    {
        SysLayerApi::const_iterator it = sysLayerApi.find(name);
        if(it != sysLayerApi.end())
            return it->second;
        return NULL;
    }

    inline Class *GetClass(Module *module, const std::string &className)
    {
        Member *member = module->GetMember(className);
        if(!member || !member->IsClass())
            throw VirtualMachineException("Failed to load " + className);

        // Cast the class.
        return static_cast<Class*> (member);
    }

    inline Class *GetTemplateClass(Module *module, const std::string &className)
    {
        // Find the class type group.
        Member *member = module->GetMember(className + " <TG> ");
        if(!member || !member->IsTypeGroup())
            throw VirtualMachineException("Failed to load " + className + " <TG> ");

        // Use the first template in the group.
        TypeGroup *group = static_cast<TypeGroup*> (member);
        Structure *building = group->GetType(0);
        if(!building || !building->IsClass())
            throw VirtualMachineException("Failed to load template class " + className);

        // Cast the class.
        return static_cast<Class*> (building);
    }

    inline Structure *GetStruct(Module *module, const std::string &className)
    {
        Member *member = module->GetMember(className);
        if(!member || !member->IsStructure())
            throw VirtualMachineException("Failed to load " + className);

        // Cast the class.
        return static_cast<Structure*> (member);
    }

    void VirtualMachine::RegisterPrimStruct(const ChelaType *primitive, const std::string &name)
    {
        // Get the associated class.
        Structure* assoc = GetStruct(runtimeModule, name);

        // Register it.
        primitiveMap.insert(std::make_pair(primitive, assoc));
        primitiveReverseMap.insert(std::make_pair(assoc, primitive));
    }

    void VirtualMachine::LoadRuntimeClasses()
    {
        // Load only once.
        if(objectClass)
            return;

        // The first loaded module must be the runtime.
        runtimeModule = loadedModules[0];
        runtimeModule->SetRuntimeFlag(true);

        // Get basic classes.
        typeClass = GetClass(runtimeModule, "Chela.Lang.Type");
        objectClass = GetClass(runtimeModule, "Chela.Lang.Object");
        stringClass = GetClass(runtimeModule, "Chela.Lang.String");
        closureClass = GetClass(runtimeModule, "Chela.Lang.Closure");
        arrayClass = GetClass(runtimeModule, "Chela.Lang.Array");
        valueTypeClass = GetClass(runtimeModule, "Chela.Lang.ValueType");
        enumClass = GetClass(runtimeModule, "Chela.Lang.Enum");
        delegateClass = GetClass(runtimeModule, "Chela.Lang.Delegate");

        // Special attributes.
        threadStaticAttribute = GetClass(runtimeModule, "Chela.Lang.ThreadStaticAttribute");
        chelaIntrinsicAttribute = GetClass(runtimeModule, "Chela.Runtime.Core.ChelaIntrinsicAttribute");

        // Get the kernel data holding classes.
        streamHolderClass = GetTemplateClass(runtimeModule, "Chela.Compute.StreamHolder");
        streamHolder1DClass = GetTemplateClass(runtimeModule, "Chela.Compute.StreamHolder1D");
        streamHolder2DClass = GetTemplateClass(runtimeModule, "Chela.Compute.StreamHolder2D");
        streamHolder3DClass = GetTemplateClass(runtimeModule, "Chela.Compute.StreamHolder3D");
        uniformHolderClass = GetTemplateClass(runtimeModule, "Chela.Compute.UniformHolder");

        // Get the reflection classes.
        assemblyClass = GetClass(runtimeModule, "Chela.Reflection.Assembly");
        constructorInfoClass = GetClass(runtimeModule, "Chela.Reflection.ConstructorInfo");
        eventInfoClass = GetClass(runtimeModule, "Chela.Reflection.EventInfo");
        fieldInfoClass = GetClass(runtimeModule, "Chela.Reflection.FieldInfo");
        memberInfoClass = GetClass(runtimeModule, "Chela.Reflection.MemberInfo");
        methodInfoClass = GetClass(runtimeModule, "Chela.Reflection.MethodInfo");
        parameterInfoClass = GetClass(runtimeModule, "Chela.Reflection.ParameterInfo");
        propertyInfoClass = GetClass(runtimeModule, "Chela.Reflection.PropertyInfo");

        // Register associated types.
        RegisterPrimStruct(GetBoolType(), "Chela.Lang.Boolean");
        RegisterPrimStruct(GetCharType(), "Chela.Lang.Char");
        RegisterPrimStruct(GetSByteType(), "Chela.Lang.SByte");
        RegisterPrimStruct(GetByteType(), "Chela.Lang.Byte");
        RegisterPrimStruct(GetShortType(), "Chela.Lang.Int16");
        RegisterPrimStruct(GetUShortType(), "Chela.Lang.UInt16");
        RegisterPrimStruct(GetIntType(), "Chela.Lang.Int32");
        RegisterPrimStruct(GetUIntType(), "Chela.Lang.UInt32");
        RegisterPrimStruct(GetLongType(), "Chela.Lang.Int64");
        RegisterPrimStruct(GetULongType(), "Chela.Lang.UInt64");
        RegisterPrimStruct(GetSizeType(), "Chela.Lang.UIntPtr");
        RegisterPrimStruct(GetPtrDiffType(), "Chela.Lang.IntPtr");
        RegisterPrimStruct(GetFloatType(), "Chela.Lang.Single");
        RegisterPrimStruct(GetDoubleType(), "Chela.Lang.Double");
        RegisterPrimStruct(GetVoidType(), "Chela.Lang.Void");

        // Register vector associated types.
        RegisterPrimStruct(GetFloatVectorTy(this, 2), "Chela.Lang.Single2");
        RegisterPrimStruct(GetFloatVectorTy(this, 3), "Chela.Lang.Single3");
        RegisterPrimStruct(GetFloatVectorTy(this, 4), "Chela.Lang.Single4");
        RegisterPrimStruct(GetDoubleVectorTy(this, 2), "Chela.Lang.Double2");
        RegisterPrimStruct(GetDoubleVectorTy(this, 3), "Chela.Lang.Double3");
        RegisterPrimStruct(GetDoubleVectorTy(this, 4), "Chela.Lang.Double4");
        RegisterPrimStruct(GetIntVectorTy(this, 2), "Chela.Lang.Int32x2");
        RegisterPrimStruct(GetIntVectorTy(this, 3), "Chela.Lang.Int32x3");
        RegisterPrimStruct(GetIntVectorTy(this, 4), "Chela.Lang.Int32x4");
        RegisterPrimStruct(GetBoolVectorTy(this, 2), "Chela.Lang.Boolean2");
        RegisterPrimStruct(GetBoolVectorTy(this, 3), "Chela.Lang.Boolean3");
        RegisterPrimStruct(GetBoolVectorTy(this, 4), "Chela.Lang.Boolean4");

        // Register matrix associated types.
        RegisterPrimStruct(GetFloatMatrixTy(this, 3,3), "Chela.Lang.Single3x3");
        RegisterPrimStruct(GetFloatMatrixTy(this, 4,4), "Chela.Lang.Single4x4");

        // Declare them.
        typeClass->DeclarePass();
        objectClass->DeclarePass();
        stringClass->DeclarePass();
        closureClass->DeclarePass();
        arrayClass->DeclarePass();
        valueTypeClass->DeclarePass();
        enumClass->DeclarePass();
        delegateClass->DeclarePass();
        streamHolderClass->DeclarePass();
        streamHolder1DClass->DeclarePass();
        streamHolder2DClass->DeclarePass();
        streamHolder3DClass->DeclarePass();
        uniformHolderClass->DeclarePass();

        assemblyClass->DeclarePass();
        constructorInfoClass->DeclarePass();
        eventInfoClass->DeclarePass();
        fieldInfoClass->DeclarePass();
        memberInfoClass->DeclarePass();
        methodInfoClass->DeclarePass();
        parameterInfoClass->DeclarePass();
        propertyInfoClass->DeclarePass();
    }

    void VirtualMachine::LoadRuntimeData()
    {
        // Create the execution engine.
        llvm::EngineBuilder factory(runtimeModule->GetTargetModule());
        factory.setEngineKind(llvm::EngineKind::JIT);
        factory.setAllocateGVsWithCode(false);
        executionEngine = factory.create();
    }

    // Target platform and llvm context data.
    llvm::LLVMContext &VirtualMachine::getContext()
    {
        return context;
    }

    const VirtualMachineOptions &VirtualMachine::GetOptions()
    {
        return options;
    }
    
    const llvm::Target *VirtualMachine::GetTarget()
    {
        return target;
    }
    
    llvm::TargetData *VirtualMachine::GetTargetData()
    {
        return targetData;
    }

    bool VirtualMachine::IsAmd64() const
    {
        return triple.getArch() == llvm::Triple::x86_64;
    }
    
    bool VirtualMachine::IsX86() const
    {
        return triple.getArch() == llvm::Triple::x86;
    }

    bool VirtualMachine::IsWindows() const
    {
        return triple.isOSWindows();
    }
    
	llvm::ExecutionEngine *VirtualMachine::GetExecutionEngine()
	{
		return executionEngine;
	}

    // Basic data types.
    const ChelaType *VirtualMachine::GetVoidType()
    {
        return &dataImpl->voidType;
    }

    const ChelaType *VirtualMachine::GetConstVoidType()
    {
        if(!dataImpl->constVoidType)
            dataImpl->constVoidType = ConstantType::Create(GetVoidType());
        return dataImpl->constVoidType;
    }

    const ChelaType *VirtualMachine::GetBoolType()
    {
        return &dataImpl->boolType;
    }

    const ChelaType *VirtualMachine::GetByteType()
    {
        return &dataImpl->byteType;
    }

    const ChelaType *VirtualMachine::GetSByteType()
    {
        return &dataImpl->sbyteType;
    }

    const ChelaType *VirtualMachine::GetCharType()
    {
        return &dataImpl->charType;
    }

    const ChelaType *VirtualMachine::GetShortType()
    {
        return &dataImpl->shortType;
    }

    const ChelaType *VirtualMachine::GetUShortType()
    {
        return &dataImpl->ushortType;
    }

    const ChelaType *VirtualMachine::GetIntType()
    {
        return &dataImpl->intType;
    }

    const ChelaType *VirtualMachine::GetUIntType()
    {
        return &dataImpl->uintType;
    }

    const ChelaType *VirtualMachine::GetLongType()
    {
        return &dataImpl->longType;
    }

    const ChelaType *VirtualMachine::GetULongType()
    {
        return &dataImpl->ulongType;
    }

    const ChelaType *VirtualMachine::GetFloatType()
    {
        return &dataImpl->floatType;
    }

    const ChelaType *VirtualMachine::GetDoubleType()
    {
        return &dataImpl->doubleType;
    }

    const ChelaType *VirtualMachine::GetSizeType()
    {
        return &dataImpl->sizeType;
    }

    const ChelaType *VirtualMachine::GetPtrDiffType()
    {
        return &dataImpl->ptrdiffType;
    }

    const ChelaType *VirtualMachine::GetCStringType()
    {
        if(dataImpl->cstringType == NULL)
            dataImpl->cstringType = PointerType::Create(ConstantType::Create(GetSByteType()));
        return dataImpl->cstringType;
    }

    const ChelaType *VirtualMachine::GetNullType()
    {
        return ReferenceType::CreateNullReference(this);
    }

    const PointerType *VirtualMachine::GetVoidPointerType()
    {
        return PointerType::Create(GetVoidType());
    }

	Class *VirtualMachine::GetObjectClass()
	{
		return objectClass;
	}

    Class *VirtualMachine::GetTypeClass()
    {
        return typeClass;
    }
	
	Class *VirtualMachine::GetStringClass()
	{
		return stringClass;
	}

    Class *VirtualMachine::GetClosureClass()
    {
        return closureClass;
    }

    Class *VirtualMachine::GetArrayClass()
    {
        return arrayClass;
    }

    Class *VirtualMachine::GetValueTypeClass()
    {
        return valueTypeClass;
    }

    Class *VirtualMachine::GetEnumClass()
    {
        return enumClass;
    }

    Class *VirtualMachine::GetDelegateClass()
    {
        return delegateClass;
    }

    Class *VirtualMachine::GetThreadStaticAttribute()
    {
        return threadStaticAttribute;
    }

    Class *VirtualMachine::GetChelaIntrinsicAttribute()
    {
        return chelaIntrinsicAttribute;
    }

    Class *VirtualMachine::GetStreamHolderClass()
    {
        return streamHolderClass;
    }

    Class *VirtualMachine::GetStreamHolder1DClass()
    {
        return streamHolder1DClass;
    }

    Class *VirtualMachine::GetStreamHolder2DClass()
    {
        return streamHolder2DClass;
    }

    Class *VirtualMachine::GetStreamHolder3DClass()
    {
        return streamHolder3DClass;
    }

    Class *VirtualMachine::GetUniformHolderClass()
    {
        return uniformHolderClass;
    }

    Class *VirtualMachine::GetAssemblyClass()
    {
        return assemblyClass;
    }

    Class *VirtualMachine::GetConstructorInfoClass()
    {
        return constructorInfoClass;
    }

    Class *VirtualMachine::GetEventInfoClass()
    {
        return eventInfoClass;
    }

    Class *VirtualMachine::GetFieldInfoClass()
    {
        return fieldInfoClass;
    }

    Class *VirtualMachine::GetMemberInfoClass()
    {
        return memberInfoClass;
    }

    Class *VirtualMachine::GetMethodInfoClass()
    {
        return methodInfoClass;
    }

    Class *VirtualMachine::GetParameterInfoClass()
    {
        return parameterInfoClass;
    }

    Class *VirtualMachine::GetPropertyInfoClass()
    {
        return propertyInfoClass;
    }

    Structure *VirtualMachine::GetAssociatedClass(const ChelaType *primitive)
    {
        PrimitiveMap::iterator it = primitiveMap.find(primitive);
        if(it != primitiveMap.end())
            return it->second;
        return NULL;
    }

    const ChelaType *VirtualMachine::GetAssociatedPrimitive(Structure *building)
    {
        PrimitiveReverseMap::iterator it = primitiveReverseMap.find(building);
        if(it != primitiveReverseMap.end())
            return it->second;
        return NULL;

    }

	Function *VirtualMachine::GetRuntimeFunction(const std::string &name, bool isStatic)
	{
		// Find the module member.
		Member *member = runtimeModule->GetMember(name);
		if(!member || !member->IsFunction() || member->IsStatic() != isStatic)
			throw VirtualMachineException("Using incompatible runtime, cannot find: " + name);
		
		// Cast and declare the function.
		Function *function = static_cast<Function*> (member);
		function->DeclarePass();
		return function;		
	}
	
	llvm::Constant *VirtualMachine::GetUnmanagedAlloc(Module *module)
	{
		if(unmanagedAlloc == NULL)
			unmanagedAlloc = GetRuntimeFunction("__chela_umgd_alloc__", true);
		return unmanagedAlloc->ImportFunction(module);
	}
	
	llvm::Constant *VirtualMachine::GetUnmanagedAllocArray(Module *module)
	{
		if(unmanagedAllocArray == NULL)
			unmanagedAllocArray = GetRuntimeFunction("__chela_umgd_allocay__", true);
		return unmanagedAllocArray->ImportFunction(module);
	}
	
	llvm::Constant *VirtualMachine::GetUnmanagedFree(Module *module)
	{
		if(unmanagedFree == NULL)
			unmanagedFree = GetRuntimeFunction("__chela_umgd_free__", true);
		return unmanagedFree->ImportFunction(module);
	}
	
	llvm::Constant *VirtualMachine::GetUnmanagedFreeArray(Module *module)
	{
		if(unmanagedFreeArray == NULL)
			unmanagedFreeArray = GetRuntimeFunction("__chela_umgd_freeay__", true);
		return unmanagedFreeArray->ImportFunction(module);
	}
	
	llvm::Constant *VirtualMachine::GetManagedAlloc(Module *module)
	{
		if(managedAlloc == NULL)
			managedAlloc = GetRuntimeFunction("__chela_mgd_alloc__", true);
		return managedAlloc->ImportFunction(module);
	}

    llvm::Constant *VirtualMachine::GetAddRef(Module *module)
    {
        if(addReference == NULL)
            addReference = GetRuntimeFunction("__chela_add_ref__", true);
        return addReference->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetRelRef(Module *module)
    {
        if(releaseReference == NULL)
            releaseReference = GetRuntimeFunction("__chela_rel_ref__", true);
        return releaseReference->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetObjectDownCast(Module *module)
    {
        if(objectDownCast == NULL)
            objectDownCast = GetRuntimeFunction("__chela_object_downcast__", true);
        return objectDownCast->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetIfaceDownCast(Module *module)
    {
        if(ifaceDownCast == NULL)
            ifaceDownCast = GetRuntimeFunction("__chela_iface_downcast__", true);
        return ifaceDownCast->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetIfaceCrossCast(Module *module)
    {
        if(ifaceCrossCast == NULL)
            ifaceCrossCast = GetRuntimeFunction("__chela_iface_crosscast__", true);
        return ifaceCrossCast->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetCheckCast(Module *module)
    {
        if(checkCast == NULL)
            checkCast = GetRuntimeFunction("__chela_check_cast__", true);
        return checkCast->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetIsRefType(Module *module)
    {
        if(isRefType == NULL)
            isRefType = GetRuntimeFunction("__chela_is_reference__", true);
        return isRefType->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetEhPersonality(Module *module)
    {
        if(ehPersonality == NULL)
            ehPersonality = GetRuntimeFunction("__chela_eh_personality__", true);
        return ehPersonality->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetEhRead(Module *module)
    {
        if(ehRead == NULL)
            ehRead = GetRuntimeFunction("__chela_eh_read__", true);
        return ehRead->ImportFunction(module);
    }

    llvm::Constant *VirtualMachine::GetEhThrow(Module *module)
    {
        if(ehThrow == NULL)
            ehThrow = GetRuntimeFunction("__chela_eh_throw__", true);
        return ehThrow->ImportFunction(module);
    }

     llvm::Constant *VirtualMachine::GetThrowNull(Module *module)
     {
        if(throwNull == NULL)
            throwNull = GetRuntimeFunction("__chela_throw_null__", true);
        return throwNull->ImportFunction(module);
     }

     llvm::Constant *VirtualMachine::GetThrowBound(Module *module)
     {
        if(throwBound == NULL)
            throwBound = GetRuntimeFunction("__chela_throw_bound__", true);
        return throwBound->ImportFunction(module);
     }

     llvm::Constant *VirtualMachine::GetComputeIsCpu(Module *module)
     {
        if(computeIsCpu == NULL)
            computeIsCpu = GetRuntimeFunction("__chela_compute_iscpu__", true);
        return computeIsCpu->ImportFunction(module);
     }

     llvm::Constant *VirtualMachine::GetComputeCpuBind(Module *module)
     {
        if(computeCpuBind == NULL)
            computeCpuBind = GetRuntimeFunction("__chela_compute_cpu_bind__", true);
        return computeCpuBind->ImportFunction(module);
     }

     llvm::Constant *VirtualMachine::GetComputeBind(Module *module)
     {
        if(computeBind == NULL)
            computeBind = GetRuntimeFunction("__chela_compute_bind__", true);
        return computeBind->ImportFunction(module);
     }
}
