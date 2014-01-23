namespace Chela.Compiler.Module
{
	public class FunctionHeader
    {
		public const int HeaderSize = 13;
		public uint functionType;
        public ushort numargs; // argument names count.
		public ushort numlocals;
		public ushort numblocks;
        public byte numexceptions;
		public short vslot;
		
		public FunctionHeader ()
		{
			functionType = 0u;
            numargs = 0;
			numlocals = 0;
			numblocks = 0;
            numexceptions = 0;
			vslot = -1;
		}
		
		public void Write (ModuleWriter writer)
		{
			writer.Write(functionType);
            writer.Write(numargs);
			writer.Write(numlocals);
			writer.Write(numblocks);
            writer.Write(numexceptions);
			writer.Write(vslot);
		}

        public void Read (ModuleReader reader)
        {
            reader.Read(out functionType);
            reader.Read(out numargs);
            reader.Read(out numlocals);
            reader.Read(out numblocks);
            reader.Read(out numexceptions);
            reader.Read(out vslot);
        }
	}
}

