#ifndef CHELAVM_FUNCTION_BLOCK_HPP
#define CHELAVM_FUNCTION_BLOCK_HPP

#include "llvm/ADT/SmallSet.h"

#include "ChelaVm/Module.hpp"
#include "ChelaVm/Function.hpp"

namespace ChelaVm
{
    class ExceptionContext;
    class BasicBlockDebugInfo;
    class BasicBlockRangePosition;
    class Class;
    class Field;
	class FunctionBlock
	{
	public:
		struct StackValue
		{
			StackValue(){}
			StackValue(llvm::Value *value, const ChelaType *type)
				: value(value), type(type) {}
				
			llvm::Value *value;
			const ChelaType *type;
		};
		
		typedef llvm::SmallPtrSet<FunctionBlock*,2> BlockSet;
		typedef llvm::SmallVector<StackValue,5> Stack;
		
		FunctionBlock(Function *parent, size_t blockId);
		~FunctionBlock();

        // VM, context.
        VirtualMachine *GetVM() const
        {
            return parent->GetVM();
        }

        llvm::LLVMContext &GetLlvmContext() const
        {
            return parent->GetLlvmContext();
        }
		
		void Read(ModuleReader &reader);

        size_t GetBlockId() const;

        size_t GetBlockOffset() const;
		size_t GetRawBlockSize() const;
		const uint8_t *GetInstructions();
        bool IsUnsafe() const;
		
		llvm::BasicBlock *GetBlock();
        llvm::BasicBlock *GetLastBlock();

        ExceptionContext *GetExceptionContext() const;
        void SetExceptionContext(ExceptionContext *context);

        bool IsCatchBlock() const;
        void SetCatchBlock(bool isCatch);

        uint32_t GetExceptionType() const;
        void SetExceptionType(uint32_t exceptionType);

		void Declare(llvm::IRBuilder<> &builder);
		void Define(llvm::IRBuilder<> &builder);
        void Finish(llvm::IRBuilder<> &builder);
		void CalculateDependencies();

        FunctionBlock *InstanceGeneric(Function *parent, const GenericInstance *instance);

	private:
        // Sub block creation.
        llvm::BasicBlock *CreateBlock(const std::string &name);

        // Type instancing.
        const ChelaType *GetType(uint32_t typeId);
        const ChelaType *InstanceType(const ChelaType *type);
        Member *GetMember(uint32_t memberId);
        Function *GetFunction(uint32_t functionId);
        Field *GetField(uint32_t fieldId);

        // Type reading.
        const ChelaType *ParseIntTarget(int n);
        const ChelaType *ParseFloatTarget(int n);

        // Function cleaning.
        void Return(llvm::IRBuilder<> &builder, llvm::Value *retValue);

        // Reference counting.
        void AddRef(llvm::IRBuilder<> &builder, llvm::Value *value);
        void AddRef(llvm::IRBuilder<> &builder, llvm::Value *value, const ChelaType *type);
        void Release(llvm::IRBuilder<> &builder, llvm::Value *value);
        void Release(llvm::IRBuilder<> &builder, llvm::Value *value, const ChelaType *type);
        void RefSet(llvm::IRBuilder<> &builder, llvm::Value *value, llvm::Value *variable, const ChelaType *type);
        void RefSetInter(llvm::IRBuilder<> &builder, llvm::Value *value, llvm::Value *variable, const ChelaType *type);
        void ImplicitDeRef(llvm::IRBuilder<> &builder, StackValue &stackValue);

        // Function calling.
        llvm::Value *CreateCall(llvm::IRBuilder<> &builder, llvm::Value *calledFunction,
                const FunctionType *calledType, int n,
                llvm::Value *retPtr = NULL,
                llvm::Value *first=NULL, const ChelaType *firstType=NULL);

        // Safety checks
        void CreateNullCheck(llvm::IRBuilder<> &builder, llvm::Value *value);
        void CreateBoundCheck(llvm::IRBuilder<> &builder, llvm::Value *array, llvm::Value *index);
        void CreateCheckCast(llvm::IRBuilder<> &builder, llvm::Value *original,
                             llvm::Value *casted, llvm::Value *typeinfo);

        // Type checking.
        void CheckEqualType(int op, const ChelaType *a, const ChelaType *b);
        void CheckCompatibleType(int op, const ChelaType *a, const ChelaType *b);
        void CheckDelegateCompatible(Function *invokeFunction, Function *delegatedFunction);
        void CheckCompatible(const ChelaType *source, const ChelaType *dest);
        Field *LoadFieldDescription(Function *context, const ChelaType *type, uint32_t fieldId, const Structure *&building);

        // Low-level coercion.
        llvm::Value *ExtractPrimitive(llvm::IRBuilder<> &builder, llvm::Value *value, Field *field);
        llvm::Value *MakeCompatible(llvm::IRBuilder<> &builder, llvm::Value *value,
                                    const ChelaType *source, const ChelaType *dest);

        // Generation helpers.
        llvm::Value *DoCompare(llvm::IRBuilder<> &builder, int opcode,
           const FunctionBlock::StackValue &a1, const FunctionBlock::StackValue &a2);
        llvm::Value *Box(llvm::IRBuilder<> &builder, llvm::Value *unboxedValue,
                         const ChelaType *unboxedType, const Structure *building);
        llvm::Value *Unbox(llvm::IRBuilder<> &builder, llvm::Value *boxedValue,
                         const ChelaType *boxedType, const Structure *boxType);
        void CastInstruction(llvm::IRBuilder<> &builder, llvm::Value *arg, const ChelaType *argType, const ChelaType *targetType, bool generic=false);

        // Safety checks.
        void Error(const std::string &message);
        void Unsafe(int op=0);

        // Debug information.
        void PrepareDebugEmition();
        void BeginDebugInstruction(size_t index);
        llvm::Value *AttachPosition(llvm::Value *value);

        // Managed array
        void LoadArrayIndices(llvm::IRBuilder<> &builder, const ChelaType *arrayType, std::vector<llvm::Value*> &indices);
        llvm::Value *GetArrayElement(llvm::IRBuilder<> &builder,
            llvm::Value *arrayRef, const ChelaType *arrayRefType,
            const std::vector<llvm::Value*> &indices,
            const ChelaType **destElementType = NULL);

        // Operand stack
        void Push(const StackValue &value);
        void Push(llvm::Value *value, const ChelaType *type);
		StackValue Pop();

        // Block data.
		Function *parent;
		Module *module;
        Module *declaringModule;
        size_t blockDataOffset;
		const uint8_t *instructions;
        size_t blockId;
        uint16_t rawblockSize;
        uint16_t instructionCount;

        // Some boolean flags.
        bool generated;
        bool checked;
        bool unsafeBlock;
        bool isCatchBlock;
        bool finishCleanup;

        // Sub blocks.
        std::vector<llvm::BasicBlock*> basicBlocks;

		// Code generation analysis.
		BlockSet predecessors;
		BlockSet successor;
		Stack stack;

        // Exception handling.
        ExceptionContext *exceptionContext;
        uint32_t exceptionType;

        // Cleanup.
        llvm::Value *returnValue;

        // Debug information.
        BasicBlockDebugInfo *blockDebugInfo;
        const BasicBlockRangePosition *currentDebugRange;
	};
};

#endif //CHELAVM_FUNCTION_BLOCK_HPP
