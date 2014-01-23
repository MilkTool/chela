using System;
using System.Collections.Generic;

namespace Chela.Compiler.Module
{
	public abstract class ScopeMember
	{
		private ChelaModule module;
		protected uint serialId;
        protected List<AttributeConstant> attributes;
        protected GenericInstance fullGenericInstance;
        protected GenericPrototype fullGenericPrototype;
		
		public ScopeMember (ChelaModule module)
		{
			this.module = module;
			this.serialId = 0;
            this.attributes = null;
		}
		
		public ChelaModule GetModule()
		{
			return module;
		}

        public abstract Scope GetParentScope();

		public uint GetSerialId()
		{
			return this.serialId;
		}
		
		public virtual string GetName()
		{
			return string.Empty;
		}

        public virtual string GetDisplayName()
        {
            Scope parentScope = GetParentScope();
            string ret = string.Empty;
            if(parentScope != null)
            {
                string parentFullname = parentScope.GetDisplayName();
                if(!string.IsNullOrEmpty(parentFullname))
                    ret = parentFullname + "." + GetName();
                else
                    ret = GetName();
            }
            else
                ret = GetName();

            // Add the generic instance.
            GenericInstance instance = GetGenericInstance();
            if(instance != null)
                ret += instance.GetDisplayName();

            // Add the generic prototype.
            GenericPrototype proto = GetGenericPrototype();
            if(proto != null)
                ret += proto.GetDisplayName();

            return ret;
        }
		
		public virtual string GetFullName()
		{
            Scope parentScope = GetParentScope();
            string ret = string.Empty;
            if(parentScope != null)
            {
                string parentFullname = parentScope.GetFullName();
                if(!string.IsNullOrEmpty(parentFullname))
                    ret = parentFullname + "." + GetName();
                else
                    ret = GetName();
            }
            else
                ret = GetName();

            // Add the generic instance.
            GenericInstance instance = GetGenericInstance();
            if(instance != null)
                ret += instance.GetFullName();

            // Add the generic prototype.
            GenericPrototype proto = GetGenericPrototype();
            if(proto != null && (instance == null || instance.GetParameterCount() == 0))
                ret += proto.GetFullName();

            return ret;
		}

        public virtual GenericInstance GetGenericInstance()
        {
            return null;
        }

        public virtual GenericPrototype GetGenericPrototype()
        {
            return null;
        }

        public GenericPrototype GetFullGenericPrototype()
        {
            // Return the cached generic prototype.
            if(fullGenericPrototype != null)
                return fullGenericPrototype;

            // Get the parent full generic prototype.
            GenericPrototype parentFull = null;
            Scope parent = GetParentScope();
            if(parent != null)
                parentFull = parent.GetFullGenericPrototype();

            // Get my generic prototype.
            GenericPrototype myPrototype = GetGenericPrototype();
            if(myPrototype == null || myPrototype.GetPlaceHolderCount() == 0)
            {
                fullGenericPrototype = parentFull;
            }
            else if(parentFull == null || parentFull.GetPlaceHolderCount() == 0)
            {
                fullGenericPrototype = myPrototype;
            }
            else
            {
                // Merge both prototypes
                int parentSize = parentFull.GetPlaceHolderCount();
                int mySize = myPrototype.GetPlaceHolderCount();
                PlaceHolderType[] newProto = new PlaceHolderType[parentSize + mySize];

                // Add the parent prototype components.
                for(int i = 0; i < parentSize; ++i)
                    newProto[i] = parentFull.GetPlaceHolder(i);

                // Add my prototype components.
                for(int i = 0; i < mySize; ++i)
                    newProto[i + parentSize] = myPrototype.GetPlaceHolder(i);

                // Store the new complete generic prototype.
                fullGenericPrototype = new GenericPrototype(newProto);
            }

            return fullGenericPrototype;
        }
        public GenericInstance GetFullGenericInstance()
        {
            // Return the cached instance.
            if(fullGenericInstance != null)
                return fullGenericInstance;

            // Get the full prototype.
            GenericPrototype fullProto = GetFullGenericPrototype();
            if(fullProto == null)
                return null;

            // Get the parent full generic instance.
            GenericInstance parentFull = null;
            Scope parent = GetParentScope();
            if(parent != null)
                parentFull = parent.GetFullGenericInstance();

            // Get my generic instance.
            GenericInstance myInstance = GetGenericInstance();
            if(myInstance == null || myInstance.GetParameterCount() == 0)
            {
                fullGenericInstance = parentFull;
            }
            else if(parentFull == null || parentFull.GetParameterCount() == 0)
            {
                fullGenericInstance = myInstance;
            }
            else
            {
                // Merge both instances
                int parentSize = parentFull.GetParameterCount();
                int mySize = myInstance.GetParameterCount();
                IChelaType[] newInstance = new IChelaType[parentSize + mySize];

                // Add the parent prototype components.
                for(int i = 0; i < parentSize; ++i)
                    newInstance[i] = parentFull.GetParameter(i);

                // Add my prototype components.
                for(int i = 0; i < mySize; ++i)
                    newInstance[i + parentSize] = myInstance.GetParameter(i);

                // Store the new complete generic instance.
                fullGenericInstance = new GenericInstance(fullProto, newInstance);
            }

            return fullGenericInstance;

        }

		public virtual MemberFlags GetFlags()
		{
			return MemberFlags.Default;
		}
		
		public virtual bool IsType()
		{
			return false;
		}
		
		public virtual bool IsNamespace()
		{
			return false;
		}

        public virtual bool IsPseudoScope()
        {
            return false;
        }

        public virtual bool IsScope()
        {
            return false;
        }

		public virtual bool IsInterface()
		{
			return false;
		}
		
		public virtual bool IsStructure()
		{
			return false;
		}
		
		public virtual bool IsClass()
		{
			return false;
		}

        public virtual bool IsPassedByReference()
        {
            return false;
        }

		public virtual bool IsFunctionGroup()
		{
			return false;
		}
		
		public virtual bool IsFunction()
		{
			return false;
		}

        public virtual bool IsGenericInstance()
        {
            return false;
        }

        public virtual bool IsGeneric()
        {
            return false;
        }

        public virtual bool IsGenericImplicit()
        {
            GenericPrototype proto = GetGenericPrototype();
            return proto == null || proto.GetPlaceHolderCount() == 0;
        }

        public virtual bool IsTypeGroup()
        {
            return false;
        }

        public virtual bool IsFunctionGroupSelector()
        {
            return false;
        }

        public virtual bool IsTypeName()
        {
            return false;
        }
		
		public virtual bool IsVariable()
		{
			return false;
		}

        public virtual bool IsStatic ()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Static;
        }

        public virtual bool IsExternal()
        {
            return (GetFlags() & MemberFlags.LinkageMask) == MemberFlags.External;
        }

        public virtual bool IsEvent()
        {
            return false;
        }

        public virtual bool IsProperty()
        {
            return false;
        }

        public virtual bool IsPublic()
        {
            return (GetFlags() & MemberFlags.VisibilityMask) == MemberFlags.Public;
        }

        public virtual bool IsInternal()
        {
            return (GetFlags() & MemberFlags.VisibilityMask) == MemberFlags.Internal;
        }

        public virtual bool IsProtected()
        {
            return (GetFlags() & MemberFlags.VisibilityMask) == MemberFlags.Protected;
        }

        public virtual bool IsPrivate()
        {
            return (GetFlags() & MemberFlags.VisibilityMask) == MemberFlags.Private;
        }

        public virtual bool IsKernel()
        {
            return (GetFlags() & MemberFlags.LanguageMask) == MemberFlags.Kernel;
        }

        public virtual bool IsSealed()
        {
            return (GetFlags() & MemberFlags.InheritanceMask) == MemberFlags.Sealed;
        }

        public virtual bool IsReadOnly()
        {
            return (GetFlags() & MemberFlags.AccessMask) == MemberFlags.ReadOnly;
        }

        public virtual bool IsUnsafe()
        {
            return (GetFlags() & MemberFlags.SecurityMask) == MemberFlags.Unsafe;
        }

        public virtual ScopeMember InstanceMember(ScopeMember factory, GenericInstance instance)
        {
            return this;
        }

		public virtual void Dump()
		{
		}

        internal virtual void PrepareSerialization()
        {
            PrepareSerializationNoBase();
        }

        protected void PrepareSerializationNoBase()
		{
            // Store my serial id.
			this.serialId = GetModule().RegisterMember(this);

            // Register the custom attributes.
            if(attributes != null)
            {
                foreach(AttributeConstant attr in attributes)
                    attr.PrepareSerialization(GetModule());
            }
		}

        internal virtual void PrepareDebug(DebugEmitter debugEmitter)
        {
        }

		public virtual void Write(ModuleWriter writer)
		{
		}

        internal virtual void Read(ModuleReader reader, MemberHeader header)
        {
            reader.Skip(header.memberSize);
        }

        internal virtual void Read2(ModuleReader reader, MemberHeader header)
        {
            reader.Skip(header.memberSize);
        }

        internal virtual void UpdateParent(Scope parentScope)
        {
        }

        internal virtual void FinishLoad()
        {
        }

        public virtual bool IsInstanceOf(ScopeMember template)
        {
            return false;
        }

        public byte GetAttributeCount()
        {
            if(attributes == null)
                return 0;
            return (byte)attributes.Count;
        }

        public void WriteAttributes(ModuleWriter writer)
        {
            // Ignore the case without attributes.
            if(attributes == null)
                return;

            // Write each one of the attributes.
            foreach(AttributeConstant attrConst in attributes)
                attrConst.Write(writer, GetModule());
        }

        public void AddAttribute(AttributeConstant attribute)
        {
            // Create the attribute vectorif wasn't created.
            if(attributes == null)
                attributes = new List<AttributeConstant> ();

            // Store the attribute.
            attributes.Add(attribute);
        }

        /// <summary>
        /// Gets or sets the member definition position.
        /// </summary>
        public virtual TokenPosition Position {
            get {
                return null;
            }
            set {
            }
        }

        public void Error(string message)
        {
            if(Position != null)
                throw new CompilerException(message, Position);
            else
                throw new ModuleException(message);
        }

        public void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

		protected string GetFlagsString()
		{
			string ret = "";
			
			switch(GetFlags() & MemberFlags.VisibilityMask)
			{
			default:
			case MemberFlags.Internal:
				ret = "internal";
				break;
			case MemberFlags.Public:
				ret = "public";
				break;
			case MemberFlags.Protected:
				ret = "protected";
				break;
			case MemberFlags.Private:
				ret = "private";
				break;
			}
			
			switch(GetFlags() & MemberFlags.InstanceMask)
			{
			default:
			case MemberFlags.Instanced:
				break;
			case MemberFlags.Static:
				ret += " static";
				break;
			case MemberFlags.Virtual:
				ret += " virtual";
				break;
			case MemberFlags.Override:
				ret += " override";
				break;
			case MemberFlags.Constructor:
				ret += " constructor";
				break;
            case MemberFlags.Abstract:
                ret += " abstract";
                break;
            case MemberFlags.Contract:
                ret += " contract";
                break;
			}
			
			switch(GetFlags() & MemberFlags.LanguageMask)
			{
			default:
			case MemberFlags.Native:
				break;
			case MemberFlags.Cdecl:
				ret += " cdecl";
				break;
			case MemberFlags.Kernel:
				ret += " kernel";
				break;
            case MemberFlags.Runtime:
                ret += " runtime";
                break;
			}
			
			switch(GetFlags() & MemberFlags.LinkageMask)
			{
			default:
			case MemberFlags.Module:
				break;
			case MemberFlags.External:
				ret += " extern";
				break;
			}

            switch(GetFlags() & MemberFlags.InheritanceMask)
            {
            default:
            case MemberFlags.NormalInheritance:
                break;
            case MemberFlags.Sealed:
                ret += " sealed";
                break;
            }
			
			return ret;
		}
	}
}

