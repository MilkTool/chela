using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Chela.Compiler
{
    public class ReferenceEqualityComparer<T>: IEqualityComparer<T>
    {
        public ReferenceEqualityComparer()
        {
        }

        public bool Equals(T x, T y)
        {
            return RuntimeHelpers.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

