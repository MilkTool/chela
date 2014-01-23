using System.Collections.Generic;

namespace Chela.Compiler.Module
{
	public class BasicBlock
	{
        public const int BlockHeaderSize = 5;
        
		private Function parentFunction;
		internal int blockIndex;
		private string name;
		private List<Instruction> instructions;
        private List<BasicBlock> preds;
        private List<BasicBlock> successors;
		private int rawInstructionSize;
        private bool isUnsafe;
		
		public BasicBlock (Function parentFunction)
		{
			this.parentFunction = parentFunction;
			this.blockIndex = parentFunction.AddBlock(this);
			this.name = null;
			this.instructions = new List<Instruction> ();
			this.rawInstructionSize = 0;
		}
		
		public Function GetParent()
		{
			return this.parentFunction;
		}
		
		public int GetIndex()
		{
			return this.blockIndex;
		}
		
		public string GetName()
		{
			if(this.name == null)
				this.name = "bb" + blockIndex.ToString();
			return this.name;
		}
		
		public override string ToString ()
		{
			return GetName();
		}

        public bool IsUnsafe {
            get {
                return isUnsafe;
            }
            set {
                isUnsafe = value;
            }
        }
		
		public void SetName(string name)
		{
			bool rename = false;
			
			foreach(BasicBlock bb in parentFunction.GetBasicBlocks())
			{
				if(bb == this)
					continue;
				
				if(bb.GetName() == name)
				{
					rename = true;
					break;
				}
			}
			
			if(rename)
			{
				// Try with new names until there's one new.
				int extra = 1;
				bool next = true;
				while(next)
				{
					string newName = name + extra++;
					next = false;
							
					foreach(BasicBlock bb in parentFunction.GetBasicBlocks())
					{
						if(bb != this && bb.GetName() == newName)
						{
							// Try again with a new name.
							next = true;
							break;
						}
					}
					
					// At last got a suitable name.
					if(!next)
					{
						this.name = newName;
						return;
					}
				}
			}
			
			this.name = name;
		}
		
		internal List<Instruction> GetInstructionList()
		{
			rawInstructionSize = 0;
			return instructions;
		}
		
		public Instruction GetFirst()
		{
            if(instructions.Count == 0)
                return null;
			return instructions[0];
		}
		
		public Instruction GetLast()
		{
            if(instructions.Count == 0)
                return null;
            return instructions[instructions.Count-1];
		}
		
		public ICollection<Instruction> GetInstructions()
		{
			return instructions;
		}
		
		public void Append(Instruction inst)
		{
			instructions.Add(inst);
			rawInstructionSize += inst.GetInstructionSize();
		}
		
		public void Prepend(Instruction inst)
		{
			instructions.Insert(0, inst);
			rawInstructionSize += inst.GetInstructionSize();
		}

        private void AddPredecesor(BasicBlock pred)
        {
            if(preds == null)
                preds = new List<BasicBlock> ();
            preds.Add(pred);
        }

        public void AddSuccessor(BasicBlock successor)
        {
            if(successors == null)
                successors = new List<BasicBlock> ();

            successors.Add(successor);
            successor.AddPredecesor(this);
        }

        public int GetPredsCount()
        {
            if(preds == null)
                return 0;
            return preds.Count;
        }

        public void Destroy()
        {
            parentFunction.RemoveBasicBlock(this);
        }

		public bool IsLastTerminator()
		{
			Instruction last = GetLast();
			return last != null && last.IsTerminator();
		}

		public int GetRawBlockSize()
		{
			if(rawInstructionSize != 0)
				return rawInstructionSize + BlockHeaderSize;
			
			foreach(Instruction inst in instructions)
				rawInstructionSize += inst.GetInstructionSize();
			return rawInstructionSize + BlockHeaderSize;
		}

        internal void PrepareSerialization (ChelaModule module)
        {
            // Prepare the instructions.
            foreach(Instruction inst in instructions)
                inst.PrepareSerialization(module);
        }
		
		public void Write(ChelaModule module, ModuleWriter writer)
		{
			// Write the number of instructions.
			writer.Write((ushort)rawInstructionSize);
			writer.Write((ushort)instructions.Count);
            writer.Write((byte)(isUnsafe ? 1 : 0));
			
			// Write the instructions.
			foreach(Instruction inst in instructions)
				inst.Write(module, writer);				
		}
		
		public void Dump()
		{
			// Write the block label.
			Dumper.Printf("%s:", GetName());
			Dumper.Incr();
			
			// Dump the instructions.
			foreach(Instruction inst in instructions)
				inst.Dump();
			
			Dumper.Decr();
		}
	}
}

