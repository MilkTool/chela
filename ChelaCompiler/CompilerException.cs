using System;
namespace Chela.Compiler
{
	public class CompilerException: ApplicationException
	{
		private TokenPosition position;
		
		public CompilerException (string what, TokenPosition position)
			: base(position.ToString() + ": " + what)
		{
			this.position = position;
		}
		
		public TokenPosition GetPosition()
		{
			return position;
		}
	}
}

