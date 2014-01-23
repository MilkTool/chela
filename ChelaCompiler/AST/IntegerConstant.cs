using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class IntegerConstant: ConstantExpression
	{
		private int value;
		
		public IntegerConstant (int value, TokenPosition position)
			: base(position)
		{
			SetNodeType(ConstantType.Create(ChelaType.GetIntType()));
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public int GetValue()
		{
			return this.value;
		}
	}
}

