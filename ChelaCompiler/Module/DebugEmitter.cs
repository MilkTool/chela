using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    public class DebugEmitter
    {
        class SubBlock
        {
            public TokenPosition Position;
            public ushort Start; // Sub block first instruction.
            public ushort End; // Sub block last instruction.
        };

        private static readonly TokenPosition NullPosition = new TokenPosition("<unknown>", 0, 0);
        private ChelaModule module;
        private List<Function> functions;
        private List<Structure> structures;
        private List<FieldVariable> fields;

        // Use a separated string table
        private Dictionary<string, uint> registeredStrings;
        private List<string> stringTable;

        public DebugEmitter (ChelaModule module)
        {
            this.module = module;
            this.functions = new List<Function> ();
            this.structures = new List<Structure> ();
            this.fields = new List<FieldVariable> ();
            this.registeredStrings = new Dictionary<string, uint> ();
            this.stringTable = new List<string> ();
        }

        public ChelaModule GetModule()
        {
            return module;
        }

        /// <summary>
        /// Debug string table registering.
        /// </summary>
        private uint RegisterString(string s)
        {
            if(s == null)
                return 0;

            // Try to get the already registered string.
            uint res;
            if(registeredStrings.TryGetValue(s, out res))
                return res;

            // Register the new string.
            res = (uint)(stringTable.Count +1);
            stringTable.Add(s);
            registeredStrings.Add(s, res);
            return res;
        }

        private uint AddString(string str)
        {
            return RegisterString(str);
        }

        private uint AddFileName(string filename)
        {
            // TODO: Convert to absolute path.
            return AddString(filename);
        }

        public void Prepare()
        {
            // Register the module file name and work directory.
            AddString(module.GetFileName());
            AddString(module.GetWorkDirectory());
        }

        public void AddFunction(Function function)
        {
            // Store the function.
            functions.Add(function);

            // Register the local scopes.
            for(int i = 0; i < function.GetLexicalScopeCount(); ++i)
            {
                LexicalScope scope = function.GetLexicalScope(i);

                // Register the position filename.
                TokenPosition pos = scope.Position;
                if(pos == null)
                    pos = NullPosition;
                AddFileName(pos.GetFileName());
            }

            // Register the variables names and positions.
            foreach(LocalVariable local in function.GetLocals())
            {
                // Register the local name.
                AddString(local.GetName());

                // Register the position filename
                TokenPosition localPos = local.Position;
                if(localPos == null)
                    localPos = NullPosition;
                AddFileName(localPos.GetFileName());
            }

            // Register the filenames.
            string lastFilename = null;
            foreach(BasicBlock bb in function.GetBasicBlocks())
            {
                foreach(Instruction instruction in bb.GetInstructions())
                {
                    // Ignore instructions without position.
                    TokenPosition position = instruction.GetPosition();
                    if(position == null)
                        position = NullPosition;

                    // Set the filename.
                    string filename = position.GetFileName();
                    if(position != null && lastFilename != filename)
                    {
                        AddFileName(filename);
                        lastFilename = filename;
                    }
                }
            }
        }

        public void AddField(FieldVariable field)
        {
            // Store the field.
            fields.Add(field);

            // Register the position filename.
            TokenPosition pos = field.Position;
            if(pos == null)
                pos = NullPosition;
            AddFileName(pos.GetFileName());
        }

        public void AddStructure(Structure structure)
        {
            // Store the structure.
            structures.Add(structure);

            // Register the position filename.
            TokenPosition pos = structure.Position;
            if(pos == null)
                pos = NullPosition;
            AddFileName(pos.GetFileName());
        }

        private void EmitDebugStringTable(ModuleWriter writer)
        {
            // Write the debug string table records.
            uint numRecords = (uint)stringTable.Count;
            writer.Write(numRecords);

            // Write the string table entries.
            foreach(string s in stringTable)
                writer.Write(s);
        }

        private void EmitPosition(ModuleWriter writer, TokenPosition position)
        {
            // Use the null position if unknown.
            if(position == null)
                position = NullPosition;

            // Emit the filename, line and column.
            uint filename = AddFileName(position.GetFileName());
            int line = position.GetLine();
            int column = position.GetColumn();
            writer.Write(filename);
            writer.Write(line);
            writer.Write(column);
        }

        private void EmitBasicBlockDebugInfo(ModuleWriter writer, BasicBlock block)
        {
            // Compute instructions subblocks.
            List<SubBlock> subBlocks = new List<SubBlock> ();
            TokenPosition position = NullPosition;
            int start = 0;
            int index = 0;
            foreach(Instruction inst in block.GetInstructions())
            {
                // Get the instruction position.
                TokenPosition instPos = inst.GetPosition();
                if(instPos == null)
                    instPos = NullPosition;

                if(instPos != position)
                {
                    // Create a sub block for the previous set.
                    if(index != 0)
                    {
                        SubBlock subBlock = new SubBlock();
                        subBlock.Position = position;
                        subBlock.Start = (ushort)start;
                        subBlock.End = (ushort)(index - 1);

                        // Store the sub block.
                        subBlocks.Add(subBlock);
                    }

                    // Store the start of the next subblock.
                    start = index;
                    position = instPos;
                }

                // Increase the index.
                ++index;
            }

            // Create the last sub block.
            SubBlock lastSubBlock = new SubBlock();
            lastSubBlock.Position = position;
            lastSubBlock.Start = (ushort)(start);
            lastSubBlock.End = (ushort)(index - 1);

            // Store the sub block.
            subBlocks.Add(lastSubBlock);

            // Emit all of the sub blocks.
            ushort numSubs = (ushort)subBlocks.Count;
            writer.Write(numSubs);
            foreach(SubBlock subBlock in subBlocks)
            {
                writer.Write(subBlock.Start);
                writer.Write(subBlock.End);
                EmitPosition(writer, subBlock.Position);
            }
        }

        private void EmitFunctionDebugInfo(ModuleWriter writer, Function function)
        {
            // Emit the function id.
            uint functionId = function.GetSerialId();
            writer.Write(functionId);

            // Emit the function position.
            EmitPosition(writer, function.Position);

            // Emit the lexical scopes.
            byte numscopes = (byte)function.GetLexicalScopeCount();
            writer.Write(numscopes);
            for(int i = 0; i < numscopes; ++i)
            {
                // Get the lexical scope.
                LexicalScope scope = function.GetLexicalScope(i);

                // Find the parent index.
                byte parentIndex = 0;
                LexicalScope parentScope = scope.GetParentScope() as LexicalScope;
                if(parentScope != null)
                    parentIndex = (byte)parentScope.Index;

                // Emit the parent scope.
                writer.Write(parentIndex);

                // Write the scope position.
                EmitPosition(writer, scope.Position);
            }

            // Emit the local data and positions.
            ushort localCount = (ushort)function.GetLocalCount();
            writer.Write(localCount);
            foreach(LocalVariable local in function.GetLocals())
            {
                // Get the local scope.
                byte localScope = 0;
                LexicalScope scope = local.GetParentScope() as LexicalScope;
                if(scope != null)
                    localScope = (byte)scope.Index;

                // Write the variable data.
                writer.Write(AddString(local.GetName()));
                writer.Write(localScope);
                writer.Write((byte)local.Type);
                writer.Write((byte)local.ArgumentIndex);
                EmitPosition(writer, local.Position);
            }

            // Emit each basic block position information.
            ushort blockCount = (ushort)function.GetBasicBlockCount();
            writer.Write(blockCount);
            foreach(BasicBlock bb in function.GetBasicBlocks())
                EmitBasicBlockDebugInfo(writer, bb);
        }

        private void EmitFieldDebugInfo(ModuleWriter writer, FieldVariable field)
        {
            // Emit the field id.
            uint fieldId = field.GetSerialId();
            writer.Write(fieldId);

            // Emit the field position.
            TokenPosition position = field.Position;
            if(position == null)
                position = NullPosition;
            EmitPosition(writer, position);
        }

        private void EmitStructureDebugInfo(ModuleWriter writer, Structure building)
        {
            // Emit the structure id.
            uint buildingId = building.GetSerialId();
            writer.Write(buildingId);

            // Emit the structure position.
            TokenPosition position = building.Position;
            if(position == null)
                position = NullPosition;
            EmitPosition(writer, position);
        }

        public void Write(ModuleWriter writer)
        {
            // Emit the debug string table.
            EmitDebugStringTable(writer);

            // Emit the module name and working directory.
            writer.Write(AddString(module.GetFileName()));
            writer.Write(AddString(module.GetWorkDirectory()));

            // Emit each function debug information.
            uint numFunctions = (uint)functions.Count;
            writer.Write(numFunctions);
            foreach(Function function in functions)
                EmitFunctionDebugInfo(writer, function);

            // Emit each structure debug information.
            uint numStructures = (uint)structures.Count;
            writer.Write(numStructures);
            foreach(Structure building in structures)
                EmitStructureDebugInfo(writer, building);

            // Emit each field debug information.
            uint numFields = (uint)fields.Count;
            writer.Write(numFields);
            foreach(FieldVariable field in fields)
                EmitFieldDebugInfo(writer, field);

        }
    }
}

