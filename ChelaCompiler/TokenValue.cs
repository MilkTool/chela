using System;
namespace Chela.Compiler
{
	internal class TokenValue: TokenPosition
	{
		private int token;
		
		public TokenValue (int token, string fileName, int line, int column)
			: base(fileName, line, column)
		{
			
			this.token = token;
		}
		
		public int GetToken()
		{
			return token;
		}
		
		public virtual object GetValue()
		{
			return null;
		}
	}
	
	internal class WordToken: TokenValue
	{
		private string word;
		
		public WordToken(string word, int token, string fileName, int line, int column)
			: base(token, fileName, line, column)
		{
			this.word = word;
		}
		
		public override object GetValue()
		{
			return word;
		}
	}
	
	internal class IntegralToken: TokenValue
	{
		private long value;
		
		public IntegralToken(long value, int token, string fileName, int line, int column)
			: base(token, fileName, line, column)
		{
			this.value = value;
		}
		
		public override object GetValue()
		{
			return value;
		}
		
		public long GetInt()
		{
			return value;
		}
	}
	
	internal class UIntegralToken: TokenValue
	{
		private ulong value;
		
		public UIntegralToken(ulong value, int token, string fileName, int line, int column)
			: base(token, fileName, line, column)
		{
			this.value = value;
		}
		
		public override object GetValue()
		{
			return value;
		}
		
		public ulong GetUInt()
		{
			return value;
		}
	}

	internal class BoolToken: TokenValue
	{
		private bool value;
		
		public BoolToken(bool value, int token, string fileName, int line, int column)
			: base(token, fileName, line, column)
		{
			this.value = value;
		}
		
		public override object GetValue()
		{
			return value;
		}
		
		public bool GetBool()
		{
			return value;
		}
	}

	internal class FloatingPointToken: TokenValue
	{
		private double value;
		
		public FloatingPointToken(double value, int token, string fileName, int line, int column)
			: base(token, fileName, line, column)
		{
			this.value = value;
		}
		
		public override object GetValue()
		{
			return value;
		}
		
		public double GetFloat()
		{
			return value;
		}
	}
	
}

