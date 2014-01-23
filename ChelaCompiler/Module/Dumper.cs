using System.Text;

namespace Chela.Compiler.Module
{
	public class Dumper
	{
		private static int dumpLevel = 0;
		
		public static void Printf(string format, params object[] args)
		{
			// Create the builder.
			StringBuilder builder = new StringBuilder();
			
			// Add the "tabs".
			for(int i = 0; i < dumpLevel; i++)
				builder.Append("    ");
			
			// Format the message.
			int len = format.Length;
			int arg = 0;
			for(int i = 0; i < len; i++)
			{
				char c = format[i];
				if(c == '%')
				{
					c = format[++i];
					if(c == '%')
					{
						builder.Append('%');
						continue;
					}
					
					if(c == 'd')
					{
						int value = (int)args[arg++];
						builder.Append(value.ToString());
					}
					else if(c == 'u')
					{
						uint value = (uint) args[arg++];
						builder.Append(value.ToString());
					}
					else if(c == 'f')
					{
						float value = (float)args[arg++];
						builder.Append(value.ToString());
					}
					else if(c == 's')
					{
						string value = (string)args[arg++];
						builder.Append(value.ToString());
					}
				}
				else
				{
					builder.Append(c);
				}
			}
			
			// Add the newline.
			builder.Append('\n');
			
			// Write the message.
			System.Console.Write(builder.ToString());			
		}
		
		public static void Incr()
		{
			// Increase the level.
			dumpLevel++;
		}
		
		public static void Decr()
		{
			// Decrease the level.
			dumpLevel--;
		}
		
	}
}

