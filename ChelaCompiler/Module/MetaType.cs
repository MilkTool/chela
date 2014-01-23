using System;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Meta type.
    /// </summary>
	public class MetaType: ChelaType, IEquatable<MetaType>
	{
		private static SimpleSet<MetaType> metaTypes = new SimpleSet<MetaType>();
		private IChelaType actualType;
		
		internal MetaType (IChelaType actualType)
		{
			this.actualType = actualType;
		}
		
		public override bool IsMetaType ()
		{
			return true;
		}

        public override string GetDisplayName()
        {
            return "typename " + actualType.GetDisplayName();
        }

        public override IChelaType InstanceGeneric (GenericInstance args, ChelaModule instModule)
        {
            return Create(actualType.InstanceGeneric(args, instModule));
        }
		
        /// <summary>
        /// Gets the type encapsulated.
        /// </summary>
		public IChelaType GetActualType()
		{
			return actualType;
		}
		
        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(actualType);
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as MetaType);
        }

        public bool Equals(MetaType obj)
        {
            return obj != null && actualType == obj.actualType;
        }

        /// <summary>
        /// Create the specified meta type.
        /// </summary>
		public static MetaType Create(IChelaType actualType)
		{
			MetaType meta = new MetaType(actualType);
            return metaTypes.GetOrAdd(meta);
		}
	}
}

