using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Matrix type.
    /// </summary>
    public class MatrixType: ChelaType, IEquatable<MatrixType>
    {
        private static SimpleSet<MatrixType> matrixTypes = new SimpleSet<MatrixType> ();
        
        private IChelaType primitiveType;
        private int numrows;
        private int numcolumns;
        private string name;

        internal MatrixType (IChelaType primitiveType, int numrows, int numcolumns)
        {
            this.primitiveType = primitiveType;
            this.numrows = numrows;
            this.numcolumns = numcolumns;
            this.name = null;
        }

        public override bool IsMatrix ()
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
            return (uint)(primitiveType.GetSize()*numrows*numcolumns);
        }

        public override uint GetComponentSize ()
        {
            return primitiveType.GetComponentSize();
        }

        /// <summary>
        /// Gets the type of the elements in the matrix..
        /// </summary>
        public IChelaType GetPrimitiveType()
        {
            return this.primitiveType;
        }

        /// <summary>
        /// Gets the number rows.
        /// </summary>
        public int GetNumRows()
        {
            return this.numrows;
        }

        /// <summary>
        /// Gets the number columns.
        /// </summary>
        public int GetNumColumns()
        {
            return this.numcolumns;
        }

        public override int GetHashCode()
        {
            return numcolumns ^ numrows ^
                   RuntimeHelpers.GetHashCode(primitiveType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals((MatrixType)obj);
        }

        public bool Equals(MatrixType obj)
        {
            return obj != null &&
                   numcolumns == obj.numcolumns &&
                   numrows == obj.numrows &&
                   primitiveType == obj.primitiveType;
        }


        /// <summary>
        /// Gets the matrix type short name.
        /// </summary>
        public override string GetName ()
        {
            if(name == null)
                name = primitiveType.GetName() + "*" + numrows + "x" + numcolumns;

            return name;
        }

        /// <summary>
        /// Create a matrix type.
        /// </summary>
        public static IChelaType Create(IChelaType primitiveType, int numrows, int numcolumns)
        {
            if(numrows == 1)
                return VectorType.Create(primitiveType, numcolumns);
            else if(numcolumns == 1)
                return VectorType.Create(primitiveType, numrows);
            else if(numrows < 1 || numcolumns < 1)
                throw new ModuleException("invalid vector number of components.");

            // First create a new matrix type.
            MatrixType matrix = new MatrixType(primitiveType, numrows, numcolumns);
            return matrixTypes.GetOrAdd(matrix);
        }
    }
}

