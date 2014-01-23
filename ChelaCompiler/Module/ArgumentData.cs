namespace Chela.Compiler.Module
{
    public class ArgumentData
    {
        private IChelaType argumentType;
        private int index;
        private string name;

        public ArgumentData (IChelaType argumentType, int index)
        {
            this.argumentType = argumentType;
            this.index = index;
        }

        public IChelaType Type {
            get {
                return argumentType;
            }
        }

        public int Index {
            get {
                return index;
            }
        }

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        public int GetSize()
        {
            return 4;
        }

        public void Write(ModuleWriter writer, ChelaModule module)
        {
            writer.Write(module.RegisterString(name));
        }
    }
}

