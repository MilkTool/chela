using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class AstNode
	{
		private TokenPosition position;
        private AstNode prev;
		private AstNode next;
        private AstNode attributes;
		private string name;
		private ChelaModule module;
		private IChelaType nodeType;
		private IChelaType coercionType;
		private object nodeValue;
		
		public AstNode (TokenPosition position)
		{
			this.position = position;
            this.prev = this;
			this.next = null;
            this.attributes = null;
			this.name = null;
			this.module = null;
			this.nodeType = null;
			this.coercionType = null;
			this.nodeValue = null;
		}
		
		public TokenPosition GetPosition()
		{
			return position;
		}
		
		public string GetName()
		{
			return name;
		}
		
		public void SetName(string name)
		{
			this.name = name;
		}
		
		public IChelaType GetNodeType()
		{
			return this.nodeType;
		}
		
		public void SetNodeType(IChelaType nodeType)
		{
			this.nodeType = nodeType;
		}
		
		public IChelaType GetCoercionType()
		{
			return this.coercionType;
		}
		
		public void SetCoercionType(IChelaType coercionType)
		{
			this.coercionType = coercionType;
		}
		
		public object GetNodeValue()
		{
			return this.nodeValue;
		}
		
		public void SetNodeValue(object value)
		{
			this.nodeValue = value;
		}
		
		public ChelaModule GetModule()
		{
			return this.module;
		}
		
		public void SetModule(ChelaModule module)
		{
			this.module = module;
		}

        public AstNode GetPrevCircular()
        {
            return prev;
        }

		public AstNode GetNext()
		{
			return next;
		}

        public void SetNext(AstNode next)
        {
            this.next = next;
        }
		
		public void SetNextCircular(AstNode next)
		{
            // Break next linking
            if(this.next != null)
            {
                AstNode tail = this.next;
                while(tail.next != null)
                    tail = tail.next;
                this.next.prev = tail;
            }

            // Make my previous point to the end.
            if(this.prev == this)
                this.prev = next.prev;

            // Set next linkings.
			this.next = next;
            next.prev = this;
		}

        public AstNode GetAttributes()
        {
            return this.attributes;
        }

        public void SetAttributes(AstNode attributes)
        {
            this.attributes = attributes;
        }
		
		public void Error(string message)
		{
			throw new CompilerException(message, position);
		}
		
		public virtual AstNode Accept(AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

