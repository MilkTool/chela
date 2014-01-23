namespace Chela.Compiler.Module
{
    public class SwizzleVariable: Variable
    {
        private Variable reference;
        private byte mask;
        private int comps;

        public SwizzleVariable (IChelaType type, Variable reference, byte mask, int comps)
            : base(type, null)
        {
            this.reference = reference;
            this.mask = mask;
            this.comps = comps;
        }

        public override bool IsSwizzleVariable()
        {
            return true;
        }

        public Variable Reference {
            get {
                return reference;
            }
        }

        public byte Mask {
            get {
                return mask;
            }
        }

        public int Components {
            get {
                return comps;
            }
        }

        public bool IsSettable()
        {
            int c1 = mask & 3;
            int c2 = (mask>>2) & 3;
            int c3 = (mask>>4) & 3;
            int c4 = (mask>>6) & 3;
            switch(Components)
            {
            case 1: return true;
            case 2: return c1 != c2;
            case 3: return c1 != c2 && c2 != c3;
            case 4: return c1 != c2 && c2 != c3 && c3 != c4;
            default:
                throw new ModuleException("Invalid swizzle size.");
            }
        }
    }
}

