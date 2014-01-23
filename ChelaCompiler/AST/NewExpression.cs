using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class NewExpression: Expression
	{
		private Expression typeExpression;
		private Expression arguments;
		private Method constructor;
		private IChelaType objectType;
			
		public NewExpression (Expression typeExpression, Expression arguments, TokenPosition position)
			: base(position)
		{
			this.typeExpression = typeExpression;
			this.arguments = arguments;
			this.constructor = null;
			this.objectType = null;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetTypeExpression()
		{
			return typeExpression;
		}
		
		public Expression GetArguments()
		{
			return arguments;
		}
		
		public Method GetConstructor()
		{
			return constructor;
		}
		
		public void SetConstructor(Method method)
		{
			this.constructor = method;
		}
		
		public IChelaType GetObjectType()
		{
			return this.objectType;
		}
		
		public void SetObjectType(IChelaType objectType)
		{
			this.objectType = objectType;
		}
	}
}

