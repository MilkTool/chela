using System.Collections.Generic;
using System.Text;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// This is used to detect ambiguous function calls.
    /// </summary>
    public class FunctionAmbiguity: Function
    {
        private List<Function> candidates;

        /// <summary>
        /// Constructs an ambiguous function description.
        /// </summary>
        public FunctionAmbiguity(string name, MemberFlags flags, Scope parentScope)
            : base(name, flags, parentScope)
        {
            candidates = new List<Function> ();
        }

        /// <summary>
        /// Adds an ambiguous function candidate
        /// </summary>
        public void AddCandidate(Function candidate)
        {
            if(candidate != null)
                candidates.Add(candidate);
        }

        /// <summary>
        /// Notifies of ambiguity
        /// </summary>
        public override bool IsAmbiguity()
        {
            return true;
        }

        /// <summary>
        /// Raises ambiguity error.
        /// </summary>
        public override void CheckAmbiguity(TokenPosition where)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Ambiguous function for '");
            builder.Append(name);
            builder.Append("', candidates are:\n");
            foreach(Function candidate in candidates)
            {
                builder.Append("    ");
                builder.Append(candidate.GetFullName());
            }
            throw new CompilerException(builder.ToString(), where);
        }
    }
}

