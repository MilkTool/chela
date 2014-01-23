using System.IO;

namespace Chela.Compiler.Module
{
	public class ModuleWriter
	{
		private Stream dest;
		
		public ModuleWriter (Stream dest)
		{
			this.dest = dest;
		}
		
		public uint GetPosition()
		{
			return (uint)this.dest.Position;
		}
		
		public void MoveBegin()
		{
			this.dest.Seek(0, SeekOrigin.Begin);
		}

        public void Write(byte[] buffer, int offset, int length)
        {
            this.dest.Write(buffer, offset, length);
        }

        public void Write(Stream file)
        {
            const int bufferSize = 2048;
            byte[] buffer = new byte[bufferSize];
            int readed;
            while((readed = file.Read(buffer, 0, bufferSize)) > 0)
                 Write(buffer, 0, readed);
         }
		
		public void Write(byte value)
		{
			this.dest.WriteByte(value);
		}
		
		public void Write(sbyte value)
		{
			Write((byte)value);
		}
		
		public void Write(ushort value)
		{
			this.dest.WriteByte((byte) (value & 0x00FF));
			this.dest.WriteByte((byte) ((value & 0xFF00)>>8));
		}
		
		public void Write(short value)
		{
			Write((ushort)value);
		}

		public void Write(uint value)
		{
			this.dest.WriteByte((byte) (value  & 0x000000FF));
			this.dest.WriteByte((byte) ((value & 0x0000FF00)>>8));
			this.dest.WriteByte((byte) ((value & 0x00FF0000)>>16));
			this.dest.WriteByte((byte) ((value & 0xFF000000)>>24));
		}
		
		public void Write(int value)
		{
			Write((uint)value);
		}
		
		public void Write(ulong value)
		{
			this.dest.WriteByte((byte) (value  & 0x00000000000000FF));
			this.dest.WriteByte((byte) ((value & 0x000000000000FF00)>>8));
			this.dest.WriteByte((byte) ((value & 0x0000000000FF0000)>>16));
			this.dest.WriteByte((byte) ((value & 0x00000000FF000000)>>24));
			this.dest.WriteByte((byte) ((value & 0x000000FF00000000)>>32));
			this.dest.WriteByte((byte) ((value & 0x0000FF0000000000)>>40));
			this.dest.WriteByte((byte) ((value & 0x00FF000000000000)>>48));
			this.dest.WriteByte((byte) ((value & 0xFF00000000000000)>>56));
		}
		
		public void Write(long value)
		{
			Write((ulong)value);
		}
		
		public void Write(float value)
		{
			byte[] bits = System.BitConverter.GetBytes(value);
			this.dest.Write(bits, 0, 4);
		}
		
		public void Write(double value)
		{
			byte[] bits = System.BitConverter.GetBytes(value);
			this.dest.Write(bits, 0, 8);
		}
		
		public void Write(string value)
		{
			// Write the length.
			Write((ushort)value.Length);
			
			for(int i = 0; i < value.Length; i++)
			{
				// TODO: perform utf-8 encoding.
				this.dest.WriteByte((byte)value[i]);
			}
		}
		
		public void Write(string value, int size)
		{
			int i;
			for(i = 0; i < value.Length && i < size; i++)
				this.dest.WriteByte((byte)value[i]);
			
			for(; i < size; i++)
				this.dest.WriteByte(0);
		}

	}
}

