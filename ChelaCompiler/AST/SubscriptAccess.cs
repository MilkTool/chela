using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class SubscriptAccess: Expression
	{
		private Expression array;
		private Expression index;
        private IChelaType[] indexCoercions;
		
		public SubscriptAccess (Expression array, Expression index, TokenPosition position)
			: base(position)
		{
			this.array = array;
			this.index = index;
            this.indexCoercions = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetArray()
		{
			return this.array;
		}
		
		public Expression GetIndex()
		{
			return this.index;
		}

        public void SetIndexCoercions(IChelaType[] indexCoercions)
        {
            this.indexCoercions = indexCoercions;
        }

        public IChelaType[] GetIndexCoercions()
        {
            return this.indexCoercions;
        }
	}
}

