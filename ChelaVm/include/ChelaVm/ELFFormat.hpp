#ifndef CHELAVM_ELFFORMAT_HPP
#define CHELAVM_ELFFORMAT_HPP

#include <inttypes.h>
#include "ModuleReader.hpp"

namespace ChelaVm
{
    const unsigned char ElfMagic[4] = {0x7f, 'E', 'L', 'F'};

    // Basic elf data types.
    typedef uint32_t Elf32_Addr;
    typedef uint16_t Elf32_Half;
    typedef uint32_t Elf32_Off;
    typedef int32_t  Elf32_Sword;
    typedef uint32_t Elf32_Word;

    typedef uint64_t Elf64_Addr;
    typedef uint16_t Elf64_Half;
    typedef uint64_t Elf64_Off;
    typedef int32_t  Elf64_Sword;
    typedef int64_t  Elf64_Sxword;
    typedef uint32_t Elf64_Word;
    typedef uint64_t Elf64_Xword;
    typedef uint8_t  Elf64_Byte;
    typedef uint16_t Elf64_Section;

    namespace ElfType
    {
        enum Type
        {
            ET_NONE   = 0,
            ET_REL    = 1,
            ET_EXEC   = 2,
            ET_DYN    = 3,
            ET_CORE   = 4,
            ET_LOPROC = 0xff00,
            ET_HIPROC = 0xffff,
        };
    }

    namespace ElfMachine
    {
        enum Machine
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
    }

    namespace ElfVersion
    {
        enum Version
        {
            EV_NONE    = 0,
            EV_CURRENT = 1,
        };
    }

    namespace ElfIdent
    {
        enum Ident
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

        enum Class
        {
            ELFCLASSNONE = 0,
            ELFCLASS32   = 1,
            ELFCLASS64   = 2,
        };

        enum Data
        {
            ELFDATANONE = 0,
            ELFDATA2LSB = 1,
            ELFDATA2MSB = 2,
        };
    }

    namespace ElfSectionNumber
    {
        enum Number
        {
            SHN_UNDEF     = 0,
            SHN_LORESERVE = 0xFF00,
            SHN_LOPROC    = 0xFF00,
            SHN_HIPROC    = 0xFF1F,
            SHN_ABS       = 0xFFF1,
            SHN_COMMON    = 0xFFF2,
            SHN_HIRESERVE = 0xFFFF,

        };
    };

    // Elf32 header.
    struct Elf32_Ehdr
    {
        //unsigned char   e_ident[EI_NIDENT];
        Elf32_Half      e_type;
        Elf32_Half      e_machine;
        Elf32_Word      e_version;
        Elf32_Addr      e_entry;
        Elf32_Off       e_phoff;
        Elf32_Off       e_shoff;
        Elf32_Word      e_flags;
        Elf32_Half      e_ehsize;
        Elf32_Half      e_phentsize;
        Elf32_Half      e_phnum;
        Elf32_Half      e_shentsize;
        Elf32_Half      e_shnum;
        Elf32_Half      e_shstrndex;

        void Read(ModuleReader &reader, bool big)
        {
            reader >> e_type >> e_machine >> e_version;
            reader >> e_entry >> e_phoff >> e_shoff;
            reader >> e_flags >> e_ehsize >> e_phentsize;
            reader >> e_phnum >> e_shentsize >> e_shnum;
            reader >> e_shstrndex;

            // Swap bytes.
            if(big)
            {
                SwapBytesD(e_type);
                SwapBytesD(e_machine);
                SwapBytesD(e_version);
                SwapBytesD(e_entry);
                SwapBytesD(e_phoff);
                SwapBytesD(e_shoff);
                SwapBytesD(e_flags);
                SwapBytesD(e_ehsize);
                SwapBytesD(e_phentsize);
                SwapBytesD(e_phnum);
                SwapBytesD(e_shentsize);
                SwapBytesD(e_shnum);
                SwapBytesD(e_shstrndex);
            }
        }
    };

    // Elf64 header
    struct Elf64_Ehdr
    {
        //unsigned char   e_ident[EI_NIDENT];
        Elf64_Half      e_type;
        Elf64_Half      e_machine;
        Elf64_Word      e_version;
        Elf64_Addr      e_entry;
        Elf64_Off       e_phoff;
        Elf64_Off       e_shoff;
        Elf64_Word      e_flags;
        Elf64_Half      e_ehsize;
        Elf64_Half      e_phentsize;
        Elf64_Half      e_phnum;
        Elf64_Half      e_shentsize;
        Elf64_Half      e_shnum;
        Elf64_Half      e_shstrndex;

        void Read(ModuleReader &reader, bool big)
        {
            reader >> e_type >> e_machine >> e_version;
            reader >> e_entry >> e_phoff >> e_shoff;
            reader >> e_flags >> e_ehsize >> e_phentsize;
            reader >> e_phnum >> e_shentsize >> e_shnum;
            reader >> e_shstrndex;

            // Swap bytes.
            if(big)
            {
                SwapBytesD(e_type);
                SwapBytesD(e_machine);
                SwapBytesD(e_version);
                SwapBytesD(e_entry);
                SwapBytesD(e_phoff);
                SwapBytesD(e_shoff);
                SwapBytesD(e_flags);
                SwapBytesD(e_ehsize);
                SwapBytesD(e_phentsize);
                SwapBytesD(e_phnum);
                SwapBytesD(e_shentsize);
                SwapBytesD(e_shnum);
                SwapBytesD(e_shstrndex);
            }
        }
    };

    namespace ElfSectionType
    {
        enum Type
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
    }

    namespace ElfSectionFlags
    {
        enum Flags
        {
            SHF_WRITE     = 0x1,
            SHF_ALLOC     = 0x2,
            SHF_EXECINSTR = 0x4,
            SHF_MASKPROC  = 0xf0000000,
        };
    }

    // Elf32 section.
    struct Elf32_Shdr
    {
        static const Elf32_Word Size = 40;

        Elf32_Word  sh_name;
        Elf32_Word  sh_type;
        Elf32_Word  sh_flags;
        Elf32_Addr  sh_addr;
        Elf32_Off   sh_offset;
        Elf32_Word  sh_size;
        Elf32_Word  sh_link;
        Elf32_Word  sh_info;
        Elf32_Word  sh_addralign;
        Elf32_Word  sh_entsize;

        Elf32_Shdr()
        {
            sh_name = 0;
            sh_type = 0;
            sh_flags = 0;
            sh_addr = 0;
            sh_offset = 0;
            sh_size = 0;
            sh_link = 0;
            sh_info = 0;
            sh_addralign = 0;
            sh_entsize = 0;
        }

        void Read(ModuleReader &reader, bool big)
        {
            reader >> sh_name >> sh_type >> sh_flags;
            reader >> sh_addr >> sh_offset >> sh_size;
            reader >> sh_link >> sh_info >> sh_addralign;
            reader >> sh_entsize;

            if(big)
            {
                SwapBytesD(sh_name);
                SwapBytesD(sh_type);
                SwapBytesD(sh_flags);
                SwapBytesD(sh_addr);
                SwapBytesD(sh_offset);
                SwapBytesD(sh_size);
                SwapBytesD(sh_link);
                SwapBytesD(sh_info);
                SwapBytesD(sh_addralign);
                SwapBytesD(sh_entsize);
            }
        }
    };

    // Elf64 section.
    struct Elf64_Shdr
    {
        static const Elf64_Word Size = 64;

        Elf64_Word  sh_name;
        Elf64_Word  sh_type;
        Elf64_Xword sh_flags;
        Elf64_Addr  sh_addr;
        Elf64_Off   sh_offset;
        Elf64_Xword sh_size;
        Elf64_Word  sh_link;
        Elf64_Word  sh_info;
        Elf64_Xword sh_addralign;
        Elf64_Xword sh_entsize;

        Elf64_Shdr()
        {
            sh_name = 0;
            sh_type = 0;
            sh_flags = 0;
            sh_addr = 0;
            sh_offset = 0;
            sh_size = 0;
            sh_link = 0;
            sh_info = 0;
            sh_addralign = 0;
            sh_entsize = 0;
        }

        void Read(ModuleReader &reader, bool big)
        {
            reader >> sh_name >> sh_type >> sh_flags;
            reader >> sh_addr >> sh_offset >> sh_size;
            reader >> sh_link >> sh_info >> sh_addralign;
            reader >> sh_entsize;

            if(big)
            {
                SwapBytesD(sh_name);
                SwapBytesD(sh_type);
                SwapBytesD(sh_flags);
                SwapBytesD(sh_addr);
                SwapBytesD(sh_offset);
                SwapBytesD(sh_size);
                SwapBytesD(sh_link);
                SwapBytesD(sh_info);
                SwapBytesD(sh_addralign);
                SwapBytesD(sh_entsize);
            }
        }
    };

    size_t ELFFormat_GetEmbeddedModule(ModuleReader &reader);
}

#endif //CHELAVM_ELFFORMAT_HPP
