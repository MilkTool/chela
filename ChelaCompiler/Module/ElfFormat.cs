using System.Text;

// Elf32 data types.
using Elf32_Addr = System.UInt32;
using Elf32_Half = System.UInt16;
using Elf32_Off = System.UInt32;
using Elf32_Sword = System.Int32;
using Elf32_Word = System.UInt32;

// Elf64 data types
using Elf64_Addr = System.UInt64;
using Elf64_Half = System.UInt16;
using Elf64_Off = System.UInt64;
using Elf64_Sword = System.Int32;
using Elf64_Sxword = System.Int64;
using Elf64_Word = System.UInt32;
using Elf64_Xword = System.UInt64;
using Elf64_Byte = System.Byte;
using Elf64_Section = System.UInt16;

namespace Chela.Compiler.Module
{
    public static class ElfFormat
    {
        public static readonly byte[] ElfMagic = new byte[] {(byte)0x7f, (byte)'E', (byte)'L', (byte)'F'};

        public enum ElfType
        {
            ET_NONE   = 0,
            ET_REL    = 1,
            ET_EXEC   = 2,
            ET_DYN    = 3,
            ET_CORE   = 4,
            ET_LOPROC = 0xff00,
            ET_HIPROC = 0xffff,
        };

        public enum ElfMachine
        {
            EM_NONE        = 0,
            EM_M32         = 1,
            EM_SPARC       = 2,
            EM_386         = 3,
            EM_68K         = 4,
            EM_88K         = 5,
            EM_860         = 7,
            EM_MIPS        = 8,
            EM_MIPS_RS4_BE = 10,
        };

        public enum ElfVersion
        {
            EV_NONE    = 0,
            EV_CURRENT = 1,
        };

        public enum ElfIdent
        {
            EI_MAG0 = 0,
            EI_MAG1 = 1,
            EI_MAG2 = 2,
            EI_MAG3 = 3,
            EI_CLASS = 4,
            EI_DATA = 5,
            EI_VERSION = 6,
            EI_PAD = 7,
            EI_NIDENT = 16,
        };

        public enum ElfClass
        {
            ELFCLASSNONE = 0,
            ELFCLASS32   = 1,
            ELFCLASS64   = 2,
        };

        public enum ElfData
        {
            ELFDATANONE = 0,
            ELFDATA2LSB = 1,
            ELFDATA2MSB = 2,
        };

        public enum ElfSectionNumber
        {
            SHN_UNDEF     = 0,
            SHN_LORESERVE = 0xFF00,
            SHN_LOPROC    = 0xFF00,
            SHN_HIPROC    = 0xFF1F,
            SHN_ABS       = 0xFFF1,
            SHN_COMMON    = 0xFFF2,
            SHN_HIRESERVE = 0xFFFF,
        };

        public enum ElfSectionType:uint
        {
            SHT_NULL        = 0,
            SHT_PROGBITS    = 1,
            SHT_SYMTAB      = 2,
            SHT_STRTAB      = 3,
            SHT_RELA        = 4,
            SHT_HASH        = 5,
            SHT_DYNAMIC     = 6,
            SHT_NOTE        = 7,
            SHT_NOBITS      = 8,
            SHT_REL         = 9,
            SHT_SHLIB       = 10,
            SHT_DYNSYM      = 11,
            SHT_LOPROC      = 0x70000000,
            SHT_HIPROC      = 0x7FFFFFFF,
            SHT_LOUSER      = 0x80000000,
            SHT_HIUSER      = 0xFFFFFFFF,
        };

        public enum ElfSectionFlags:uint
        {
            SHF_WRITE     = 0x1,
            SHF_ALLOC     = 0x2,
            SHF_EXECINSTR = 0x4,
            SHF_MASKPROC  = 0xf0000000,
        };

        // Elf32 header.
        public struct Elf32_Ehdr
        {
            //unsigned char   e_ident[EI_NIDENT];
            public Elf32_Half      e_type;
            public Elf32_Half      e_machine;
            public Elf32_Word      e_version;
            public Elf32_Addr      e_entry;
            public Elf32_Off       e_phoff;
            public Elf32_Off       e_shoff;
            public Elf32_Word      e_flags;
            public Elf32_Half      e_ehsize;
            public Elf32_Half      e_phentsize;
            public Elf32_Half      e_phnum;
            public Elf32_Half      e_shentsize;
            public Elf32_Half      e_shnum;
            public Elf32_Half      e_shstrndex;

            public void Read(ModuleReader reader, bool big)
            {
                reader.Read(out e_type);
                reader.Read(out e_machine);
                reader.Read(out e_version);
                reader.Read(out e_entry);
                reader.Read(out e_phoff);
                reader.Read(out e_shoff);
                reader.Read(out e_flags);
                reader.Read(out e_ehsize);
                reader.Read(out e_phentsize);
                reader.Read(out e_phnum);
                reader.Read(out e_shentsize);
                reader.Read(out e_shnum);
                reader.Read(out e_shstrndex);

                // Swap bytes.
                if(big)
                {
                    ModuleReader.SwapBytesD(ref e_type);
                    ModuleReader.SwapBytesD(ref e_machine);
                    ModuleReader.SwapBytesD(ref e_version);
                    ModuleReader.SwapBytesD(ref e_entry);
                    ModuleReader.SwapBytesD(ref e_phoff);
                    ModuleReader.SwapBytesD(ref e_shoff);
                    ModuleReader.SwapBytesD(ref e_flags);
                    ModuleReader.SwapBytesD(ref e_ehsize);
                    ModuleReader.SwapBytesD(ref e_phentsize);
                    ModuleReader.SwapBytesD(ref e_phnum);
                    ModuleReader.SwapBytesD(ref e_shentsize);
                    ModuleReader.SwapBytesD(ref e_shnum);
                    ModuleReader.SwapBytesD(ref e_shstrndex);
                }
            }
        }

        // Elf64 header
        public struct Elf64_Ehdr
        {
            //unsigned char   e_ident[EI_NIDENT];
            public Elf64_Half      e_type;
            public Elf64_Half      e_machine;
            public Elf64_Word      e_version;
            public Elf64_Addr      e_entry;
            public Elf64_Off       e_phoff;
            public Elf64_Off       e_shoff;
            public Elf64_Word      e_flags;
            public Elf64_Half      e_ehsize;
            public Elf64_Half      e_phentsize;
            public Elf64_Half      e_phnum;
            public Elf64_Half      e_shentsize;
            public Elf64_Half      e_shnum;
            public Elf64_Half      e_shstrndex;

            public void Read(ModuleReader reader, bool big)
            {
                reader.Read(out e_type);
                reader.Read(out e_machine);
                reader.Read(out e_version);
                reader.Read(out e_entry);
                reader.Read(out e_phoff);
                reader.Read(out e_shoff);
                reader.Read(out e_flags);
                reader.Read(out e_ehsize);
                reader.Read(out e_phentsize);
                reader.Read(out e_phnum);
                reader.Read(out e_shentsize);
                reader.Read(out e_shnum);
                reader.Read(out e_shstrndex);

                // Swap bytes.
                if(big)
                {
                    ModuleReader.SwapBytesD(ref e_type);
                    ModuleReader.SwapBytesD(ref e_machine);
                    ModuleReader.SwapBytesD(ref e_version);
                    ModuleReader.SwapBytesD(ref e_entry);
                    ModuleReader.SwapBytesD(ref e_phoff);
                    ModuleReader.SwapBytesD(ref e_shoff);
                    ModuleReader.SwapBytesD(ref e_flags);
                    ModuleReader.SwapBytesD(ref e_ehsize);
                    ModuleReader.SwapBytesD(ref e_phentsize);
                    ModuleReader.SwapBytesD(ref e_phnum);
                    ModuleReader.SwapBytesD(ref e_shentsize);
                    ModuleReader.SwapBytesD(ref e_shnum);
                    ModuleReader.SwapBytesD(ref e_shstrndex);
                }
            }
        }

        // Elf32 section.
        public struct Elf32_Shdr
        {
            public const Elf32_Word Size = 40;

            public Elf32_Word  sh_name;
            public Elf32_Word  sh_type;
            public Elf32_Word  sh_flags;
            public Elf32_Addr  sh_addr;
            public Elf32_Off   sh_offset;
            public Elf32_Word  sh_size;
            public Elf32_Word  sh_link;
            public Elf32_Word  sh_info;
            public Elf32_Word  sh_addralign;
            public Elf32_Word  sh_entsize;

            public void Read(ModuleReader reader, bool big)
            {
                reader.Read(out sh_name);
                reader.Read(out sh_type);
                reader.Read(out sh_flags);
                reader.Read(out sh_addr);
                reader.Read(out sh_offset);
                reader.Read(out sh_size);
                reader.Read(out sh_link);
                reader.Read(out sh_info);
                reader.Read(out sh_addralign);
                reader.Read(out sh_entsize);
    
                if(big)
                {
                    ModuleReader.SwapBytesD(ref sh_name);
                    ModuleReader.SwapBytesD(ref sh_type);
                    ModuleReader.SwapBytesD(ref sh_flags);
                    ModuleReader.SwapBytesD(ref sh_addr);
                    ModuleReader.SwapBytesD(ref sh_offset);
                    ModuleReader.SwapBytesD(ref sh_size);
                    ModuleReader.SwapBytesD(ref sh_link);
                    ModuleReader.SwapBytesD(ref sh_info);
                    ModuleReader.SwapBytesD(ref sh_addralign);
                    ModuleReader.SwapBytesD(ref sh_entsize);
                }
            }
        }

        // Elf64 section.
        public struct Elf64_Shdr
        {
            public const Elf64_Word Size = 64;
    
            public Elf64_Word  sh_name;
            public Elf64_Word  sh_type;
            public Elf64_Xword sh_flags;
            public Elf64_Addr  sh_addr;
            public Elf64_Off   sh_offset;
            public Elf64_Xword sh_size;
            public Elf64_Word  sh_link;
            public Elf64_Word  sh_info;
            public Elf64_Xword sh_addralign;
            public Elf64_Xword sh_entsize;

            public void Read(ModuleReader reader, bool big)
            {
                reader.Read(out sh_name);
                reader.Read(out sh_type);
                reader.Read(out sh_flags);
                reader.Read(out sh_addr);
                reader.Read(out sh_offset);
                reader.Read(out sh_size);
                reader.Read(out sh_link);
                reader.Read(out sh_info);
                reader.Read(out sh_addralign);
                reader.Read(out sh_entsize);

                if(big)
                {
                    ModuleReader.SwapBytesD(ref sh_name);
                    ModuleReader.SwapBytesD(ref sh_type);
                    ModuleReader.SwapBytesD(ref sh_flags);
                    ModuleReader.SwapBytesD(ref sh_addr);
                    ModuleReader.SwapBytesD(ref sh_offset);
                    ModuleReader.SwapBytesD(ref sh_size);
                    ModuleReader.SwapBytesD(ref sh_link);
                    ModuleReader.SwapBytesD(ref sh_info);
                    ModuleReader.SwapBytesD(ref sh_addralign);
                    ModuleReader.SwapBytesD(ref sh_entsize);
                }
            }
        }

        private static void GetEmbeddedModule32(ModuleReader reader, byte[] ident)
        {
            // Read the endiannes.
            bool big = ident[(int)ElfIdent.EI_DATA] == (int)ElfData.ELFDATA2MSB;

            // Read the header.
            Elf32_Ehdr header = new Elf32_Ehdr();
            header.Read(reader, big);

            // Reject files without section names.
            if(header.e_shstrndex == (int)ElfSectionNumber.SHN_UNDEF)
                throw new ModuleException("Unsupported elfs without section names");

            // Read the "section names" section.
            reader.SetPosition((uint) (header.e_shoff + header.e_shstrndex*header.e_shentsize));
            Elf32_Shdr namesHeader = new Elf32_Shdr();
            namesHeader.Read(reader, big);

            // Reject files without real section names.
            if(namesHeader.sh_size == 0)
                throw new ModuleException("This elf doesn't have section names.");

            // Read the name table.
            byte[] nameTable;
            reader.SetPosition(namesHeader.sh_offset);
            reader.Read(out nameTable, (int)namesHeader.sh_size);

            // Move to the section header table.
            reader.SetPosition(header.e_shoff);

            // Read the section until hit '.cbm'.
            Elf32_Shdr sectionHeader = new Elf32_Shdr();
            bool found = false;
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < header.e_shnum; ++i)
            {
                // Read the section header.
                sectionHeader.Read(reader, big);

                // Check the section name.
                if(sectionHeader.sh_name >= nameTable.Length)
                    throw new ModuleException("Invalid section name.");

                // Build the section name.
                builder.Length = 0;
                int pos = (int)sectionHeader.sh_name;
                while(nameTable[pos] != 0)
                    builder.Append((char)nameTable[pos++]);
                string sectionName = builder.ToString();

                // Compare the section name.
                if(sectionName == ".cbm")
                {
                    found = true;
                    break;
                }

                // Skip the extra data.
                reader.Skip(header.e_shentsize - Elf32_Shdr.Size);
            }

            // Make sure the section was found.
            if(!found)
                throw new ModuleException("The elf doesn't have a chela module.");

            // Make sure the section type is PROGBITS.
            if(sectionHeader.sh_type != (int)ElfSectionType.SHT_PROGBITS)
                throw new ModuleException("The elf section that can have the module is not supported.");

            // Move to the section offset.
            reader.SetPosition(sectionHeader.sh_offset);
        }

        private static void GetEmbeddedModule64(ModuleReader reader, byte[] ident)
        {
            // Read the endiannes.
            bool big = ident[(int)ElfIdent.EI_DATA] == (int)ElfData.ELFDATA2MSB;

            // Read the header.
            Elf64_Ehdr header = new Elf64_Ehdr();
            header.Read(reader, big);

            // Reject files without section names.
            if(header.e_shstrndex == (int)ElfSectionNumber.SHN_UNDEF)
                throw new ModuleException("Unsupported elfs without section names");

            // Read the "section names" section.
            reader.SetPosition(header.e_shoff + (ulong)header.e_shstrndex*header.e_shentsize);
            Elf64_Shdr namesHeader = new Elf64_Shdr();
            namesHeader.Read(reader, big);

            // Reject files without real section names.
            if(namesHeader.sh_size == 0)
                throw new ModuleException("This elf doesn't have section names.");

            // Read the name table.
            byte[] nameTable;
            reader.SetPosition(namesHeader.sh_offset);
            reader.Read(out nameTable, (int)namesHeader.sh_size);

            // Move to the section header table.
            reader.SetPosition(header.e_shoff);

            // Read the section until hit '.cbm'.
            Elf64_Shdr sectionHeader = new Elf64_Shdr();
            bool found = false;
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < header.e_shnum; ++i)
            {
                // Read the section header.
                sectionHeader.Read(reader, big);

                // Check the section name.
                if(sectionHeader.sh_name >= nameTable.Length)
                    throw new ModuleException("Invalid section name.");

                // Build the section name.
                builder.Length = 0;
                int pos = (int)sectionHeader.sh_name;
                while(nameTable[pos] != 0)
                    builder.Append((char)nameTable[pos++]);
                string sectionName = builder.ToString();

                // Compare the section name.
                if(sectionName == ".cbm")
                {
                    found = true;
                    break;
                }

                // Skip the extra data.
                reader.Skip(header.e_shentsize - Elf64_Shdr.Size);
            }

            // Make sure the section was found.
            if(!found)
                throw new ModuleException("The elf doesn't have a chela module.");

            // Make sure the section type is PROGBITS.
            if(sectionHeader.sh_type != (int)ElfSectionType.SHT_PROGBITS)
                throw new ModuleException("The elf section that can have the module is not supported.");

            // Move to the section offset.
            reader.SetPosition(sectionHeader.sh_offset);
        }

        public static void GetEmbeddedModule(ModuleReader reader)
        {
            // Read the elf identifier.
            byte[] ident;
            reader.Read(out ident, (int)ElfIdent.EI_NIDENT);

            // Check the magic number.
            for(int i = 0; i < ElfMagic.Length; ++i)
            {
                if(ident[i] != ElfMagic[i])
                    throw new ModuleException("File is not an ELF.");
            }

            // Check the version.
            if(ident[(int)ElfIdent.EI_VERSION] != (int)ElfVersion.EV_CURRENT)
                throw new ModuleException("Invalid elf version.");

            // Check the elf class.
            ElfClass clazz = (ElfClass)ident[(int)ElfIdent.EI_CLASS];
            if(clazz == ElfClass.ELFCLASS32)
                GetEmbeddedModule32(reader, ident);
            else if(clazz == ElfClass.ELFCLASS64)
                GetEmbeddedModule64(reader, ident);
            else
                throw new ModuleException("Unsupported ELF class.");
        }
    }
}

