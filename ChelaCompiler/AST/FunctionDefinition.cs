using System.Collections.Generic;
using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class FunctionDefinition: ScopeNode
	{
		private FunctionPrototype prototype;
		private Function function;
        private bool isGenerator;
        private bool hasReturns;
        private bool isEnumerable;
        private bool isGenericIterator;
        private IChelaType yieldType;
        private Class generatorClass;
        private Structure generatorClassInstance;
        private FieldVariable yieldedValue;
        private FieldVariable generatorState;
        private FieldVariable generatorSelf;
        private List<ReturnStatement> yields;
        private List<ArgumentVariable> argVariables;
		
		public FunctionDefinition (FunctionPrototype prototype, AstNode children, TokenPosition position)
			: base(children, position)
		{
			this.prototype = prototype;
			this.function = null;
            this.isGenerator = false;
            this.hasReturns = false;
            this.yields = new List<ReturnStatement> ();
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public FunctionPrototype GetPrototype()
		{
			return prototype;
		}
		
		public Function GetFunction()
		{
			return this.function;
		}
		
		public void SetFunction(Function function)
		{
			this.function = function;
		}

        public void MakeGenerator()
        {
            isGenerator = true;
        }

        public bool IsGenerator {
            get {
                return isGenerator;
            }
        }

        public bool HasReturns {
            get {
                return hasReturns;
            }
            set {
                hasReturns = value;
            }
        }

        public bool IsEnumerable {
            get {
                return isEnumerable;
            }
            set {
                isEnumerable = value;
            }
        }

        public bool IsGenericIterator {
            get {
                return isGenericIterator;
            }
            set {
                isGenericIterator = value;
            }
        }

        public IChelaType YieldType {
            get {
                return yieldType;
            }
            set {
                yieldType = value;
            }
        }

        public Class GeneratorClass {
            get {
                return generatorClass;
            }
            set {
                this.generatorClass = value;
            }
        }

        public Structure GeneratorClassInstance {
            get {
                return generatorClassInstance;
            }
            set {
                this.generatorClassInstance = value;
            }
        }

        public FieldVariable YieldedValue {
            get {
                return yieldedValue;
            }
            set {
                this.yieldedValue = value;
            }
        }

        public FieldVariable GeneratorState {
            get {
                return generatorState;
            }
            set {
                generatorState = value;
            }
        }

        public FieldVariable GeneratorSelf {
            get {
                return generatorSelf;
            }
            set {
                generatorSelf = value;
            }
        }

        public List<ReturnStatement> Yields {
            get {
                if(yields == null)
                    yields = new List<ReturnStatement>();
                return yields;
            }
        }

        public List<ArgumentVariable> ArgumentVariables {
            get {
                if(argVariables == null)
                    argVariables = new List<ArgumentVariable>();
                return argVariables;
            }
        }

	}
}

