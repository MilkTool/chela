namespace Chela.Compiler.Module
{
    public class ReferencedSlot: Variable
    {
        public ReferencedSlot (IChelaType type)
            : base(type, null)
        {
        }
        
        public override bool IsReferencedSlot ()
        {
            return true;
        }
    }
}

