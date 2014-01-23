using System.Collections.Generic;
using System.Text;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Ambiguous structure.
    /// </summary>
    public class AmbiguousStructure: Structure
    {
        private List<Structure> candidates;

        public AmbiguousStructure(string name, MemberFlags flags, Scope parentScope)
            : base(name, flags, parentScope)
        {
            this.candidates = new List<Structure> ();
        }

        public void AddCandidate(Structure candidate)
        {
            candidates.Add(candidate);
        }

        public override bool IsAmbiguity()
        {
            return true;
        }

        public override void CheckAmbiguity(TokenPosition where)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Ambiguous structure type for '");
            builder.Append(name);
            builder.Append("', candidates are:\n");
            foreach(Structure candidate in candidates)
            {
                builder.Append("    ");
                builder.Append(candidate.GetFullName());
            }
            throw new CompilerException(builder.ToString(), where);
        }
    }
}

