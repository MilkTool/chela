using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ConstantGrouping: ObjectDeclarator
    {
        private Dictionary<FieldVariable, ConstantData> constants;

        public ConstantGrouping ()
        {
            this.constants = new Dictionary<FieldVariable, ConstantData> ();
        }

        public override AstNode Visit (FieldDefinition node)
        {
            // Visit the declarations.
            VisitList(node.GetDeclarations());
            return node;
        }

        public override AstNode Visit (EnumConstantDefinition node)
        {
            // Get the field.
            FieldVariable field = node.GetVariable();

            // Ignore external constants.
            if(field.IsExternal())
                return node;

            // Get the initializer.
            Expression initializer = node.GetValue();
            if(initializer == null)
                Error(node, "constants must have initializers.");

            // Don't allow multiple definitions.
            if(constants.ContainsKey(field))
                Error(node, "multiples definitions of a constant.");

            // Store the constant.
            ConstantData data = new ConstantData(field, initializer);
            this.constants.Add(field, data);

            // Return the node.
            return node;
        }

        public override AstNode Visit (FieldDeclaration node)
        {
            // Get the field.
            FieldVariable field = (FieldVariable)node.GetVariable();

            // Get the field type.
            IChelaType fieldType = field.GetVariableType();
            if(!fieldType.IsConstant() || field.IsExternal())
                return node; // Ignore not constant fields.

            // Get the initializer.
            Expression initializer = node.GetDefaultValue();
            if(initializer == null)
                Error(node, "constants must have initializers.");
            
            // Don't allow multiple definitions.
            if(constants.ContainsKey(field))
                Error(node, "multiples definitions of a constant.");

            // Store the constant.
            ConstantData data = new ConstantData(field, initializer);
            this.constants.Add(field, data);

            // Return the node.
            return node;
        }

        public override AstNode Visit (VariableReference node)
        {
            return node;
        }

        public override AstNode Visit (MemberAccess node)
        {
            return node;
        }

        public override AstNode Visit (IndirectAccess node)
        {
            return node;
        }

        public Dictionary<FieldVariable, ConstantData> GetConstants()
        {
            return this.constants;
        }
    }
}

