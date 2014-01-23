#include <cstring>
#include "ChelaVm/PEFormat.hpp"
#include "ChelaVm/Module.hpp"

namespace ChelaVm
{
    // COFF header.
    struct CoffHeader
    {
        uint16_t machineType;
        uint16_t numSections;
        uint32_t timeStamp;
        uint32_t symbolTablePointer;
        uint32_t numSymbols;
        uint16_t optionalHeaderSize;
        uint16_t characteristics;

        void Read(ModuleReader &reader)
        {
            reader >> machineType >> numSections >> timeStamp;
            reader >> symbolTablePointer >> numSymbols;
            reader >> optionalHeaderSize >> characteristics;
        }
    };

    // COFF section header record.
    struct CoffSectionHeader
    {
        uint8_t name[8];
        uint32_t virtualSize;
        uint32_t virtualAddress;
        uint32_t rawDataSize;
        uint32_t rawDataPointer;
        uint32_t relocationsPointer;
        uint32_t lineNumberPointer;
        uint16_t numRelocations;
        uint16_t numLineNumbers;
        uint32_t characteristics;

        void Read(ModuleReader &reader)
        {
            reader.Read(name, 8);
            reader >> virtualSize >> virtualAddress >> rawDataSize;
            reader >> rawDataPointer >> relocationsPointer;
            reader >> lineNumberPointer >> numRelocations;
            reader >> numLineNumbers >> characteristics;
        }
    };
        
    size_t PEFormat_GetEmbeddedModule(ModuleReader &reader)
    {
        // Read the MZ signature.
        uint8_t signature[4];
        reader.Read(signature, 2);
        if(signature[0] != 'M' || signature[1] != 'Z')
            throw ModuleException("Invalid PE signature.");
            
        // Read the PE signature offset.
        uint32_t peOffset;
        reader.Seek(0x3c, SEEK_SET);
        reader >> peOffset;
        reader.Seek(peOffset, SEEK_SET);
        
        // Read the PE\0\0 signature.
        reader.Read(signature, 4);
        if(signature[0] != 'P' || signature[1] != 'E' ||
           signature[2] != 0 || signature[3] != 0)
           throw ModuleException("Unsupported MS DOS programs.");
           
        // Read the COFF header.
        CoffHeader header;
        header.Read(reader);
        
        // Ignore the optional header.
        reader.Skip(header.optionalHeaderSize);
        
        // Read the sections until hit '.cbm'
        CoffSectionHeader sectionHeader;
        for(int i = 0; i < header.numSections; ++i)
        {
            // Read the section header.
            sectionHeader.Read(reader);
            
            // Check if this is the section.
            if(!strncmp((const char*)sectionHeader.name, ".cbm", 8))
            {
                // Found the section with the embedded module.
                reader.Seek(sectionHeader.rawDataPointer, SEEK_SET);
                return sectionHeader.rawDataSize;
            }
        }

        // Could't find an embedded module.
        throw ModuleException("Couldn't find embedded module.");        
    }
}
