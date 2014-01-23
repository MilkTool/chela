using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class DoubleConstant: ConstantExpression
	{
		private double value;
		
		public DoubleConstant (double value, TokenPosition position)
			: base(position)
		{
			SetNodeType(ConstantType.Create(ChelaType.GetDoubleType()));
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public double GetValue()
		{
			return this.value;
		}
	}
}

