using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Vector type.
    /// </summary>
    public class VectorType: ChelaType, IEquatable<VectorType>
    {
        private static SimpleSet<VectorType> vectorTypes = new SimpleSet<VectorType> ();        
        private IChelaType primitiveType;
        private int numcomponents;
        private string name;

        internal VectorType (IChelaType primitiveType, int numcomponents)
        {
            this.primitiveType = primitiveType;
            this.numcomponents = numcomponents;
            this.name = null;
        }

        public override bool IsVector ()
        {
            return true;
        }

        public override bool IsNumber()
        {
            return primitiveType.IsNumber();
        }

        public override bool IsInteger ()
        {
            return primitiveType.IsInteger();
        }

        public override bool IsFloatingPoint ()
        {
            return primitiveType.IsFloatingPoint();
        }

        public override uint GetSize ()
        {
            return (uint)(primitiveType.GetSize()*numcomponents);
        }

        public override uint GetComponentSize ()
        {
            return primitiveType.GetComponentSize();
        }

        public IChelaType GetPrimitiveType()
        {
            return this.primitiveType;
        }
        
        public int GetNumComponents()
        {
            return this.numcomponents;
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        public override string GetName ()
        {
            if(name == null)
            {
                name = primitiveType.GetName() + "*" + numcomponents;
            }
            
            return name;
        }

        public override int GetHashCode()
        {
            return numcomponents ^ RuntimeHelpers.GetHashCode(primitiveType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as VectorType);
        }

        public bool Equals(VectorType obj)
        {
            return obj != null && 
                primitiveType == obj.primitiveType  &&
                    numcomponents == obj.numcomponents;
        }

        public static IChelaType Create(IChelaType primitiveType, int numcomponents)
        {
            if(numcomponents == 1)
                return primitiveType;
            else if(numcomponents < 1)
                throw new ModuleException("invalid vector number of components.");

            // Create a new vector type.
            VectorType vector = new VectorType(primitiveType, numcomponents);
            return vectorTypes.GetOrAdd(vector);
        }
    }
}

