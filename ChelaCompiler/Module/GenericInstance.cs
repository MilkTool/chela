using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    public class GenericInstance: IEquatable<GenericInstance>
    {
        private GenericPrototype prototype;
        private IChelaType[] parameters;
        private string name;
        private string displayName;
        private string fullName;
        private bool isComplete;

        public GenericInstance (GenericPrototype prototype, IChelaType[] parameters)
        {
            if(prototype == null)
                throw new ArgumentNullException("prototype");
            this.prototype = prototype;
            this.parameters = parameters;
            this.name = null;
            this.fullName = null;
            CheckCompletion();
        }

        public GenericInstance(GenericPrototype prototype, List<IChelaType> parameters)
        {
            if(prototype == null)
                throw new ArgumentNullException("prototype");

            // Check the parameter count.
            if(parameters.Count != prototype.GetPlaceHolderCount())
                throw new ModuleException("invalid generic parameter count.");

            this.prototype = prototype;
            this.parameters = parameters.ToArray();
            CheckCompletion();
        }

        public override int GetHashCode ()
        {
            int hash = parameters.Length;
            for(int i = 0; i < parameters.Length; ++i)
                hash ^= RuntimeHelpers.GetHashCode(parameters[i]);

            return hash;
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as GenericInstance);
        }

        public bool Equals (GenericInstance obj)
        {
            // Check first differences.
            if(obj == null ||
                parameters.Length != obj.parameters.Length)
                return false;

            // Check each paramter.
            for(int i = 0; i < parameters.Length; ++i)
            {
                // Handle pass by reference.
                IChelaType leftParam = parameters[i];
                if(leftParam.IsPassedByReference())
                    leftParam = ReferenceType.Create(leftParam);

                // Handle pass by reference.
                IChelaType rightParam = obj.parameters[i];
                if(rightParam.IsPassedByReference())
                    rightParam = ReferenceType.Create(rightParam);

                // Compare the types.
                if(leftParam != rightParam)
                    return false;
            }

            // No differences found.
            return true;
        }
        public GenericPrototype GetPrototype()
        {
            return prototype;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public IChelaType GetParameter(int index)
        {
            return parameters[index];
        }

        private void CheckCompletion()
        {
            for(int i = 0; i < parameters.Length; ++i)
            {
                if(parameters[i].IsGenericType())
                {
                    isComplete = false;
                    return;
                }
            }

            // No generic types found.
            isComplete = true;
        }

        public bool IsComplete()
        {
            return isComplete;
        }

        public string GetName()
        {
            if(name == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('<');
                for(int i = 0; i < parameters.Length; ++i)
                {
                    builder.Append(parameters[i].GetName());
                    if(i + 1 < parameters.Length)
                        builder.Append(',');
                }
                builder.Append('>');
                name = builder.ToString();
            }
            return name;
        }

        public string GetDisplayName()
        {
            if(displayName == null && parameters.Length == 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('<');
                for(int i = 0; i < parameters.Length; ++i)
                {
                    builder.Append(parameters[i].GetDisplayName());
                    if(i + 1 < parameters.Length)
                        builder.Append(',');
                }
                builder.Append('>');
                displayName = builder.ToString();
            }
            else if(displayName == null)
                displayName = string.Empty;

            return displayName;
        }

        public string GetFullName()
        {
            if(fullName == null && parameters.Length != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('<');
                for(int i = 0; i < parameters.Length; ++i)
                {
                    builder.Append(parameters[i].GetFullName());
                    if(i + 1 < parameters.Length)
                        builder.Append(',');
                }
                builder.Append('>');
                fullName = builder.ToString();
            }
            else if(fullName == null)
                fullName = string.Empty;
            return fullName;
        }
        
        public int GetSize()
        {
            return parameters.Length*4 + 1;
        }

        public GenericInstance InstanceFrom(GenericInstance instance, ChelaModule instModule)
        {
            // Avoid instancing.
            if(instance == null)
                return this;

            // Instance the arguments.
            IChelaType[] arguments = new IChelaType[parameters.Length];
            for(int i = 0; i < parameters.Length; ++i)
                arguments[i] = parameters[i].InstanceGeneric(instance, instModule);

            // Create the new instance.
            return new GenericInstance(prototype, arguments);
        }

        internal void PrepareSerialization(ChelaModule module)
        {
            for(int i = 0; i < parameters.Length; ++i)
                module.RegisterType(parameters[i]);
        }

        public void Write(ModuleWriter writer, ChelaModule module)
        {
            // Write the type count.
            writer.Write((byte)parameters.Length);

            // Write the types.
            for(int i = 0; i < parameters.Length; ++i)
                writer.Write((uint)module.RegisterType(parameters[i]));
        }

        public static GenericInstance Read(GenericPrototype prototype, ModuleReader reader, ChelaModule module)
        {
            // Read the type count.
            int count = reader.ReadByte();

            // Read the types.
            IChelaType[] types = new IChelaType[count];
            for(int i = 0; i < count; ++i)
                types[i] = module.GetType(reader.ReadUInt());

            // Create the readed instance.
            return new GenericInstance(prototype, types);
        }
    }
}

