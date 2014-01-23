using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Pointer type.
    /// </summary>
    public class PointerType: ChelaType, IEquatable<PointerType>
    {
        private static SimpleSet<PointerType> pointerTypes = new SimpleSet<PointerType> ();
        private IChelaType pointedType;
        private string name;
        private string fullName;
        private string displayName;
        
        internal PointerType (IChelaType actualType)
        {
            this.pointedType = actualType;
            this.name = null;
            this.displayName = null;
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        public override string GetName ()
        {
            if(name == null)
                name = pointedType.GetName() + "*";
            return name;
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public override string GetFullName ()
        {
            if(fullName == null)
                fullName = pointedType.GetFullName() + "*";
            return fullName;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string GetDisplayName ()
        {
            if(displayName == null)
                displayName = pointedType.GetDisplayName() + "*";
            return displayName;
        }

        public override bool IsPointer ()
        {
            return true;
        }

        public override bool IsGenericType ()
        {
            return pointedType.IsGenericType();
        }

        public override bool IsUnsafe()
        {
            return true;
        }

        public override IChelaType InstanceGeneric (GenericInstance args, ChelaModule instModule)
        {
            return Create(pointedType.InstanceGeneric(args, instModule));
        }
                
        public IChelaType GetPointedType()
        {
            return pointedType;
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(pointedType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as PointerType);
        }

        public bool Equals(PointerType obj)
        {
            return obj != null && pointedType == obj.pointedType;
        }

        /// <summary>
        /// Create the specified pointer type.
        /// </summary>
        public static PointerType Create(IChelaType pointedType)
        {
            PointerType pointer = new PointerType(pointedType);
            return pointerTypes.GetOrAdd(pointer);
        }
    }
}

