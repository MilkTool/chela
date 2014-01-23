using System.Collections.Generic;
using System.Text;

namespace Chela.Compiler.Module
{
    public class Function: ScopeMember
    {
        protected string name;
        private string displayName;
        protected MemberFlags flags;
        private FunctionType type;
        private List<LexicalScope> lexicalScopes;
        private List<BasicBlock> basicBlocks;
        private List<LocalVariable> locals;
        internal Scope parentScope;
        private TokenPosition position;
        internal ExceptionContext exceptionContext;
        private BasicBlock returnBlock;
        private LocalVariable returningVar;
        private LocalVariable returnValueVar;
        private GenericPrototype genericPrototype;
        private ArgumentData[] arguments;
        private KernelEntryPoint kernelEntryPoint;
        private int nextLocalId;

        // Properties used by the compiler.
        private Ast.FunctionDefinition definitionNode;

        protected Function(ChelaModule module)
            : base(module)
        {
            this.name = string.Empty;
            this.flags = MemberFlags.Default;
            this.parentScope = null;
            this.basicBlocks = new List<BasicBlock> ();
            this.lexicalScopes = new List<LexicalScope>();
            this.locals = new List<LocalVariable> ();
            this.position = null;
            this.exceptionContext = null;
            this.returnBlock = null;
            this.returningVar = null;
            this.returnValueVar = null;
            this.genericPrototype = GenericPrototype.Empty;
            this.arguments  = null;
        }
        
        public Function (string name, MemberFlags flags, Scope parentScope)
            : this(parentScope.GetModule())
        {
            this.name = name;
            this.flags = flags;
            this.parentScope = parentScope;
        }

        public override Scope GetParentScope()
        {
            return parentScope;
        }
        
        public override string GetFullName()
        {
            // Return the name if its cdecl.
            if((GetFlags() & MemberFlags.LanguageMask) == MemberFlags.Cdecl)
                return GetName();

            // Build the full name.
            return base.GetFullName() + " " + GetFunctionType().GetFullName();
        }

        public override string GetDisplayName()
        {
            if(displayName == null)
            {
                StringBuilder builder = new StringBuilder();
                FunctionType functionType = GetFunctionType();

                // Add the return type.
                builder.Append(functionType.GetReturnType().GetDisplayName());

                // Add the function name.
                builder.Append(" ");
                builder.Append(base.GetDisplayName());

                // Add the argument list.
                builder.Append(" (");
                int argCount = functionType.GetArgumentCount();
                for(int i = 0; i < argCount; ++i)
                {
                    if(i+1 == argCount)
                    {
                        if(functionType.IsVariable())
                            builder.Append("params ");
                        builder.Append(functionType.GetArgument(i).GetDisplayName());
                    }
                    else
                    {
                        builder.Append(functionType.GetArgument(i).GetDisplayName());
                        builder.Append(", ");
                    }
                }
                builder.Append(")");
                displayName = builder.ToString();
            }

            return displayName;
        }

        public override string GetName ()
        {
            return name;
        }

        internal void SetName(string name)
        {
            this.name = name;
        }
        
        public override string ToString ()
        {
            return GetFullName();
        }
        
        public virtual bool IsMethod()
        {
            return false;
        }

        public virtual bool IsKernelEntryPoint()
        {
            return false;
        }

        public virtual bool IsAmbiguity()
        {
            return false;
        }

        public virtual void CheckAmbiguity(TokenPosition where)
        {
            // Do nothing.
        }
        
        public FunctionType GetFunctionType()
        {
            return type;
        }
        
        public void SetFunctionType(FunctionType type)
        {
            this.type = type;

            // Update the argument array.
            int count = type.GetArgumentCount();
            arguments = new ArgumentData[count];
            for(int i = 0; i < count; ++i)
            {
                ArgumentData arg = new ArgumentData(type.GetArgument(i), i);
                arguments[i] = arg;
            }
        }

        public KernelEntryPoint EntryPoint {
            get {
                return kernelEntryPoint;
            }
            set {
                kernelEntryPoint = value;
            }
        }

        public ExceptionContext GetExceptionContext()
        {
            return exceptionContext;
        }
        
        public override MemberFlags GetFlags ()
        {
            return this.flags;
        }

        public override TokenPosition Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }
        
        public override bool IsFunction ()
        {
            return true;
        }

        public virtual bool IsConstructor()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Constructor;
        }

        public virtual bool IsStaticConstructor()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.StaticConstructor;
        }

        public virtual bool IsCdecl()
        {
            return (GetFlags() & MemberFlags.LanguageMask) == MemberFlags.Cdecl;
        }

        public virtual bool IsVirtual()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Virtual;
        }

        public virtual bool IsOverride()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Override;
        }
     
        public virtual bool IsAbstract ()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Abstract;
        }

        public virtual bool IsContract ()
        {
            return (GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Contract;
        }

        public virtual bool IsVirtualCallable()
        {
            return false;
        }

        internal int AddLexicalScope(LexicalScope scope)
        {
            int ret = lexicalScopes.Count;
            this.lexicalScopes.Add(scope);
            return ret;
        }

        public int GetLexicalScopeCount()
        {
            return lexicalScopes.Count;
        }

        public LexicalScope GetLexicalScope(int index)
        {
            return lexicalScopes[index];
        }

        internal int AddBlock(BasicBlock block)
        {
            int ret = this.basicBlocks.Count;
            this.basicBlocks.Add(block);
            return ret;
        }
        
        public int GetBasicBlockCount()
        {
            return this.basicBlocks.Count;
        }
        
        public BasicBlock GetBasicBlock(int index)
        {
            return this.basicBlocks[index];
        }
        
        public ICollection<BasicBlock> GetBasicBlocks()
        {
            return this.basicBlocks;
        }

        internal void RemoveBasicBlock(BasicBlock block)
        {
            // Displace the upper blocks.
            int numblocks = basicBlocks.Count;
            for(int i = block.blockIndex; i < numblocks-1; ++i)
            {
                BasicBlock next = (BasicBlock)basicBlocks[i+1];
                next.blockIndex = i;
                basicBlocks[i] = next;
            }

            // Remove the last block.
            basicBlocks.RemoveAt(numblocks-1);

            // Remove the block from the exception contexts
            if(exceptionContext != null)
                exceptionContext.RemoveBlock(block);
        }
        
        internal int AddLocal(LocalVariable local)
        {
            // Don't write pseudo locals.
            if(local.IsPseudoLocal)
                return -1;

            // Add the local.
            this.locals.Add(local);
            return nextLocalId++;
        }
        
        public int GetLocalCount()
        {
            return this.locals.Count;
        }
        
        public LocalVariable GetLocal(int index)
        {
            return this.locals[index];
        }
        
        public ICollection<LocalVariable> GetLocals()
        {
            return this.locals;
        }

        public ArgumentData[] GetArguments()
        {
            return arguments;
        }

        public void RebaseLocals()
        {
            nextLocalId = 0;
            foreach(LocalVariable local in locals)
            {
                if(!local.IsPseudoLocal)
                    local.SetLocalIndex(nextLocalId++);
            }
        }

        private int CountExceptionContexts(ExceptionContext context)
        {
            int ret = context.GetChildrenCount();
            foreach(ExceptionContext subcontext in context.GetChildren())
                ret += CountExceptionContexts(subcontext);
            return ret;
        }

        private int CountExceptionContexts()
        {
            if(exceptionContext != null)
                return CountExceptionContexts(exceptionContext) + 1;
            return 0;
        }

        internal void SwapExceptionContexts(Function other)
        {
            // Make sure the other end also takes my exception context.
            ExceptionContext newContext = other.exceptionContext;
            if(exceptionContext != null)
            {
                other.exceptionContext = null;
                other.SwapExceptionContexts(this);
            }

            // Use the new exception context.
            exceptionContext = newContext;
            if(newContext != null)
                exceptionContext.UpdateParent(this);
        }
        /// <summary>
        /// Gets or sets the definition AST node.
        /// </summary>
        public Ast.FunctionDefinition DefinitionNode {
            get {
                return definitionNode;
            }
            set {
                definitionNode = value;
            }
        }

        public void SetGenericPrototype(GenericPrototype genericPrototype)
        {
            this.genericPrototype = genericPrototype;
        }

        public override GenericPrototype GetGenericPrototype()
        {
            return genericPrototype;
        }

        public BasicBlock ReturnBlock {
            get {
                return returnBlock;
            }
            set {
                returnBlock = value;
            }
        }

        public LocalVariable ReturningVar {
            get {
                return returningVar;
            }
            set {
                returningVar = value;
            }
        }

        public LocalVariable ReturnValueVar {
            get {
                return returnValueVar;
            }
            set {
                returnValueVar = value;
            }
        }

        internal override void PrepareSerialization ()
        {
            ChelaModule module = GetModule();

            // Prepare myself.
            base.PrepareSerialization ();

            // Register the function type.
            module.RegisterType(type);

            // Prepare the blocks.
            foreach(BasicBlock bb in basicBlocks)
                bb.PrepareSerialization(module);

            // Prepare the locals.
            foreach(LocalVariable local in locals)
                local.PrepareSerialization();

            // Prepare exceptions.
            if(exceptionContext != null)
                exceptionContext.PrepareSerialization();
        }

        public override void Write (ModuleWriter writer)
        {
            // Count the block size.
            int blockSize = 0;
            foreach(BasicBlock bb in basicBlocks)
                blockSize += bb.GetRawBlockSize();

            // Count the argument size.
            int argSize = 0;
            for(int i = 0; i < arguments.Length; ++i)
                argSize += arguments[i].GetSize();

            // Count the exceptions.
            int numexceptions = CountExceptionContexts();
            int exceptionSize = 0;
            if(numexceptions > 0)
                exceptionSize = exceptionContext.GetFullSize();

            // Create the member header.
            MemberHeader mheader = new MemberHeader();
            mheader.memberType = (byte)MemberHeaderType.Function;
            mheader.memberFlags = (uint) GetFlags();
            mheader.memberName = GetModule().RegisterString(GetName());
            mheader.memberSize = (uint)(FunctionHeader.HeaderSize + blockSize +
                argSize + nextLocalId*LocalVariable.LocalDataSize + exceptionSize +
                genericPrototype.GetSize());
            mheader.memberAttributes = GetAttributeCount();

            // Write the member header.
            mheader.Write(writer);

            // Write the attributes.
            WriteAttributes(writer);

            // Create the header.
            FunctionHeader header = new FunctionHeader();
            header.functionType = GetModule().RegisterType(GetFunctionType());
            header.numargs = (ushort)arguments.Length;
            header.numlocals = (ushort)nextLocalId;
            header.numblocks = (ushort)basicBlocks.Count;
            header.numexceptions = (byte)numexceptions;
            if(IsMethod() && !IsStatic())
            {
                Method method = (Method)this;
                if(method.IsAbstract() || method.IsContract() ||
                   method.IsOverride() || method.IsVirtual())
                    header.vslot = (short)method.GetVSlot();
            }
            
            // Write the header.
            header.Write(writer);

            // Write the generic signature.
            genericPrototype.Write(writer, GetModule());

            // Write the argument names.
            ChelaModule module = GetModule();
            for(int i = 0; i < arguments.Length; ++i)
                arguments[i].Write(writer, module);
            
            // Write the locals.
            foreach(LocalVariable local in locals)
            {
                if(!local.IsPseudoLocal)
                    local.Write(writer);
            }
            
            // Write the basic blocks.
            foreach(BasicBlock bb in basicBlocks)
                bb.Write(GetModule(), writer);

            // Write the exception contexts.
            if(exceptionContext != null)
                exceptionContext.Write(writer);
        }

        internal static void PreloadMember(ChelaModule module, ModuleReader reader, MemberHeader header)
        {
            // Check if the function is a method.
            MemberFlags instanceFlags = (MemberFlags)header.memberFlags & MemberFlags.InstanceMask;
            bool method = instanceFlags != MemberFlags.Static &&
                          instanceFlags != MemberFlags.StaticConstructor;

            // Create the temporal function and register it.
            Function function = method ? new Method(module) : new Function(module);
            module.RegisterMember(function);

            // Read the name and flags.
            function.name = module.GetString(header.memberName);
            function.flags = (MemberFlags)header.memberFlags;

            // Skip the structure elements.
            reader.Skip(header.memberSize);
        }

        internal override void Read(ModuleReader reader, MemberHeader header)
        {
            // Read the function header.
            FunctionHeader fheader = new FunctionHeader();
            fheader.Read(reader);

            // Read the function type.
            type = (FunctionType)GetModule().GetType(fheader.functionType);

            // Read the generic prototype.
            genericPrototype = GenericPrototype.Read(reader, GetModule());

            // Read the vslot.
            if(IsMethod())
            {
                Method method = (Method)this;
                method.vslot = fheader.vslot;
            }

            // Skip the elements.
            reader.Skip(header.memberSize - FunctionHeader.HeaderSize - genericPrototype.GetSize());
        }

        internal override void UpdateParent (Scope parentScope)
        {
            // Store the new parent.
            this.parentScope = parentScope;
        }

        internal override void PrepareDebug(DebugEmitter debugEmitter)
        {
            debugEmitter.AddFunction(this);
        }

        public FunctionInstance InstanceGeneric(GenericInstance genericInstance, ChelaModule instanceModule)
        {
            return new FunctionInstance(this, genericInstance, instanceModule);
        }

        public override ScopeMember InstanceMember (ScopeMember factory, GenericInstance genericInstance)
        {
            return new FunctionInstance(this, genericInstance, factory);
        }

        public override void Dump ()
        {
            // Get the linkage flags.
            MemberFlags linkage = GetFlags() & MemberFlags.LinkageMask;
            bool external = linkage == MemberFlags.External;
            if(GetAttributeCount() != 0)
                Dumper.Printf("[%d]", (int)GetAttributeCount());
            Dumper.Printf("%s function %s %s%s", GetFlagsString(), name,
                          type != null ? type.GetFullName() : "void ()",
                          external ? ";" : "");
            
            // There isn't content in external functions.
            if(external)
                return;            
            
            Dumper.Printf("{");
            
            DumpContent();

            Dumper.Printf("}");
        }
        
        protected void DumpContent()
        {
            Dumper.Incr();
            Dumper.Printf("; Locals %d", nextLocalId);
            foreach(LocalVariable local in locals)
            {
                if(!local.IsPseudoLocal)
                    local.Dump();
            }

            if(exceptionContext != null)
            {
                Dumper.Printf("@exceptions");
                exceptionContext.Dump();
            }

            Dumper.Decr();
            
            foreach(BasicBlock bb in basicBlocks)
                bb.Dump();
        }
    }
}
