using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Constant type.
    /// </summary>
    public class ConstantType: ChelaType, IEquatable<ConstantType>
    {
        private static SimpleSet<ConstantType> constantTypes = new SimpleSet<ConstantType> ();
        private IChelaType valueType;
        private string name;

        internal ConstantType (IChelaType valueType)
        {
            this.valueType = valueType;
            this.name = null;
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        public override string GetName ()
        {
            if(name == null)
            {
                if(valueType.IsPointer())
                    name = valueType.GetName() + " const";
                else
                    name = "const " + valueType.GetName();
            }
            return name;
        }
        
        public override bool IsConstant ()
        {
            return true;
        }

        public override bool IsGenericType ()
        {
            return valueType.IsGenericType();
        }

        public override IChelaType InstanceGeneric (GenericInstance args, ChelaModule instModule)
        {
            return Create(valueType.InstanceGeneric(args, instModule));
        }

        public IChelaType GetValueType()
        {
            return valueType;
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(valueType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as ConstantType);
        }

        public bool Equals(ConstantType obj)
        {
            return obj != null && valueType == obj.valueType;
        }

        /// <summary>
        /// Create the specified constant type.
        /// </summary>
        public static ConstantType Create(IChelaType valueType)
        {
            ConstantType constant = new ConstantType(valueType);
            return constantTypes.GetOrAdd(constant);
        }
    }
}

