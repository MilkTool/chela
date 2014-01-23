namespace Chela.Compiler.Module
{
	public class StructureHeader
	{
		public const uint HeaderSize = 14;
		
		public uint baseStructure;
        public ushort numinterfaces;
		public ushort numfields;
		public ushort numvmethods;
        public ushort numcontracts;
        public ushort nummembers;
		
		public StructureHeader ()
		{
		}
		
		public void Write (ModuleWriter writer)
		{
			writer.Write(baseStructure);
            writer.Write(numinterfaces);
			writer.Write(numfields);
			writer.Write(numvmethods);
            writer.Write(numcontracts);
            writer.Write(nummembers);
		}

        public void Read (ModuleReader reader)
        {
            reader.Read(out baseStructure);
            reader.Read(out numinterfaces);
            reader.Read(out numfields);
            reader.Read(out numvmethods);
            reader.Read(out numcontracts);
            reader.Read(out nummembers);
        }
	}
}

