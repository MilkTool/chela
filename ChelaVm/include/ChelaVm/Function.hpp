#ifndef CHELAVM_FUNCTION_HPP
#define CHELAVM_FUNCTION_HPP

#include <set>
#include <vector>
#include "Member.hpp"
#include "Module.hpp"
#include "GenericPrototype.hpp"
#include "llvm/Metadata.h"

namespace ChelaVm
{
    class Structure;
    class FunctionBlock;
    class ExceptionHandler
    {
    public:
        ExceptionHandler()
            : exceptionType(NULL)
        {
        }

        const Class *exceptionType;
        FunctionBlock *handler;
    };

    class ExceptionContext
    {
    public:
        ExceptionContext()
            : parent(NULL), parentId(0), cleanup(NULL), landingPad(NULL), selectionPad(NULL)
        {
        }

        ExceptionContext *parent;
        size_t parentId;
        std::set<FunctionBlock*> blocks;
        std::vector<ExceptionHandler> handlers;
        FunctionBlock *cleanup;
        llvm::BasicBlock *landingPad;
        llvm::BasicBlock *selectionPad;
    };

    class ArgumentData;
    class Closure;
    class FunctionArgument;
    class FunctionLocal;
	class Function: public Member
	{
	public:
        struct Intermediate
        {
            const ChelaType *type;
            llvm::Value *value;
            bool refCounted;
        };

		Function(Module *module);
		~Function();
		
        Module *GetDeclaringModule() const;

		virtual std::string GetName() const;
        virtual std::string GetFullName() const;
        virtual std::string GetMangledName() const;

		virtual MemberFlags GetFlags() const;
		virtual int GetVSlot() const;
		const ChelaType *GetType() const;
        const FunctionType *GetFunctionType() const;

		virtual bool IsFunction() const;
		
		virtual void DeclarePass();
		virtual void DefinitionPass();

        void AddDependency(Function *dependency);
        void BuildDependencies();

		llvm::Function *GetTarget();
        llvm::Constant *ImportFunction(llvm::Module *targetModule);
        llvm::Constant *ImportFunction(Module *targetModule);
		
		FunctionArgument *GetArgument(uint32_t id);

        size_t GetLocalCount() const;
		FunctionLocal *GetLocal(uint32_t id);

		FunctionBlock *GetBlock(uint32_t id);

        llvm::Value *CreateIntermediate(const ChelaType *type, bool refCounted);
        size_t GetIntermediateCount() const;
        const Intermediate &GetIntermediate(size_t index) const;

        static Function *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
		void Read(ModuleReader &reader, const MemberHeader &header);

        bool IsHandlingExceptions() const;

        llvm::BasicBlock *GetTopLandingPad() const;
        llvm::BasicBlock *GetTopCleanup() const;
        llvm::Value *GetExceptionCaught() const;
        llvm::Value *GetExceptionLocal() const;

        llvm::Value *GetReturnPointer() const;

        llvm::DILexicalBlock GetTopLexicalScope() const;

        void PerformCleanup();

        // Member information.
        virtual llvm::GlobalVariable *GetMemberInfo();

        // Generic support.
        const GenericPrototype *GetGenericPrototype() const;
        virtual const GenericInstance *GetGenericInstanceData() const;
        virtual Member *GetTemplateMember() const;
        virtual bool IsGeneric() const;

        Function *InstanceGeneric(Member *factory, Module *implementingModule, const GenericInstance *instance);
        Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);

        // Debug information.
        virtual llvm::DIDescriptor GetDebugNode(DebugInformation *context) const;

        // Kernel support.
        const FunctionType *GetKernelBinderType();
        llvm::Function *GetKernelBinder();

	private:
        void InstanceExceptionContext(Function *newFunction, ExceptionContext &context);
        void ImplementLandingPath(ExceptionContext &context);
        void ImplementTopLandingPath();
        void DefineSpecialFunction();
        void DefineSpecialConstructor();
        void DefineDelegateInvoke();
        void DeclareDebugVariables();
        void CreateMemberInfo();

        void DeclareKernelSupport();
        void CreateKernelSupport();
        void CreateKernelBinder();
        void CreateKernelThread();

        // Function identification.
        Module *declaringModule;
        const FunctionType *functionType;
		std::string name;

        // Function members
        std::vector<ArgumentData*> argumentData;
		std::vector<FunctionArgument*> arguments;
		std::vector<FunctionLocal*> locals;
		std::vector<FunctionBlock*> blocks;
        std::vector<Intermediate> intermediates;
        std::vector<ExceptionContext> exceptions;
        std::set<Function*> dependencies;

        // Generic prototype.
        GenericPrototype genericPrototype;
        const GenericInstance *genericInstance;
        Function *templateFunction;

        // llvm function.
		llvm::Function *targetFunction;

        // Variable declarations
        llvm::BasicBlock *declarations;
        llvm::BasicBlock *initializations;

        // Exception handling
        llvm::BasicBlock *topLandingPad;
        llvm::BasicBlock *topCleanup;
        llvm::Value *exceptionStructLocal;
        llvm::Value *exceptionLocal;
        llvm::Value *selectedExceptionLocal;
        llvm::Value *exceptionCaught;

        // Return pointer.
        llvm::Value *returnPointer;

        // Debug information.
        llvm::DISubprogram subprogramScope;
        llvm::DILexicalBlock topLexicalScope;

        // Member info
        llvm::GlobalVariable *methodInfo;

        // Kernel supports.
        const FunctionType *kernelBinderType;
        const FunctionType *kernelThreadType;
        Closure *kernelClosure;
        llvm::Function *kernelBinder;
        llvm::Function *kernelThread;

        // Flags.
        MemberFlags flags;
        int32_t vslot;
		bool defined;
        bool handlingExceptions;
	};
};

#endif //CHELAVM_FUNCTION_HPP
