using System;
namespace Chela.Compiler.Module
{
	public class ModuleException: ApplicationException
	{
		public ModuleException (string what)
			: base(what)
		{
		}
	}
}

