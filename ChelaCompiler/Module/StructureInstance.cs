using System.IO;

namespace Chela.Compiler.Module
{
    public class StructureInstance: Structure
    {
        private Structure template;
        private GenericInstance genericInstance;
        private ScopeMember factory;
        private bool instancedMembers = false;
        private bool instancedBases = false;
        private byte[] rawData = null;
        private bool readedData = false;

        public StructureInstance (Structure template, GenericInstance instance,
            ScopeMember factory, ChelaModule module)
            : base(module)
        {
            this.factory = factory;
            Initialize(template, instance);
        }

        private StructureInstance (ChelaModule module)
            : base(module)
        {
        }

        private void Initialize(Structure template, GenericInstance instance)
        {
            // Copy the template name and flags.
            this.name = template.GetName();
            this.flags = template.flags;

            // Store the template and the generic instance.
            this.template = template;
            this.genericInstance = instance;

            // Copy the prototype if this is not the final instance.
            SetGenericPrototype(template.GetGenericPrototype());

            // Use the factory
            this.parentScope = (Scope)factory;
            template.genericInstanceList.Add(this);

            // Register myself in the module.
            GetModule().RegisterGenericInstance(this);

            // Get the complete instance.
            GetFullGenericInstance();
        }

        private void InstanceBases()
        {
            // Only instance once.
            if(instancedBases || template.IncompleteBases)
                return;
            instancedBases = true;

            // Full instance.
            GenericInstance fullInstance = GetFullGenericInstance();

            // Instance the base.
            Structure building = template.GetBase();
            if(building != null)
                baseStructure = (Structure)building.InstanceGeneric(fullInstance, GetModule());

            // Instance the interfaces.
            for(int i = 0; i < template.GetInterfaceCount(); ++i)
            {
                Structure iface = template.GetInterface(i);
                iface = (Structure)iface.InstanceGeneric(fullInstance, GetModule());
                interfaces.Add(iface);
            }
        }

        public override Structure GetBase ()
        {
            // Instance my bases
            InstanceBases();

            // Return the base.
            return base.GetBase ();
        }

        public override bool IsDerivedFrom(Structure candidate)
        {
            // Instance my bases.
            InstanceBases();

            return base.IsDerivedFrom(candidate);
        }

        public override bool Implements(Structure candidate)
        {
            // Instance my bases.
            InstanceBases();

            return base.Implements(candidate);
        }

        public override int GetInterfaceCount ()
        {
            // Instance my bases.
            InstanceBases();

            // Return the interface count.
            return base.GetInterfaceCount ();
        }

        public override bool IsGenericInstance()
        {
            return true;
        }

        public override bool IsTypeInstance()
        {
            return true;
        }

        public override bool IsPassedByReference ()
        {
            return template.IsPassedByReference();
        }

        public override bool IsStructure ()
        {
            return template.IsStructure();
        }

        public override bool IsInterface ()
        {
            return template.IsInterface();
        }

        public override bool IsClass ()
        {
            return template.IsClass();
        }

        public override bool IsGeneric()
        {
            // Handle type instance dependencies.
            if(parentScope == null)
            {
                ReadData();
                if(parentScope == null)
                    throw new ModuleException("Cannot have cyclic type instances.");
            }

            return base.IsGeneric();
        }

        public override Structure GetTemplateBuilding()
        {
            return template.GetTemplateBuilding();
        }
        
        public override GenericInstance GetGenericInstance ()
        {
            return genericInstance;
        }

        public override IChelaType InstanceGeneric (GenericInstance instance, ChelaModule instanceModule)
        {
            // Instance the instance.
            GenericInstance newInstance = genericInstance.InstanceFrom(instance, instanceModule);

            // Instance the structure.
            return template.InstanceGeneric(newInstance, instanceModule);
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance instance)
        {
            // Instance the instance.
            ChelaModule module = factory.GetModule();
            GenericInstance newInstance = genericInstance.InstanceFrom(instance, module);

            // Instance the structure.
            return template.InstanceMember(factory, newInstance);
        }

        public override FunctionGroup GetConstructor ()
        {
            if(constructor == null)
                constructor = (FunctionGroup)template.GetConstructor().InstanceMember(this, GetFullGenericInstance());
            return constructor;
        }
        
        public override ScopeMember FindMember (string member)
        {
            // Find the instanced member.
            ScopeMember instanced;
            if(this.members.TryGetValue(member, out instanced))
                return instanced;

            // Find the templated member.
            ScopeMember templated = template.FindMember(member);
            if(templated != null)
            {
                // Instance the templated member.
                instanced = templated.InstanceMember(this, GetFullGenericInstance());
                this.members.Add(member, instanced);
                return instanced;
            }

            // Couldn't find.
            return null;
        }

        public override ScopeMember FindMemberRecursive (string member)
        {
            // Instance the bases.
            InstanceBases();

            // Use the inherited implementation.
            return base.FindMemberRecursive (member);
        }

        private void InstanceMembers()
        {
            if(instancedMembers)
                return;
            instancedMembers = true;

            // Instance all of the members.
            foreach(ScopeMember member in template.memberList)
            {
                string name = member.GetName();
                if(!members.ContainsKey(name))
                {
                    ScopeMember instanced = member.InstanceMember(this, GetFullGenericInstance());
                    this.members.Add(member.GetName(), instanced);
                    this.memberList.Add(instanced);
                }
                else
                {
                    this.memberList.Add(members[name]);
                }
            }
        }

        public override void FixInheritance ()
        {
            // Instance the bases.
            InstanceBases();

            // Instance the members.
            InstanceMembers();

            // Fix the inheritance.
            base.FixInheritance ();
        }

        internal override void PrepareSerialization ()
        {
            //base.PrepareSerialization ();
            PrepareSerializationNoBase();

            // Register the template.
            ChelaModule module = GetModule();
            module.RegisterMember(template);
            module.RegisterMember(factory);

            // Register the types used.
            genericInstance.PrepareSerialization(module);
        }

        public override void Write (ModuleWriter writer)
        {
            // Get the module.
            ChelaModule module = GetModule();

            // Create the member header.
            MemberHeader mheader = new MemberHeader();
            mheader.memberType = (byte)MemberHeaderType.TypeInstance;
            mheader.memberFlags = (uint) GetFlags();
            mheader.memberName = 0;
            mheader.memberSize = (uint)(8 + genericInstance.GetSize());
            mheader.memberAttributes = 0;

            // Write the member header.
            mheader.Write(writer);

            // Write the template id.
            writer.Write(module.RegisterMember(template));

            // Write the factory id.
            writer.Write(module.RegisterMember(factory));

            // Write the template parameters.
            genericInstance.Write(writer, GetModule());
        }

        internal static new void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Create the temporal structure instance.
            StructureInstance instance = new StructureInstance(module);
            module.RegisterMember(instance);

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read (ModuleReader reader, MemberHeader header)
        {
            reader.Read(out rawData, (int)header.memberSize);
        }

        internal override void Read2 (ModuleReader reader, MemberHeader header)
        {
            // Skip the data.
            reader.Skip(header.memberSize);

            // Handle type instance dependencies.
            ReadData();
        }

        private void ReadData()
        {
            // Only read once the data.
            if(readedData)
                return;
            readedData = true;

            ModuleReader reader = new ModuleReader(new MemoryStream(rawData));

            // Get the module.
            ChelaModule module = GetModule();

            // Read the template.
            template = (Structure)module.GetMember(reader.ReadUInt());

            // Read the factory.
            factory = (Scope)module.GetMember(reader.ReadUInt());

            // Read the generic instance.
            genericInstance = GenericInstance.Read(template.GetGenericPrototype(), reader, module);

            // Initialize.
            Initialize(template, genericInstance);
            rawData = null;
        }

        public override bool IsInstanceOf(ScopeMember template)
        {
            if(template == this.template)
                return true;
            if(factory != null)
                return factory.IsInstanceOf(this.template);
            return false;
        }
    }
}

