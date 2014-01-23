namespace Chela.Compiler.Ast
{
	public class Expression: Statement
	{
        private int hints;

        public const int TypeHint = 1;
        public const int MemberHint = 2;

		public Expression(TokenPosition position)
			: base(position)
		{
            this.hints = 0;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}

        public virtual bool IsConstant()
        {
            return false;
        }

        public int Hints {
            get {
                return hints;
            }
        }

        public void SetHints(int hints)
        {
            this.hints |= hints;
        }

        public bool HasHints(int hints)
        {
            return (this.hints & hints ) == hints;
        }
	}
}

