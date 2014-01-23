using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class CallExpression: Expression
	{
		private Expression function;
		private Expression arguments;
		private bool hasImplicitArgument;
        private bool vectorConstruction;
		private int vslot;
		private Function selectedFunction;
		
		public CallExpression (Expression function, Expression arguments, TokenPosition position)
			: base(position)			
		{
			this.function = function;
			this.arguments = arguments;
			this.hasImplicitArgument = false;
            this.vectorConstruction = false;
			this.vslot = -1;
			this.selectedFunction = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetFunction()
		{
			return this.function;
		}
		
		public Expression GetArguments()
		{
			return this.arguments;
		}
		
		public bool HasImplicitArgument()
		{
			return this.hasImplicitArgument;
		}
		
		public void SetImplicitArgument(bool value)
		{
			this.hasImplicitArgument = value;
		}

        public bool IsVectorConstruction()
        {
            return vectorConstruction;
        }

        public void SetVectorConstruction(bool value)
        {
            this.vectorConstruction = value;
        }

		public int GetVSlot()
		{
			return this.vslot;
		}
		
		public void SetVSlot(int vslot)
		{
			this.vslot = vslot;
		}
		
		public void SelectFunction(Function function)
		{
			this.selectedFunction = function;
		}
		
		public Function GetSelectedFunction()
		{
			return this.selectedFunction;
		}
	}
}

