using System;
namespace Chela.Compiler
{
	public class TokenPosition
	{
		private string fileName;
		private int line;
		private int column;
		
		public TokenPosition (string fileName, int line, int column)
		{
			this.fileName = fileName;
			this.line = line;
			this.column = column;
		}

        public TokenPosition (TokenPosition position)
        {
            this.fileName = position.fileName;
            this.line = position.line;
            this.column = position.column;
        }
		
		public string GetFileName()
		{
			return this.fileName;
		}
		
		public int GetLine()
		{
			return this.line;
		}
		
		public int GetColumn()
		{
			return this.column;
		}
		
		public override string ToString ()
		{
			return fileName + ":" + line.ToString() + ":" + column.ToString();
		}
	}
}

