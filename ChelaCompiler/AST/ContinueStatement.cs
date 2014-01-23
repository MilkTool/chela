namespace Chela.Compiler.Ast
{
	public class ContinueStatement: Statement
	{
		public ContinueStatement (TokenPosition position)
			: base(position)
		{
		}

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }
	}
}

