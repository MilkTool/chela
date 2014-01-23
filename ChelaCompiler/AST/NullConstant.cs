using Chela.Compiler.Module;
	
namespace Chela.Compiler.Ast
{
	public class NullConstant: ConstantExpression
	{
		public NullConstant (TokenPosition position)
			: base(position)
		{
			SetNodeType(ConstantType.Create(ChelaType.GetNullType()));
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}

