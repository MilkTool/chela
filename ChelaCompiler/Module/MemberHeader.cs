namespace Chela.Compiler.Module
{
    public enum MemberHeaderType
    {
        Namespace = 0,
        Structure,
        Class,
        Interface,
        Function,
        Field,
        FunctionGroup,
        Property,
        TypeName,
        TypeInstance,
        TypeGroup,
        Event,
        Reference,
        FunctionInstance,
        MemberInstance,
    };
    
    public class MemberHeader
    {
        public byte memberType;
        public uint memberName;
        public uint memberFlags;
        public uint memberSize;
        public byte memberAttributes;
        
        public MemberHeader ()
        {
            this.memberName = 0;
            this.memberType = 0;
            this.memberFlags = 0;
            this.memberSize = 0;
            this.memberAttributes = 0;
        }
        
        public virtual void Write(ModuleWriter writer)
        {
            writer.Write(memberType);
            writer.Write(memberName);            
            writer.Write(memberFlags);
            writer.Write(memberSize);
            writer.Write(memberAttributes);
        }

        public virtual void Read(ModuleReader reader)
        {
            reader.Read(out memberType);
            reader.Read(out memberName);
            reader.Read(out memberFlags);
            reader.Read(out memberSize);
            reader.Read(out memberAttributes);
        }
    }
}

