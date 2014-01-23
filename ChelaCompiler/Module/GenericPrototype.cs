using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Generic prototype.
    /// </summary>
    public class GenericPrototype: IEquatable<GenericPrototype>
    {
        public static readonly GenericPrototype Empty = new GenericPrototype(new PlaceHolderType[] {});
        private readonly PlaceHolderType[] placeHolders;
        private string fullName;
        private string displayName;
        private string mangledName;

        public GenericPrototype (PlaceHolderType[] placeHolders)
        {
            this.placeHolders = placeHolders;
            this.fullName = null;
            this.mangledName = null;
        }

        public GenericPrototype (List<PlaceHolderType> placeholders)
            : this(placeholders.ToArray())
        {
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string GetFullName()
        {
            if(fullName == null && placeHolders.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('<');
                for(int i = 0; i < placeHolders.Length; ++i)
                {
                    builder.Append(placeHolders[i].GetFullName());
                    if(i + 1 < placeHolders.Length)
                        builder.Append(',');
                }
                builder.Append('>');
                fullName = builder.ToString();
            }
            else if(fullName == null)
                fullName = string.Empty;
            
            return fullName;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string GetDisplayName()
        {
            if(displayName == null && placeHolders.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('<');
                for(int i = 0; i < placeHolders.Length; ++i)
                {
                    builder.Append(placeHolders[i].GetDisplayName());
                    if(i + 1 < placeHolders.Length)
                        builder.Append(',');
                }
                builder.Append('>');
                displayName = builder.ToString();
            }
            else if(displayName == null)
                displayName = string.Empty;
            
            return displayName;
        }

        /// <summary>
        /// Gets the mangled name.
        /// </summary>
        public string GetMangledName()
        {
            return mangledName;
        }

        public int GetPlaceHolderCount()
        {
            return placeHolders.Length;
        }

        public PlaceHolderType GetPlaceHolder(int index)
        {
            return placeHolders[index];
        }

        public int GetSize()
        {
            return placeHolders.Length*4 + 1;
        }

        public GenericInstance InstanceFrom(GenericInstance instance, ChelaModule module)
        {
            IChelaType[] instancedTypes = new IChelaType[placeHolders.Length];
            for(int i = 0; i < placeHolders.Length; ++i)
                instancedTypes[i] = placeHolders[i].InstanceGeneric(instance, module);

            GenericInstance res = new GenericInstance(this, instancedTypes);
            return res;
        }

        public override int GetHashCode()
        {
            int hash = placeHolders.Length;
            for(int i = 0; i < placeHolders.Length; ++i)
                hash ^= placeHolders[i].GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            // Avoid casting.
            if(obj == this)
                return true;
            return Equals(obj as GenericPrototype);
        }

        public bool Equals(GenericPrototype obj)
        {
            // Check first differences.
            if(obj == null ||
               placeHolders.Length != obj.placeHolders.Length)
                return false;

            // Check differences in the place holders.
            for(int i = 0; i < placeHolders.Length; ++i)
            {
                if(!placeHolders[i].Equals(obj.placeHolders[i]))
                    return false;
            }

            // No differences found.
            return true;
        }

        public void PrepareSerialization(ChelaModule module)
        {
            // Register the placeholders
            for(int i = 0; i < placeHolders.Length; ++i)
            {
                PlaceHolderType placeHolder = placeHolders[i];
                module.RegisterType(placeHolder);
            }
        }

        public void Write(ModuleWriter writer, ChelaModule module)
        {
            // Write the placeholder count.
            writer.Write((byte)placeHolders.Length);

            // Write the placeholders.
            for(int i = 0; i < placeHolders.Length; ++i)
            {
                PlaceHolderType placeHolder = placeHolders[i];
                writer.Write((uint)module.RegisterType(placeHolder));
            }
        }

        public static GenericPrototype Read(ModuleReader reader, ChelaModule module)
        {
            byte count;
            uint placeHolder;

            // Read the place holder count;
            reader.Read(out count);

            // Read the place holders.
            PlaceHolderType[] placeHolders = new PlaceHolderType[count];
            for(int i = 0; i < count; ++i)
            {
                reader.Read(out placeHolder);
                placeHolders[i] = (PlaceHolderType)module.GetType(placeHolder);
            }

            // Return the prototype.
            return new GenericPrototype(placeHolders);
        }
    }
}

