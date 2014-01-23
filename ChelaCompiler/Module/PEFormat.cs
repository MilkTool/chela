using System.Text;

namespace Chela.Compiler.Module
{
    public static class PEFormat
    {
        // COFF header.
        public struct CoffHeader
        {
            public ushort machineType;
            public ushort numSections;
            public uint timeStamp;
            public uint symbolTablePointer;
            public uint numSymbols;
            public ushort optionalHeaderSize;
            public ushort characteristics;

            public void Read(ModuleReader reader)
            {
                reader.Read(out machineType);
                reader.Read(out numSections);
                reader.Read(out timeStamp);
                reader.Read(out symbolTablePointer);
                reader.Read(out numSymbols);
                reader.Read(out optionalHeaderSize);
                reader.Read(out characteristics);
            }
        };

        // COFF section header record.
        public struct CoffSectionHeader
        {
            public string name;
            public uint virtualSize;
            public uint virtualAddress;
            public uint rawDataSize;
            public uint rawDataPointer;
            public uint relocationsPointer;
            public uint lineNumberPointer;
            public ushort numRelocations;
            public ushort numLineNumbers;
            public uint characteristics;

            public void Read(ModuleReader reader)
            {
                name = reader.ReadString(8);
                reader.Read(out virtualSize);
                reader.Read(out virtualAddress);
                reader.Read(out rawDataSize);
                reader.Read(out rawDataPointer);
                reader.Read(out relocationsPointer);
                reader.Read(out lineNumberPointer);
                reader.Read(out numRelocations);
                reader.Read(out numLineNumbers);
                reader.Read(out characteristics);
            }
        }

        public static void GetEmbeddedModule(ModuleReader reader)
        {
            // Read the MZ signature.
            if(reader.ReadByte() != 'M' || reader.ReadByte() != 'Z')
                throw new ModuleException("Invalid PE signature.");

            // Read the PE offset.
            reader.SetPosition(0x3c);
            uint peOffset = reader.ReadUInt();

            // Read the PE\0\0 sinature
            reader.SetPosition(peOffset);
            if(reader.ReadByte() != 'P' || reader.ReadByte() != 'E' ||
                reader.ReadByte() != 0 || reader.ReadByte() != 0)
                throw new ModuleException("Unsupported MS DOS programs.");

            // Read the COFF header.
            CoffHeader header = new CoffHeader();
            header.Read(reader);

            // Ignore the optional header.
            reader.Skip(header.optionalHeaderSize);

            // Read the sections until finding the .cbm section.
            CoffSectionHeader sectionHeader = new CoffSectionHeader();
            for(int i = 0; i < header.numSections; ++i)
            {
                // Read the section header.
                sectionHeader.Read(reader);

                // If this is the .cbm section, done.
                if(sectionHeader.name == ".cbm")
                {
                    reader.SetPosition(sectionHeader.rawDataPointer);
                    return;
                }
            }

            // Couldn't find embedded module.
            throw new ModuleException("Couldn't find embedded Chela module.");
        }
    }
}

