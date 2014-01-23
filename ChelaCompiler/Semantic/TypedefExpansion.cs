using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class TypedefExpansion: ObjectDeclarator
    {
        private List<TypeNameMember> incompletes;

        public TypedefExpansion ()
        {
        }

        public override void BeginPass ()
        {
            incompletes = new List<TypeNameMember> ();
        }

        private void ExpandTypeNode(TypedefDefinition node, bool sorted)
        {
            // Use the scope in the same place as the definition.
            Scope[] expansionScope = null;
            if(sorted)
            {
                expansionScope = node.GetExpansionScope();
                foreach(Scope scope in expansionScope)
                    PushScope(scope);
            }

            // Allow unsafe typedefs.
            PushUnsafe();

            // Get the type name member.
            TypeNameMember typeName = node.GetTypeName();

            // Parse the type expression.
            Expression typeExpr = node.GetTypeExpression();
            typeExpr.Accept(this);

            // Make sure the type expression is a type.
            IChelaType actualType = typeExpr.GetNodeType();
            actualType = ExtractActualType(typeExpr, actualType);

            // Store the type.
            typeName.SetActualType(actualType);

            // Store the incomplete typename.
            if(actualType.IsIncompleteType())
            {
                // Don't allow cyclic expansion.
                if(sorted)
                    Error(node, "typedef cannot be expanded.");

                // Store the incomplete type name.
                incompletes.Add(typeName);

                // Store the typedef scope stack.
                int numscopes = scopeStack.Count + 1;
                Scope[] scopeData = new Scope[numscopes];
                int i = numscopes - 1;
                scopeData[i--] = currentScope;
                foreach(Scope scope in scopeStack)
                    scopeData[i--] = scope;
                node.SetExpansionScope(scopeData);
            }

            // Restore the safe context.
            PopUnsafe();

            // Restore the scope.
            if(sorted)
            {
                for(int i = 0; i < expansionScope.Length; ++i)
                    PopScope();
                node.SetExpansionScope(null);
            }
        }

        public override AstNode Visit (TypedefDefinition node)
        {
            ExpandTypeNode(node, false);
            return node;
        }

        private void ExpandType(TypeNameMember typeName)
        {
            // Circular references are wrong.
            AstNode typedefNode = typeName.GetTypedefNode();
            if(typeName.IsExpanding)
                Error(typedefNode, "typedef with circular reference.");

            // Ignore expanded types.
            IChelaType type = typeName.GetActualType();
            if(!type.IsIncompleteType())
                return;

            // Set the expanding flag.
            typeName.IsExpanding = true;

            // Expand the dependencies first.
            IncompleteType incomplete = (IncompleteType)type;
            foreach(TypeNameMember dep in incomplete.Dependencies)
                ExpandType(dep);

            // Expand the type name itself.
            ExpandTypeNode((TypedefDefinition)typedefNode, true);

            // Unset the expanding flag.
            typeName.IsExpanding = false;
        }

        public override void EndPass ()
        {
            // Expand recursively undefined types.
            foreach(TypeNameMember typeName in incompletes)
                ExpandType(typeName);
        }
    }
}

