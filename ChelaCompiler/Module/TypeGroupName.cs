using System;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Type group name.
    /// </summary>
    public class TypeGroupName: IEquatable<TypeGroupName>
    {
        private Structure building;
        private GenericPrototype prototype;
        private bool isNamespaceLevel;

        public TypeGroupName (Structure building, GenericPrototype prototype)
        {
            this.prototype = prototype;
            this.building = building;
            this.isNamespaceLevel = false;
        }

        public TypeGroupName(TypeGroupName other)
            : this(other.GetBuilding(), other.GetGenericPrototype())
        {
        }

        public override int GetHashCode()
        {
            return prototype.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Avoid castin.
            if(obj == this)
                return true;
            return Equals(obj as TypeGroupName);
        }

        public bool Equals(TypeGroupName obj)
        {
            return obj != null && prototype.Equals(obj.prototype);
        }
         
        public GenericPrototype GetGenericPrototype ()
        {
            return prototype;
        }

        public Structure GetBuilding ()
        {
            return building;
        }

        public void SetBuilding (Structure building)
        {
            this.building = building;
        }

        public bool IsNamespaceLevel {
            get {
                return isNamespaceLevel;
            }
            set {
                isNamespaceLevel = value;
            }
        }
    }
}

