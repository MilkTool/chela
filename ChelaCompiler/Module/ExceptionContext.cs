using System.Collections.Generic;

namespace Chela.Compiler.Module
{
    public class ExceptionContext
    {
        private class Handler
        {
            public Structure exception;
            public BasicBlock handler;
        };

        private Function parentFunction;
        private ChelaModule module;
        private ExceptionContext parentContext;
        private List<ExceptionContext> children;
        private List<BasicBlock> blocks;
        private List<Handler> handlers;
        private BasicBlock cleanup;

        public ExceptionContext (ExceptionContext parentContext)
        {
            // Store parent-child relation.
            this.parentContext = parentContext;
            this.parentFunction = parentContext.parentFunction;
            parentContext.children.Add(this);
            module = parentFunction.GetModule();

            // Initialize more variables.
            this.children = new List<ExceptionContext> ();
            this.blocks = new List<BasicBlock> ();
            this.handlers = new List<Handler> ();
            this.cleanup = null;
        }

        public ExceptionContext (Function parentFunction)
        {
            // Store parent-child relation.
            this.parentContext = null;
            this.parentFunction = parentFunction;
            module = parentFunction.GetModule();
            if(parentFunction.exceptionContext != null)
                throw new ModuleException("A function only can have one (toplevel) exception context.");

            parentFunction.exceptionContext = this;

            // Initialize more variables.
            this.children = new List<ExceptionContext> ();
            this.blocks = new List<BasicBlock> ();
            this.handlers = new List<Handler> ();
            this.cleanup = null;
        }

        public ExceptionContext GetParentContext()
        {
            return parentContext;
        }

        public ICollection<ExceptionContext> GetChildren()
        {
            return children;
        }

        public int GetChildrenCount()
        {
            return children.Count;
        }

        public ExceptionContext GetChild(int index)
        {
            if(index < 0 || index > children.Count)
                throw new ModuleException("Invalid child index.");
            return (ExceptionContext)children[index];
        }

        public void AddBlock(BasicBlock block)
        {
            blocks.Add(block);
        }

        public void RemoveBlock(BasicBlock block)
        {
            // Remove it from my blocks.
            for(int i = 0; i < blocks.Count; ++i)
            {
                if(blocks[i] == block)
                {
                    blocks.RemoveAt(i);
                    break;
                }
            }

            // Remove it from my handlers.
            for(int i = 0; i < handlers.Count; ++i)
            {
                Handler h = (Handler)handlers[i];
                if(h.handler == block)
                {
                    handlers.RemoveAt(i);
                    break;
                }
            }

            // Remove it from my cleanup.
            if(cleanup == block)
                cleanup = null;

            // Remove it from my children.
            foreach(ExceptionContext child in children)
                child.RemoveBlock(block);
        }

        public void AddCatch(Structure exception, BasicBlock block)
        {
            foreach(Handler h in handlers)
            {
                if(h.exception == exception)
                    throw new ModuleException("Exception is already handled.");
            }

            // Store.
            Handler ha = new Handler();
            ha.exception = exception;
            ha.handler = block;
            handlers.Add(ha);
        }

        public BasicBlock GetCleanup()
        {
            return cleanup;
        }

        public void SetCleanup(BasicBlock block)
        {
            this.cleanup = block;
        }

        internal void PrepareSerialization()
        {
            foreach(Handler handler in handlers)
                module.RegisterType(handler.exception);
        }

        internal void UpdateParent(Function newParent)
        {
            // Update the parent function.
            parentFunction = newParent;

            // Remove references to old blocks.
            blocks.Clear();
            handlers.Clear();

            // Update the parent in the children.
            foreach(ExceptionContext child in children)
                child.UpdateParent(newParent);
        }

        private int Write(ModuleWriter writer, int parentId, int nextIndex)
        {
            // Write the parent id.
            int myindex = nextIndex;
            writer.Write((sbyte)parentId);

            // Write the number of blocks.
            writer.Write((ushort)blocks.Count);

            // Write the number of catches.
            writer.Write((byte)handlers.Count);

            // Write the cleanup.
            if(cleanup != null)
                writer.Write((int)cleanup.GetIndex());
            else
                writer.Write((int)-1);

            // Write the blocks.
            foreach(BasicBlock block in blocks)
            {
                writer.Write((ushort) block.GetIndex());
            }

            // Write the handlers.
            foreach(Handler handler in handlers)
            {
                writer.Write((uint)module.RegisterType(handler.exception));
                writer.Write((ushort)handler.handler.GetIndex());
            }

            // Write the children.
            foreach(ExceptionContext child in children)
                nextIndex = child.Write(writer, myindex, nextIndex);

            // Return the next index.
            return nextIndex;
        }

        public int GetRawSize()
        {
            int size = 8; // Header size.
            size += blocks.Count*2; // Blocks.
            size += handlers.Count*6; // Handlers.
            return size;
        }

        public int GetFullSize()
        {
            int ret = GetRawSize();
            foreach(ExceptionContext child in children)
                ret += child.GetFullSize();
            return ret;
        }

        public void Write(ModuleWriter writer)
        {
            Write(writer, -1, 0);
        }

        public void Dump()
        {
            Dumper.Printf("{");
            Dumper.Incr();

            // Write the blocks.
            Dumper.Printf("blocks");
            Dumper.Printf("{");
            Dumper.Incr();

            foreach(BasicBlock block in blocks)
            {
                Dumper.Printf("%s", block.GetName());
            }

            Dumper.Decr();
            Dumper.Printf("}");

            // Write the catches.
            foreach(Handler h in handlers)
            {
                Dumper.Printf("catch %s %s", h.exception.GetFullName(), h.handler.GetName());
            }

            // Write the cleanup.
            if(cleanup != null)
                Dumper.Printf("cleanup %s", cleanup.GetName());

            // Write the children context
            foreach(ExceptionContext child in children)
            {
                Dumper.Printf("context");
                child.Dump();
            }

            Dumper.Decr();
            Dumper.Printf("}");
        }
    }
}

