using System.Collections.Generic;
using System.IO;

namespace Chela.Compiler.Module
{
	public class ChelaModule
	{
		internal class ModuleHeader
		{
			public static readonly string Signature = "CHELAHDR";
			//public byte signature[8]; ""
			public uint moduleSize;
			public uint formatVersionMajor;
			public uint formatVersionMinor;
            public uint moduleType;
            public uint entryPoint;
			public uint moduleRefTableEntries;
			public uint moduleRefTableOffset;
			public uint memberTableOffset;
			public uint memberTableSize;
			public uint anonTypeTableOffset;
			public uint anonTypeTableSize;
			public uint typeTableEntries;
			public uint typeTableOffset;
			public uint stringTableEntries;
			public uint stringTableOffset;
            public uint libTableOffset;
            public uint libTableEntries;
            public uint debugInfoSize;
            public uint debugInfoOffset;
            public uint resourceDataOffset;
            public uint resourceDataSize;

			
			public ModuleHeader()
			{
				moduleSize = 0;
				formatVersionMajor = 0;
				formatVersionMinor = 1;
                moduleType = 0;
                entryPoint = 0;
				moduleRefTableEntries = 0;
				moduleRefTableOffset = 0;
				memberTableOffset = 0;
				memberTableSize = 0;
				anonTypeTableOffset = 0;
				anonTypeTableSize = 0;
				typeTableEntries = 0;
				typeTableOffset = 0;
				stringTableEntries = 0;
				stringTableOffset = 0;
                libTableOffset = 0;
                libTableEntries = 0;
                debugInfoSize = 0;
                debugInfoOffset = 0;
                resourceDataSize = 0;
                resourceDataOffset = 0;
			}
			
			public void Write(ModuleWriter writer)
			{
				writer.Write(Signature, 8);
				writer.Write(moduleSize);
				writer.Write(formatVersionMajor);
				writer.Write(formatVersionMinor);
                writer.Write(moduleType);
                writer.Write(entryPoint);
				writer.Write(moduleRefTableEntries);
				writer.Write(moduleRefTableOffset);
				writer.Write(memberTableOffset);
				writer.Write(memberTableSize);
				writer.Write(anonTypeTableOffset);
				writer.Write(anonTypeTableSize);
				writer.Write(typeTableEntries);
				writer.Write(typeTableOffset);
				writer.Write(stringTableEntries);
				writer.Write(stringTableOffset);
                writer.Write(libTableOffset);
                writer.Write(libTableEntries);
                writer.Write(debugInfoSize);
                writer.Write(debugInfoOffset);
                writer.Write(resourceDataSize);
                writer.Write(resourceDataOffset);
			}

            public void Read(ModuleReader reader)
            {
                // Read the signature.
                string sign = reader.ReadString(8);
                if(sign != Signature)
                    throw new ModuleException("Invalid module signature. " + sign);

                // Read the header members
                reader.Read(out moduleSize);
                reader.Read(out formatVersionMajor);
                reader.Read(out formatVersionMinor);
                reader.Read(out moduleType);
                reader.Read(out entryPoint);
                reader.Read(out moduleRefTableEntries);
                reader.Read(out moduleRefTableOffset);
                reader.Read(out memberTableOffset);
                reader.Read(out memberTableSize);
                reader.Read(out anonTypeTableOffset);
                reader.Read(out anonTypeTableSize);
                reader.Read(out typeTableEntries);
                reader.Read(out typeTableOffset);
                reader.Read(out stringTableEntries);
                reader.Read(out stringTableOffset);
                reader.Read(out libTableOffset);
                reader.Read(out libTableEntries);
                reader.Read(out debugInfoSize);
                reader.Read(out debugInfoOffset);
                reader.Read(out resourceDataSize);
                reader.Read(out resourceDataOffset);
            }
		};
		
		internal class ModuleReference
		{
			public uint moduleName;
			public ushort versionMajor;
			public ushort versionMinor;
			public ushort versionMicro;
				
			// TODO: Add the module certificate.
			
			public ModuleReference()
			{
				moduleName = 0;
				versionMajor = 0;
				versionMinor = 0;
				versionMicro = 0;				
			}
			
			public void Write(ModuleWriter writer)
			{
				writer.Write(moduleName);
				writer.Write(versionMajor);
				writer.Write(versionMinor);
				writer.Write(versionMicro);
			}

            public void Read(ModuleReader reader)
            {
                reader.Read(out moduleName);
                reader.Read(out versionMajor);
                reader.Read(out versionMinor);
                reader.Read(out versionMicro);
            }
		};

		internal enum TypeKind
		{
			BuiltIn = 0,
			Structure,
			Class,
            Interface,
			Function,
			Reference,
			Pointer,
            Constant,
            Array,
            Vector,
            Matrix,
            PlaceHolder,
            Instance,
		};

		internal class AnonTypeHeader
		{
			public byte typeKind;
			public uint recordSize;
            public byte[] readedData;
            public bool loading;
            private int position;
			
			public AnonTypeHeader()
			{
				typeKind = 0;
				recordSize = 0;
                readedData = null;
                loading = false;
                position = 0;
			}
			
			public void Write(ModuleWriter writer)
			{
				writer.Write(typeKind);
				writer.Write(recordSize);
			}

            public void Read(ModuleReader reader)
            {
                reader.Read(out typeKind);
                reader.Read(out recordSize);
                reader.Read(out readedData, (int)recordSize);
            }

            public byte FetchByte()
            {
                return readedData[position++];
            }

            public uint FetchUInt()
            {
                return (uint)(FetchByte() |
                       (FetchByte() << 8) |
                       (FetchByte() << 16) |
                       (FetchByte() << 24));
            }
		};
		
		internal class TypeReference
		{
			public uint typeName;
			public byte typeKind;
			public uint memberId;
			
			public TypeReference()
			{
				typeName = 0;
				typeKind = (byte)TypeKind.BuiltIn;
				memberId = 0;
			}
			
			public void Write(ModuleWriter writer)
			{
				writer.Write(typeName);
				writer.Write(typeKind);
				writer.Write(memberId);
			}

            public void Read(ModuleReader reader)
            {
                reader.Read(out typeName);
                reader.Read(out typeKind);
                reader.Read(out memberId);
            }
		};
		
		private string name;
        private string fileName;
        private string workDir;
		private Namespace globalNamespace;
		private Dictionary<string, uint> registeredStrings;
		private List<string> stringTable;
		private Dictionary<IChelaType, uint> registeredTypes;
		private List<object> typeTable;
		private Dictionary<IChelaType, uint> anonymousTypeMap;
        private List<ScopeMember> memberTable;
        private Dictionary<ScopeMember, uint> registeredMembers;
        private Dictionary<string, ScopeMember> memberNameTable;
		private List<object> anonymousTypes;
        private List<ChelaModule> referencedModules;
        private List<string> nativeLibraries;
		private Dictionary<IChelaType, IChelaType> typeMap;
        private Dictionary<IChelaType, Structure> associatedClass;
        private Dictionary<Structure, IChelaType> associatedPrimitive;
        private List<ResourceData> resources;
        private List<ScopeMember> genericInstances;
        private Class attributeClass;
        private Class arrayClass;
        private Class valueTypeClass;
        private Class enumClass;
        private Class typeClass;
        private Class delegateClass;
        private Class streamHolderClass;
        private Class streamHolder1DClass;
        private Class streamHolder2DClass;
        private Class streamHolder3DClass;
        private Class uniformHolderClass;
        private Class computeBindingDelegate;
        private Interface numberConstraint;
        private Interface integerConstraint;
        private Interface floatingPointConstraint;
        private Interface enumerableIface;
        private Interface enumerableGIface;
        private Interface enumeratorIface;
        private Interface enumeratorGIface;
        private ModuleType moduleType;
        private ChelaModule runtimeModule;
        private bool debugBuild;
        private string mainName;
        private Function mainFunction;
        private int placeHolderId;
        private bool writingModule;
		private static Dictionary<string, ChelaModule> loadedModules = new Dictionary<string, ChelaModule> ();
        private static List<string> libraryPaths = new List<string> ();

		public ChelaModule ()
		{
			this.name = "";
            this.fileName = "";
            this.workDir = "";
			this.globalNamespace = new Namespace(this);
            this.memberTable = new List<ScopeMember> ();
            this.registeredMembers = new Dictionary<ScopeMember, uint> ();
            this.memberNameTable = new Dictionary<string, ScopeMember> ();
			this.registeredStrings = new Dictionary<string, uint> ();
			this.stringTable = new List<string> ();
            this.referencedModules = new List<ChelaModule> ();
            this.nativeLibraries = new List<string> ();
			this.typeMap = new Dictionary<IChelaType, IChelaType> ();
            this.associatedClass = new Dictionary<IChelaType, Structure> ();
            this.associatedPrimitive = new Dictionary<Structure, IChelaType> ();
            this.resources = new List<ResourceData> ();
            this.genericInstances = new List<ScopeMember>();
            this.attributeClass = null;
            this.arrayClass = null;
            this.valueTypeClass = null;
            this.enumClass = null;
            this.typeClass = null;
            this.delegateClass = null;
            this.moduleType = ModuleType.Library;
            this.runtimeModule = null;
            this.debugBuild = false;
            this.mainName = null;
            this.mainFunction = null;
            this.placeHolderId = 0;
            this.writingModule = false;

            // Type tables.
            this.registeredTypes = new Dictionary<IChelaType, uint> (0, new ReferenceEqualityComparer<IChelaType> ());
            this.typeTable = new List<object> ();
            this.anonymousTypeMap = new Dictionary<IChelaType, uint> (0, new ReferenceEqualityComparer<IChelaType> ());
            this.anonymousTypes = new List<object> ();
		}
		
		public string GetName()
		{
			return this.name;
		}
		
		public void SetName(string name)
		{
			this.name = name;
		}

        public string GetFileName()
        {
            return this.fileName;
        }

        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        public string GetWorkDirectory()
        {
            return this.workDir;
        }

        public void SetWorkDirectory(string workDir)
        {
            this.workDir = workDir;
        }

        public ModuleType GetModuleType()
        {
            return moduleType;
        }

        public void SetModuleType(ModuleType type)
        {
            moduleType = type;
        }

        public bool IsDebugBuild()
        {
            return debugBuild;
        }

        public void SetDebugBuild(bool debug)
        {
            debugBuild = debug;
        }

        public string GetMainName()
        {
            return mainName;
        }

        public void SetMainName(string name)
        {
            mainName = name;
        }

        public Function GetMainFunction()
        {
            return mainFunction;
        }

        public void SetMainFunction(Function mainFunction)
        {
            this.mainFunction = mainFunction;
        }

        public int CreatePlaceHolderId()
        {
            return placeHolderId++;
        }

        /// <summary>
        /// Adds a reference into another module.
        /// </summary>
        public int AddReference(ChelaModule reference)
        {
            // Don't add duplicated references.
            for(int i = 0; i < referencedModules.Count; ++i)
                if(referencedModules[i] == reference)
                    return i + 1;

            // Store the module reference.
            referencedModules.Add(reference);

            // The first module reference must be the runtime.
            if(referencedModules.Count == 1)
                LoadRuntimeReferences(reference);

            // Return the module index, plus one.
            return referencedModules.Count;
        }

        /// <summary>
        /// Adds a reference into a native library.
        /// </summary>
        public void AddNativeLibrary(string name)
        {
            nativeLibraries.Add(name);
        }

        /// <summary>
        /// Adds a library search path
        /// </summary>
        public static void AddLibraryPath(string path)
        {
            libraryPaths.Add(path);
        }

        public void AddResource(ResourceData resource)
        {
            resources.Add(resource);
        }

        /// <summary>
        /// Retrieves a referenced module.
        /// </summary>
        public ChelaModule GetReferenced(int index)
        {
            // The first referenced module is myself.
            if(index == 0)
                return this;

            /// Return the referenced module.
            return referencedModules[index - 1];
        }

        public ICollection<ChelaModule> GetReferencedModules()
        {
            return referencedModules;
        }

        public static ICollection<ChelaModule> GetLoadedModules()
        {
            return loadedModules.Values;
        }
		
		public void RegisterTypeMap(IChelaType alias, IChelaType target)
		{
			typeMap.Add(alias, target);
		}

		public IChelaType TypeMap(IChelaType type)
		{
			IChelaType mapped;
            if(typeMap.TryGetValue(type, out mapped))
				return mapped;
			else
				return type;
		}

        public void RegisterArrayClass(Class clazz)
        {
            arrayClass = clazz;
        }

        public void RegisterAssociatedClass(IChelaType type, Structure clazz)
        {
            associatedClass.Add(type, clazz);
            associatedPrimitive.Add(clazz, type);
        }

        public Structure GetAssociatedClass(IChelaType type)
        {
            if(type.IsArray())
                return arrayClass;

            Structure ret;
            if(associatedClass.TryGetValue(type, out ret))
                return ret;
            else
                return null;
        }

        public IChelaType GetAssociatedClassPrimitive(Structure building)
        {
            IChelaType ret;
            if(associatedPrimitive.TryGetValue(building, out ret))
                return ret;
            else
                return null;
        }

        public void RegisterValueTypeClass(Class valueTypeClass)
        {
            this.valueTypeClass = valueTypeClass;
        }

        public Class GetValueTypeClass()
        {
            return valueTypeClass;
        }

        public Class GetObjectClass()
        {
            return (Class)TypeMap(ChelaType.GetObjectType());
        }

        public void RegisterAttributeClass(Class attributeClass)
        {
            this.attributeClass = attributeClass;
        }

        public Class GetAttributeClass()
        {
            return attributeClass;
        }

        public void RegisterEnumClass(Class enumClass)
        {
            this.enumClass = enumClass;
        }

        public Class GetEnumClass()
        {
            return enumClass;
        }

        public void RegisterTypeClass(Class typeClass)
        {
            this.typeClass = typeClass;
        }

        public Class GetTypeClass()
        {
            return typeClass;
        }

        public void RegisterDelegateClass(Class delegateClass)
        {
            this.delegateClass = delegateClass;
        }

        public Class GetDelegateClass()
        {
            return this.delegateClass;
        }

        public void RegisterStreamHolderClass(Class streamHolderClass)
        {
            this.streamHolderClass = streamHolderClass;
        }

        public Class GetStreamHolderClass()
        {
            return this.streamHolderClass;
        }

        public void RegisterStreamHolder1DClass(Class streamHolder1DClass)
        {
            this.streamHolder1DClass = streamHolder1DClass;
        }

        public Class GetStreamHolder1DClass()
        {
            return this.streamHolder1DClass;
        }

        public void RegisterStreamHolder2DClass(Class streamHolder2DClass)
        {
            this.streamHolder2DClass = streamHolder2DClass;
        }

        public Class GetStreamHolder2DClass()
        {
            return this.streamHolder2DClass;
        }

        public void RegisterStreamHolder3DClass(Class streamHolder3DClass)
        {
            this.streamHolder3DClass = streamHolder3DClass;
        }

        public Class GetStreamHolder3DClass()
        {
            return this.streamHolder3DClass;
        }

        public void RegisterUniformHolderClass(Class uniformHolderClass)
        {
            this.uniformHolderClass = uniformHolderClass;
        }

        public Class GetUniformHolderClass()
        {
            return this.uniformHolderClass;
        }

        public void RegisterComputeBindingDelegate(Class binding)
        {
            this.computeBindingDelegate = binding;
        }

        public Class GetComputeBindingDelegate()
        {
            return this.computeBindingDelegate;
        }

        public void RegisterNumberConstraint(Interface constraint)
        {
            numberConstraint = constraint;
        }

        public Interface GetNumberConstraint()
        {
            return numberConstraint;
        }

        public void RegisterIntegerConstraint(Interface constraint)
        {
            integerConstraint = constraint;
        }

        public Interface GetIntegerConstraint()
        {
            return integerConstraint;
        }

        public void RegisterFloatingPointConstraint(Interface constraint)
        {
            floatingPointConstraint = constraint;
        }

        public Interface GetFloatingPointConstraint()
        {
            return floatingPointConstraint;
        }

        public Interface GetEnumerableIface()
        {
            return enumerableIface;
        }

        public void RegisterEnumerableIface(Interface iface)
        {
            enumerableIface = iface;
        }

        public Interface GetEnumerableGIface()
        {
            return enumerableGIface;
        }

        public void RegisterEnumerableGIface(Interface iface)
        {
            enumerableGIface = iface;
        }

        public Interface GetEnumeratorIface()
        {
            return enumeratorIface;
        }

        public void RegisterEnumeratorIface(Interface iface)
        {
            enumeratorIface = iface;
        }

        public Interface GetEnumeratorGIface()
        {
            return enumeratorGIface;
        }

        public void RegisterEnumeratorGIface(Interface iface)
        {
            enumeratorGIface = iface;
        }

        public void RegisterGenericInstance(ScopeMember instance)
        {
            genericInstances.Add(instance);
        }

        private IChelaType GetFloatVectorTy(int comps)
        {
            return VectorType.Create(ChelaType.GetFloatType(), comps);
        }

        private IChelaType GetDoubleVectorTy(int comps)
        {
            return VectorType.Create(ChelaType.GetDoubleType(), comps);
        }

        private IChelaType GetIntVectorTy(int comps)
        {
            return VectorType.Create(ChelaType.GetIntType(), comps);
        }

        private IChelaType GetBoolVectorTy(int comps)
        {
            return VectorType.Create(ChelaType.GetBoolType(), comps);
        }

        private Structure GetDefaultType(Scope scope, string name)
        {
            // Get the member.
            ScopeMember member = scope.FindMember(name);

            // Use the default type of the group.
            if(member.IsTypeGroup())
            {
                TypeGroup group = (TypeGroup)member;
                return group.GetDefaultType();
            }
            else if(!member.IsClass() && !member.IsStructure() && !member.IsInterface())
                throw new ModuleException("expected class/structure/interface.");

            // Cast the member.
            return (Structure) member;
        }

        private Structure GetFirstType(Scope scope, string name)
        {
            // Get the member.
            ScopeMember member = scope.FindMember(name);

            // Use the default type of the group.
            if(member.IsTypeGroup())
            {
                TypeGroup group = (TypeGroup)member;
                foreach(TypeGroupName gname in @group.GetBuildings())
                {
                    Structure building = gname.GetBuilding();
                    if(building != null)
                        return building;
                }
                throw new ModuleException("failed to get first type in group.");
            }
            else if(!member.IsClass() && !member.IsStructure() && !member.IsInterface())
                throw new ModuleException("expected class/structure/interface.");

            // Cast the member.
            return (Structure) member;
        }

        private void LoadRuntimeReferences(ChelaModule module)
        {
            // Store the runtime module.
            runtimeModule = module;

            // All of the runtime members must be under Chela.Lang namespace.
            Namespace chela = (Namespace)module.globalNamespace.FindMember("Chela");
            Namespace chelaCollections = (Namespace)chela.FindMember("Collections");
            Namespace chelaCollectionsGeneric = (Namespace)chelaCollections.FindMember("Generic");
            Namespace chelaLang = (Namespace)chela.FindMember("Lang");
            Namespace chelaCompute = (Namespace)chela.FindMember("Compute");

            // Perform type maps.
            RegisterTypeMap(ChelaType.GetObjectType(), GetDefaultType(chelaLang, "Object"));
            RegisterTypeMap(ChelaType.GetStringType(), GetDefaultType(chelaLang, "String"));

            // Register associated types.
            RegisterAssociatedClass(ChelaType.GetBoolType(), GetDefaultType(chelaLang, "Boolean"));
            RegisterAssociatedClass(ChelaType.GetCharType(), GetDefaultType(chelaLang, "Char"));
            RegisterAssociatedClass(ChelaType.GetSByteType(), GetDefaultType(chelaLang, "SByte"));
            RegisterAssociatedClass(ChelaType.GetByteType(),  GetDefaultType(chelaLang, "Byte"));
            RegisterAssociatedClass(ChelaType.GetShortType(), GetDefaultType(chelaLang, "Int16"));
            RegisterAssociatedClass(ChelaType.GetUShortType(), GetDefaultType(chelaLang, "UInt16"));
            RegisterAssociatedClass(ChelaType.GetIntType(), GetDefaultType(chelaLang, "Int32"));
            RegisterAssociatedClass(ChelaType.GetUIntType(), GetDefaultType(chelaLang, "UInt32"));
            RegisterAssociatedClass(ChelaType.GetLongType(), GetDefaultType(chelaLang, "Int64"));
            RegisterAssociatedClass(ChelaType.GetULongType(), GetDefaultType(chelaLang, "UInt64"));
            RegisterAssociatedClass(ChelaType.GetSizeType(), GetDefaultType(chelaLang, "UIntPtr"));
            RegisterAssociatedClass(ChelaType.GetPtrDiffType(), GetDefaultType(chelaLang, "IntPtr"));
            RegisterAssociatedClass(ChelaType.GetFloatType(), GetDefaultType(chelaLang, "Single"));
            RegisterAssociatedClass(ChelaType.GetDoubleType(), GetDefaultType(chelaLang, "Double"));

            // Register vector associated types.
            RegisterAssociatedClass(GetFloatVectorTy(2), GetDefaultType(chelaLang, "Single2"));
            RegisterAssociatedClass(GetFloatVectorTy(3), GetDefaultType(chelaLang, "Single3"));
            RegisterAssociatedClass(GetFloatVectorTy(4), GetDefaultType(chelaLang, "Single4"));
            RegisterAssociatedClass(GetDoubleVectorTy(2), GetDefaultType(chelaLang, "Double2"));
            RegisterAssociatedClass(GetDoubleVectorTy(3), GetDefaultType(chelaLang, "Double3"));
            RegisterAssociatedClass(GetDoubleVectorTy(4), GetDefaultType(chelaLang, "Double4"));
            RegisterAssociatedClass(GetIntVectorTy(2), GetDefaultType(chelaLang, "Int32x2"));
            RegisterAssociatedClass(GetIntVectorTy(3), GetDefaultType(chelaLang, "Int32x3"));
            RegisterAssociatedClass(GetIntVectorTy(4), GetDefaultType(chelaLang, "Int32x4"));
            RegisterAssociatedClass(GetBoolVectorTy(2), GetDefaultType(chelaLang, "Boolean2"));
            RegisterAssociatedClass(GetBoolVectorTy(3), GetDefaultType(chelaLang, "Boolean3"));
            RegisterAssociatedClass(GetBoolVectorTy(4), GetDefaultType(chelaLang, "Boolean4"));

            // Register special classes.
            RegisterArrayClass((Class)GetDefaultType(chelaLang, "Array"));
            RegisterAttributeClass((Class)GetDefaultType(chelaLang, "Attribute"));
            RegisterValueTypeClass((Class)GetDefaultType(chelaLang, "ValueType"));
            RegisterEnumClass((Class)GetDefaultType(chelaLang, "Enum"));
            RegisterTypeClass((Class)GetDefaultType(chelaLang, "Type"));
            RegisterDelegateClass((Class)GetDefaultType(chelaLang, "Delegate"));
            RegisterNumberConstraint((Interface)GetDefaultType(chelaLang, "NumberConstraint"));
            RegisterIntegerConstraint((Interface)GetDefaultType(chelaLang, "IntegerConstraint"));
            RegisterFloatingPointConstraint((Interface)GetDefaultType(chelaLang, "FloatingPointConstraint"));

            // Register iterator classes.
            RegisterEnumerableIface((Interface)GetDefaultType(chelaCollections, "IEnumerable"));
            RegisterEnumerableGIface((Interface)GetFirstType(chelaCollectionsGeneric, "IEnumerable"));
            RegisterEnumeratorIface((Interface)GetDefaultType(chelaCollections, "IEnumerator"));
            RegisterEnumeratorGIface((Interface)GetFirstType(chelaCollectionsGeneric, "IEnumerator"));


            // Register stream computing iface classes.
            RegisterStreamHolderClass((Class)GetFirstType(chelaCompute, "StreamHolder"));
            RegisterStreamHolder1DClass((Class)GetFirstType(chelaCompute, "StreamHolder1D"));
            RegisterStreamHolder2DClass((Class)GetFirstType(chelaCompute, "StreamHolder2D"));
            RegisterStreamHolder3DClass((Class)GetFirstType(chelaCompute, "StreamHolder3D"));
            RegisterUniformHolderClass((Class)GetFirstType(chelaCompute, "UniformHolder"));
            RegisterComputeBindingDelegate((Class)GetDefaultType(chelaCompute, "ComputeBinding"));
        }

        public ScopeMember GetLangMember(string name)
        {
            if(runtimeModule == null)
                runtimeModule = this;

            Namespace chela = (Namespace)runtimeModule.globalNamespace.FindMember("Chela");
            Namespace chelaLang = (Namespace)chela.FindMember("Lang");
            return chelaLang.FindMember(name);
        }

        public ScopeMember GetThreadingMember(string name, bool type)
        {
            if(runtimeModule == null)
                runtimeModule = this;

            Namespace chela = (Namespace)runtimeModule.globalNamespace.FindMember("Chela");
            Namespace chelaThreading = (Namespace)chela.FindMember("Threading");
            if(type)
                return GetDefaultType(chelaThreading, name);
            else
                return chelaThreading.FindMember(name);
        }

        /// <summary>
        /// Registers a string and returns the string id.
        /// </summary>
		public uint RegisterString(string s)
		{
            if(string.IsNullOrEmpty(s))
                return 0;

            // Find an already registered version of the string.
			uint res;
            if(registeredStrings.TryGetValue(s, out res))
                return res;

            // Register the string.
			res = (uint)(stringTable.Count+1);
			stringTable.Add(s);
			registeredStrings.Add(s, res);
			return res;
		}

        /// <summary>
        /// Retrieves a string using his id.
        /// </summary>
        public string GetString(uint stringId)
        {
            if(stringId == 0)
                return string.Empty;
            else
                return stringTable[(int)stringId-1];
        }

        /// <summary>
        /// Registers a type in the module, giving back his id.
        /// </summary>
        internal uint RegisterType(IChelaType type)
		{
			// Used to link standard runtime types with his respective aliases.
			IChelaType typeMapped = TypeMap(type);
			if(typeMapped != null && typeMapped != type)
				return RegisterType(typeMapped);
			
			if(type.IsPrimitive())
			{
				PrimitiveType primType = (PrimitiveType) type;
				return (uint)primType.GetPrimitiveId();
			}
			
			uint res;
            if(registeredTypes.TryGetValue(type, out res))
                return res;

            // Don't allow registration when writing the module.
            if(writingModule)
                throw new BugException("Writing unregistered type " + type.GetFullName());

			// Add into the type table.
			res = (uint)(typeTable.Count + 0x100);
			typeTable.Add(type);
			
			// Add into anonymous types.
			if(!type.IsClass() && !type.IsStructure() && !type.IsInterface()
               && !type.IsTypeInstance())
			{
				anonymousTypeMap.Add(type, (uint)anonymousTypes.Count);
				anonymousTypes.Add(type);
			}
            else
            {
                // Make sure the member is registered.
                RegisterMember((ScopeMember)type);
            }

			// Add into the registered types.
			registeredTypes.Add(type, res);
			return res;
		}

        /// <summary>
        /// Retrieves a registered type.
        /// </summary>
        public IChelaType GetType(uint typeId)
        {
            // Look up for primitive types.
            if(typeId < 0x100)
                return PrimitiveType.GetPrimitiveType(typeId);

            // Return the type from the type table.
            int index = (int) (typeId - 0x100);
            object candidate = typeTable[index];
            IChelaType loadedType = candidate as IChelaType;
            if(loadedType != null)
                return loadedType;

            // Load the type.
            TypeReference typeRef = (TypeReference)candidate;

            // Must be an anonymous type.
            loadedType = GetAnonType(typeRef.memberId);

            // Register the loaded type.
            typeTable[index] = loadedType;
            registeredTypes[loadedType] = typeId;

            // Return the type.
            return loadedType;
        }

        public IChelaType GetAnonType(uint anonId)
        {
            // Get the candidate from the type table.
            object candidate = anonymousTypes[(int)anonId];

            // Try to cast into an actual type.
            IChelaType actualType = candidate as IChelaType;
            if(actualType != null)
                return actualType;

            // Load the anonymous type.
            AnonTypeHeader anonType = (AnonTypeHeader)candidate;

            // Prevent circular references.
            if(anonType.loading)
                throw new ModuleException("circular derived types.");

            // Load the anonymous type.
            TypeKind kind = (TypeKind)anonType.typeKind;
            switch(kind)
            {
            case TypeKind.Reference:
                actualType = ReferenceType.Create(GetType(anonType.FetchUInt()), (ReferenceFlow)anonType.FetchByte(), anonType.FetchByte() != 0);
                break;
            case TypeKind.Pointer:
                actualType = PointerType.Create(GetType(anonType.FetchUInt()));
                break;
            case TypeKind.Constant:
                actualType = ConstantType.Create(GetType(anonType.FetchUInt()));
                break;
            case TypeKind.Array:
                actualType = ArrayType.Create(GetType(anonType.FetchUInt()), anonType.FetchByte(), anonType.FetchByte() != 0);
                break;
            case TypeKind.Vector:
                actualType = VectorType.Create(GetType(anonType.FetchUInt()), anonType.FetchByte());
                break;
            case TypeKind.Matrix:
                actualType = MatrixType.Create(GetType(anonType.FetchUInt()), anonType.FetchByte(), anonType.FetchByte());
                break;
            case TypeKind.Function:
                {
                    // Read the flags data.
                    uint flags = anonType.FetchUInt();
                    bool vararg = (flags & 1)  != 0;

                    // Read the variable arguments and the return type.
                    IChelaType retType = GetType(anonType.FetchUInt());

                    // Read the arguments.
                    int numargs = ((int)anonType.recordSize - 8)/4;
                    IChelaType[] args = new IChelaType[numargs];
                    for(int i = 0; i < numargs; ++i)
                        args[i] = GetType(anonType.FetchUInt());

                    // Create the function type.
                    actualType = FunctionType.Create(retType, args, vararg, (MemberFlags)flags);
                }
                break;
            case TypeKind.PlaceHolder:
                {
                    // Read the name.
                    string name = GetString(anonType.FetchUInt());

                    // Read the id.
                    int id = (int)anonType.FetchUInt();

                    // Read the value type flag.
                    bool valueType = anonType.FetchByte() != 0;

                    // Read the number of bases.
                    int numbases = anonType.FetchByte();

                    // Read the bases.
                    Structure[] bases = new Structure[numbases];
                    for(int i = 0; i < numbases; ++i)
                        bases[i] = (Structure)GetMember(anonType.FetchUInt());

                    // Create the place holder.
                    actualType = new PlaceHolderType(this, id, name, valueType, bases);
                }
                break;
            default:
                throw new System.NotSupportedException("unsupported anonymous type.");
            }

            // Remove the previous anonymous type.
            anonymousTypes[(int)anonId] = actualType;
            anonymousTypeMap[actualType] = anonId;

            // Return the loaded type.
            return actualType;
        }

        /// <summary>
        /// Registers a member in the module.
        /// </summary>
        public uint RegisterMember(ScopeMember member)
        {
            // Use the null member.
            if(member == null)
                return 0;

            // Find the already registered member.
            uint res;
            if(registeredMembers.TryGetValue(member, out res))
                return res;

            // Don't allow registering new members.
            if(writingModule)
                throw new BugException("Writing unregistered member " + member.GetFullName());

            // Add into the member table.
            res = (uint)(memberTable.Count + 1);
            memberTable.Add(member);

            // Prevent multiple definitions.
            registeredMembers.Add(member, res);

            // Return the member id.
            return res;
        }

        /// <summary>
        /// Retrieves a registered member.
        /// </summary>
        public ScopeMember GetMember(uint id)
        {
            // Zero id means no member.
            if(id == 0)
                return null;

            // Retrieve the member.
            return memberTable[(int)id - 1];
        }

        /// <summary>
        /// Retrieves a member by his full name.
        /// </summary>
        public ScopeMember GetMember(string name)
        {
            ScopeMember ret;
            if(memberNameTable.TryGetValue(name, out ret))
                return ret;
            else
                return null;
        }

		public Namespace GetGlobalNamespace()
		{
			return this.globalNamespace;
		}
		
		public void Write(ModuleWriter writer)
		{
			// Create the header.
			ModuleHeader header = new ModuleHeader();
			
			// Populate it.

            // Store the module type.
            header.moduleType = (uint)moduleType;

			// Write the header.
			header.Write(writer);
			
			// Prepare the members.
			globalNamespace.PrepareSerialization();
            foreach(ScopeMember instance in genericInstances)
                instance.PrepareSerialization();

            // Prepare type referemnces.
            PrepareTypeReferences();

            // Prepare resource writing.
            PrepareResources();

            // Prepare debug information.
            DebugEmitter debugEmitter = null;
            if(debugBuild)
            {
                debugEmitter = new DebugEmitter(this);
                debugEmitter.Prepare();
                globalNamespace.PrepareDebug(debugEmitter);
            }

            // Don't allow registering members.
            writingModule = true;

			// Write the members.
			header.memberTableOffset = writer.GetPosition();
            foreach(ScopeMember member in memberTable)
            {
                // Write local members, and a pointer for the externals.
                ChelaModule memberModule = member.GetModule();
                if(memberModule == this)
                {
                    member.Write(writer);
                }
                else
                {
                    // Don't write external generic instances.
                    if(member.GetGenericInstance() != null)
                        throw new ModuleException("Write external generic instance.");

                    // Create the member reference header.
                    MemberHeader mheader = new MemberHeader();
                    mheader.memberSize = 1;
                    mheader.memberName = RegisterString(member.GetFullName()); // TODO: Use mangled name.
                    mheader.memberType = (byte)MemberHeaderType.Reference;

                    // Write the reference header and module id.
                    mheader.Write(writer);
                    writer.Write((byte)AddReference(memberModule));
                }
            }
			header.memberTableSize = writer.GetPosition() - header.memberTableOffset;

            // Write module references.
            header.moduleRefTableOffset = writer.GetPosition();
            header.moduleRefTableEntries = (uint) (referencedModules.Count + 1);

            // Write myself.
            ModuleReference modRef = new ModuleReference();
            modRef.moduleName = RegisterString(GetName());
            modRef.Write(writer);

            // Write module references.
            foreach(ChelaModule mod in referencedModules)
            {
                modRef.moduleName = RegisterString(mod.GetName());
                modRef.Write(writer);
            }

            // Write the libraries table.
            header.libTableOffset = writer.GetPosition();
            header.libTableEntries = (uint)nativeLibraries.Count;
            foreach(string libname in nativeLibraries)
                writer.Write(RegisterString(libname));

			// Write the anonymous types.
			header.anonTypeTableOffset = writer.GetPosition();
			WriteAnonType(writer);
			header.anonTypeTableSize = writer.GetPosition() - header.anonTypeTableOffset;
			
			// Write the type table.
			header.typeTableOffset = writer.GetPosition();
			header.typeTableEntries = (uint)typeTable.Count;
			TypeReference typeRef = new TypeReference();
			foreach(IChelaType type in typeTable)
			{
                // Write the type kind.
                if(type.IsTypeInstance())
                    typeRef.typeKind = (byte)TypeKind.Instance;
				else if(type.IsStructure())
					typeRef.typeKind = (byte)TypeKind.Structure;
				else if(type.IsClass())
					typeRef.typeKind = (byte)TypeKind.Class;
                else if(type.IsInterface())
                    typeRef.typeKind = (byte)TypeKind.Interface;
				else if(type.IsFunction())
					typeRef.typeKind = (byte)TypeKind.Function;
				else if(type.IsReference())
					typeRef.typeKind = (byte)TypeKind.Reference;
				else if(type.IsPointer())
					typeRef.typeKind = (byte)TypeKind.Pointer;
                else if(type.IsConstant())
                    typeRef.typeKind = (byte)TypeKind.Constant;
                else if(type.IsArray())
                    typeRef.typeKind = (byte)TypeKind.Array;
                else if(type.IsVector())
                    typeRef.typeKind = (byte)TypeKind.Vector;
                else if(type.IsMatrix())
                    typeRef.typeKind = (byte)TypeKind.Matrix;
                else if(type.IsPlaceHolderType())
                    typeRef.typeKind = (byte)TypeKind.PlaceHolder;
				
				if(!type.IsStructure() && !type.IsClass() && !type.IsInterface() &&
                   !type.IsTypeInstance())
				{
                    // Write a reference into the anonymous types.
					typeRef.memberId = (uint)anonymousTypeMap[type];
					typeRef.typeName = 0;//RegisterString(type.GetName());
				}
				else
				{
                    // Write the type member reference.
                    ScopeMember member = (ScopeMember)type;
                    typeRef.memberId = RegisterMember(member);
                    typeRef.typeName = 0;//RegisterString(type.GetName());
				}
				
				// Write the type reference.
				typeRef.Write(writer);
			}
			
			// Write the string table.
			header.stringTableOffset = writer.GetPosition();
			header.stringTableEntries = (uint)stringTable.Count;
			foreach(string s in stringTable)
				writer.Write(s);

            // Write the debug info
            if(debugBuild)
            {
                header.debugInfoOffset = writer.GetPosition();
                debugEmitter.Write(writer);
                header.debugInfoSize = writer.GetPosition() - header.debugInfoOffset;
            }

            // Write the resources.
            header.resourceDataOffset = writer.GetPosition();
            WriteResources(writer);
            header.resourceDataSize = writer.GetPosition() - header.resourceDataOffset;

			// Store the module size.
			header.moduleSize = writer.GetPosition();

            // Store the entry point.
            if(mainFunction != null)
                header.entryPoint = mainFunction.GetSerialId();

			// Write the header again.
			writer.MoveBegin();
			header.Write(writer);

            // Allow more modifications from here.
            writingModule = false;
		}

		private void WriteAnonType(ModuleWriter writer)
		{
			AnonTypeHeader header = new AnonTypeHeader();
			
			foreach(IChelaType anonType in anonymousTypes)
			{
				if(anonType.IsReference())
				{
					// Write the header.
					ReferenceType refType = (ReferenceType) anonType;
					header.typeKind = (byte) TypeKind.Reference;
					header.recordSize = 6;
					header.Write(writer);
					
					// Write the referenced type.
					writer.Write((uint)RegisterType(refType.GetReferencedType()));
                    writer.Write((byte)refType.GetReferenceFlow());
                    writer.Write((byte)(refType.IsStreamReference() ? 1 : 0));
				}
				else if(anonType.IsPointer())
				{
					// Write the header.
					PointerType pointerType = (PointerType) anonType;
					header.typeKind = (byte) TypeKind.Pointer;
					header.recordSize = 4;
					header.Write(writer);
					
					// Write the referenced type.
					writer.Write((uint)RegisterType(pointerType.GetPointedType()));

				}
                else if(anonType.IsConstant())
                {
                    // Write the header.
                    ConstantType constantType = (ConstantType) anonType;
                    header.typeKind = (byte) TypeKind.Constant;
                    header.recordSize = 4;
                    header.Write(writer);

                    // Write the value type.
                    writer.Write((uint)RegisterType(constantType.GetValueType()));
                }
                else if(anonType.IsArray())
                {
                    // Write the header.
                    ArrayType arrayType = (ArrayType) anonType;
                    header.typeKind = (byte) TypeKind.Array;
                    header.recordSize = 6;
                    header.Write(writer);

                    // Write the value type.
                    writer.Write((uint)RegisterType(arrayType.GetValueType()));
                    writer.Write((byte)arrayType.GetDimensions());
                    writer.Write((byte)(arrayType.IsReadOnly() ? 1 : 0));
                }
                else if(anonType.IsVector())
                {
                    // Write the header.
                    VectorType vectorType = (VectorType)anonType;
                    header.typeKind = (byte)TypeKind.Vector;
                    header.recordSize = 5;
                    header.Write(writer);

                    // Write the primitive type.
                    writer.Write((uint)RegisterType(vectorType.GetPrimitiveType()));

                    // Write the number of components.
                    writer.Write((byte)vectorType.GetNumComponents());
                }
                else if(anonType.IsMatrix())
                {
                    // Write the header.
                    MatrixType matrixType = (MatrixType)anonType;
                    header.typeKind = (byte)TypeKind.Matrix;
                    header.recordSize = 6;
                    header.Write(writer);

                    // Write the primitive type.
                    writer.Write((uint)RegisterType(matrixType.GetPrimitiveType()));

                    // Write the number of rows and columns.
                    writer.Write((byte)matrixType.GetNumRows());
                    writer.Write((byte)matrixType.GetNumColumns());
                }
				else if(anonType.IsFunction())
				{
					// Get the type.
					FunctionType functionType = (FunctionType) anonType;
					
					// Write the header.
					header.typeKind =(byte) TypeKind.Function;
					header.recordSize = (uint)(functionType.GetArgumentCount()*4 + 8);
					header.Write(writer);

					// Write the flags.
                    int flags = (int)functionType.GetFunctionFlags();
                    flags |= functionType.HasVariableArgument() ? 1 : 0;
					writer.Write((uint)flags);
					
					// Write the return type.
					writer.Write((uint)RegisterType(functionType.GetReturnType()));
					
					// Write the arguments.
					foreach(IChelaType arg in functionType.GetArguments())
						writer.Write((uint)RegisterType(arg));
				}
                else if(anonType.IsPlaceHolderType())
                {
                    // Cast the type.
                    PlaceHolderType placeholder = (PlaceHolderType)anonType;

                    // Write the header.
                    header.typeKind =(byte) TypeKind.PlaceHolder;
                    header.recordSize = (uint)(placeholder.GetBaseCount()*4 + 10);
                    header.Write(writer);

                    // Write the name.
                    writer.Write((uint)RegisterString(placeholder.GetName()));

                    // Write the id.
                    writer.Write((uint)placeholder.GetPlaceHolderId());

                    // Write the value type flag.
                    writer.Write((byte) (placeholder.IsValueType() ? 1 : 0));

                    // Write the number of bases.
                    writer.Write((byte) placeholder.GetBaseCount());

                    // Write the bases.
                    for(int i = 0; i < placeholder.GetBaseCount(); ++i)
                        writer.Write((uint) RegisterMember(placeholder.GetBase(i)));
                }
                else
                {
                    throw new ModuleException("Trying to write an invalid anon type.");
                }
			}
		}

        private void RegisterAnonTypeLeaf(IChelaType type, bool registered)
        {
            // Make sure lower types are registered.
            if(!registered)
                RegisterType(type);

            if(type.IsReference())
            {
                ReferenceType refType = (ReferenceType)type;
                RegisterAnonTypeLeaf(refType.GetReferencedType());
            }
            else if(type.IsPointer())
            {
                PointerType pointerType = (PointerType)type;
                RegisterAnonTypeLeaf(pointerType.GetPointedType());
            }
            else if(type.IsConstant())
            {
                ConstantType constType = (ConstantType)type;
                RegisterAnonTypeLeaf(constType.GetValueType());
            }
            else if(type.IsArray())
            {
                ArrayType arrayType = (ArrayType)type;
                RegisterAnonTypeLeaf(arrayType.GetValueType());
            }
            else if(type.IsVector())
            {
                VectorType vectorType = (VectorType)type;
                RegisterAnonTypeLeaf(vectorType.GetPrimitiveType());
            }
            else if(type.IsMatrix())
            {
                MatrixType matrixType = (MatrixType)type;
                RegisterAnonTypeLeaf(matrixType.GetPrimitiveType());
            }
            else if(type.IsFunction())
            {
                FunctionType functionType = (FunctionType)type;

                // Register the return type.
                RegisterAnonTypeLeaf(functionType.GetReturnType());

                // Register the function arguments.
                for(int i = 0; i < functionType.GetArgumentCount(); ++i)
                    RegisterAnonTypeLeaf(functionType.GetArgument(i));
            }
            else if(type.IsPlaceHolderType())
            {
                // Register the base types.
                PlaceHolderType placeholder = (PlaceHolderType)type;
                for(int i = 0; i < placeholder.GetBaseCount(); ++i)
                    RegisterMember(placeholder.GetBase(i));
            }
            else if(type.IsPrimitive())
            {
                // Do nothing.
            }
            else if(type.IsTypeInstance())
            {
                StructureInstance instance = (StructureInstance)type;
                instance.PrepareSerialization();
                RegisterMember(instance);
            }
            else
            {
                // Found a leaf.
                ScopeMember member = (ScopeMember)type;
                RegisterMember(member);
            }
        }

        private void RegisterAnonTypeLeaf(IChelaType type)
        {
            RegisterAnonTypeLeaf(type, false);
        }

        private void PrepareTypeReferences()
        {
            // Register anonymous types leafs.
            // New members are added at the end, so its safe to use a for loop.
            for(int i = 0; i < anonymousTypes.Count; ++i)
                RegisterAnonTypeLeaf((IChelaType)anonymousTypes[i], true);
        }

        private void PrepareResources()
        {
            // Register the resources names.
            foreach(ResourceData resource in resources)
                RegisterString(resource.Name);
        }

        private void WriteResources(ModuleWriter writer)
        {
            // Compute the initial offsets.
            int numresources = resources.Count;
            int headerSize = numresources*12;
            int nextOffset = (int)writer.GetPosition() + headerSize + /*numresources*/4;

            // Sort the resources by name.
            resources.Sort(delegate(ResourceData r1, ResourceData r2) {
                return string.Compare(r1.Name, r2.Name);
            });

            // Write the number of resources.
            writer.Write(numresources);
            
            // Write the resource headers.
            foreach(ResourceData resource in resources)
            {
                // Writer the resource metadata.
                uint name = RegisterString(resource.Name);
                int offset = nextOffset;
                int length = resource.Length;
                writer.Write(name);
                writer.Write(offset);
                writer.Write(length);

                // Increase the next offset.
                nextOffset += length;
            }

            // Write the resource data.
            foreach(ResourceData resource in resources)
                writer.Write(resource.File);
        }

        private static void SkipAttributes(ModuleReader reader, int count)
        {
            for(int i = 0; i < count; ++i)
                AttributeConstant.Skip(reader);
        }

        public static ChelaModule LoadModule(ModuleReader reader, bool fullLoad)
        {
            // Try to guess the file type.
            uint moduleStart = reader.GetPosition();
            byte[] signature;
            reader.Read(out signature, 8);

            // Check for elf file type.
            bool isElf = true;
            for(int i = 0; i < 4; ++i)
            {
                if(signature[i] != ElfFormat.ElfMagic[i])
                {
                    isElf = false;
                    break;
                }
            }

            // Check for PE.
            bool isPE = signature[0] == 'M' && signature[1] == 'Z';

            // Find the module content.
            reader.SetPosition(moduleStart);
            if(isElf)
            {
                // Move to the embedded module.
                ElfFormat.GetEmbeddedModule(reader);
                reader.FixBase();
            }
            else if(isPE)
            {
                // Move to the embedded module.
                PEFormat.GetEmbeddedModule(reader);
                reader.FixBase();
            }

            // Read the module header.
            ModuleHeader header = new ModuleHeader();
            header.Read(reader);

            // Create the output module.
            ChelaModule result = new ChelaModule();

            // Read the string table.
            reader.SetPosition(header.stringTableOffset);
            for(uint i = 0; i < header.stringTableEntries; ++i)
            {
                uint id = i+1u;
                string s = reader.ReadString();
                result.stringTable.Add(s);
                result.registeredStrings.Add(s, id);
            }

            // Read myself data.
            ModuleReference modref = new ModuleReference();
            reader.SetPosition(header.moduleRefTableOffset);
            modref.Read(reader);
            result.SetName(result.GetString(modref.moduleName));

            // Read the module type.
            result.moduleType = (ModuleType)header.moduleType;

            // Read the referenced modules.
            for(uint i = 1; i < header.moduleRefTableEntries; ++i)
            {
                // Read the referenced module.
                modref.Read(reader);

                // Load it.
                ChelaModule referencedModule = LoadNamedModule(result.GetString(modref.moduleName));
                result.referencedModules.Add(referencedModule);
            }

            // Preload the members.
            reader.SetPosition(header.memberTableOffset);
            uint end = header.memberTableOffset + header.memberTableSize;
            MemberHeader mheader = new MemberHeader();
            while(reader.GetPosition() < end)
            {
                // Read the member header.
                mheader.Read(reader);

                // Skip the attributes.
                SkipAttributes(reader, mheader.memberAttributes);

                // Preload the member.
                MemberHeaderType mtype = (MemberHeaderType)mheader.memberType;
                switch(mtype)
                {
                case MemberHeaderType.Namespace:
                    Namespace.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Structure:
                    Structure.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Class:
                    Class.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Interface:
                    Interface.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Function:
                    Function.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Field:
                    FieldVariable.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.FunctionGroup:
                    FunctionGroup.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.TypeInstance:
                    StructureInstance.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Property:
                    PropertyVariable.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Event:
                    EventVariable.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.TypeName:
                    TypeNameMember.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.MemberInstance:
                case MemberHeaderType.FunctionInstance:
                    // Ignore the member instance.
                    result.memberTable.Add(null);
                    reader.Skip(mheader.memberSize);
                    break;
                case MemberHeaderType.TypeGroup:
                    TypeGroup.PreloadMember(result, reader, mheader);
                    break;
                case MemberHeaderType.Reference:
                    {
                        // Read the module id.
                        byte moduleId = reader.ReadByte();

                        // Get the referenced module.
                        ChelaModule refMod = result.GetReferenced(moduleId);

                        // Find the member there.
                        ScopeMember member = refMod.GetMember(result.GetString(mheader.memberName));
                        result.memberTable.Add(member);
                    }
                    break;
                default:
                    // Unknown member.
                    System.Console.WriteLine("add unknown member {0}", mtype);
                    result.memberTable.Add(null);
                    reader.Skip(mheader.memberSize);
                    break;
                }
            }

            // Read the type table.
            reader.SetPosition(header.typeTableOffset);
            for(uint i = 0; i < header.typeTableEntries; ++i)
            {
                // Read the type reference.
                TypeReference typeRef = new TypeReference();
                typeRef.Read(reader);

                // Try to load direct members.
                TypeKind kind = (ChelaModule.TypeKind)typeRef.typeKind;
                if(kind == TypeKind.Class || kind == TypeKind.Interface ||
                   kind == TypeKind.Structure || kind == TypeKind.Instance)
                {
                    // Store the actual type.
                    result.typeTable.Add(result.GetMember(typeRef.memberId));
                }
                else
                {
                    // Store the reference for delayed loading.
                    result.typeTable.Add(typeRef);
                }
            }

            // Read the anonymous types.
            reader.SetPosition(header.anonTypeTableOffset);
            end = header.anonTypeTableOffset + header.anonTypeTableSize;
            while(reader.GetPosition() < end)
            {
                // Read the anonymous type data.
                AnonTypeHeader anonType = new AnonTypeHeader();
                anonType.Read(reader);

                // Store it in the anonymous type table.
                result.anonymousTypes.Add(anonType);
            }

            // Now, read the actual members.
            reader.SetPosition(header.memberTableOffset);
            for(int i = 0; i < result.memberTable.Count; ++i)
            {
                // Get the preloaded member.
                ScopeMember member = result.memberTable[i];

                // Read his header.
                mheader.Read(reader);

                // Skip the attributes.
                SkipAttributes(reader, mheader.memberAttributes);

                // Read the member.
                if(member != null && mheader.memberType != (int)MemberHeaderType.Reference)
                    member.Read(reader, mheader);
                else
                    reader.Skip(mheader.memberSize); // Skip unknown members.
            }

            // The first member must be the global namespace.
            result.globalNamespace = (Namespace)result.GetMember(1);

            // Update the parent children relationship.
            result.globalNamespace.UpdateParent(null);

            // Now, perform second read pass, required by function groups.
            reader.SetPosition(header.memberTableOffset);
            for(int i = 0; i < result.memberTable.Count; ++i)
            {
                // Get the preloaded member.
                ScopeMember member = result.memberTable[i];

                // Read his header.
                mheader.Read(reader);

                // Skip the attributes.
                SkipAttributes(reader, mheader.memberAttributes);

                // Read the member.
                if(member != null && mheader.memberType != (int)MemberHeaderType.Reference)
                    member.Read2(reader, mheader);
                else
                    reader.Skip(mheader.memberSize); // Skip unknown members.
            }

            // Finish all of the loading.
            result.globalNamespace.FinishLoad();

            // Register the member names.
            for(int i = 0; i < result.memberTable.Count; ++i)
            {
                ScopeMember member = result.memberTable[i];
                if(member != null)
                    result.memberNameTable[member.GetFullName()] = member;
            }

            // Load the runtime classes.
            if(result.referencedModules.Count == 0)
                result.LoadRuntimeReferences(result);

            // Dump the readed module.
            //result.Dump();

            // Return the result module.
            return result;
        }

        public static ChelaModule LoadModule(string filename, bool fullLoad)
        {
            // Open the file.
            FileStream file = new FileStream(filename, FileMode.Open);

            // Create the module reader.
            ModuleReader reader = new ModuleReader(file);

            // Read the module.
            ChelaModule res = LoadModule(reader, fullLoad);

            // Set the loaded module filename.
            res.SetFileName(filename);

            // Return the module.
            return res;
        }

        public static ChelaModule LoadModule(string filename)
        {
            return LoadModule(filename, false);
        }

        private static bool ExistsFile(string filename)
        {
            return System.IO.File.Exists(filename);
        }

        public static ChelaModule LoadNamedModule(string name)
        {
            // Cache loaded modules.
            ChelaModule module;
            if(loadedModules.TryGetValue(name, out module))
                return module;

            // File name candidates.
            string[] candidates = new string[] {
                name, name + ".cbm", "lib" + name + ".so", name + ".dll"
            };

            // Find the module.
            string fileName = null;
            foreach(string candidate in candidates)
            {
                // Test the raw candidate.
                if(ExistsFile(candidate))
                {
                    fileName = candidate;
                    break;
                }

                // TODO: Test in the module cache.

                // Test in the library search path.
                foreach(string path in libraryPaths)
                {
                    string test = System.IO.Path.Combine(path, candidate);
                    if(ExistsFile(test))
                    {
                        fileName = test;
                        break;
                    }
                }

                // Stop searching early.
                if(fileName != null)
                    break;
            }

            // Make sure the module was found
            if(fileName == null)
                throw new ModuleException("Couldn't find module " + name);

            // Load the module.
            module = LoadModule(fileName);

            // Register it.
            loadedModules.Add(module.GetName(), module);

            // Return the module.
            return module;
        }

		public void Dump()
		{
			// Dump the name.
			Dumper.Printf("; name = %s", this.name);
			Dumper.Printf("");

            // Dump the references.
            Dumper.Printf("references");
            Dumper.Printf("{");
            Dumper.Incr();
            
            foreach(ChelaModule refMod in referencedModules)
                Dumper.Printf("%s,", refMod.GetName());

            Dumper.Decr();
            Dumper.Printf("}");
			Dumper.Printf("");

			// Dump the global namespace.
			globalNamespace.Dump();
		}
	}
}

