using Chela.Compiler.Module;
namespace Chela.Compiler.Ast
{
	public class TypeNode: Expression
	{
		private TypeKind kind;
        private int numcomponents;
        private int numrows;
        private int numcolumns;
		private IChelaType otherType;
		
		public TypeNode (TypeKind kind, int numcomponents, TokenPosition position)
			: base(position)
		{
			this.kind = kind;
            this.numcomponents = numcomponents;
            this.numrows = 0;
            this.numcolumns = 0;
			this.otherType = null;
		}

        public TypeNode (TypeKind kind, int numrows, int numcolumns, TokenPosition position)
            : base(position)
        {
            this.kind = kind;
            this.numcomponents = 0;
            this.numrows = numrows;
            this.numcolumns = numcolumns;
            this.otherType = null;
        }

        public TypeNode (TypeKind kind, TokenPosition position)
            : this(kind, 1, position)
        {
        }

		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public TypeKind GetKind()
		{
			return kind;
		}

        public int GetNumComponents()
        {
            return this.numcomponents;
        }

        public int GetNumRows()
        {
            return numrows;
        }

        public int GetNumColumns()
        {
            return numcolumns;
        }

		public IChelaType GetOtherType()
		{
			return this.otherType;
		}
		
		public void SetOtherType(IChelaType otherType)
		{
			this.otherType = otherType;
		}
	}
}

