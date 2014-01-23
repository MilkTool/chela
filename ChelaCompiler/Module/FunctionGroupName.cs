using System;
	
namespace Chela.Compiler.Module
{
    /// <summary>
    /// A function type pair.
    /// </summary>
	public class FunctionGroupName: IEquatable<FunctionGroupName>
	{
		private FunctionType type;
		private bool isStatic;
		private Function function;
        private bool isNamespaceLevel;
			
		public FunctionGroupName(FunctionType type, bool isStatic)
		{
			this.type = type;
			this.isStatic = isStatic;
			this.function = null;
            this.isNamespaceLevel = false;
		}

        public override int GetHashCode()
        {
            return isStatic.GetHashCode() ^
                type.GetHashCode(isStatic ? 0 : 1, true, true);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as FunctionGroupName);
        }

        public bool Equals(FunctionGroupName obj)
        {
            return isStatic == obj.isStatic &&
                type.Equals(obj.type, isStatic ? 0 : 1, true, true);
        }

        /// <summary>
        /// Gets the type of the function.
        /// </summary>
		public FunctionType GetFunctionType()
		{
			return type;
		}
		
        /// <summary>
        /// Tells if this a static function
        /// </summary>
		public bool IsStatic()
		{
			return isStatic;
		}
		
        /// <summary>
        /// Gets the function.
        /// </summary>
		public Function GetFunction()
		{
			return function;
		}
		
        /// <summary>
        /// Sets the function.
        /// </summary>
		public void SetFunction(Function function)
		{
			this.function = function;
		}

        /// <summary>
        /// Tell if this function is defined in namespace scope.
        /// </summary>
        public bool IsNamespaceLevel {
            get {
                return isNamespaceLevel;
            }
            set {
                isNamespaceLevel = value;
            }
        }
	};
}

