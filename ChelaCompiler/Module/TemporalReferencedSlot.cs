namespace Chela.Compiler.Module
{
    public class TemporalReferencedSlot: Variable
    {
        public TemporalReferencedSlot (IChelaType type)
            : base(type, null)
        {
        }
        
        public override bool IsTemporalReferencedSlot ()
        {
            return true;
        }
    }
}

