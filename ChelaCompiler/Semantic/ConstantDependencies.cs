using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ConstantDependencies: ObjectDeclarator
    {
        private Dictionary<FieldVariable, ConstantData> constants;
        private ConstantData currentConstant;

        public ConstantDependencies (Dictionary<FieldVariable, ConstantData> constants)
        {
            this.constants = constants;
            this.currentConstant = null;
        }

        public List<ConstantData> BuildGraph()
        {
            // First build the constants dependency graph.
            foreach(ConstantData constant in constants.Values)
            {
                // Store the current constant.
                currentConstant = constant;

                // Visit the initializer expression.
                Expression init = constant.GetInitializer();
                init.Accept(this);
            }

            // Now, perform topological sort.
            List<ConstantData> sorted = new List<ConstantData> ();
            foreach(ConstantData constant in constants.Values)
                TopoVisit(constant, sorted);
            return sorted;
        }

        private void TopoVisit(ConstantData constant, List<ConstantData> sorted)
        {
            // Prevent circular references.
            if(constant.visiting)
                Error(constant.GetInitializer(), "circular constant initialization.");

            // Ignore visited constants.
            if(constant.visited)
                return;

            // Set the visiting flag.
            constant.visiting = true;

            // Visit the constant dependencies.
            foreach(ConstantData dep in constant.GetDependencies())
                TopoVisit(dep, sorted);

            // Append the constant to the sorted list.
            sorted.Add(constant);

            // Unset the visiting flag, mark as visited.
            constant.visited = true;
            constant.visiting = false;
        }

        public override AstNode Visit (UnaryOperation node)
        {
            // Visit recursively.
            node.GetExpression().Accept(this);
            return node;
        }

        public override AstNode Visit (BinaryOperation node)
        {
            // Visit recursively.
            node.GetLeftExpression().Accept(this);
            node.GetRightExpression().Accept(this);
            return node;
        }

        public override AstNode Visit (TernaryOperation node)
        {
            // Visit recursively.
            node.GetCondExpression().Accept(this);
            node.GetLeftExpression().Accept(this);
            node.GetRightExpression().Accept(this);
            return node;
        }

        public override AstNode Visit (CastOperation node)
        {
            // Visit recursively.
            node.GetValue().Accept(this);
            return node;
        }

        public override AstNode Visit (CallExpression node)
        {
            Error(node, "function call unallowed in a constant declaration.");
            return node;
        }

        public override AstNode Visit (VariableReference node)
        {
            // Get the node type.
            IChelaType variableType = node.GetNodeType();

            // Ignore type references, namespaces and functions.
            if(variableType.IsMetaType() || variableType.IsNamespace() ||
               variableType.IsFunctionGroup() || variableType.IsFunction())
                return node;

            // The type must be a reference.
            variableType = DeReferenceType(variableType);

            // Now, it must be a constant.
            if(!variableType.IsConstant())
                Error(node, "constant initialization can't reference no constant variables.");

            // The node value, must be the constant variable.
            FieldVariable constantVar = (FieldVariable)node.GetNodeValue();

            // Find the corresponding constant data.
            ConstantData depData;
            if(constants.TryGetValue(constantVar, out depData))
                currentConstant.AddDependency(depData);

            return node;
        }

        public override AstNode Visit (MemberAccess node)
        {
            // This is the same as VariableReference implementation.
            // Get the node type.
            IChelaType variableType = node.GetNodeType();

            // Ignore type references, namespaces and functions.
            if(variableType.IsMetaType() || variableType.IsNamespace() ||
               variableType.IsFunctionGroup() || variableType.IsFunction())
                return node;

            // The type must be a reference.
            variableType = DeReferenceType(variableType);

            // Now, it must be a constant.
            if(!variableType.IsConstant())
                Error(node, "constant initialization can't reference no constant variables.");

            // The node value, must be the constant variable.
            FieldVariable constantVar = (FieldVariable)node.GetNodeValue();

            // Find the corresponding constant data.
            ConstantData depData;
            if(constants.TryGetValue(constantVar, out depData))
                currentConstant.AddDependency(depData);

            return node;
        }

        public override AstNode Visit (IndirectAccess node)
        {
            Error(node, "indirect access(->) unallowed in a constant declaration.");
            return node;
        }
    }
}

