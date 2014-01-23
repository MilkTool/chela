using System;

namespace Chela.Compiler
{
    public class BugException: ApplicationException
    {
        public BugException(string message)
            : base(message)
        {
        }
    }
}

