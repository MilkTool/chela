using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ConstantData
    {
        private FieldVariable variable;
        private Expression initializer;
        private List<ConstantData> dependencies; // TODO: Use a hashset.
        internal bool visited;
        internal bool visiting;

        public ConstantData (FieldVariable variable, Expression initializer)
        {
            this.variable = variable;
            this.initializer = initializer;
            this.dependencies = new List<ConstantData> ();
            this.visited = false;
            this.visiting = false;
        }

        public FieldVariable GetVariable()
        {
            return this.variable;
        }

        public Expression GetInitializer()
        {
            return this.initializer;
        }

        public void SetInitializer(Expression initializer)
        {
            this.initializer = initializer;
        }

        public ICollection<ConstantData> GetDependencies()
        {
            return dependencies;
        }

        public void AddDependency(ConstantData dep)
        {
            foreach(ConstantData field in dependencies)
                if(field == dep)
                    return;

            dependencies.Add(dep);
        }
    }
}

