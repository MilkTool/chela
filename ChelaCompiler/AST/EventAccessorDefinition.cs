using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class EventAccessorDefinition: ScopeNode
    {
        private MemberFlags flags;
        private EventVariable eventVariable;
        private Function function;
        private LocalVariable selfLocal;
        private LocalVariable valueLocal;

        public EventAccessorDefinition (MemberFlags flags, string name, AstNode children, TokenPosition position)
            : base(children, position)
        {
            SetName(name);
            this.flags = flags;
            this.eventVariable = null;
            this.function = null;
            this.selfLocal = null;
            this.valueLocal = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public EventVariable GetEvent()
        {
            return eventVariable;
        }

        public void SetEvent(EventVariable eventVariable)
        {
            this.eventVariable = eventVariable;
        }

        public Function GetFunction()
        {
            return function;
        }

        public void SetFunction(Function function)
        {
            this.function = function;
        }

        public LocalVariable GetSelfLocal()
        {
            return selfLocal;
        }

        public void SetSelfLocal(LocalVariable local)
        {
            selfLocal = local;
        }

        public LocalVariable GetValueLocal()
        {
            return valueLocal;
        }

        public void SetValueLocal(LocalVariable local)
        {
            valueLocal = local;
        }
    }
}

