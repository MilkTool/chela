namespace Chela.Compiler.Ast
{
	public class MakePointer: Expression
	{
		private Expression pointedType;
		
		public MakePointer (Expression pointedType, TokenPosition position)
			: base(position)
		{
			this.pointedType = pointedType;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetPointedType()
		{
			return pointedType;
		}
	}
}

