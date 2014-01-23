using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class StringConstant: ConstantExpression
	{
		private string value;
		
		public StringConstant (string value, TokenPosition position)
			: base(position)
		{
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public string GetValue()
		{
			return this.value;
		}
	}
}

