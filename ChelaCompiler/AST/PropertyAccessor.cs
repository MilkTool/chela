using System.Collections.Generic;
using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class PropertyAccessor: FunctionDefinition
    {
        private MemberFlags flags;
        private PropertyVariable property;
        private LocalVariable selfLocal;
        private AstNode indices;
        private List<LocalVariable> indexerVariables;

        public PropertyAccessor (MemberFlags flags, AstNode children, TokenPosition position)
            : base(null, children, position)
        {
            this.flags = flags;
            this.property = null;
            this.selfLocal = null;
            this.indices = null;
            this.indexerVariables = null;
        }

        public virtual bool IsGetAccessor()
        {
            return false;
        }

        public virtual bool IsSetAccessor()
        {
            return false;
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }

        public PropertyVariable GetProperty()
        {
            return property;
        }

        public void SetProperty(PropertyVariable property)
        {
            this.property = property;
        }

        public LocalVariable GetSelfLocal()
        {
            return selfLocal;
        }

        public void SetSelfLocal(LocalVariable local)
        {
            selfLocal = local;
        }

        public AstNode GetIndices()
        {
            return indices;
        }

        public void SetIndices(AstNode indices)
        {
            this.indices = indices;
        }

        public List<LocalVariable> GetIndexerVariables()
        {
            return this.indexerVariables;
        }

        public void SetIndexerVariables(List<LocalVariable> variables)
        {
            this.indexerVariables = variables;
        }
    }
}

