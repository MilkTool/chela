using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chela.Compiler
{
	public class Lexer: yyParser.yyInput
	{
		private const int EOF = -1;
		private string fileName;
		private Stream stream;
		private int line;
		private int column;
		private int look;
		private TokenValue tokenValue;
		private Dictionary<string, int> keywords;
        private Dictionary<string, bool> definitions;
		private List<bool> preprocessorStates;
        private bool ignoring;
        private int ignoreCount;
        private bool hasBeenTruePrep;
        private bool start;
        private List<int> genericStack;

		public Lexer (string fileName, Stream stream)
		{
			this.fileName = fileName;
			this.stream = stream;
			this.line = 1;
			this.column = 0;
			this.tokenValue = null;
			this.keywords = new Dictionary<string, int> ();
            this.definitions = new Dictionary<string, bool> ();
            this.preprocessorStates = new List<bool> ();
            this.ignoring = false;
            this.ignoreCount = 0;
            this.hasBeenTruePrep = false;
            this.start = true;
            this.genericStack = new List<int> ();

			// Look the first character.
			Next();
			
			// Set the keywords.
			this.keywords.Add("bool", Token.KBOOL);
			this.keywords.Add("byte", Token.KBYTE);
			this.keywords.Add("sbyte", Token.KSBYTE);
			this.keywords.Add("char", Token.KCHAR);
			this.keywords.Add("uchar", Token.KUCHAR);
			this.keywords.Add("double", Token.KDOUBLE);
			this.keywords.Add("float", Token.KFLOAT);
			this.keywords.Add("int", Token.KINT);
			this.keywords.Add("uint", Token.KUINT);
			this.keywords.Add("long", Token.KLONG);
			this.keywords.Add("ulong", Token.KULONG);
			this.keywords.Add("short", Token.KSHORT);
			this.keywords.Add("ushort", Token.KUSHORT);
			this.keywords.Add("object", Token.KOBJECT);
			this.keywords.Add("string", Token.KSTRING);
			this.keywords.Add("size_t", Token.KSIZE_T);
            this.keywords.Add("ptrdiff_t", Token.KPTRDIFF_T);
			this.keywords.Add("void", Token.KVOID);
            this.keywords.Add("vec2", Token.KVEC2);
            this.keywords.Add("vec3", Token.KVEC3);
            this.keywords.Add("vec4", Token.KVEC4);
            this.keywords.Add("dvec2", Token.KDVEC2);
            this.keywords.Add("dvec3", Token.KDVEC3);
            this.keywords.Add("dvec4", Token.KDVEC4);
            this.keywords.Add("ivec2", Token.KIVEC2);
            this.keywords.Add("ivec3", Token.KIVEC3);
            this.keywords.Add("ivec4", Token.KIVEC4);
            this.keywords.Add("bvec2", Token.KBVEC2);
            this.keywords.Add("bvec3", Token.KBVEC3);
            this.keywords.Add("bvec4", Token.KBVEC4);

            this.keywords.Add("mat2"  , Token.KMAT2);
            this.keywords.Add("mat2x3", Token.KMAT2x3);
            this.keywords.Add("mat2x4", Token.KMAT2x4);
            this.keywords.Add("mat3x2", Token.KMAT3x2);
            this.keywords.Add("mat3"  , Token.KMAT3);
            this.keywords.Add("mat3x4", Token.KMAT3x4);
            this.keywords.Add("mat4x2", Token.KMAT4x2);
            this.keywords.Add("mat4x3", Token.KMAT4x3);
            this.keywords.Add("mat4"  , Token.KMAT4);

            this.keywords.Add("dmat2"  , Token.KDMAT2);
            this.keywords.Add("dmat2x3", Token.KDMAT2x3);
            this.keywords.Add("dmat2x4", Token.KDMAT2x4);
            this.keywords.Add("dmat3x2", Token.KDMAT3x2);
            this.keywords.Add("dmat3"  , Token.KDMAT3);
            this.keywords.Add("dmat3x4", Token.KDMAT3x4);
            this.keywords.Add("dmat4x2", Token.KDMAT4x2);
            this.keywords.Add("dmat4x3", Token.KDMAT4x3);
            this.keywords.Add("dmat4"  , Token.KDMAT4);

            this.keywords.Add("imat2"  , Token.KIMAT2);
            this.keywords.Add("imat2x3", Token.KIMAT2x3);
            this.keywords.Add("imat2x4", Token.KIMAT2x4);
            this.keywords.Add("imat3x2", Token.KIMAT3x2);
            this.keywords.Add("imat3"  , Token.KIMAT3);
            this.keywords.Add("imat3x4", Token.KIMAT3x4);
            this.keywords.Add("imat4x2", Token.KIMAT4x2);
            this.keywords.Add("imat4x3", Token.KIMAT4x3);
            this.keywords.Add("imat4"  , Token.KIMAT4);

            this.keywords.Add("typedef", Token.KTYPEDEF);
			this.keywords.Add("class", Token.KCLASS);
            this.keywords.Add("delegate", Token.KDELEGATE);
            this.keywords.Add("event", Token.KEVENT);
            this.keywords.Add("interface", Token.KINTERFACE);
            this.keywords.Add("enum", Token.KENUM);
			this.keywords.Add("namespace", Token.KNAMESPACE);
			this.keywords.Add("struct", Token.KSTRUCT);
            this.keywords.Add("operator", Token.KOPERATOR);
			
			this.keywords.Add("public", Token.KPUBLIC);
			this.keywords.Add("internal", Token.KINTERNAL);
			this.keywords.Add("protected", Token.KPROTECTED);
			this.keywords.Add("private", Token.KPRIVATE);
			this.keywords.Add("extern", Token.KEXTERN);
			this.keywords.Add("kernel", Token.KKERNEL);
			this.keywords.Add("virtual", Token.KVIRTUAL);
			this.keywords.Add("static", Token.KSTATIC);
			this.keywords.Add("override", Token.KOVERRIDE);
            this.keywords.Add("abstract", Token.KABSTRACT);
            this.keywords.Add("sealed", Token.KSEALED);
            this.keywords.Add("const", Token.KCONST);
            this.keywords.Add("readonly", Token.KREADONLY);
            this.keywords.Add("unsafe", Token.KUNSAFE);
            this.keywords.Add("partial", Token.KPARTIAL);

            this.keywords.Add("__cdecl", Token.KCDECL);
            this.keywords.Add("__stdcall", Token.KSTDCALL);
            this.keywords.Add("__apicall", Token.KAPICALL);

			this.keywords.Add("break", Token.KBREAK);
			this.keywords.Add("case", Token.KCASE);
            this.keywords.Add("catch", Token.KCATCH);
			this.keywords.Add("continue", Token.KCONTINUE);
            this.keywords.Add("default", Token.KDEFAULT);
			this.keywords.Add("do", Token.KDO);
			this.keywords.Add("else", Token.KELSE);
            this.keywords.Add("finally", Token.KFINALLY);
            this.keywords.Add("fixed", Token.KFIXED);
			this.keywords.Add("for", Token.KFOR);
			this.keywords.Add("foreach", Token.KFOREACH);
            this.keywords.Add("goto", Token.KGOTO);
			this.keywords.Add("if", Token.KIF);
            this.keywords.Add("in", Token.KIN);
            this.keywords.Add("lock", Token.KLOCK);
			this.keywords.Add("return", Token.KRETURN);
			this.keywords.Add("while", Token.KWHILE);
            this.keywords.Add("using", Token.KUSING);
            this.keywords.Add("switch", Token.KSWITCH);
            this.keywords.Add("throw", Token.KTHROW);
            this.keywords.Add("try", Token.KTRY);
			
			this.keywords.Add("null", Token.KNULL);
            this.keywords.Add("this", Token.KTHIS);
            this.keywords.Add("base", Token.KBASE);

			this.keywords.Add("bitand", Token.BITAND);
			this.keywords.Add("bitor", Token.BITOR);
			this.keywords.Add("compl", Token.BITNOT);
			this.keywords.Add("xor", Token.BITXOR);
			this.keywords.Add("and", Token.LAND);
			this.keywords.Add("or", Token.LOR);
			this.keywords.Add("not", Token.LNOT);
            this.keywords.Add("sizeof", Token.SIZEOF);
            this.keywords.Add("typeof", Token.TYPEOF);
            this.keywords.Add("is", Token.IS);
            this.keywords.Add("as", Token.AS);
			this.keywords.Add("new", Token.NEW);
            this.keywords.Add("heapalloc", Token.HEAPALLOC);
            this.keywords.Add("stackalloc", Token.STACKALLOC);
			this.keywords.Add("delete", Token.KDELETE);
            this.keywords.Add("reinterpret_cast", Token.REINTERPRET_CAST);
            this.keywords.Add("checked", Token.CHECKED);
            this.keywords.Add("unchecked", Token.UNCHECKED);

            this.keywords.Add("ref", Token.KREF);
            this.keywords.Add("out", Token.KOUT);
            this.keywords.Add("params", Token.KPARAMS);
		}
		
		private void Next()
		{
			if(this.look == '\t')
				this.column = column + 4 - column % 4;
			else
				this.column++;

            this.look = this.stream.ReadByte();
			// TODO: Decode UTF-8 characters.
		}
		
		private bool IsWhite(int c)
		{
			return c == ' ' || c == '\t';
		}
		
		private bool IsDigit(int c)
		{
			return c >= '0' && c <= '9';
		}
		
		private bool IsAlpha(int c)
		{
			return (c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z');
		}

        private void Warning(string message)
        {
            // TODO: Do something cleaner.
        }
		
		private void Error(string message)
		{
			throw new CompilerException(message, new TokenPosition(fileName, line, column));
		}

        private void SkipWhite()
        {
            while(IsWhite(look))
                Next();
        }

        private string ReadIdentifier(string prefix)
        {
            // Check for the first characters of the identifier.
            if(!IsAlpha(look) && look != '_' && prefix == null)
                Error("expected identifier.");

            // Read the identifier characters.
            StringBuilder identifier = new StringBuilder();
            while(IsAlpha(look) || IsDigit(look) || look == '_')
            {
                identifier.Append((char)look);
                Next();
            }

            if(prefix != null)
                return prefix + identifier.ToString();
            return identifier.ToString();
        }

        private string ReadIdentifier()
        {
            return ReadIdentifier(null);
        }

        private string ReadLine()
        {
            StringBuilder line = new StringBuilder();

            // Read the characters until hit a \r or \n.
            while(look != '\n' && look != '\r' && look != EOF)
            {
                line.Append((char)look);
                Next();
            }

            // Skip the new line sequence.
            if(look == '\r')
            {
                Next();
                if(look == '\n')
                    Next();
            }
            else if(look == '\n')
            {
                Next();
            }

            // Increase the line number.
            this.line++;
            this.column = 0;

            return line.ToString();
        }

        private int ReadInteger()
        {
            // Read the sign.
            bool neg = false;
            if(look == '-')
            {
                neg = true;
                Next();
            }
            else if(look == '+')
                Next();

            // It must be a number.
            if(!IsDigit(look))
                Error("expected integer number.");

            // Read the integer.
            int val = 0;
            while(!IsDigit(look))
            {
                val = val*10 + (look - '0');
                Next();
            }

            return neg ? -val : val;
        }

        private string ReadLiteral()
        {
            if(look != '"')
                Error("expected literal string.");

            // Skip the first '"'
            Next();
            StringBuilder builder = new StringBuilder();
            while(look != '"' && look != EOF)
            {
                if(look == '\\')
                {
                    // Parse escape sequences.
                    Next();
                    switch(look)
                    {
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    default:
                        builder.Append((char) look);
                        break;
                    }
                }
                else
                {
                    // Handle newlines.
                    if(look == '\r')
                    {
                        builder.Append('\r');
                        Next();
                        
                        // Update the line and column.
                        this.line++; this.column = 0;
                        if(look != '\n')
                        {
                            builder.Append('\n');
                            continue;
                        }
                    }
                    else if(look == '\n')
                    {
                        this.line++; this.column = 0;
                    }

                    builder.Append((char)look);
                }

                Next();
            }

            // Shouldn't hit EOF.
            if(look == EOF)
                Error("Hit eof.");

            // Ignore the last '"'
            Next();

            // Return the literal.
            return builder.ToString();
        }

        private bool PrepFactor()
        {
            if(IsAlpha(look) || look == '_')
            {
                string ident = ReadIdentifier();
                return definitions.ContainsKey(ident);
            }
            else if(look == '(')
            {
                Next(); SkipWhite();

                bool ret = PrepLogicalExpression();
                SkipWhite();
                if(look != ')')
                    Error("expecting a ')'");
                return ret;
            }

            Error("expected a valid preprocessor expression");
            return false;
        }

        private bool PrepNegationExpression()
        {
            if(look == '!')
            {
                Next(); SkipWhite();
                return !PrepFactor();
            }
            else
            {
                return PrepFactor();
            }
        }

        private bool PrepComparisonExpression()
        {
            // Read the first operand.
            bool accum = PrepNegationExpression();

            // Read the operations.
            SkipWhite();
            while(look == '!' || look == '=')
            {
                bool eq = look == '=';

                // Complete the "==" and "!=".
                Next();
                if(look != '=')
                    Error("expected '!=' or '=='.");
                Next(); SkipWhite(); // Skip the whitespaces

                // Read the second operand.
                bool second = PrepNegationExpression();
                SkipWhite(); // To read the next operation.

                if(eq)
                    accum = accum == second;
                else
                    accum = accum != second;
            }

            return accum;
        }

        private bool PrepLogicalExpression()
        {
            // Read the first operand.
            bool accum = PrepComparisonExpression();

            // Read the operations.
            SkipWhite();
            while(look == '&' || look == '|')
            {
                int oldc = look;

                // Complete the "&&" and "||"
                Next();
                if(look != oldc)
                    Error("expected '&&' or '||'.");
                Next(); SkipWhite(); // Skip the whitespaces.

                // Read the second operand.
                bool second = PrepComparisonExpression();
                SkipWhite(); // To read the next operation.

                if(oldc == '&')
                    accum = accum && second;
                else
                    accum = accum || second;
            }

            return accum;
        }

        private bool EvaluatePrepExpr()
        {
            // Skip white.
            SkipWhite();

            // Read the logical expression.
            return PrepLogicalExpression();
        }

        private int Preprocess()
        {
            bool singleComment = false;
            bool multiComment = false;
            bool newline = start;
            start = false;
            StringBuilder directiveBuilder = null;

            // Skip white spaces and comments.
            while(ignoring || IsWhite(look) || look == '\n' || look == '\r'
                  || look == '/' || singleComment || multiComment
                  || look == '#')
            {
                if(look == EOF)
                {
                    // Hit eof.
                    if(ignoring)
                        Error("hit EOF before completing preprocessing.");
                    break;
                }
                else if(look == '#' && newline && !multiComment && !singleComment)
                {
                    // Got a preprocessor directive.
                    Next(); // Skip the '#'

                    // Skip white.
                    SkipWhite();

                    // Create the directive builder.
                    if(directiveBuilder == null)
                        directiveBuilder = new StringBuilder();
                    else
                        directiveBuilder.Length = 0;

                    // Read the directive.
                    while(IsDigit(look) || IsAlpha(look) || look == '_' )
                    {
                        directiveBuilder.Append((char)look);
                        Next();
                    }

                    // Perform the directive.
                    string directive = directiveBuilder.ToString();
                    //System.Console.WriteLine("dir '{0}'", directive);
                    if(directive == "define" && !ignoring)
                    {
                        // Skip white.
                        SkipWhite();

                        // Read the identifier.
                        string definition = ReadIdentifier();
                        //System.Console.WriteLine("define '{0}'", definition);

                        // Store it.
                        definitions.Add(definition, true);
                        newline = false;
                        singleComment = true;
                    }
                    else if(directive == "undef" && !ignoring)
                    {
                        // Skip white.
                        SkipWhite();

                        // Read the identifier.
                        string definition = ReadIdentifier();

                        // Remove it.
                        definitions.Remove(definition);
                        newline = false;
                        singleComment = true;
                    }
                    else if(directive == "if")
                    {
                        if(ignoring)
                        {
                            ignoreCount++;
                            newline = false;
                        }
                        else
                        {
                            // Evaluate the if expression
                            bool value = EvaluatePrepExpr();

                            // Store the old state.
                            preprocessorStates.Add(hasBeenTruePrep);

                            // Change the state.
                            hasBeenTruePrep = value;
                            ignoring = !hasBeenTruePrep;
                            ignoreCount = hasBeenTruePrep ? 0 : 1;
                        }

                        singleComment = true;
                    }
                    else if(directive == "elif" && (!ignoring || ignoreCount <= 1))
                    {
                        if(!hasBeenTruePrep)
                        {
                            if(preprocessorStates.Count == 0)
                                Error("elif without matching if");

                            // Evaluate the elif expression
                            bool value = EvaluatePrepExpr();

                            // Change the state.
                            hasBeenTruePrep = value;
                            ignoring = !hasBeenTruePrep;
                            ignoreCount = hasBeenTruePrep ? 0 : 1;
                        }
                        else
                        {
                            ignoring = true;
                            if(ignoreCount == 0)
                                ignoreCount = 1;
                        }

                        singleComment = true;
                    }
                    else if(directive == "else" && (!ignoring || ignoreCount <= 1))
                    {
                        if(preprocessorStates.Count == 0)
                            Error("else without matching if");

                        singleComment = true;
                        ignoring = hasBeenTruePrep;
                        ignoreCount = hasBeenTruePrep ? 1 : 0;
                    }
                    else if(directive == "endif")
                    {
                        // Try to stop ignoring and change into the previous state.
                        if(ignoring)
                        {
                            ignoreCount--;
                            if(ignoreCount == 0)
                                ignoring = false;
                        }

                        // Change into the previous state, if needed.
                        if(!ignoring)
                        {
                            if(preprocessorStates.Count == 0)
                                Error("unmatched endif");
                            int lastIndex = preprocessorStates.Count - 1;
                            hasBeenTruePrep = preprocessorStates[lastIndex];
                            preprocessorStates.RemoveAt(lastIndex);
                        }
                        singleComment = true;
                    }
                    else if(directive == "line" && !ignoring)
                    {
                        // Skip white.
                        SkipWhite();

                        // Read the line.
                        int newLine = ReadInteger();
                        string newFile = fileName;

                        // Read the optional filename.
                        SkipWhite();
                        if(look == '"')
                        {
                            newFile = ReadLiteral();
                        }

                        // Ignore the rest of the line.
                        ReadLine();

                        // Set the new line data.
                        this.line = newLine;
                        this.fileName = newFile;
                    }
                    else if(directive == "warning" && !ignoring)
                    {
                        Warning(ReadLine());
                    }
                    else if(directive == "error" && !ignoring)
                    {
                        Error(ReadLine());
                    }
                    else if(directive == "region" || directive == "endregion")
                    {
                        // Ignore the directive.
                        ReadLine();
                    }
                    else if(directive == "pragma")
                    {
                        // Ignore the directive.
                        ReadLine();
                    }
                    else if(ignoring)
                    {
                        newline = false;
                    }
                    else
                    {
                        Error("expected preprocessor directive instead of '" + directive + "'.");
                    }
                    continue;
                }
                else if(look == '\n')
                {
                    this.line++; this.column = 0;

                    // Reset single line comment flag.
                    singleComment = false;
                    newline = true;
                }
                else if(look == '\r')
                {
                    Next();
                    if(look == '\n') Next();
                    this.line++; this.column = 0;

                    // Reset single line comment flag.
                    singleComment = false;
                    newline = true;
                    continue;
                }
                else if(look == '/' && !multiComment && !singleComment)
                {
                    // Comment begin.
                    Next();
                    if(look == '/')
                    {
                        // Single line comment.
                        singleComment = true;
                    }
                    else if(look == '*')
                    {
                        // Multi line comment start.
                        multiComment = true;
                    }
                    else
                    {
                        // Got a possible token, return the character.
                        return '/';
                    }
                }
                else if(look == '*' && multiComment)
                {
                    // Check for comment end.
                    Next();
                    if(look == '/')
                    {
                        // End multi comment.
                        multiComment = false;
                    }
                    else
                    {
                        // Use normal space processing for new lines.
                        continue;
                    }
                }
                else if(ignoring)
                {
                    // Just ignore.
                }

                // Read the next character.
                Next();
            }

            return 0;
        }

	    /** move on to next token.
	        @return false if positioned beyond tokens.
	        @throws IOException on input error.
	      */
	    public virtual bool advance ()
		{
            // Perform preprocessing.
            int c1 = Preprocess();

            // Store the line and column.
            int line = this.line;
            int column = this.column;

			// Read numbers.
			bool afterPeriod = false;
			if(c1 == 0 && look == '.')
			{
				afterPeriod = true;
				Next();
				if(!IsDigit(look))
				{
					// It was only a period.
					tokenValue = new TokenValue('.', fileName, line, column);
					return true;
				}		
			}
			
			// Check for numbers of the form ([0-9].[0-9]*(e[+-]?[0-9]+)?) | .[0-9]+(e[+-]?[0-9]+)?
            if(c1 == 0 && IsDigit(look))
			{
				ulong mantissa = 0;
				int exponent = 0;

                // Check for octal and hex.
                bool otherBase = false;
                if(look == '0' && !afterPeriod)
                {
                    Next();
                    if(look == 'x' || look == 'X')
                    {
                        // Set the other base flag.
                        otherBase = true;

                        // Hexadecimal number.
                        Next();
                        while(IsDigit(look) ||
                              (look >= 'a' && look <= 'f') ||
                              (look >= 'A' && look <= 'F'))
                        {
                            // Increase the mantissa.
                            mantissa *= 0x10;

                            // Append the digit.
                            if(look >= '0' && look <= '9')
                            {
                                mantissa += (ulong)(look - '0');
                            }
                            else if(look >= 'a' && look <= 'f')
                            {
                                mantissa += (ulong)(look - 'a' + 0xA);
                            }
                            else if(look >= 'A' && look <= 'F')
                            {
                                mantissa += (ulong)(look - 'A' + 0xA);
                            }

                            // Read the next digit.
                            Next();
                        }
                    }
                    else
                    {
                        // Set the other base flag.
                        otherBase = true;

                        // Octal number.
                        while(look >= '0' && look <= '7')
                        {
                            // Increase the mantissa.
                            mantissa *= 010u;

                            // Append the digit.
                            if(look >= '0' && look <= '7')
                                mantissa += (ulong)(look - '0');

                            // Read the next digit.
                            Next();
                        }

                        if(IsDigit(look) || look == '.' || look == 'e' || look == 'E')
                        {
                            // Unset the other base.
                            otherBase = false;

                            // Convert the octal into decimal.
                            mantissa = ulong.Parse(System.Convert.ToString((long)mantissa, 8));
                        }
                    }

                }

                if(!otherBase)
                {
    				while(IsDigit(look) || look == '.' || look == 'e' || look == 'E')
    				{
    					if(look == '.')
    					{
    						if(afterPeriod)
    							break;
    						afterPeriod = true;
    					}
    					else if(look == 'e' || look == 'E')
    					{
    						// Set the after period flag, to make sure we output a floating point number.
    						afterPeriod = true;
    						
    						// Read the exponent sign.
    						Next();
    						bool pos = true;
    						if(look == '+')
    						{
    							Next();
    						}
    						else if(look == '-')
    						{
    							pos = false;
    							Next();
    						}
    						
    						// Read the exponent.
    						int extraExponent = 0;
    						while(IsDigit(look))
    						{
    							extraExponent = extraExponent*10 + look - '0';
    							Next();
    						}

    						// Update the full exponent.
    						exponent += pos ? extraExponent : -extraExponent;

    						// Break of the loop.
    						break;
    					}
    					else
    					{
    						mantissa = mantissa*10 + (ulong)(look - '0');
    						if(afterPeriod)
    							exponent -= 1;
    					}
    					
    					// Read the next character.
    					Next();
    				}
                }
				
				// Read the suffix.
				int suffix = 0;
				if(IsAlpha(look))
				{
					suffix = look;
					Next();
				}
				
				// Create the token.
				if(afterPeriod)
				{
					// Check the suffix.
					int tok = Token.DOUBLE;
					if(suffix == 'f' || suffix == 'F')
						tok = Token.FLOAT;
					else if(suffix != 'd' && suffix != 'D' && suffix != 0)
					{
						// Invalid suffix.
						Error("Invalid floating point suffix '" + (char)suffix + "'");
					}
					
					// Make the value.
					double value = (double)mantissa;
					int exp = exponent;
					if(exp < 0) exp = -exp;
					for(int i = 0; i < exp; i++)
					{
						if(exponent > 0)
							value *= 10.0;
						else
							value *= 0.1;
					}

					// Create the token.
					tokenValue = new FloatingPointToken(value, tok, fileName, line, column);
				}
				else
				{
					if(suffix == 'u' || suffix == 'U')
					{
                        if(look == 'l' || look == 'L')
                        {
                            // ulong constant.
                            Next();

                            // Create the token.
                            tokenValue = new UIntegralToken(mantissa, Token.ULONG, fileName, line, column);
                        }
                        else
                        {
                            // Create the token.
    						tokenValue = new UIntegralToken(mantissa, Token.UINTEGER, fileName, line, column);
                        }
					}
					else
					{
                        if(suffix == 'l' || suffix == 'L')
                        {
                            // Create the token.
                            tokenValue = new IntegralToken((long)mantissa, Token.LONG, fileName, line, column);
                        }
                        else // Create the token, using the least capable type.
                        {
                            int tok = Token.INTEGER;
                            if(mantissa > 0x7FFFFFFF)
                                tok = Token.LONG;
                            tokenValue = new IntegralToken((long)mantissa, tok, fileName, line, column);
                        }
                    }
				}
				
				return true;
			}


            // Check for C-string.
            bool cstring = false;
            string identprefix = null;
            if(c1 == 0 && look == 'c')
            {
                Next();
                if(look == '"')
                    cstring = true;
                else
                    identprefix = "c";
            }

            // Read identifiers.
			if(c1 == 0 &&  (IsAlpha(look) || look == '_' || identprefix != null))
			{
				// Build the identifier.
				string identifier = ReadIdentifier(identprefix);

				// Boolean values has special treatment.
				if(identifier == "true")
				{
					tokenValue = new BoolToken(true, Token.BOOL, fileName, line, column);
					return true;
				}
				else if(identifier == "false")
				{
					tokenValue = new BoolToken(false, Token.BOOL, fileName, line, column);
					return true;
				}
				
				// Check for keywords.
				int key;
                if(!keywords.TryGetValue(identifier, out key))
                    key = Token.IDENTIFIER;
				
				// Create the token.
				tokenValue = new WordToken(identifier, key,
				                           fileName, line, column);				
				return true;
			}

            // Read raw identifiers.
            if(c1 == 0 && look == '@')
            {
                // Skip the @.
                Next();

                // Read the identifier.
                string identifier = ReadIdentifier();

                // Create the token.
                tokenValue = new WordToken(identifier, Token.IDENTIFIER,
                                           fileName, line, column);
                return true;
            }
			
			// Read literals.
			if(c1 == 0 && look == '"')
			{
                // Create the token.
				tokenValue = new WordToken(ReadLiteral(), cstring ? Token.CSTRING : Token.STRING, fileName, line, column);
				return true;
			}

            // Read characters.
            if(c1 == 0 && look == '\'')
            {
                // Skip the '
                Next();

                // Parse the character.
                char character = '\0';

                if(look == '\\')
                {
                    // Escape sequence.
                    Next();
                    switch(look)
                    {
                    case 'a':
                        character = '\a';
                        break;
                    case 'b':
                        character = '\b';
                        break;
                    case 'f':
                        character = '\f';
                        break;
                    case 't':
                        character = '\t';
                        break;
                    case 'r':
                        character = '\r';
                        break;
                    case 'n':
                        character = '\n';
                        break;
                    case 'v':
                        character = '\v';
                        break;
                    case '0':
                        character = '\0';
                        break;
                    case 'x':
                        break;
                    default:
                        character = (char)look;
                        break;
                    }

                    Next();
                    if(look != '\'')
                        Error("unclosed character literal.");
                }
                else if(look != '\'')
                {
                    character = (char)look;
                    Next();
                    if(look != '\'')
                        Error("unclosed character literal.");
                }
                else
                {
                    Error("empty character literal.");
                }

                // Skip the last \'
                Next();

                // Create the token.
                tokenValue = new IntegralToken(character, Token.CHARACTER, fileName, line, column);
                return true;
            }

			// Check for end-of-file.
			if(c1 == 0 && look == EOF)
			{
				tokenValue = new TokenValue(Token.EOF, fileName, line, column);
				return true;
			}
			
			// It must be an operator.
            int c2;
            if(c1 == 0)
            {
			    c1 = look;
			    Next();
			    c2 = look;
            }
            else
            {
                c2 = look;
            }
			
			// Check for multi character operators.
			if(c2 == '=')
			{
				int tok = -1;
				switch(c1)
				{
				case '<':
					tok = Token.LEQ;
					break;
				case '>':
					tok = Token.GEQ;
					break;
				case '=':
					tok = Token.EQ;
					break;
				case '!':
					tok = Token.NEQ;
					break;
				case '+':
					tok = Token.ADD_SET;
					break;
				case '-':
					tok = Token.SUB_SET;
					break;
				case '*':
					tok = Token.MUL_SET;
					break;
				case '/':
					tok = Token.DIV_SET;
					break;
				case '%':
					tok = Token.MOD_SET;
					break;
				case '&':
					tok = Token.AND_SET;
					break;
				case '|':
					tok = Token.OR_SET;
					break;
				case '^':
					tok = Token.XOR_SET;
					break;
				default:
					break;
				}
				
				if(tok > -1)
				{
					// Advance.
					Next();
					tokenValue = new TokenValue(tok, fileName, line, column);
					return true;
				}
			}
			else if(c1 == '<' && c2 == '<')
			{
				// Advance.
				Next();
				
				// The operator can be << and <<=;
				int tok = Token.LBITSHIFT;
				if(look == '=')
				{
					tok = Token.LSHIFT_SET;
					Next();
				}
				
				// Create the token.
				tokenValue = new TokenValue(tok, fileName, line, column);
				return true;
			}
			else if(c1 == '>' && c2 == '>')
			{
				// Advance.
				Next();
				
				// The operator can be >> or >>=;
				int tok = Token.RBITSHIFT;
				if(look == '=')
				{
					tok = Token.RSHIFT_SET;
					Next();
				}
				
				// Create the token.
				tokenValue = new TokenValue(tok, fileName, line, column);
				return true;
			}
			else if(c1 == '-' && c2 == '>')
			{
				// Advance.
				Next();
				
				// Create the token.
				tokenValue = new TokenValue(Token.ARROW, fileName, line, column);
				return true;
			}
			else if(c1 == '|' && c2 == '|')
			{
				// Advance
				Next();
				
				// Create the token.
				tokenValue = new TokenValue(Token.LOR, fileName, line, column);
				return true;
			}
			else if(c1 == '&' && c2 == '&')
			{
				// Advance
				Next();
				
				// Create the token.
				tokenValue = new TokenValue(Token.LAND, fileName, line, column);
				return true;
			}
            else if(c1 == c2 && c2 == '+')
            {
                // Advance
                Next();

                // Create the token.
                tokenValue = new TokenValue(Token.INCR, fileName, line, column);
                return true;
            }
            else if(c1 == c2 && c2 == '-')
            {
                // Advance
                Next();

                // Create the token.
                tokenValue = new TokenValue(Token.DECR, fileName, line, column);
                return true;
            }
            else if(c1 == '(' && c2 == '*')
            {
                // Check for function pointer start.
                long oldPosition = stream.Position;
                if(stream.ReadByte() == ')')
                {
                    // Consume the last ')'.
                    ++this.column;
                    Next();

                    tokenValue = new TokenValue(Token.FUNC_PTR, fileName, line, column);
                    return true;
                }
                else
                {
                    // Restore the position.
                    stream.Position = oldPosition;
                }
            }
            else if(c1 == '!')
            {
                // Create the token.
                tokenValue = new TokenValue(Token.LNOT, fileName, line, column);
                return true;
            }
            else if(c1 == '&')
            {
                tokenValue = new TokenValue(Token.BITAND, fileName, line, column);
                return true;
            }
            else if(c1 == '|')
            {
                tokenValue = new TokenValue(Token.BITOR, fileName, line, column);
                return true;
            }
            else if(c1 == '^')
            {
                tokenValue = new TokenValue(Token.BITXOR, fileName, line, column);
                return true;
            }
            else if(c1 == '~')
            {
                tokenValue = new TokenValue(Token.BITNOT, fileName, line, column);
                return true;
            }
            else if(c1 == '<')
            {
                return CheckGenericStart();
            }
            else if(c1 == '>' && genericStack.Count > 0)
            {
                int lastIndex = genericStack.Count - 1;
                int tokenId = genericStack[lastIndex];
                genericStack.RemoveAt(lastIndex);
                tokenValue = new TokenValue(tokenId, fileName, line, column);
                return true;
            }


			// Single character token. The token itself is his value.
			tokenValue = new TokenValue(c1, fileName, line, column);
			return true;
		}

        private bool CheckGenericStart()
        {
            // Store the old state.
            int oldLine = line;
            int oldColumn = column;
            int oldLook = look;
            long oldPosition = stream.Position;

            // Start with GENERIC_START.
            int token = Token.GENERIC_START;

            // Make sure there are only comments, ',', '.', '*', '<' and identifiers
            // before hitting '>'.
            int genericCheckCount = 1;
            bool foundDot = false;
            for(;;)
            {
                // Preprocess
                int firstLook = Preprocess();
                if(firstLook == 0)
                    firstLook = look;

                // If hit EOF, end loop.
                if(firstLook == EOF)
                {
                    token = '<';
                    break;
                }

                if(genericCheckCount == 0)
                {
                    foundDot = firstLook == '.';
                    break;
                }

                // Check for identifiers.
                if(IsAlpha(firstLook) || firstLook == '_')
                {
                    // Identifier.
                    Next();
                    while(IsAlpha(look) || IsDigit(look) || look == '_')
                        Next();

                    // Consume the next token.
                    continue;
                }

                // Check for end single character tokens.
                if(firstLook == ',' || firstLook == '.' || firstLook == '*')
                {
                    // Consume the token.
                    Next();
                }
                else if(firstLook == '<')
                {
                    // Consume the token.
                    Next();
                    ++genericCheckCount;

                }
                else if(firstLook == '>')
                {
                    // Consume the token.
                    Next();
                    --genericCheckCount;
                    if(genericCheckCount == 0)
                        continue;
                }
                else
                {
                    // This is not a generic.
                    token = '<';
                    break;
                }

            }

            // Restore the old state.
            line = oldLine;
            column = oldColumn;
            look = oldLook;
            stream.Seek(oldPosition, SeekOrigin.Begin);

            // Increase the generic count.
            if(token == Token.GENERIC_START)
            {
                if(foundDot)
                {
                    token = Token.GENERIC_START_DOT;
                    genericStack.Add(Token.GENERIC_END_DOT);
                }
                else
                {
                    genericStack.Add(Token.GENERIC_END);
                }
            }

            // Return the token.
            tokenValue = new TokenValue(token, fileName, line, column);
            return true;
        }
		
	    /** classifies current token.
	        Should not be called if advance() returned false.
	        @return current %token or single character.
	      */
	    public virtual int token ()
		{
			return tokenValue.GetToken();
		}
		
	    /** associated with current token.
	        Should not be called if advance() returned false.
	        @return value for token().
	      */
	    public virtual object value ()
		{
			return tokenValue;
		}
	}
}

