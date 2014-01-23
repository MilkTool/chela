using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Array type.
    /// </summary>
    public class ArrayType: ChelaType, IEquatable<ArrayType>
    {
        private static SimpleSet<ArrayType> arrayTypes = new SimpleSet<ArrayType> ();
        private IChelaType valueType;
        private string name;
        private string displayName;
        private string fullName;
        private int dimensions;
        private bool readOnly;

        internal ArrayType (IChelaType valueType, int dimensions, bool readOnly)
        {
            this.valueType = valueType;
            this.name = null;
            this.fullName = null;
            this.displayName = null;
            this.dimensions = dimensions;
            this.readOnly = readOnly;
        }

        /// <summary>
        /// Array type short name.
        /// </summary>
        public override string GetName ()
        {
            if(name == null)
            {
                name = valueType.GetName() + "[" + dimensions + "]";
                if(readOnly)
                    name = "readonly " + name;
            }
            return name;
        }

        /// <summary>
        /// Array type full name.
        /// </summary>
        public override string GetFullName ()
        {
            if(fullName == null)
            {
                fullName = valueType.GetFullName() + "[" + dimensions + "]";
                if(readOnly)
                    fullName = "readonly " + fullName;
            }
            return fullName;
        }

        /// <summary>
        /// Array type display name.
        /// </summary>
        public override string GetDisplayName()
        {
            if(displayName == null)
            {
                if(readOnly)
                    displayName = "readonly ";
                displayName += valueType.GetDisplayName() + "[";
                for(int i = 1; i < dimensions; ++i)
                    displayName += ",";
                displayName += "]";
            }
            return displayName;
        }
        
        public override bool IsArray ()
        {
            return true;
        }

        public override bool IsPassedByReference ()
        {
            return true;
        }

        public override bool IsReadOnly()
        {
            return readOnly;
        }

        /// <summary>
        /// Array dimensions.
        /// </summary>
        public int GetDimensions()
        {
            return dimensions;
        }

        /// <summary>
        /// The type of each element in the array
        /// </summary>
        public IChelaType GetValueType()
        {
            return valueType;
        }

        public override int GetHashCode()
        {
            return dimensions ^ readOnly.GetHashCode() ^
                    RuntimeHelpers.GetHashCode(valueType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting,
            if(obj == this)
                return true;
            return Equals(obj as ArrayType);
        }

        public bool Equals(ArrayType obj)
        {
            return obj != null &&
                   dimensions == obj.dimensions &&
                   readOnly == obj.readOnly &&
                   valueType == obj.valueType;
        }

        public override IChelaType InstanceGeneric (GenericInstance args, ChelaModule instModule)
        {
            return Create(valueType.InstanceGeneric(args, instModule), dimensions, readOnly);
        }

        /// <summary>
        /// Create the specified array type.
        /// </summary>
        public static ArrayType Create(IChelaType valueType, int dimensions, bool readOnly)
        {
            // Remove the reference layer.
            if(valueType.IsReference())
            {
                ReferenceType refType = (ReferenceType)valueType;
                valueType = refType.GetReferencedType();
            }

            // Don't allow 0 dimensions.
            if(dimensions == 0)
                dimensions = 1;

            // Create the array type.
            ArrayType array = new ArrayType(valueType, dimensions, readOnly);
            return arrayTypes.GetOrAdd(array);
        }

        /// <summary>
        /// Create a writable single dimension array type with the specified
        /// element type.
        /// </summary>
        public static ArrayType Create(IChelaType valueType)
        {
            return Create(valueType, 1, false);
        }
    }
}
