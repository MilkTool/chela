#ifndef CHELAVM_FUNCTION_LOCAL_HPP
#define CHELAVM_FUNCTION_LOCAL_HPP

#include "ChelaVm/Module.hpp"
#include "llvm/Support/IRBuilder.h"

namespace ChelaVm
{
	class Function;
	class FunctionLocal
	{
	public:
		FunctionLocal(Function *parent);
		~FunctionLocal();
		
		void Read(ModuleReader &reader);
		
		void Declare(llvm::IRBuilder<> &builder);
		
		const ChelaType *GetType();
		llvm::Value	*GetValue();

        bool IsRefCounted();
        void SetRefCounted(bool refCounted);

        FunctionLocal *InstanceGeneric(Function *parent, const GenericInstance *instance);

	private:
		Function *parent;
		Module *module;
		const ChelaType *type;
		llvm::Value *value;
        bool isRefCounted;
	};
};

#endif //CHELAVM_FUNCTION_LOCAL_HPP
