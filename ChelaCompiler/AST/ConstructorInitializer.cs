using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class ConstructorInitializer: Expression
    {
        private bool baseCall;
        private AstNode arguments;
        private FunctionGroup ctorGroup;
        private Function constructor;

        public ConstructorInitializer (bool baseCall, AstNode arguments, TokenPosition position)
            : base(position)
        {
            this.baseCall = baseCall;
            this.arguments = arguments;
            this.ctorGroup = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public bool IsBaseCall()
        {
            return baseCall;
        }

        public AstNode GetArguments()
        {
            return arguments;
        }

        public FunctionGroup GetConstructorGroup()
        {
            return ctorGroup;
        }

        public void SetConstructorGroup(FunctionGroup ctorGroup)
        {
            this.ctorGroup = ctorGroup;
        }

        public Function GetConstructor()
        {
            return this.constructor;
        }

        public void SetConstructor(Function constructor)
        {
            this.constructor = constructor;
        }
    }
}

