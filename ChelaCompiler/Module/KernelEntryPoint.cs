
namespace Chela.Compiler.Module
{
    public class KernelEntryPoint: Function
    {
        private Function kernelFunction;

        public KernelEntryPoint(Function kernelFunction, FunctionType entryPointType)
            : base(kernelFunction.GetModule())
        {
            SetFunctionType(entryPointType);
            this.flags = MemberFlags.Static;
            this.kernelFunction = kernelFunction;
        }

        public override bool IsKernelEntryPoint()
        {
            return true;
        }

        public Function Kernel {
            get {
                return kernelFunction;
            }
        }
    }
}

