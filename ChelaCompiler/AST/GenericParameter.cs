using System.Collections.Generic;
using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class GenericParameter: AstNode
    {
        private bool isValueType;
        private List<Structure> bases;

        public GenericParameter (string name, TokenPosition position)
            : base(position)
        {
            SetName(name);
            this.isValueType = false;
            this.bases = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public void SetValueType(bool isValueType)
        {
            this.isValueType = isValueType;
        }

        public bool IsValueType()
        {
            return isValueType;
        }

        public void SetBases(List<Structure> bases)
        {
            this.bases = bases;
        }

        public List<Structure> GetBases()
        {
            return bases;
        }
    }
}
