#ifndef _CHELAVM_MODULE_HPP_
#define _CHELAVM_MODULE_HPP_

#include <string>
#include <vector>
#include <map>

#include "llvm/LLVMContext.h"
#include "llvm/Module.h"
#include "llvm/PassManager.h"
#include "llvm/Target/TargetData.h"
#include "llvm/Support/IRBuilder.h"

#include "ChelaVm/ModuleFile.hpp"
#include "ChelaVm/Types.hpp"

namespace ChelaVm
{
    class GenericInstance;
    extern bool GenericInstance_LessThan(const GenericInstance *a, const GenericInstance *b);

	class ModuleException: public std::runtime_error
	{
	public:
		ModuleException(const std::string &what)
			: std::runtime_error(what) {}
	};
	
	struct ModuleName
	{
		std::string name;
        std::string filename;
		uint16_t versionMajor;
		uint16_t versionMinor;
		uint16_t versionMicro;
	};

    struct ResourceData
    {
        ResourceData(std::string name, uint32_t offset, uint32_t length)
            : name(name), offset(offset), length(length)
        {
        }

        bool operator<(const ResourceData &o)
        {
            return name < o.name;
        }

        std::string name;
        uint32_t offset;
        uint32_t length;
    };

    class AttributeConstant;
    class DebugInformation;
    class Field;
    class Function;
    class Interface;
	class Member;
	class Namespace;
    class TypeInstance;
    class VirtualMachine;
	struct TypeReferenceCache;
	struct AnonTypeCache;
	class Module
	{
	public:
		~Module();

        // Module tables.
		const std::string &GetString(size_t id);
		const ChelaType *GetType(uint32_t id);
		Module *GetReferencedModule(uint32_t id);

        // Member finding.
		Member *GetMember(uint32_t id);
		Member *GetMember(const std::string &fullname);

        // Member finding utils.
        Function *GetFunction(uint32_t id);
        Class *GetClass(uint32_t id);
        const Class *GetClassType(uint32_t id);
        Structure *GetStructure(uint32_t id);
        Interface *GetInterface(uint32_t id);

        // Module naming.
		ModuleName &GetName();
        const std::string &GetMangledName() const;
		const ModuleName &GetName() const;

        // Module namespace.
		Namespace *GetGlobalNamespace() const;

        // Module loading.
		void LoadFromFile(ModuleReader &reader);
        void LoadFromMemory(size_t moduleSize, uint8_t *moduleData);

        // Raw module data access.
        const uint8_t *GetModuleData() const;

        // Important handles.
		VirtualMachine *GetVirtualMachine();
        const llvm::TargetData *GetTargetData();
		llvm::Module *GetTargetModule();
		llvm::FunctionPassManager *GetFunctionPassManager();
        llvm::LLVMContext &GetLlvmContext();
        llvm::IRBuilder<> &GetIRBuilder();

        // Managed strings
		llvm::Constant *CompileString(uint32_t id);
		llvm::Constant *CompileString(const std::string &str);

        // C-Strings
        llvm::Constant *CompileCString(uint32_t id);
        llvm::Constant *CompileCString(const std::string &str);

        // External module importing.
        llvm::Constant *ImportGlobal(llvm::GlobalVariable *original);
        llvm::Constant *ImportFunction(llvm::Function *function);

        // Type-info structures.
        llvm::StructType *GetInterfaceImplStruct();

        DebugInformation *GetDebugInformation();

        int GetOptimizationLevel() const;
        void SetOptimizationLevel(int level);

        // Libraries.
        void AddLibrary(const std::string &library);

        // Library paths.
        void AddLibraryPath(const std::string &libPath);
        size_t GetLibraryPathCount() const;
        const std::string &GetLibraryPath(size_t index) const;
        
        // Module type.
        bool IsSharedLibrary() const;
        bool IsStaticLibrary() const;
        bool IsProgram() const;

        // Compilation pipeline
		void Compile();
		void Dump();
        bool WriteNativeAssembler(const std::string &fileName);
		bool WriteBitcode(const std::string &fileName);
        bool WriteObjectFile(const std::string &fileName);
		bool WriteModule(const std::string &fileName);

		int Run(int argc, const char *argv[]);

        // Static constructors.
        void RegisterStaticConstructor(Function *constructor);
        void RegisterAttributeConstant(AttributeConstant *attribute);

        // Runtime modules.
        void SetRuntimeFlag(bool flag);
        bool IsRuntime() const;

        // Reflection support.
        bool HasReflection() const;

        // Generic support.
        Field *FindGenericImplementation(Field *declaration, const GenericInstance *instance);
        Function *FindGenericImplementation(Function *declaration, const GenericInstance *instance);
        Structure *FindGenericImplementation(Structure *declaration, const GenericInstance *instance);
        void RegisterGenericImplementation(Field *declaration, Field *implementation, const GenericInstance *instance);
        void RegisterGenericImplementation(Function *declaration, Function *implementation, const GenericInstance *instance);
        void RegisterGenericImplementation(Structure *declaration, Structure *implementation, const GenericInstance *instance);

        // Reflection support.
        llvm::Constant *GetArrayTypeInfo(const ChelaType *type);
        llvm::Constant *GetInstanceTypeInfo(const TypeInstance *type);
        llvm::Constant *GetPlaceHolderTypeInfo(const ChelaType *type);
        llvm::Constant *GetPointerTypeInfo(const ChelaType *type);
        llvm::Constant *GetReferenceTypeInfo(const ChelaType *type);
        llvm::Constant *GetFunctionTypeInfo(const ChelaType *type);
        llvm::Constant *GetTypeInfo(const ChelaType *type);
        llvm::GlobalVariable *GetAssemblyVariable();
        void RegisterTypeInfo(const std::string &name, llvm::GlobalVariable *typeInfo);

	private:

        // Generic instance name.
        template<typename T>
        struct GenericImplName
        {
            GenericImplName(T *decl, const GenericInstance *args)
                : declaration(decl), arguments(args)
            {
            }

            T *declaration; // Template.
            const GenericInstance *arguments; // Generic parameters.

            bool operator <(const GenericImplName<T> &o) const
            {
                if(declaration == o.declaration)
                    return GenericInstance_LessThan(arguments, o.arguments);
                return declaration < o.declaration;
            }
        };

        typedef std::map<GenericImplName<Field>, Field*> GenericFieldImpls;
        typedef std::map<GenericImplName<Function>, Function*> GenericFunctionImpls;
        typedef std::map<GenericImplName<Structure>, Structure*> GenericStructureImpls;

		typedef std::map<std::string, Member*> MemberNameTable;
		
		friend class VirtualMachine;
		Module(VirtualMachine *virtualMachine, bool noreflection, bool debugging);

        void LoadFromMemory();
		const ChelaType *LoadAnonType(TypeKind kind, uint32_t id);
		
		// Module compilation.
        void PrepareJit();
        void DeclareRuntimeFunctions();
		void DeclarePass();
		void DefinitionPass();
		void BuildTypeInformationStructs();
        bool WriteFile(llvm::raw_ostream &stream, bool assembly);
        bool WriteExportDefinition(llvm::raw_ostream &stream);

        llvm::Constant *EmbedModule();
        void CreateStaticConstructionList();
        void CreateAssemblyData();
        void CreateAssemblyReflectionData(ConstantStructurePtr &value);
        void CreateAssemblyResourceData(ConstantStructurePtr &value);
        void CreateEntryPoint();
        llvm::Function *CreateAttributeConstructor();

		// Current virtual machine.
		VirtualMachine *virtualMachine;
		
		// String table.
		std::vector<std::string> stringTable;
		
		// Member table.
		std::vector<Member*> memberTable;
		MemberNameTable memberNameTable;
		
		// Module references.
		std::vector<Module*> moduleReferences;

        // Raw data.
        size_t moduleSize;
        uint8_t *moduleData;
        llvm::Constant *moduleDataStartPointer;

        // Libraries references.
        std::vector<std::string> libReferences;
        std::vector<std::string> libPaths;
		
		// Anon type table;
		std::vector<AnonTypeCache> anonTypeTable;
		
		// Type table
		std::vector<TypeReferenceCache> typeTable;

        // Resources
        std::vector<ResourceData> resources;
		
		// Module description.
		ModuleName name;
        ModuleType moduleType;
        mutable std::string mangledName;

		// The global namespace.
		Namespace *globalNamespace;
		
		// Code generation.
		llvm::Module *targetModule;
     
        // Intermediate code builder.
        llvm::IRBuilder<> irBuilder;
		
		// Function optimizations.
		llvm::FunctionPassManager *functionPassManager;

        // Type information.
        llvm::StructType *interfaceImplStruct;

        // Assembly data.
        bool noreflection;
        llvm::GlobalVariable *assemblyVariable;

        // Static constructors.
        std::vector<Function*> staticConstructors;

        // Attributes
        std::vector<AttributeConstant*> attributes;

        // Type infos.
        std::vector<std::pair<std::string, llvm::Constant*> > assemblyTypes;

        // Entry point.
        Function *entryPoint;

        // Generic implementations.
        GenericFieldImpls genericFieldImpls;
        GenericFunctionImpls genericFunctionImpls;
        GenericStructureImpls genericStructureImpls;

        // Used by jit.
        bool invokedConstructors;

        // Runtime module.
        bool runtimeFlag;

        // Module dependencies.
        bool declared;
        bool defined;
        bool definingGenericStructures;
        bool definingGenericFunctions;
        bool compiled;

        // Emition flags.
        int optimizationLevel;

        // Debug information.
        DebugInformation *debugInfo;
        bool debugging;
	};

    // Utility to safely have a global IRBuilder<>
    struct IRBeginFunction
    {
        IRBeginFunction(llvm::IRBuilder<> &builder)
            : builder(builder)
        {
            oldBlock = builder.GetInsertBlock();
        }

        ~IRBeginFunction()
        {
            if(oldBlock)
                builder.SetInsertPoint(oldBlock);
            else
                builder.ClearInsertionPoint();
        }

    private:
        llvm::IRBuilder<> &builder;
        llvm::BasicBlock *oldBlock;
    };
}

#endif //_CHELAVM_MODULE_HPP_
