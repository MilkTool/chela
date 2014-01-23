using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class UIntegerConstant: ConstantExpression
	{
		private uint value;
		
		public UIntegerConstant (uint value, TokenPosition position)
			: base(position)
		{
			SetNodeType(ConstantType.Create(ChelaType.GetUIntType()));
			this.value = value;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public uint GetValue()
		{
			return this.value;
		}
	}
}

