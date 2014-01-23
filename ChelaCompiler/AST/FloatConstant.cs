using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class FloatConstant: ConstantExpression
	{
		private float value;
		
		public FloatConstant (float value, TokenPosition position)
			: base(position)
		{
			SetNodeType(ConstantType.Create(ChelaType.GetFloatType()));
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public float GetValue()
		{
			return this.value;
		}
	}
}

