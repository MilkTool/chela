using System.IO;
using System.Text;

namespace Chela.Compiler.Module
{
	public class ModuleReader
	{
		private Stream src;
        private long currentBase;
		
		public ModuleReader (Stream src)
		{
			this.src = src;
            this.currentBase = 0;
		}

        public void FixBase()
        {
            currentBase = this.src.Position;
        }

		public uint GetPosition()
		{
			return (uint)(this.src.Position - currentBase);
		}

        public void SetPosition(uint position)
        {
            this.src.Seek(position + currentBase, SeekOrigin.Begin);
        }

        public void SetPosition(ulong position)
        {
            this.src.Seek((long)position + currentBase, SeekOrigin.Begin);
        }
		
		public void MoveBegin()
		{
			this.src.Seek(0, SeekOrigin.Begin);
		}

        public void Skip(long amount)
        {
            this.src.Seek(amount, SeekOrigin.Current);
        }
		
		public byte ReadByte()
		{
			return (byte)this.src.ReadByte();
		}
		
		public sbyte ReadSByte()
		{
			return (sbyte) ReadByte();
		}
		
		public ushort ReadUShort()
		{
			return (ushort)(ReadByte() | (ReadByte() << 8));
		}
		
		public short ReadShort()
		{
			return (short) ReadUShort();
		}

		public uint ReadUInt()
		{
			return (uint)(ReadByte() |
                         (ReadByte()<< 8) |
				         (ReadByte()<<16) |
				         (ReadByte()<<24));
		}

		public int ReadInt()
		{
            return (int) ReadUInt();
		}

        public ulong ReadULong()
        {
            return (ulong)( (ulong)ReadByte()      |
                           ((ulong)ReadByte()<< 8) |
                           ((ulong)ReadByte()<<16) |
                           ((ulong)ReadByte()<<24) |
                           ((ulong)ReadByte()<<32) |
                           ((ulong)ReadByte()<<40) |
                           ((ulong)ReadByte()<<48) |
                           ((ulong)ReadByte()<<56));
        }

        public long ReadLong()
        {
            return (long) ReadULong();
        }

        public float ReadFloat()
        {
            byte[] bytes = new byte[4] {ReadByte(), ReadByte(),
                    ReadByte(), ReadByte()};
            return System.BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble()
        {
            byte[] bytes = new byte[8] {ReadByte(), ReadByte(),
                    ReadByte(), ReadByte(), ReadByte(), ReadByte(),
                    ReadByte(), ReadByte()};
            return System.BitConverter.ToDouble(bytes, 0);
        }

        public string ReadString()
        {
            // Read the length.
            ushort length = ReadUShort();

            // Create the string.
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < length; i++)
            {
                // TODO: Perform utf-8 decoding.
                builder.Append((char) ReadByte());
            }

            return builder.ToString();
        }

        public string ReadString(int size)
        {
            StringBuilder builder = new StringBuilder();
            int i;

            // Read the string until hit a zero.
            for(i = 0; i < size;)
            {
                // Read and count the byte.
                byte c = ReadByte();
                i++;

                // Store or stop.
                if(c == 0)
                    break;
                else
                    builder.Append((char)c);
            }

            // Skip the remaining zeros
            Skip(size - i);

            return builder.ToString();
        }

        public void Read(out byte[] data, int size)
        {
            data = new byte[size];
            src.Read(data, 0, size);
        }

        public void Read(out byte value)
        {
            value = ReadByte();
        }

        public void Read(out sbyte value)
        {
            value = ReadSByte();
        }

        public void Read(out short value)
        {
            value = ReadShort();
        }

        public void Read(out ushort value)
        {
            value = ReadUShort();
        }

        public void Read(out int value)
        {
            value = ReadInt();
        }

        public void Read(out uint value)
        {
            value = ReadUInt();
        }

        public void Read(out long value)
        {
            value = ReadLong();
        }

        public void Read(out ulong value)
        {
            value = ReadULong();
        }

        public void Read(out float value)
        {
            value = ReadFloat();
        }

        public void Read(out double value)
        {
            value = ReadDouble();
        }

        public void Read(out string value)
        {
            value = ReadString();
        }

        public void Read(out string value, int size)
        {
            value = ReadString(size);
        }

        // Byte swapping functions.
        public static ulong SwapBytes(ulong value)
        {
            return (ulong) (
                   ((value & 0x00000000000000FFL) << 56) |
                   ((value & 0x000000000000FF00L) << 40) |
                   ((value & 0x0000000000FF0000L) << 24) |
                   ((value & 0x00000000FF000000L) <<  8) |
                   ((value & 0x000000FF00000000L) >>  8) |
                   ((value & 0x0000FF0000000000L) >> 24) |
                   ((value & 0x00FF000000000000L) >> 40) |
                   ((value & 0xFF00000000000000L) >> 56));
        }

        public static long SwapBytes(long temp)
        {
            ulong value = (ulong)temp;
            value =
                   ((value & 0x00000000000000FFL) << 56) |
                   ((value & 0x000000000000FF00L) << 40) |
                   ((value & 0x0000000000FF0000L) << 24) |
                   ((value & 0x00000000FF000000L) <<  8) |
                   ((value & 0x000000FF00000000L) >>  8) |
                   ((value & 0x0000FF0000000000L) >> 24) |
                   ((value & 0x00FF000000000000L) >> 40) |
                   ((value & 0xFF00000000000000L) >> 56);
            return (long)value;
        }

        public static uint SwapBytes(uint value)
        {
            return (uint) (
                   ((value & 0x000000FF) << 24) |
                   ((value & 0x0000FF00) <<  8) |
                   ((value & 0x00FF0000) >>  8) |
                   ((value & 0xFF000000) >> 24));
        }
    
        public static int SwapBytes(int temp)
        {
            uint value = (uint)temp;
            value =
                   ((value & 0x000000FF) << 24)|
                   ((value & 0x0000FF00) << 8) |
                   ((value & 0x00FF0000) >> 8) |
                   ((value & 0xFF000000) >> 24);
            return (int) value;
        }

        public static ushort SwapBytes(ushort value)
        {
            return (ushort) (
                   ((value & 0x00FF) << 8)|
                   ((value & 0xFF00) >> 8));
        }

        public static short SwapBytes(short value)
        {
            return (short) (
                   ((value & 0x00FF) << 8)|
                   ((value & 0xFF00) >> 8));
        }

        public static void SwapBytesD(ref ulong value)
        {
            value = SwapBytes(value);
        }

        public static void SwapBytesD(ref long value)
        {
            value = SwapBytes(value);
        }

        public static void SwapBytesD(ref uint value)
        {
            value = SwapBytes(value);
        }

        public static void SwapBytesD(ref int value)
        {
            value = SwapBytes(value);
        }

        public static void SwapBytesD(ref ushort value)
        {
            value = SwapBytes(value);
        }

        public static void SwapBytesD(ref short value)
        {
            value = SwapBytes(value);
        }
	}
}

