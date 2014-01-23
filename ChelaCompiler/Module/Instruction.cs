using System.Text;

namespace Chela.Compiler.Module
{
	public class Instruction
	{
		private static object[] EmptyArguments = new object[] {};
		private OpCode opcode;
		private object[] arguments;
		private int size;
        private TokenPosition position;
		
		public Instruction (OpCode opcode, object[] arguments)
		{
			this.opcode = opcode;
			this.arguments = arguments;
			this.size = -1;
            this.position = null;
			
			if(this.arguments == null)
				this.arguments = EmptyArguments;
		}
		
		public OpCode GetOpCode()
		{
			return this.opcode;
		}
		
		public object[] GetArguments()
		{
			return this.arguments;
		}

        public void SetPosition(TokenPosition position)
        {
            this.position = position;
        }

        public TokenPosition GetPosition()
        {
            return this.position;
        }
		
		public bool IsTerminator()
		{
			return opcode == OpCode.Ret || opcode == OpCode.RetVoid ||
                opcode == OpCode.Jmp || opcode == OpCode.JumpResume ||
                opcode == OpCode.Throw || opcode == OpCode.Switch;
		}
		
		public int GetInstructionSize()
		{
			if(size > 0)
				return size;
			
			// At least the opcode must be counted.
			size = 1;
			
			// Count the arguments.
			InstructionDescription desc = InstructionDescription.GetInstructionTable()[(int)opcode];
			InstructionArgumentType[] args = desc.GetArguments();
			int n;
			for(int i = 0; i < arguments.Length; i++)
			{
				object arg = arguments[i];
				switch(args[i])
				{
				case InstructionArgumentType.UInt8:
				case InstructionArgumentType.Int8:
					size += 1;
					break;
				case InstructionArgumentType.UInt8V:
				case InstructionArgumentType.Int8V:
					n = (byte)arg;
					size += n;
					i += n;
					break;
				case InstructionArgumentType.UInt16:
				case InstructionArgumentType.Int16:
					size += 2;
					break;
				case InstructionArgumentType.UInt16V:
				case InstructionArgumentType.Int16V:
					n = (byte)arg;
					size += 2*n+1;
					i += n;
					break;
				case InstructionArgumentType.UInt32:
				case InstructionArgumentType.Int32:
					size += 4;
					break;
				case InstructionArgumentType.UInt32V:
				case InstructionArgumentType.Int32V:
				case InstructionArgumentType.Fp32V:
					n = (byte)arg;
					size += 4*n+1;
					i += n;
					break;
				case InstructionArgumentType.UInt64:
				case InstructionArgumentType.Int64:
					size += 8;
					break;
				case InstructionArgumentType.UInt64V:
				case InstructionArgumentType.Int64V:
				case InstructionArgumentType.Fp64V:
					n = (byte)arg;
					size += 8*n+1;
					i += n;
					break;
				case InstructionArgumentType.Fp32:
					size += 4;
					break;
				case InstructionArgumentType.Fp64:
					size += 8;
					break;
				case InstructionArgumentType.TypeID:
					size += 4;
					break;
				case InstructionArgumentType.GlobalID:
					size += 4;
					break;
                case InstructionArgumentType.FieldID:
				case InstructionArgumentType.FunctionID:
					size += 4;
					break;
				case InstructionArgumentType.StringID:
					size += 4;
					break;
				case InstructionArgumentType.BasicBlockID:
					size += 2;
					break;
                case InstructionArgumentType.JumpTable:
                    {
                        n = (ushort)arg;
                        size += n*6 + 2;
                        i += n*2;
                    }
                    break;
				}
			}
			
			// Return it.
			return size;			
		}

        internal void PrepareSerialization (ChelaModule module)
        {
            // Register types and external members used.
            InstructionDescription desc = InstructionDescription.GetInstructionTable()[(int)opcode];
            InstructionArgumentType[] args = desc.GetArguments();
            for(int i = 0, k = 0; i < arguments.Length; i++, k++)
            {
                object arg = arguments[i];
                switch(args[k])
                {
                case InstructionArgumentType.UInt8V:
                case InstructionArgumentType.Int8V:
                case InstructionArgumentType.UInt16V:
                case InstructionArgumentType.Int16V:
                case InstructionArgumentType.UInt32V:
                case InstructionArgumentType.Int32V:
                case InstructionArgumentType.UInt64V:
                case InstructionArgumentType.Int64V:
                case InstructionArgumentType.Fp32V:
                case InstructionArgumentType.Fp64V:
                    {
                        // Ignore the vector arguments.
                        byte n = (byte)arg;
                        i += n;
                    }
                    break;
                case InstructionArgumentType.TypeID:
                    {
                        // Prepare serialization of generic instances.
                        IChelaType argType = (IChelaType)arg;
                        if(argType.IsGenericInstance() &&
                            (argType.IsStructure() || argType.IsClass() || argType.IsInterface()))
                        {
                            ScopeMember member = (ScopeMember)argType;
                            member.PrepareSerialization();
                        }

                        // Register types.
                        module.RegisterType(argType);
                    }
                    break;
                case InstructionArgumentType.GlobalID:
                case InstructionArgumentType.FieldID:
                case InstructionArgumentType.FunctionID:
                    {
                        // Register external members.
                        ScopeMember member = (ScopeMember) arg;
                        if(member != null)
                        {
                            if(member.IsGenericInstance())
                                member.PrepareSerialization();
                            module.RegisterMember(member);
                        }
                    }
                    break;
                case InstructionArgumentType.JumpTable:
                    {
                        // Ignore the jump table
                        ushort tableLen = (ushort) arg;
                        i += tableLen*2;
                    }
                    break;
                default:
                    // Do nothing.
                    break;
                }
            }
        }

		public void Write(ChelaModule module, ModuleWriter writer)
		{
			// Write the opcode.
			writer.Write((byte)opcode);
			
			// Write the instruction arguments.
			InstructionDescription desc = InstructionDescription.GetInstructionTable()[(int)opcode];
			InstructionArgumentType[] args = desc.GetArguments();
			byte n;
			for(int i = 0, k = 0; i < arguments.Length; i++, k++)
			{
				object arg = arguments[i];
				switch(args[k])
				{
				case InstructionArgumentType.UInt8:
					writer.Write((byte)arg);
					break;
		        case InstructionArgumentType.UInt8V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((byte)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Int8:
					writer.Write((sbyte)arg);
					break;
		        case InstructionArgumentType.Int8V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((sbyte)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.UInt16:
					writer.Write((ushort)arg);
					break;
		        case InstructionArgumentType.UInt16V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((ushort)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Int16:
					writer.Write((short)arg);
					break;
		        case InstructionArgumentType.Int16V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((short)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.UInt32:
					writer.Write((uint)arg);
					break;
		        case InstructionArgumentType.UInt32V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((uint)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Int32:
					writer.Write((int)arg);
					break;
		        case InstructionArgumentType.Int32V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((int)arguments[i++]);
					}
					break;					
		        case InstructionArgumentType.UInt64:
					writer.Write((ulong)arg);
					break;
		        case InstructionArgumentType.UInt64V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((ulong)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Int64:
					writer.Write((long)arg);
					break;
		        case InstructionArgumentType.Int64V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((long)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Fp32:
					writer.Write((float)arg);
					break;
		        case InstructionArgumentType.Fp32V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((float)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.Fp64:
					writer.Write((double)arg);
					break;
		        case InstructionArgumentType.Fp64V:
					{
						n = (byte)arg;
						writer.Write(n); i++;
						for(int j = 0; j < n; j++)
							writer.Write((double)arguments[i++]);
					}
					break;
		        case InstructionArgumentType.TypeID:
					{
						uint tyid = module.RegisterType((IChelaType)arg);
						writer.Write(tyid);
					}
					break;
		        case InstructionArgumentType.GlobalID:
                case InstructionArgumentType.FieldID:
				case InstructionArgumentType.FunctionID:
					{
						ScopeMember member = (ScopeMember) arg;
						uint id = member != null ? module.RegisterMember(member) : 0u;
						writer.Write(id);
					}
					break;
		        case InstructionArgumentType.StringID:
					{
						uint sid = module.RegisterString((string)arg);
						writer.Write(sid);
					}
					break;
		        case InstructionArgumentType.BasicBlockID:
					{
						BasicBlock bb = (BasicBlock) arg;
						writer.Write((ushort)bb.GetIndex());
					}
					break;
                case InstructionArgumentType.JumpTable:
                    {
                        ushort tableLen = (ushort) arg;
                        writer.Write(tableLen); ++i;
                        for(int j = 0; j < tableLen; j++)
                        {
                            int constant = (int)arguments[i++];
                            writer.Write((int)constant);
                            BasicBlock bb = (BasicBlock) arguments[i++];
                            writer.Write((ushort)bb.GetIndex());
                        }
                    }
                    break;
				}
			}
		}
		
		public void Dump()
		{
			StringBuilder builder = new StringBuilder();
			InstructionDescription desc =
				InstructionDescription.GetInstructionTable()[(int)opcode];
			builder.Append(desc.GetMnemonic());
			
			bool first = true;
			foreach(object a in arguments)
			{
				if(first)
				{
					builder.Append(" ");
					first = false;
				}
				else
				{
					builder.Append(", ");
				}
				
				string s = a != null ? a.ToString() : "None";
				if(opcode == OpCode.LoadString)
					s = "\"" + s + "\"";
				builder.Append(s);
			}
			
			Dumper.Printf(builder.ToString());
		}
	}
}

