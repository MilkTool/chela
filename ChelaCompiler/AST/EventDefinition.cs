using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class EventDefinition: AstNode
    {
        private MemberFlags flags;
        private Expression eventType;
        private EventVariable eventVariable;
        private EventAccessorDefinition addAccessor;
        private EventAccessorDefinition removeAccessor;
        private AstNode accessors;
        private Class delegateType;

        public EventDefinition (MemberFlags flags, Expression eventType,
                                string name, AstNode accessors, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.flags = flags;
            this.eventType = eventType;
            this.eventVariable = null;
            this.accessors = accessors;
            this.delegateType = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public void SetFlags(MemberFlags flags)
        {
            this.flags = flags;
        }

        public Expression GetEventType()
        {
            return eventType;
        }

        public EventVariable GetEvent()
        {
            return eventVariable;
        }

        public void SetEvent(EventVariable eventVariable)
        {
            this.eventVariable = eventVariable;
        }

        public AstNode GetAccessors()
        {
            return accessors;
        }

        public Class GetDelegateType()
        {
            return delegateType;
        }

        public void SetDelegateType(Class delegateType)
        {
            this.delegateType = delegateType;
        }

        public EventAccessorDefinition AddAccessor {
            get {
                return addAccessor;
            }
            set {
                addAccessor = value;
            }
        }

        public EventAccessorDefinition RemoveAccessor {
            get {
                return removeAccessor;
            }
            set {
                removeAccessor = value;
            }
        }
    }
}

