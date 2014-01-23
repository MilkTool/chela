using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    public class IncompleteType: ChelaType
    {
        private TypeNameMember[] deps;

        public IncompleteType (params TypeNameMember[] deps)
        {
            this.deps = deps;
        }

        public IncompleteType (List<object> vector)
        {
            // Read the type names.
            List<TypeNameMember> incompletes = new List<TypeNameMember> ();
            foreach(object dep in vector)
            {
                TypeNameMember typeName = dep as TypeNameMember;
                if(typeName != null)
                {
                    incompletes.Add(typeName);
                }
                else
                {
                    IncompleteType inc = (IncompleteType)dep;
                    foreach(TypeNameMember incTypeName in inc.Dependencies)
                        incompletes.Add(incTypeName);
                }
            }

            // Create the dependencies array.
            deps = incompletes.ToArray();
        }

        public override string GetName()
        {
            return "<incomplete type>";
        }

        public TypeNameMember[] Dependencies {
            get {
                return deps;
            }
        }

        public override bool IsIncompleteType ()
        {
            return true;
        }
    }
}

