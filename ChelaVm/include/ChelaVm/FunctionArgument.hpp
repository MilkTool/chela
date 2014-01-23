#ifndef CHELAVM_FUNCTION_ARGUMENT_HPP
#define CHELAVM_FUNCTION_ARGUMENT_HPP

#include "ChelaVm/Module.hpp"
#include "llvm/Support/IRBuilder.h"

namespace ChelaVm
{
	class Function;
    class ArgumentData;
	class FunctionArgument
	{
	public:
		FunctionArgument(Function *parent, const ChelaType *type, llvm::Value *value,
                         ArgumentData *argumentData, size_t index);
		~FunctionArgument();
		
		Function *GetParent();
		const ChelaType *GetType();
		llvm::Value	*GetValue();

        llvm::Constant *CreateParameterInfo();

	private:
		Function *parent;
		const ChelaType * type;
		llvm::Value *value;
        ArgumentData *argumentData;
        size_t index;
	};
};

#endif //CHELAVM_FUNCTION_ARGUMENT_HPP
