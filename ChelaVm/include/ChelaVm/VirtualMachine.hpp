#ifndef CHELAVM_VIRTUAL_MACHINE_HPP
#define CHELAVM_VIRTUAL_MACHINE_HPP

#include <stdexcept>
#include <vector>
#include <map>

#include "llvm/LLVMContext.h"
#include "llvm/Target/TargetOptions.h"
#include "llvm/ExecutionEngine/ExecutionEngine.h"
#include "llvm/ExecutionEngine/JIT.h"
#include "llvm/ADT/Triple.h"

namespace ChelaVm
{
    /// Virtual machine scope exception.
	class VirtualMachineException: public std::runtime_error
	{
	public:
		VirtualMachineException(const std::string &what)
			: std::runtime_error(what) {}
	};
    
    // Virtual machine creation data.
    class VirtualMachineOptions
    {
    public:
        std::string Triple;
        std::string Cpu;
        std::string FeatureString;
        llvm::TargetOptions TargetOptions;
    };
	
	class Structure;
	class Class;
    class ChelaType;
	class Function;
    class Member;
    class Module;
    class ModuleReader;
    struct ModuleName;
    class PointerType;
    class _VirtualMachineDataImpl;
	class VirtualMachine
	{
	public:
		VirtualMachine(llvm::LLVMContext &context, const VirtualMachineOptions &options);
		~VirtualMachine();
		
		Module* LoadModule(ModuleReader &reader, bool noreflection = false,
                           bool debug = false, std::string *libPaths=NULL, size_t libPathCount=0);
		Module* LoadModule(const std::string &filename, bool noreflection = false,
                           bool debug = false, std::string *libPaths=NULL, size_t libPathCount=0);
		Module* LoadModule(const ModuleName &name, bool noreflection = false, bool debug = false, Module *hint = NULL);
		
		void CompileModules();

        // Context, target platform data.
        llvm::LLVMContext &getContext();
        const VirtualMachineOptions &GetOptions();
        const llvm::Target *GetTarget();
        llvm::TargetData *GetTargetData();
        bool IsAmd64() const;
        bool IsX86() const;
        bool IsWindows() const;
        
        // LLVM jiting. Currently broken support.
		llvm::ExecutionEngine *GetExecutionEngine();

        // Primitive types.
        const ChelaType *GetVoidType();
        const ChelaType *GetConstVoidType();
        const ChelaType *GetBoolType();
        const ChelaType *GetByteType();
        const ChelaType *GetSByteType();
        const ChelaType *GetCharType();
        const ChelaType *GetShortType();
        const ChelaType *GetUShortType();
        const ChelaType *GetIntType();
        const ChelaType *GetUIntType();
        const ChelaType *GetLongType();
        const ChelaType *GetULongType();
        const ChelaType *GetFloatType();
        const ChelaType *GetDoubleType();
        const ChelaType *GetSizeType();
        const ChelaType *GetPtrDiffType();
        const ChelaType *GetCStringType();
        const ChelaType *GetNullType();
        const PointerType *GetVoidPointerType();

        // Basic classes.
		Class *GetObjectClass();
        Class *GetTypeClass();
		Class *GetStringClass();
        Class *GetClosureClass();
        Class *GetArrayClass();
        Class *GetValueTypeClass();
        Class *GetEnumClass();
        Class *GetDelegateClass();

        // Special attributes.
        Class *GetThreadStaticAttribute();
        Class *GetChelaIntrinsicAttribute();

        // Kernel parameter holding class.
        Class *GetStreamHolderClass();
        Class *GetStreamHolder1DClass();
        Class *GetStreamHolder2DClass();
        Class *GetStreamHolder3DClass();
        Class *GetUniformHolderClass();

        // Reflection info classes.
        Class *GetAssemblyClass();
        Class *GetConstructorInfoClass();
        Class *GetEventInfoClass();
        Class *GetFieldInfoClass();
        Class *GetMemberInfoClass();
        Class *GetMethodInfoClass();
        Class *GetParameterInfoClass();
        Class *GetPropertyInfoClass();

        // Primitive type structures.
        Structure *GetAssociatedClass(const ChelaType *primitive);
        const ChelaType *GetAssociatedPrimitive(Structure *building);

		llvm::Constant *GetUnmanagedAlloc(Module *module);
		llvm::Constant *GetUnmanagedAllocArray(Module *module);
		llvm::Constant *GetUnmanagedFree(Module *module);
		llvm::Constant *GetUnmanagedFreeArray(Module *module);
		llvm::Constant *GetManagedAlloc(Module *module);
        llvm::Constant *GetAddRef(Module *module);
        llvm::Constant *GetRelRef(Module *module);
        llvm::Constant *GetObjectDownCast(Module *module);
        llvm::Constant *GetIfaceDownCast(Module *module);
        llvm::Constant *GetIfaceCrossCast(Module *module);
        llvm::Constant *GetEhPersonality(Module *module);
        llvm::Constant *GetEhThrow(Module *module);
        llvm::Constant *GetEhRead(Module *module);
        llvm::Constant *GetCheckCast(Module *module);
        llvm::Constant *GetIsRefType(Module *module);
        llvm::Constant *GetThrowNull(Module *module);
        llvm::Constant *GetThrowBound(Module *module);
        llvm::Constant *GetComputeIsCpu(Module *module);
        llvm::Constant *GetComputeCpuBind(Module *module);
        llvm::Constant *GetComputeBind(Module *module);
        const void *GetSystemApi(const std::string &name);

        Function *GetRuntimeFunction(const std::string &name, bool isStatic);

	private:
        friend class Module;
        typedef std::map<std::string, const void*> SysLayerApi;
        typedef std::map<const ChelaType*, Structure*> PrimitiveMap;
        typedef std::map<Structure*, const ChelaType*> PrimitiveReverseMap;

        void InitSysLayer();
        void LoadRuntimeClasses();
		void LoadRuntimeData();
        void NotifyLoadedModule(Module *module);
        void PrepareJit();
        void RegisterPrimStruct(const ChelaType*, const std::string &name);
		
		std::vector<Module*> loadedModules;
        typedef std::map<std::string, Module*> LoadedModulesByName;
        LoadedModulesByName loadedModulesByName;

        llvm::LLVMContext &context;
        VirtualMachineOptions options;
        llvm::Triple triple;
		llvm::ExecutionEngine *executionEngine;
        const llvm::Target *target;
        llvm::TargetData *targetData;
		Module *runtimeModule;

        // Some virtual machine data.
        _VirtualMachineDataImpl *dataImpl;

        // Base classes.
		Class *objectClass;
        Class *typeClass;
		Class *stringClass;
        Class *closureClass;
        Class *arrayClass;
        Class *valueTypeClass;
        Class *enumClass;
        Class *delegateClass;

        // Special attributes.
        Class *threadStaticAttribute;
        Class *chelaIntrinsicAttribute;

        // Kernel data holders.
        Class *streamHolderClass;
        Class *streamHolder1DClass;
        Class *streamHolder2DClass;
        Class *streamHolder3DClass;
        Class *uniformHolderClass;

        // Reflection info.
        Class *assemblyClass;
        Class *constructorInfoClass;
        Class *eventInfoClass;
        Class *fieldInfoClass;
        Class *memberInfoClass;
        Class *methodInfoClass;
        Class *parameterInfoClass;
        Class *propertyInfoClass;
        PrimitiveMap primitiveMap;
        PrimitiveReverseMap primitiveReverseMap;

		Function *unmanagedAlloc;
		Function *unmanagedAllocArray;
		Function *unmanagedFree;
		Function *unmanagedFreeArray;
		Function *managedAlloc;
        Function *addReference;
        Function *releaseReference;
        Function *objectDownCast;
        Function *ifaceDownCast;
        Function *ifaceCrossCast;
        Function *checkCast;
        Function *isRefType;
        Function *ehPersonality;
        Function *ehThrow;
        Function *ehRead;
        Function *throwNull;
        Function *throwBound;
        Function *computeIsCpu;
        Function *computeCpuBind;
        Function *computeBind;
        SysLayerApi sysLayerApi;
	};
};

#endif //CHELAVM_VIRTUAL_MACHINE_HPP
