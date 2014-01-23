using System.Collections.Generic;
using Chela.Compiler.Module;
using Chela.Compiler.Ast;

namespace Chela.Compiler.Semantic
{
    public class ObjectDeclarator: AstVisitor
    {
        protected ChelaModule currentModule;
        protected Function currentFunction;
        private static int gensymCount = 0;

        public ObjectDeclarator ()
        {
            currentFunction = null;
            currentModule = null;
        }

        public static string GenSym()
        {
            return "._gsym_" + (gensymCount++) + "_.";
        }
        
        public override AstNode Visit (ModuleNode node)
        {
            currentModule = node.GetModule();
            currentScope = currentModule.GetGlobalNamespace();
            currentContainer = currentScope;
            VisitList(node.GetChildren());

            return node;
        }

        public override AstNode Visit (FileNode node)
        {
            VisitList(node.GetChildren());

            return node;
        }

        public override AstNode Visit (NamespaceDefinition node)
        {
            // Handle nested namespaces.
            LinkedList<Namespace> namespaceChain = new LinkedList<Namespace> ();
            Scope target = currentContainer;
            Namespace current = node.GetNamespace();
            while(current != target)
            {
                namespaceChain.AddFirst(current);
                current = (Namespace)current.GetParentScope();
            }

            // Update the scope.
            foreach(Namespace space in namespaceChain)
                PushScope(space);

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            for(int i = 0; i < namespaceChain.Count; i++)
                PopScope();

            return node;
        }

        public override AstNode Visit (UsingStatement node)
        {
            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (AliasDeclaration node)
        {
            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (StructDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Update the scope.
            PushScope(node.GetScope());
            
            // Visit his children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            // Pop the generic scope.
            if(genScope != null)
                PopScope();

            return node;
        }
        
        public override AstNode Visit (ClassDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Update the scope.
            PushScope(node.GetScope());
            
            // Visit his children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            // Pop the generic scope.
            if(genScope != null)
                PopScope();

            return node;
        }

        public override AstNode Visit (InterfaceDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Pop the generic scope.
            if(genScope != null)
                PopScope();

            return node;
        }

        public override AstNode Visit (EnumDefinition node)
        {
            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (EnumConstantDefinition node)
        {
            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            return node;
        }

        public override AstNode Visit (FieldDefinition node)
        {
            return node;
        }

        public override AstNode Visit (FunctionDefinition node)
        {
            return node;
        }

        public override AstNode Visit (FunctionPrototype node)
        {
            return node;
        }

        public override AstNode Visit (PropertyDefinition node)
        {
            return node;
        }

        public override AstNode Visit (EventDefinition node)
        {
            return node;
        }
        
        public override AstNode Visit (TypedefDefinition node)
        {
            return node;
        }

        public void CheckScopeVisibility(AstNode node, Scope scope)
        {
            // Check the parent scope access.
            Scope parentScope = scope.GetParentScope();
            if(parentScope != null)
                CheckScopeVisibility(node, parentScope);

            // Internal scopes module must be the current one.
            if(scope.IsInternal() && scope.GetModule() != currentModule)
                Error(node, "cannot access internal member " + scope.GetFullName() +" of another module.");
        }

        public void CheckMemberVisibility(AstNode node, ScopeMember member)
        {
            // Ignore placeholder types.
            if(member.IsType())
            {
                IChelaType type = (IChelaType)member;
                if(type.IsPlaceHolderType())
                    return;
            }

            // Special treatment for scope member.
            if(member.IsScope())
            {
                CheckScopeVisibility(node, (Scope)member);
                return;
            }

            // Check the parent scope access.
            Scope parentScope = member.GetParentScope();
            if(parentScope != null)
                CheckScopeVisibility(node, parentScope);

            // Check the actual member visibility.
            if(member.IsPrivate())
            {
                // Walk the scope hierarchy.
                Scope scope = currentScope;
                while(scope != null)
                {
                    // Found the defining scope, stop checking.
                    if(scope == parentScope ||
                       parentScope.IsInstanceOf(scope))
                        return;

                    // Found the scope.
                    scope = scope.GetParentScope();
                }

                // Couldn't find scope, raise error.
                Error(node, "cannot access private member " + member.GetFullName());
            }
            else if(member.IsProtected())
            {
                // The parent scope must be a Structure derivative.
                Structure parentBuilding = (Structure)parentScope;

                // Walk the scope hierarchy.
                Scope scope = currentScope;
                while(scope != null)
                {
                    // Found the defining scope, stop checking.
                    if(scope is Structure)
                    {
                        Structure derived = (Structure)scope;
                        if(derived == parentBuilding ||
                           derived.IsDerivedFrom(parentBuilding))
                            return;
                    }

                    // Found the scope.
                    scope = scope.GetParentScope();
                }

                // Couldn't find scope, raise error.
                Error(node, "cannot access protected member " + member.GetFullName());
            }
            else if(member.IsInternal())
            {
                if(member.GetModule() != currentModule)
                    Error(node, "cannot access internal member " + member.GetFullName() + " of another module.");
            }
        }

        public override AstNode Visit (BaseExpression node)
        {
            // Get the container scope.
            Scope scope = currentScope;
            while(scope != null && (scope.IsPseudoScope() || scope.IsFunction() || scope.IsLexicalScope()))
                scope = scope.GetParentScope();

            // The current container must be an structure.
            Structure building = scope as Structure;
            if(building == null)
                Error(node, "base cannot be used here. " + scope.GetFullName() + " " + scope.GetType().ToString());

            // Get the base structure.
            Structure baseStructure = building.GetBase();
            if(baseStructure == null)
                Error(node, currentContainer.GetFullName() + " doesn't have a base.");

            // Set the node type.
            node.SetNodeType(MetaType.Create(baseStructure));

            return node;
        }

        private bool CheckTypeHint(ScopeMember value, bool typeHint)
        {
            return value != null && (!typeHint || value.IsType() || value.IsNamespace() || value.IsTypeName());
        }

        public override AstNode Visit (VariableReference node)
        {
            Scope scope = currentScope;
            IEnumerator<Scope> iterator = scopeStack.GetEnumerator();
            ScopeMember value = null;
            string attrName = null;
            bool typeHint = node.HasHints(Expression.TypeHint);
            bool isTypeGroup = false;
            bool isFunctionGroup = false;
            bool isNamespaceLevel = false;
            int mergedLevels = 0;
            while(scope != null)
            {
                // Find the variable in the scope.
                ScopeMember newValue = scope.FindMemberRecursive(node.GetName());
                bool validValue = CheckTypeHint(newValue, typeHint);

                // Check the attribute name.
                if(newValue == null && node.IsAttributeName())
                {
                    if(attrName == null)
                        attrName = node.GetName() + "Attribute";
                    newValue = scope.FindMemberRecursive(attrName);
                    validValue = CheckTypeHint(newValue, typeHint);
                }

                // Use the value.
                if(validValue)
                {
                    // If the first value its a type group, begin merging.
                    if(value == null)
                    {
                        value = newValue;
                        isTypeGroup = value.IsTypeGroup();
                        isFunctionGroup = value.IsFunctionGroup();

                        // If not merging, found the member.
                        if(!isTypeGroup && !isFunctionGroup)
                            break;
                    }

                    // Check if its a namespace level
                    isNamespaceLevel = isNamespaceLevel || scope.IsPseudoScope();

                    // Found the next level.
                    if(isTypeGroup && newValue.IsTypeGroup())
                    {
                        TypeGroup typeGroup = (TypeGroup)value;
                        TypeGroup newGroup = (TypeGroup)newValue;

                        // Create the merged type group.
                        if(mergedLevels == 0)
                            value = typeGroup.CreateMerged(newGroup, isNamespaceLevel);
                        else
                            typeGroup.AppendLevel(newGroup, isNamespaceLevel);
                    }
                    else if(isFunctionGroup && newValue.IsFunctionGroup())
                    {
                        FunctionGroup functionGroup = (FunctionGroup)value;
                        FunctionGroup newGroup = (FunctionGroup)newValue;

                        // Create the merged function group.
                        if(mergedLevels == 0)
                            value = functionGroup.CreateMerged(newGroup, isNamespaceLevel);
                        else
                            functionGroup.AppendLevel(newGroup, isNamespaceLevel);
                    }

                    // Increase the level count.
                    ++mergedLevels;
                }

                // Increase the scope.
                if(!iterator.MoveNext())
                    break;
                
                scope = iterator.Current;
            }
            
            // If the value couldn't be found.
            if(value == null)
                Error(node, "undeclared {0} '{1}'.", typeHint ? "type" : "variable",
                        node.GetName());

            // Check the accesiblity.
            CheckMemberVisibility(node, value);

            // Calculate the type.
            IChelaType type = null;
            if(value.IsType())
            {
                type = MetaType.Create((IChelaType)value);
            }
            else if(value.IsNamespace())
            {
                type = ChelaType.GetNamespaceType();
            }
            else if(value.IsTypeName())
            {
                // Read the type name.
                TypeNameMember typeName = (TypeNameMember)value;
                type = typeName.GetActualType();

                // Create an incomplete type if necessary.
                if(type == null)
                    type = new IncompleteType(typeName);

                type = MetaType.Create(type);
            }
            else if(value.IsVariable())
            {
                Variable variable = (Variable)value;

                // Check for aliases(ref/out parameters).
                IChelaType varType = variable.GetVariableType();
                bool isAliasVariable = false;
                if(varType.IsReference())
                {
                    // De-reference.
                    ReferenceType refType = (ReferenceType)varType;
                    IChelaType referenced = refType.GetReferencedType();
                    if(!referenced.IsPassedByReference() || referenced.IsReference())
                    {
                        // This variable is reference.
                        isAliasVariable = true;
                        type = varType;
                        node.SetAliasVariable(variable);
                        value = new ReferencedSlot(referenced);
                    }
                }

                if(!isAliasVariable)
                    type = ReferenceType.Create(variable.GetVariableType());
                
                // Special checks for field variables
                if(variable.IsField())
                {
                    FieldVariable field = (FieldVariable) variable;
                    if(!field.IsStatic() && currentFunction.IsStatic())
                        Error(node, "cannot access no static fields without an object.");
                }
            }
            else if(value.IsFunction())
            {
                Function function = (Function)value;
                type = function.GetFunctionType();
                
                // Special checks for methods.
                if(function.IsMethod())
                {
                    Method method = (Method) function;
                    if(!method.IsStatic() && currentFunction.IsStatic())
                        Error(node, "cannot access no static methods without an object.");
                }
            }
            else if(value.IsFunctionGroup())
            {
                FunctionGroup fgroup = (FunctionGroup)value;
                value = new FunctionGroupSelector(fgroup);

                type = ChelaType.GetStaticFunctionGroupType();

                // Check for implicit this.
                if(!currentFunction.IsStatic())
                {
                    Structure groupScope = fgroup.GetParentScope() as Structure;
                    Structure myScope = currentFunction.GetParentScope() as Structure;
                    if(groupScope != null && myScope != null &&
                        (groupScope == myScope || myScope.IsDerivedFrom(groupScope)))
                        type = ChelaType.GetAnyFunctionGroupType();
                }
            }
            else
                Error(node, "unexpected member type.");
            
            // Set the value.
            node.SetNodeValue(value);
            
            // Set the node type.
            node.SetNodeType(type);

            return node;
        }
        
        public override AstNode Visit (MemberAccess node)
        {
            // Evaluate the base reference.
            Expression baseReference = node.GetReference();
            baseReference.SetHints(node.Hints);
            baseReference.Accept(this);
            
            // Get the base reference type.
            IChelaType baseRefType = baseReference.GetNodeType();
            if(baseRefType.IsConstant())
                baseRefType = DeConstType(baseRefType);
            
            // Set a default coercion type.
            node.SetCoercionType(baseRefType);
            
            // Get the scope.
            Scope scope = null;
            bool canStatic = true;
            bool canNoStatic = node.HasHints(Expression.MemberHint);
            bool suppressVirtual = false;
            bool implicitThis = false;
            if(baseRefType.IsMetaType())
            {
                // Read the base reference type.
                baseRefType = ExtractActualType(node, baseRefType);

                // Handle incomplete types.
                if(baseRefType.IsIncompleteType())
                {
                    node.SetNodeType(baseRefType);
                    return node;
                }

                // Handle built-in types
                if(!baseRefType.IsStructure() && !baseRefType.IsClass() && !baseRefType.IsInterface())
                {
                    baseRefType = currentModule.GetAssociatedClass(baseRefType);
                    if(baseRefType == null)
                        Error(node, "expected a known type.");
                }

                scope = (Scope) baseRefType;
                suppressVirtual = true;

                // Get the current container scope.
                Scope contScope = currentScope;
                while(contScope != null && (contScope.IsPseudoScope() || contScope.IsFunction() || contScope.IsLexicalScope()))
                    contScope = contScope.GetParentScope();

                // Check if the base type is a base of me.
                Structure building = contScope as Structure;
                if(building != null)
                {
                    Structure scopeBuilding = (Structure) baseRefType;
                    if(building.IsDerivedFrom(scopeBuilding))
                    {
                        canNoStatic = true;
                        implicitThis = true;
                    }
                }
            }
            else if(baseRefType.IsNamespace())
            {
                scope = (Namespace) baseReference.GetNodeValue();
            }
            else if(baseRefType.IsReference())
            {
                canStatic = false;
                canNoStatic = true;
                
                // Get the variable referenced type.
                ReferenceType refType = (ReferenceType)baseRefType;
                IChelaType objectType = refType.GetReferencedType();
                
                if(objectType.IsReference())
                {
                    // Set the variable type as the coercion type.
                    node.SetCoercionType(objectType);
                    
                    // Dereference.
                    refType = (ReferenceType)objectType;
                    objectType = refType.GetReferencedType();
                }
                else if(objectType.IsStructure())
                {
                    // Use the structure type as the coercion type.
                    node.SetCoercionType(objectType);
                }
                else if(objectType.IsPrimitive() || objectType.IsVector() ||
                        objectType.IsPlaceHolderType())
                {
                    node.SetCoercionType(objectType);
                }

                if(objectType.IsStructure() || objectType.IsClass() || objectType.IsInterface() ||
                    objectType.IsTypeInstance())
                    scope = (Scope)objectType;
                else if(objectType.IsPlaceHolderType())
                    scope = new PseudoScope((PlaceHolderType)objectType);
                else
                {
                    scope = currentModule.GetAssociatedClass(objectType);
                    if(scope == null)
                        Error(node, "unimplemented primitive type classes.");
                }
            }
            else if(baseRefType.IsStructure())
            {
                canStatic = false;
                canNoStatic = true;
                scope = (Scope)baseRefType;
            }
            else if(baseRefType.IsPrimitive())
            {
                canStatic = false;
                canNoStatic = true;
                scope = currentModule.GetAssociatedClass(baseRefType);
                if(scope == null)
                    Error(node, "unimplemented primitive type classes.");
            }
            else if(baseRefType.IsVector())
            {
                canStatic = false;
                canNoStatic = true;
                scope = currentModule.GetAssociatedClass(baseRefType);
                if(scope == null)
                    Error(node, "unimplemented vector type classes.");
            }
            else if(baseRefType.IsPlaceHolderType())
            {
                canStatic = false;
                canNoStatic = true;
                scope = new PseudoScope((PlaceHolderType)baseRefType);
            }
            else
            {
                Error(node, "expected reference, namespace, struct, class, primitive, vector.");
            }

            // Check vectorial swizzle.
            IChelaType coercionType = node.GetCoercionType();
            if(coercionType.IsVector())
            {
                VectorType vectorType = (VectorType)coercionType;
                int minSize = 0;
                int mask = 0;
                string fieldName = node.GetName();
                if(fieldName.Length <= 4)
                {
                    // It can be a vectorial swizzle
                    bool isSwizzle = true;
                    for(int i = 0; i < fieldName.Length; ++i)
                    {
                        switch(fieldName[i])
                        {
                        case 'x':
                        case 'r':
                        case 's':
                            mask |= 0<<(2*i);
                            if(minSize < 1)
                                minSize = 1;
                            break;
                        case 'y':
                        case 'g':
                        case 't':
                            mask |= 1<<(2*i);
                            if(minSize < 2)
                                minSize = 2;
                            break;
                        case 'z':
                        case 'b':
                        case 'p':
                            mask |= 2<<(2*i);
                            if(minSize < 3)
                                minSize = 3;
                            break;
                        case 'w':
                        case 'a':
                        case 'q':
                            mask |= 3<<(2*i);
                            if(minSize < 4)
                                minSize = 4;
                            break;
                        default:
                            i = fieldName.Length + 1;
                            isSwizzle = false;
                            break;
                        }
                    }

                    // Swizzle found.
                    if(isSwizzle)
                    {
                        // Check the validity.
                        if(minSize > vectorType.GetNumComponents())
                            Error(node, "accessing inexistent elements in the vector.");

                        // Store the base variable.
                        Variable baseVar = baseReference.GetNodeValue() as Variable;

                        // Return the swizzle.
                        IChelaType swizzleType = VectorType.Create(vectorType.GetPrimitiveType(), fieldName.Length);
                        SwizzleVariable swizzleVar =
                            new SwizzleVariable(swizzleType, baseVar, (byte)mask, fieldName.Length);
                        node.SetNodeValue(swizzleVar);
                        node.SetNodeType(ReferenceType.Create(swizzleType));
                        return node;
                    }
                }
            }
            
            // Find the scope member.
            ScopeMember member = scope.FindMemberRecursive(node.GetName());
            if(member == null)
            {
                // Check attribute name.
                if(node.IsAttributeName())
                    member = scope.FindMemberRecursive(node.GetName() + "Attribute");

                if(member == null)
                    Error(node, "couldn't find member '" + node.GetName()
                                + "' in '" + scope.GetFullName() + "'.");
            }

            // Check the accesiblity.
            CheckMemberVisibility(node, member);

            // Return it.
            IChelaType type = null;
            if(member.IsNamespace())
            {
                type = ChelaType.GetNamespaceType();
            }
            else if(member.IsType())
            {
                type = MetaType.Create((IChelaType)member);
            }
            else if(member.IsTypeName())
            {
                // Read the type name.
                TypeNameMember typeName = (TypeNameMember)member;
                type = typeName.GetActualType();

                // Create an incomplete type if necessary.
                if(type == null)
                    type = new IncompleteType(typeName);

                type = MetaType.Create(type);
            }
            else if(member.IsFunctionGroup())
            {
                FunctionGroup fgroup = (FunctionGroup)member;
                FunctionGroupSelector selector = new FunctionGroupSelector(fgroup);
                selector.SuppressVirtual = suppressVirtual;
                selector.ImplicitThis = implicitThis;
                member = selector;
                if(canNoStatic && canStatic)
                    type = ChelaType.GetAnyFunctionGroupType();
                else if(canStatic)
                    type = ChelaType.GetStaticFunctionGroupType();
                else
                    type = ChelaType.GetFunctionGroupType();
            }
            else if(member.IsFunction())
            {
                Function function = (Function)member;
                type = function.GetFunctionType();

                // Check the static flag.
                if(!canNoStatic &&
                   (function.GetFlags() & MemberFlags.InstanceMask) != MemberFlags.Static)
                {
                    Error(node, "expected a static function.");
                }
                else if(function.IsMethod())
                {
                    Method method = (Method)function;
                    node.SetSlot(method.GetVSlot());
                }
            }
            else
            {
                Variable variable = (Variable)member;
                IChelaType variableType = variable.GetVariableType();
                if(variableType.IsPassedByReference())
                    variableType = ReferenceType.Create(variableType); // Required for nested access.
                type = ReferenceType.Create(variableType);
                
                // Check the static flag.
                if(!canNoStatic && !variable.IsStatic())
                {
                    Error(node, "expected a static variable.");
                }
                else if(variable.IsField())
                {
                    // Use the slot.
                    FieldVariable field = (FieldVariable) variable;
                    node.SetSlot(field.GetSlot());
                }
                else if(variable.IsProperty() && !variable.IsStatic() && suppressVirtual)
                {
                    // Wrap the property in a direct property slot.
                    member = new DirectPropertySlot((PropertyVariable)variable);
                }

                // Store the implicit this property.
                if(!variable.IsStatic())
                    node.ImplicitSelf = implicitThis;
            }
            
            // Set the value.
            node.SetNodeValue(member);
            
            // Set the type.
            node.SetNodeType(type);

            return node;
        }
        
        public override AstNode Visit (MakePointer node)
        {
            // Don't create unsafe types under safe contexts.
            UnsafeError(node, "cannot use pointers under safe contexts.");

            // Get the pointed expression.
            Expression pointed = node.GetPointedType();
            pointed.SetHints(Expression.TypeHint);
            
            // Vist the pointed type.
            pointed.Accept(this);
            
            // Get the pointer type.
            IChelaType pointedType = pointed.GetNodeType();
            if(!pointedType.IsMetaType())
                Error(node, "expected type expression.");
            
            // Get the actual pointed type.
            pointedType = ExtractActualType(node, pointedType);

            // Handle incomplete types.
            if(pointedType.IsIncompleteType())
            {
                node.SetNodeType(pointed.GetNodeType());
                return node;
            }

            // Create the pointer meta type.
            IChelaType pointerType = MetaType.Create(PointerType.Create(pointedType));
            
            // Set the node type.
            node.SetNodeType(pointerType);

            return node;
        }

        public override AstNode Visit (MakeReference node)
        {
            // Get the referenced expression.
            Expression referenced = node.GetReferencedType();
            referenced.SetHints(Expression.TypeHint);

            // Visit the referenced type.
            referenced.Accept(this);

            // Get the pointer type.
            IChelaType referencedType = referenced.GetNodeType();
            if(!referencedType.IsMetaType())
                Error(node, "expected type expression.");

            // Get the actual pointed type.
            referencedType = ExtractActualType(node, referencedType);

            // Handle incomplete types.
            if(referencedType.IsIncompleteType())
            {
                node.SetNodeType(referenced.GetNodeType());
                return node;
            }

            // Create the reference meta type.
            IChelaType referenceType = MetaType.Create(ReferenceType.Create(referencedType, node.GetFlow(), node.IsStreamReference()));

            // Set the node type.
            node.SetNodeType(referenceType);

            return node;
        }

        public override AstNode Visit (MakeConstant node)
        {
            // Get the value expression.
            Expression value = node.GetValueType();
            value.SetHints(Expression.TypeHint);

            // Vist the pointed type.
            value.Accept(this);

            // Get the value type.
            IChelaType valueType = value.GetNodeType();
            if(!valueType.IsMetaType())
                Error(node, "expected type expression.");

            // Get the actual value type.
            valueType = ExtractActualType(node, valueType);

            // Handle incomplete types.
            if(valueType.IsIncompleteType())
            {
                node.SetNodeType(value.GetNodeType());
                return node;
            }

            // Don't support const references.
            if(valueType.IsReference())
                Error(node, "reference types cannot be constant.");

            // Create the constant meta type.
            IChelaType constantType = MetaType.Create(ConstantType.Create(valueType));

            // Set the node type.
            node.SetNodeType(constantType);

            return node;
        }

        public override AstNode Visit (MakeArray node)
        {
            // Get the value expression.
            Expression value = node.GetValueType();
            value.SetHints(Expression.TypeHint);

            // Visit the pointed type.
            value.Accept(this);

            // Get the value type.
            IChelaType valueType = value.GetNodeType();
            if(!valueType.IsMetaType())
                Error(node, "expected type expression.");

            // Get the actual value type.
            valueType = ExtractActualType(node, valueType);

            // Handle incomplete types.
            if(valueType.IsIncompleteType())
            {
                node.SetNodeType(value.GetNodeType());
                return node;
            }

            // Don't support const array.
            if(valueType.IsConstant())
                Error(node, "cannot create managed array of constants.");
            if(valueType.IsReference())
                valueType = DeReferenceType(valueType);

            // Create the array meta type.
            IChelaType arrayType = ArrayType.Create(valueType, node.GetDimensions(), node.IsReadOnly);
            arrayType = ReferenceType.Create(arrayType);

            // Set the node type.
            node.SetNodeType(MetaType.Create(arrayType));

            return node;
        }

        public override AstNode Visit (MakeFunctionPointer node)
        {
            UnsafeError(node, "cannot use function pointers under safe contexts.");

            // Get the return type expression.
            Expression returnTypeExpr = node.GetReturnType();
            returnTypeExpr.Accept(this);

            // Get the return type.
            IChelaType returnType = returnTypeExpr.GetNodeType();
            if(!returnType.IsMetaType())
                Error(node, "expected a return type.");

            // Store the incomplete types.
            List<object> incompletes = new List<object> ();

            // Get the actual return type.
            returnType = ExtractActualType(node, returnType);
            if(returnType.IsIncompleteType())
                incompletes.Add(returnType);

            // Process the arguments.
            List<IChelaType> argTypes = new List<IChelaType> ();
            AstNode arg = node.GetArguments();
            while(arg != null)
            {
                // Visit the argument.
                arg.Accept(this);

                // Get the argument type.
                IChelaType argType = arg.GetNodeType();
                if(!argType.IsMetaType())
                    Error(arg, "expected an argument type instead of a value of type {0}.",  argType.GetDisplayName());

                // Extract the actual argument type.
                argType = ExtractActualType(arg, argType);
                if(argType.IsPassedByReference())
                    argType = ReferenceType.Create(argType);

                // Arg type cannot be void.
                if(argType == ChelaType.GetVoidType())
                    Error(arg, "cannot have parameters of void type.");

                // Store the argument type.
                argTypes.Add(argType);

                // Store the incomplete argument.
                if(argType.IsIncompleteType())
                    incompletes.Add(argType);

                // Process the next argument.
                arg = arg.GetNext();
            }

            // Use an incomplete type if one of the argument is incomplete.
            if(incompletes.Count > 0)
            {
                node.SetNodeType(MetaType.Create(new IncompleteType(incompletes)));
            }
            else
            {
                // Create the function type.
                FunctionType functionType = FunctionType.Create(returnType, argTypes, false, node.GetFlags());

                // Create pointer type.
                PointerType pointerType = PointerType.Create(functionType);

                // Set the node type.
                node.SetNodeType(MetaType.Create(pointerType));
            }

            return node;
        }
        
        public override AstNode Visit (TypeNode node)
        {
            // Evaluate the type.
            IChelaType nodeType = null;
            switch(node.GetKind())
            {
            case TypeKind.Void:
                nodeType = ChelaType.GetVoidType();
                break;
            case TypeKind.Bool:
                nodeType = ChelaType.GetBoolType();
                break;
            case TypeKind.Byte:
                nodeType = ChelaType.GetByteType();
                break;
            case TypeKind.SByte:
                nodeType = ChelaType.GetSByteType();
                break;
            case TypeKind.Char:
                nodeType = ChelaType.GetCharType();
                break;
            case TypeKind.Double:
                nodeType = ChelaType.GetDoubleType();
                break;
            case TypeKind.Float:
                nodeType = ChelaType.GetFloatType();
                break;
            case TypeKind.Int:
                nodeType = ChelaType.GetIntType();
                break;
            case TypeKind.UInt:
                nodeType = ChelaType.GetUIntType();
                break;
            case TypeKind.Long:
                nodeType = ChelaType.GetLongType();
                break;
            case TypeKind.ULong:
                nodeType = ChelaType.GetULongType();
                break;
            case TypeKind.Short:
                nodeType = ChelaType.GetShortType();
                break;
            case TypeKind.UShort:
                nodeType = ChelaType.GetUShortType();
                break;
            case TypeKind.String:
                nodeType = ChelaType.GetStringType();
                break;
            case TypeKind.Object:
                nodeType = ChelaType.GetObjectType();
                break;
            case TypeKind.Size:
                nodeType = ChelaType.GetSizeType();
                break;
            case TypeKind.PtrDiff:
                nodeType = ChelaType.GetPtrDiffType();
                break;
            case TypeKind.Other:
                nodeType = node.GetOtherType();
                break;
            case TypeKind.Reference:
                nodeType = ReferenceType.Create(currentModule.TypeMap(node.GetOtherType()));
                break;
            default:
                throw new System.NotSupportedException();
            }

            // Vectorial type check.
            if(node.GetNumComponents() > 1)
            {
                if(!nodeType.IsPrimitive())
                    Error(node, "invalid vectorial type.");
                nodeType = VectorType.Create(nodeType, node.GetNumComponents());
            }
            else if(node.GetNumRows() > 1 || node.GetNumColumns() > 1)
            {
                if(!nodeType.IsPrimitive())
                    Error(node, "invalid matrix type.");
                nodeType = MatrixType.Create(nodeType, node.GetNumRows(), node.GetNumColumns());
            }
            
            // Set the node type.
            node.SetNodeType(MetaType.Create(currentModule.TypeMap(nodeType)));

            return node;
        }

        public override AstNode Visit (GenericInstanceExpr node)
        {
            // Visit the generic expression.
            Expression genericExpr = node.GetGenericExpression();
            genericExpr.Accept(this);

            // Visit the parameters.
            List<IChelaType> parameters = new List<IChelaType> ();
            AstNode currentParam = node.GetParameters();
            while(currentParam != null)
            {
                // Visit the parameter.
                currentParam.Accept(this);

                // Get the parameter type.
                IChelaType paramType = currentParam.GetNodeType();
                paramType = ExtractActualType(currentParam, paramType);
                parameters.Add(FilterGenericParameter(paramType));

                // Visit the next parameter.
                currentParam = currentParam.GetNext();
            }

            // Create the parameter array.
            IChelaType[] typeArgs = parameters.ToArray();

            // Get the generic member.
            IChelaType genericExprType = genericExpr.GetNodeType();
            if(genericExprType.IsMetaType())
            {
                // Extract the actual type.
                MetaType metaType = (MetaType)genericExprType;
                genericExprType = metaType.GetActualType();

                // Handle type groups.
                Structure building = null;
                if(genericExprType.IsTypeGroup())
                {
                    // Check each member of the group, until find a match.
                    TypeGroup group = (TypeGroup)genericExprType;

                    foreach(TypeGroupName gname in group.GetBuildings())
                    {
                        // Use the first match
                        // TODO: Use the best match.
                        if(CheckGenericArguments(null, gname.GetGenericPrototype(), typeArgs))
                        {
                            building = gname.GetBuilding();
                            break;
                        }
                    }

                    // Make sure a match was found.
                    if(building == null)
                    {
                        if(@group.GetBuildingCount() == 1)
                        {
                            foreach(TypeGroupName gname in group.GetBuildings())
                                CheckGenericArguments(node, gname.GetGenericPrototype(), typeArgs);
                        }
                        else
                        {
                            // TODO: Give a more descriptive error.
                            Error(node, "couldn't find a matching generic type.");
                        }
                    }

                    // Prevent ambiguity.
                    building.CheckAmbiguity(node.GetPosition());
                }
                else
                {
                    // This must be a class/struct/interface.
                    if(!genericExprType.IsClass() && !genericExprType.IsStructure() &&
                       !genericExprType.IsInterface())
                        Error(node, "only class/structure/interface/delegate types can be generic.");

                    building = (Structure)genericExprType;
                    building.CheckAmbiguity(node.GetPosition());
                }

                // Check the generic member.
                GenericPrototype genProto = building.GetGenericPrototype();
                CheckGenericArguments(node, genProto, typeArgs);

                // Use a generic structure instance type.
                GenericInstance args = new GenericInstance(genProto, typeArgs);
                IChelaType instancedType = building.InstanceGeneric(args, currentModule);
                node.SetNodeType(MetaType.Create(instancedType));

            }
            else if(genericExprType.IsFunctionGroup())
            {
                // This is a function group, so delay generic member selection.
                FunctionGroupSelector selector = (FunctionGroupSelector)genericExpr.GetNodeValue();
                selector.GenericParameters = typeArgs;
                node.SetNodeType(genericExpr.GetNodeType());
                node.SetNodeValue(selector);
            }
            else
            {
                // Unexpected member.
                Error(node, "expected class/struct/interface/delegate/function group.");
            }

            return node;
        }

        protected IChelaType FilterGenericParameter(IChelaType genericParameter)
        {
            // Remove reference.
            genericParameter = DeReferenceType(genericParameter);

            // Remove constant.
            genericParameter = DeConstType(genericParameter);

            // Remove reference, again.
            genericParameter = DeReferenceType(genericParameter);

            // Remove constant, again.
            genericParameter = DeConstType(genericParameter);

            // Return the filtered parameter.
            return genericParameter;
        }

        protected bool CheckGenericPlaceArgument(AstNode where, PlaceHolderType prototype, PlaceHolderType argument)
        {
            // Check for value type.
            if(prototype.IsValueType() && !argument.IsValueType())
                TryError(where, "expected value type argument.");

            // Get the argument top base.
            Structure topBase = currentModule.GetObjectClass();
            if(argument.IsValueType())
                topBase = currentModule.GetValueTypeClass();

            // Check for the bases.
            for(int i = 0; i < prototype.GetBaseCount(); ++i)
            {
                // Get the prototype base.
                Structure protoBase = prototype.GetBase(i);

                // Check for the top base.
                if(protoBase.IsClass() && (protoBase == topBase ||
                    topBase.IsDerivedFrom(protoBase)))
                    continue;
                else if(protoBase.IsInterface() && topBase.Implements(protoBase))
                    continue;

                // Check the argument bases until hit.
                bool found = false;
                for(int j = 0; j < argument.GetBaseCount(); ++j)
                {
                    // Get the argument base.
                    Structure argBase = argument.GetBase(j);

                    // Check the constraint.
                    if(protoBase == argBase ||
                       (protoBase.IsClass() && argBase.IsDerivedFrom(protoBase)) ||
                       (protoBase.IsInterface() && argBase.Implements(protoBase)))
                    {
                        found = true;
                        break;
                    }
                }

                // Raise the error.
                if(!found)
                    return TryError(where, "argument type {0} doesn't satisfy constraint {1}",
                        argument.GetFullName(), protoBase.GetFullName());
            }

            return true;
        }

        protected bool CheckGenericArguments(AstNode where, GenericPrototype prototype, IChelaType[] arguments)
        {
            // The argument count must match.
            if(prototype.GetPlaceHolderCount() != arguments.Length)
                return TryError(where, "not matching generic argument count.");

            // Check each argument.
            for(int i = 0; i < arguments.Length; ++i)
            {
                // Get the placeholder and the type.
                PlaceHolderType placeHolder = prototype.GetPlaceHolder(i);
                IChelaType argument = arguments[i];

                // Check for value types.
                if(placeHolder.IsValueType() && !argument.IsFirstClass() && !argument.IsStructure() && !argument.IsPlaceHolderType())
                    return TryError(where, "the generic argument number {0} must be a value type.", i+1);

                // Get the argument structure.
                Structure argumentBuilding = null;
                if(argument.IsClass() || argument.IsStructure() || argument.IsInterface())
                    argumentBuilding = (Structure)argument;
                else if(argument.IsFirstClass())
                    argumentBuilding = currentModule.GetAssociatedClass(argument);
                else if(argument.IsPointer())
                {
                    // TODO: Support pointers.
                    argumentBuilding = currentModule.GetAssociatedClass(ChelaType.GetSizeType());
                }
                else if(argument.IsPlaceHolderType())
                {
                    // Check the place holder.
                    if(CheckGenericPlaceArgument(where, placeHolder, (PlaceHolderType)argument))
                        continue;
                    else
                        return false;
                }
                else
                    return TryError(where, "cannot get class for type {0}", argument.GetDisplayName());

                // Check for constraints.
                for(int j = 0; j < placeHolder.GetBaseCount(); ++j)
                {
                    // Get the base building.
                    Structure baseBuilding = placeHolder.GetBase(j);

                    // If the argument is the base pass.
                    if(argumentBuilding == baseBuilding)
                        continue;

                    // If the base is a class, check for inheritance.
                    if(baseBuilding.IsClass() && argumentBuilding.IsDerivedFrom(baseBuilding))
                        continue;

                    // If the base is a interface, check for implementation.
                    if(baseBuilding.IsInterface() && argumentBuilding.Implements(baseBuilding))
                        continue;

                    // The constraint couldn't be satisfied.
                    return TryError(where, "generic argument {0} of type {1} doesn't support constraint {2}",
                                    i+1, argumentBuilding.GetDisplayName(), baseBuilding.GetDisplayName());
                }
            }

            return true;
        }

        public override AstNode Visit (GenericParameter node)
        {
            // Create the place holder type.
            PlaceHolderType holder = new PlaceHolderType(currentModule, node.GetName(), node.IsValueType(), node.GetBases());
            node.SetNodeType(holder);
            return node;
        }

        public override AstNode Visit (GenericConstraint node)
        {
            // Find the parameter.
            GenericSignature signature = node.GetSignature();
            AstNode current = signature.GetParameters();
            GenericParameter parameter = null;
            while(current != null)
            {
                if(current.GetName() == node.GetName())
                {
                    // Found.
                    parameter = (GenericParameter) current;
                    break;
                }

                // Check the next parameter.
                current = current.GetNext();
            }

            // Raise an error for unknown parameter.
            if(parameter == null)
                Error(node, "unknown generic parameter '{0}'", node.GetName());

            // Fetch the bases.
            List<Structure> bases = new List<Structure> ();
            current = node.GetBases();
            bool hasClass = false;
            while(current != null)
            {
                // Visit the base expression.
                current.Accept(this);

                // Get the base type.
                IChelaType nodeType = current.GetNodeType();
                if(!nodeType.IsMetaType())
                    Error(current, "expected class/interface type.");

                // Remove the meta layer.
                nodeType = ExtractActualType(current, nodeType);

                // Make sure is a class or a structure.
                if(!nodeType.IsClass() && !nodeType.IsStructure() && !nodeType.IsInterface())
                    Error(current, "expected class/struct/interface type.");

                // Enforce single inheritance.
                if(nodeType.IsClass())
                {
                    if(hasClass)
                        Error(current, "only single inheritance is supported.");
                    hasClass = true;
                }

                // Store the base.
                bases.Add((Structure)nodeType);

                // Check the next base.
                current = current.GetNext();
            }

            // Store the constraint data in the parameter.
            parameter.SetValueType(node.IsValueType());
            parameter.SetBases(bases);

            return node;
        }

        public override AstNode Visit (GenericSignature node)
        {
            // Visit the constraints first.
            AstNode child = node.GetConstraints();
            while(child != null)
            {
                GenericConstraint constraint = (GenericConstraint)child;
                constraint.SetSignature(node);
                constraint.Accept(this);

                // Visit the next constraint.
                child = child.GetNext();
            }

            // Visit the generic parameters.
            child = node.GetParameters();
            List<PlaceHolderType> placeholders = new List<PlaceHolderType> ();
            while(child !=  null)
            {
                // Visit the child.
                child.Accept(this);

                // Store the child type.
                placeholders.Add((PlaceHolderType)child.GetNodeType());

                // Visit the next parameter.
                child = child.GetNext();
            }

            // Create the generic prototype.
            GenericPrototype prototype = new GenericPrototype(placeholders);
            node.SetPrototype(prototype);

            return node;
        }
        
        public override AstNode Visit (CStringConstant node)
        {
            node.SetNodeType(PointerType.Create(ConstantType.Create(ChelaType.GetSByteType())));
            return node;
        }

        public override AstNode Visit (StringConstant node)
        {
            if(node.GetNodeType() == null)
            {
                IChelaType stringRefType = ReferenceType.Create(currentModule.TypeMap(ChelaType.GetStringType()));
                node.SetNodeType(ConstantType.Create(stringRefType));
            }

            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (CharacterConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (BoolConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (ByteConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (SByteConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (ShortConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (UShortConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (IntegerConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (UIntegerConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (LongConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (ULongConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (FloatConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (DoubleConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(new ConstantValue(node.GetValue()));
            return node;
        }

        public override AstNode Visit (NullConstant node)
        {
            if(node.GetNodeValue() == null)
                node.SetNodeValue(ConstantValue.CreateNull());
            return node;
        }

        public override AstNode Visit (NullStatement node)
        {
            return node;
        }

        public static IChelaType DeReferenceType(IChelaType type)
        {
            if(!type.IsReference())
                return type;
            
            ReferenceType refType = (ReferenceType)type;
            return refType.GetReferencedType();
        }
        
        public static IChelaType DePointerType(IChelaType type)
        {
            if(!type.IsPointer())
                return type;

            PointerType pointerType = (PointerType)type;
            return pointerType.GetPointedType();
        }

        public static IChelaType DeConstType(IChelaType type)
        {
            if(type == null || !type.IsConstant())
                return type;

            ConstantType constantType = (ConstantType)type;
            return constantType.GetValueType();
        }

        public static IChelaType ExtractMetaType(IChelaType type)
        {
            MetaType metaType = (MetaType) type;
            return metaType.GetActualType();
        }

        public Structure ExtractClass(AstNode where, ScopeMember member)
        {
            // Make sure its a structure or type group.
            if(!member.IsClass() && !member.IsStructure() &&!member.IsInternal() &&
                !member.IsTypeGroup())
                Error(where, "coudn't load runtime class.");

            // Read the type group.
            if(member.IsTypeGroup())
            {
                TypeGroup group = (TypeGroup)member;
                Structure building = group.GetDefaultType();
                if(building == null)
                    Error(where, "unexpected type group {0}", @group.GetDisplayName());

                // Prevent ambiguity of merged type group.
                building.CheckAmbiguity(where.GetPosition());
                return building;
            }

            return (Structure)member;
        }

        public IChelaType ExtractActualType(AstNode where, IChelaType type)
        {
            // Extract from the meta type.
            if(!type.IsMetaType())
                Error(where, "expected a type.");
            type = ExtractMetaType(type);

            // Use the default element from the type group.
            if(type.IsTypeGroup())
            {
                TypeGroup group = (TypeGroup)type;
                Structure building = group.GetDefaultType();
                type = building;
                if(building == null)
                    Error(where, "unexpected type group {0}", @group.GetDisplayName());

                // Prevent ambiguity of merged type group.
                building.CheckAmbiguity(where.GetPosition());
            }
            return type;
        }

        protected LexicalScope CreateLexicalScope(AstNode where, Function parentFunction)
        {
            LexicalScope ret = new LexicalScope(currentContainer, parentFunction);
            ret.Position = where.GetPosition();
            return ret;
        }

        protected LexicalScope CreateLexicalScope(AstNode where)
        {
            return CreateLexicalScope(where, currentFunction);
        }
    }
}
