using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class BoolConstant: ConstantExpression
	{
		private bool value;
		
		public BoolConstant (bool value, TokenPosition position)
			: base(position)
		{
            SetNodeType(ConstantType.Create(ChelaType.GetBoolType()));
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public bool GetValue()
		{
			return this.value;
		}
	}
}

