#include "ChelaVm/ELFFormat.hpp"
#include "ChelaVm/Module.hpp"

namespace ChelaVm
{
    size_t ELFFormat_GetEmbeddedModule32(ModuleReader &reader, unsigned char *ident)
    {
        // Read the endiannes.
        bool big = ident[ElfIdent::EI_DATA] == ElfIdent::ELFDATA2MSB;

        // Read the header.
        Elf32_Ehdr header;
        header.Read(reader, big);

        // Reject files without section names.
        if(header.e_shstrndex == ElfSectionNumber::SHN_UNDEF)
            throw ModuleException("Unsupported elfs without section names");

        // Read the "section names" section.
        reader.Seek(header.e_shoff + header.e_shstrndex*header.e_shentsize, SEEK_SET);
        Elf32_Shdr namesHeader;
        namesHeader.Read(reader, big);

        // Reject files without real section names.
        if(namesHeader.sh_size == 0)
            throw ModuleException("This elf doesn't have section names.");

        // Read the section names.
        size_t nameTableSize = namesHeader.sh_size;
        char *nameTable = new char[nameTableSize+1];
        reader.Seek(namesHeader.sh_offset, SEEK_SET);
        reader.Read(nameTable, nameTableSize);
        nameTable[nameTableSize] = 0; // Append \0.

        // Move to the section header table.
        reader.Seek(header.e_shoff, SEEK_SET);

        // Read the section until find '.cbm'
        Elf32_Shdr sectionHeader;
        bool found = false;
        for(size_t i = 0; i < header.e_shnum; ++i)
        {
            // Read the section header.
            sectionHeader.Read(reader, big);

            // Check the section name.
            if(sectionHeader.sh_name >= nameTableSize)
            {
                delete [] nameTable;
                throw ModuleException("Invalid section name.");
            }

            // Compare the section name
            const char *sectionName = nameTable + sectionHeader.sh_name;
            if(!strcmp(sectionName, ".cbm"))
            {
                found = true;
                break;
            }

            // Skip the extra data.
            reader.Skip(header.e_shentsize - Elf32_Shdr::Size);
        }

        // Delete the name table.
        delete [] nameTable;

        // Make sure the section was found.
        if(!found)
            throw ModuleException("The elf doesn't have a chela module.");

        // Make sure section type is PROGBITS.
        if(sectionHeader.sh_type != ElfSectionType::SHT_PROGBITS)
            throw ModuleException("The elf section that can have the module is not supported.");

        // Move to the section offset.
        reader.Seek(sectionHeader.sh_offset, SEEK_SET);
        return sectionHeader.sh_size;
    }

    size_t ELFFormat_GetEmbeddedModule64(ModuleReader &reader, unsigned char *ident)
    {
        // Read the endiannes.
        bool big = ident[ElfIdent::EI_DATA] == ElfIdent::ELFDATA2MSB;

        // Read the header.
        Elf64_Ehdr header;
        header.Read(reader, big);

        // Reject files without section names.
        if(header.e_shstrndex == ElfSectionNumber::SHN_UNDEF)
            throw ModuleException("Unsupported elfs without section names");

        // Read the "section names" section.
        reader.Seek(header.e_shoff + header.e_shstrndex*header.e_shentsize, SEEK_SET);
        Elf64_Shdr namesHeader;
        namesHeader.Read(reader, big);

        // Reject files without real section names.
        if(namesHeader.sh_size == 0)
            throw ModuleException("This elf doesn't have section names.");

        // Read the section names.
        size_t nameTableSize = namesHeader.sh_size;
        char *nameTable = new char[nameTableSize+1];
        reader.Seek(namesHeader.sh_offset, SEEK_SET);
        reader.Read(nameTable, nameTableSize);
        nameTable[nameTableSize] = 0; // Append \0.

        // Move to the section header table.
        reader.Seek(header.e_shoff, SEEK_SET);

        // Read the section until find '.cbm'
        Elf64_Shdr sectionHeader;
        bool found = false;
        for(size_t i = 0; i < header.e_shnum; ++i)
        {
            // Read the section header.
            sectionHeader.Read(reader, big);

            // Check the section name.
            if(sectionHeader.sh_name >= nameTableSize)
            {
                delete [] nameTable;
                throw ModuleException("Invalid section name.");
            }

            // Compare the section name
            const char *sectionName = nameTable + sectionHeader.sh_name;
            if(!strcmp(sectionName, ".cbm"))
            {
                found = true;
                break;
            }

            // Skip the extra data.
            reader.Skip(header.e_shentsize - Elf64_Shdr::Size);
        }

        // Delete the name table.
        delete [] nameTable;

        // Make sure the section was found.
        if(!found)
            throw ModuleException("The elf doesn't have a chela module.");

        // Make sure section type is PROGBITS.
        if(sectionHeader.sh_type != ElfSectionType::SHT_PROGBITS)
            throw ModuleException("The elf section that can have the module is not supported.");

        // Move to the section offset.
        reader.Seek(sectionHeader.sh_offset, SEEK_SET);
        return sectionHeader.sh_size;
    }

    size_t ELFFormat_GetEmbeddedModule(ModuleReader &reader)
    {
        // Read the elf identifier.
        unsigned char ident[ElfIdent::EI_NIDENT];
        reader.Read(ident, ElfIdent::EI_NIDENT);

        // Check the magic number.
        if(strncmp((char*)ident, (char*)ElfMagic, 4))
            throw ModuleException("File is not an ELF.");

        // Check the version.
        if(ident[ElfIdent::EI_VERSION] != ElfVersion::EV_CURRENT)
            throw ModuleException("Invalid elf version.");

        // Check the elf class.
        int elfClass = ident[ElfIdent::EI_CLASS];
        if(elfClass == ElfIdent::ELFCLASS32)
            return ELFFormat_GetEmbeddedModule32(reader, ident);
        else if(elfClass == ElfIdent::ELFCLASS64)
            return ELFFormat_GetEmbeddedModule64(reader, ident);
        else
            throw ModuleException("Unsupported elf class.");
    }
}
