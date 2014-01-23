namespace Chela.Compiler.Ast
{
	public class ConstantExpression: Expression
	{
		public ConstantExpression (TokenPosition position)
			: base(position)
		{
		}

        public override bool IsConstant ()
        {
            return true;
        }
	}
}

