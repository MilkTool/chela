namespace Chela.Compiler.Module
{
    public class PointedSlot: Variable
    {
        public PointedSlot (IChelaType type)
            : base(type, null)
        {
        }
        
        public override bool IsPointedSlot ()
        {
            return true;
        }
    }
}

