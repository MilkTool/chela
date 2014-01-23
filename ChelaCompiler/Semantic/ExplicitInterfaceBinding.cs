using Chela.Compiler.Module;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Semantic
{
    public class ExplicitInterfaceBinding: ObjectDeclarator
    {
        public ExplicitInterfaceBinding ()
        {
        }

        public override AstNode Visit (FunctionDefinition node)
        {
            // Visit the prototype.
            node.GetPrototype().Accept(this);

            return node;
        }

        private bool MatchFunction(FunctionType left, FunctionType right, int skipArgs)
        {
            // The return type must be equal.
            if(left.GetReturnType() != right.GetReturnType())
                return false;

            // The argument count must be equal.
            if(left.GetArgumentCount() != right.GetArgumentCount())
                return false;

            // The variable flag must be equal.
            if(left.HasVariableArgument() != right.HasVariableArgument())
                return false;

            // The arguments must be equals.
            for(int i = 1; i < left.GetArgumentCount(); ++i)
            {
                if(left.GetArgument(i) != right.GetArgument(i))
                    return false;
            }

            return true;
        }

        public override AstNode Visit (FunctionPrototype node)
        {
            // Get the name expression.
            Expression nameExpression = node.GetNameExpression();
            if(nameExpression == null)
                return node;

            // Visit the name expression.
            nameExpression.SetHints(Expression.MemberHint);
            nameExpression.Accept(this);

            // Get the function and his type.
            Function function = node.GetFunction();
            FunctionType functionType = function.GetFunctionType();
            if(!function.IsMethod())
                Error(node, "expected a method prototype.");
            Method method = (Method)function;

            // It must be a function selector.
            IChelaType nameType = nameExpression.GetNodeType();
            if(!nameType.IsFunctionGroup())
                Error(nameExpression, "expected a function group.");
            FunctionGroupSelector selector = (FunctionGroupSelector)nameExpression.GetNodeValue();
            FunctionGroup group = selector.GetFunctionGroup();

            // Find a matching function in the group.
            Function match = null;
            foreach(FunctionGroupName gname in group.GetFunctions())
            {
                // Ignore static functions.
                if(gname.IsStatic())
                    continue;

                // Check for match.
                FunctionType candidate = gname.GetFunctionType();

                // Found a match?.
                if(MatchFunction(candidate, functionType, 1))
                {
                    match = (Function)gname.GetFunction();
                    break;
                }
            }

            // Raise an error.
            if(match == null)
                Error(nameExpression, "couldn't find matching interface member for {0}", functionType.GetName());

            // TODO: Rename the method.

            // Bind the contract.
            method.SetExplicitContract(match);

            return node;
        }

        public override AstNode Visit (PropertyDefinition node)
        {
            // Get the name expression.
            Expression nameExpression = node.GetNameExpression();
            if(nameExpression == null)
                return node;

            // Visit the name expression.
            nameExpression.SetHints(Expression.MemberHint);
            nameExpression.Accept(this);

            // Try to cast into a property.
            PropertyVariable contractProperty;
            DirectPropertySlot directProp = nameExpression.GetNodeValue() as DirectPropertySlot;
            if(directProp != null)
                contractProperty = directProp.Property;
            else
                contractProperty = nameExpression.GetNodeValue() as PropertyVariable;

            // It must be a property reference.
            if(contractProperty == null)
                Error(nameExpression, "expected property reference.");

            // Compare the property types.
            PropertyVariable property = node.GetProperty();
            if(property.GetVariableType() != contractProperty.GetVariableType())
                Error(nameExpression, "contracted property type mismatch.");

            // Instance the get accessor.
            if(contractProperty.GetAccessor != null)
            {
                if(property.GetAccessor == null)
                    Error(nameExpression, "property implementation doesn't have a get accessor.");

                // Both accessors must be methods.
                if(!contractProperty.GetAccessor.IsMethod() || !property.GetAccessor.IsMethod())
                    Error(nameExpression, "property get accessor is not a static method.");

                // The accessors must match.
                if(!MatchFunction(contractProperty.GetAccessor.GetFunctionType(),
                        property.GetAccessor.GetFunctionType(), 1))
                    Error(nameExpression, "get accessors have mismatching signatures.");

                // Bind the contract.
                Method impl = (Method)property.GetAccessor;
                impl.SetExplicitContract(contractProperty.GetAccessor);
            }
            else if(property.GetAccessor != null)
                Error(nameExpression, "property explicit contract doesn't have a get accessor.");

            // Instance the set accessor.
            if(contractProperty.SetAccessor != null)
            {
                if(property.SetAccessor == null)
                    Error(nameExpression, "property implementation doesn't have a set accessor.");

                // Both accessors must be methods.
                if(!contractProperty.SetAccessor.IsMethod() || !property.SetAccessor.IsMethod())
                    Error(nameExpression, "property set accessor is not a static method.");

                // The accessors must match.
                if(!MatchFunction(contractProperty.SetAccessor.GetFunctionType(),
                        property.SetAccessor.GetFunctionType(), 1))
                    Error(nameExpression, "set accessors have mismatching signatures.");

                // Bind the contract.
                Method impl = (Method)property.SetAccessor;
                impl.SetExplicitContract(contractProperty.SetAccessor);
            }
            else if(property.SetAccessor != null)
                Error(nameExpression, "property explicit contract doesn't have a set accessor.");

            return node;
        }
    }
}

