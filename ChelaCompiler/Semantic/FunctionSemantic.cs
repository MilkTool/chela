using System;
using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class FunctionSemantic: ObjectDeclarator
    {
        protected ExceptionContext currentExceptionContext;
        protected SwitchStatement currentSwitch;

        public FunctionSemantic ()
        {
            currentExceptionContext = null;
            currentSwitch = null;
        }
        
        public override AstNode Visit (StructDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

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
            // Process attributes.
            ProcessAttributes(node);

            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Check default constructor initialization.
            Method defaultCtor = node.GetDefaultConstructor();
            if(defaultCtor != null)
                CheckDefaultConstructorInit(node, defaultCtor);

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
            // Process attributes.
            ProcessAttributes(node);

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

        public override AstNode Visit (FieldDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Process the field declarations.
            VisitList(node.GetDeclarations());
            return node;
        }

        public override AstNode Visit (EnumDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

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
            // Get the value expression
            Expression valueExpression = node.GetValue();
            valueExpression.Accept(this);

            // Check the value compatibility.
            IChelaType enumType = node.GetCoercionType();
            IChelaType valueType = valueExpression.GetNodeType();
            if(!valueType.IsConstant())
                Error(valueExpression, "expected constant expresion.");
            if(valueType != enumType &&
               Coerce(valueType, enumType) != enumType)
                Error(valueExpression, "incompatible value type.");

            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            return node;
        }

        public override AstNode Visit (FieldDeclaration node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Get the field type.
            //FieldVariable field = (FieldVariable)node.GetVariable();
            IChelaType fieldType = node.GetNodeType();

            // Get the field initializer.
            Expression initializer = node.GetDefaultValue();
            if(initializer == null)
                return node;

            // Process the initializer.
            initializer.Accept(this);

            // Check the initializer type.
            IChelaType initType = initializer.GetNodeType();
            node.SetCoercionType(fieldType);
            if(Coerce(node, fieldType, initType, initializer.GetNodeValue()) != fieldType)
                Error(node, "incompatible field initialization expression.");

            // Append it to the initialization list.
            if(!fieldType.IsConstant())
            {
                if(currentContainer.IsNamespace())
                {
                    Namespace space = (Namespace)currentContainer;
                    space.AddGlobalInitialization(node);
                }
                else
                {
                    Structure building = (Structure)currentContainer;
                    building.AddFieldInitialization(node);
                }
            }
            return node;
        }
        
        public override AstNode Visit (FunctionPrototype node)
        {
            // Do nothing. ???
            return node;
        }

        private void CheckDefaultConstructorInit(AstNode node, Method function)
        {
            // Get the base building.
            Structure building = (Structure)function.GetParentScope();
            Structure baseBuilding = building.GetBase();
            if(baseBuilding == null)
                return; // Object class.

            // Find a default construcotor.
            FunctionGroup ctorGroup = baseBuilding.GetConstructor();
            Method ctor = null;
            foreach(FunctionGroupName ctorName in ctorGroup.GetFunctions())
            {
                // Ignore static constructors.
                if(ctorName.IsStatic())
                    continue;

                // Check the constructor type.
                FunctionType ctorType = ctorName.GetFunctionType();
                if(ctorType.GetArgumentCount() == 1)
                {
                    // Found, the first argument must be "this".
                    ctor = (Method)ctorName.GetFunction();
                    break;
                }
            }

            // If not found, raise error.
            if(ctor == null)
                Error(node, baseBuilding.GetFullName() + " doesn't have a default constructor.");

            // Store the default constructor.
            function.SetCtorParent(ctor);
        }

        private void CheckConstructorCycles(AstNode node, Method ctor)
        {
            Method current = ctor.GetCtorParent();
            while(current != null)
            {
                // Don't continue checking when hitting a leaf.
                if(current.IsCtorLeaf())
                    return;

                // If the parent is equal to the start point,
                // a cycle has been found.
                if(current == ctor)
                    Error(node, "constructor initializers produces a cycle.");

                // Check the next parent.
                current = current.GetCtorParent();
            }
        }

        private void CheckConstructorInit(FunctionDefinition node)
        {
            // Get the prototype.
            FunctionPrototype prototype = node.GetPrototype();

            // Get the constructor initializer.
            ConstructorInitializer ctorInit = prototype.GetConstructorInitializer();

            // Get the function.
            Method function = (Method)prototype.GetFunction();

            // Special check for the implicit constructor.
            if(ctorInit == null)
            {
                CheckDefaultConstructorInit(node, function);
                return;
            }

            // Get the base constructor.
            Structure building = (Structure)function.GetParentScope();
            FunctionGroup ctorGroup;
            if(ctorInit.IsBaseCall())
            {
                Structure baseBuilding = building.GetBase();
                if(baseBuilding == null)
                    Error(node, "cannot invoke base constructor in Object definition.");

                ctorGroup = baseBuilding.GetConstructor();
            }
            else
            {
                ctorGroup = building.GetConstructor();
            }

            // Store the constructor group.
            ctorInit.SetConstructorGroup(ctorGroup);

            // Visit the constructor initializer
            ctorInit.Accept(this);

            // Set the parent constructor.
            function.SetCtorParent((Method)ctorInit.GetConstructor());

            // Make sure the construction invocation is acyclic.
            if(!function.IsCtorLeaf())
                CheckConstructorCycles(node, function);
        }

        private void MakeCurrentGenerator(AstNode where)
        {
            // Get the function definition node.
            FunctionDefinition functionNode = currentFunction.DefinitionNode;
            // Get the actual return type.
            IChelaType returnType = currentFunction.GetFunctionType().GetReturnType();

            // Make sure the return type its an interface.
            if(!returnType.IsReference() || !(returnType = DeReferenceType(returnType)).IsInterface())
                Error(functionNode, "expected an iterator interface as return type.");

            // Cast the return type into a structure.
            Structure returnBuilding = (Structure)returnType;

            // Assume object type as yield type for now,
            IChelaType yieldType = currentModule.GetObjectClass();

            // Extract the template of the return type.
            Structure returnTemplate = returnBuilding;
            bool isGenericInstance = returnBuilding.IsGenericInstance();
            if(isGenericInstance)
            {
                returnTemplate = returnBuilding.GetTemplateBuilding();
                yieldType = returnBuilding.GetGenericInstance().GetParameter(0);
            }

            // Make sure its one runtime IEnumerator or IEnumerable interfaces.
            bool isEnumerable = false;
            if(returnTemplate == currentModule.GetEnumerableIface() ||
                returnTemplate == currentModule.GetEnumerableGIface())
            {
                isEnumerable = true;
            }
            else if(returnTemplate != currentModule.GetEnumeratorIface() &&
                returnTemplate != currentModule.GetEnumeratorGIface())
            {
                // Raise error for unsupported interface.
                Console.WriteLine("return template {0} == {1}, {2}", returnTemplate.GetFullName(), currentModule.GetEnumeratorGIface().GetFullName(), returnTemplate == currentModule.GetEnumeratorGIface());
                Error(functionNode, "expected an iterator interface such as IEnumerator and IEnumerable as return type.");
            }

            // Now convert the function in a generator.
            functionNode.MakeGenerator();
            if(yieldType.IsPassedByReference())
                functionNode.YieldType = ReferenceType.Create(yieldType);
            else
                functionNode.YieldType = yieldType;
            functionNode.IsGenericIterator = isGenericInstance;
            functionNode.IsEnumerable = isEnumerable;

            // Convert all of the local variables in pseudo-locals.
            foreach(LocalVariable local in currentFunction.GetLocals())
                local.MakePseudoLocal();

            // Convert all of the argument variables into pseudo-arguments.
            foreach(ArgumentVariable arg in functionNode.ArgumentVariables)
                arg.MakePseudoArgument();
        }

        public override AstNode Visit (FunctionDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Get the prototype.
            FunctionPrototype prototype = node.GetPrototype();

            // Get the function.
            Function function = prototype.GetFunction();

            // Store the definition node in the function.
            if(function.DefinitionNode != null)
                Error(node, "multiples definition of a function.");
            function.DefinitionNode = node;

            // Check for declarations, interfaces.
            MemberFlags instanceFlags = function.GetFlags() & MemberFlags.InstanceMask;
            if(instanceFlags == MemberFlags.Abstract || instanceFlags == MemberFlags.Contract
               || function.IsExternal())
            {
                if(node.GetChildren() != null)
                    Error(node, "abstract/extern functions cannot have a definition body.");
                return node;
            }
            else
            {
                if(node.GetChildren() == null)
                    Error(node, "functions must have a definition.");
            }

            // Store the old function.
            // TODO: Add closures
            Function oldFunction = currentFunction;
            currentFunction = function;
            
            // Create the function lexical scope.
            LexicalScope topScope = CreateLexicalScope(node);
            node.SetScope(topScope);

            // Use the prototype scope.
            Scope protoScope = prototype.GetScope();
            if(protoScope != null)
                PushScope(protoScope);

            // Update the scope.
            PushScope(topScope);

            // Push the security.
            if(function.IsUnsafe())
                PushUnsafe();
            
            // Declare the argument variables.
            AstNode argument = prototype.GetArguments();
            int index = 0;
            while(argument != null)
            {
                // Cast the argument node.
                FunctionArgument argNode = (FunctionArgument)argument;

                // Set the argument name.
                string name = argument.GetName();
                ArgumentData argData = function.GetArguments()[index++];
                argData.Name = name;

                // Create the argument variable.
                if(name != null && name != "" && name != "this")
                {                    
                    // Create the argument local variable
                    LocalVariable argLocal = new LocalVariable(name, topScope, argNode.GetNodeType());
                    argLocal.Type = LocalType.Argument;
                    argLocal.ArgumentIndex = index;

                    // Store it in the argument node.
                    argNode.SetVariable(argLocal);
                }
                else if(name == "this")
                {
                    // Create the this argument.
                    node.ArgumentVariables.Add(
                        new ArgumentVariable(0, "this", topScope, argument.GetNodeType())
                    );
                }
                
                argument = argument.GetNext();
            }

            // Check the constructor initializer.
            if(function.IsConstructor())
                CheckConstructorInit(node);

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the security.
            if(function.IsUnsafe())
                PopUnsafe();

            // Restore the scope.
            PopScope();

            // Restore the prototype scope.
            if(protoScope != null)
                PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return node;
        }

        public override AstNode Visit (PropertyDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Push the security.
            PropertyVariable propVar = node.GetProperty();
            if(propVar.IsUnsafe())
                PushUnsafe();

            // Visit the get accessors.
            if(node.GetAccessor != null)
                node.GetAccessor.Accept(this);

            // Visit the set accessors.
            if(node.SetAccessor != null)
                node.SetAccessor.Accept(this);

            // Restore the security.
            if(propVar.IsUnsafe())
                PopUnsafe();

            return node;
        }

        public override AstNode Visit (GetAccessorDefinition node)
        {
            // Get the function.
            Function function = node.GetFunction();
            if(node.GetChildren() == null)
                return node;

            // Store the definition node in the function.
            if(function.DefinitionNode != null)
                Error(node, "multiples definition of a function.");
            function.DefinitionNode = node;

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return node;
        }

        public override AstNode Visit (SetAccessorDefinition node)
        {
            // Get the function.
            Function function = node.GetFunction();
            if(node.GetChildren() == null)
                return node;

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return node;
        }

        public override AstNode Visit (EventDefinition node)
        {
            // Make sure the event delegate type is actually a delegate.
            Class delegateClass = currentModule.GetDelegateClass();
            Class delegateType = node.GetDelegateType();
            if(!delegateType.IsDerivedFrom(delegateClass))
                Error(node.GetEventType(), "event types must be delegates.");

            // Visit the add accessors.
            if(node.AddAccessor != null)
                node.AddAccessor.Accept(this);

            // Visit the remove accessors.
            if(node.RemoveAccessor != null)
                node.RemoveAccessor.Accept(this);

            return node;
        }

        public override AstNode Visit (EventAccessorDefinition node)
        {
            // Get the function.
            Function function = node.GetFunction();
            if(node.GetChildren() == null)
                return node;

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return node;
        }

        public override AstNode Visit (AttributeArgument node)
        {
            // Process the value expression.
            Expression expr = node.GetValueExpression();
            expr.Accept(this);

            // Store the node type temporarily.
            IChelaType valueType = expr.GetNodeType();
            node.SetNodeType(valueType);

            // Check the property compatibility.
            string propName = node.GetName();
            if(string.IsNullOrEmpty(propName))
                return node;

            // Find the property member.
            Class attributeClass = node.GetAttributeClass();
            ScopeMember propMember = attributeClass.FindMemberRecursive(propName);
            if(propMember == null)
                Error(node, "cannot find attribute property.");
            if(!propMember.IsVariable())
                Error(node, "cannot set something that isn't an attribute property.");

            // Only support settable properties and fields.
            Variable propVar = (Variable)propMember;
            if(propVar.IsProperty())
            {
                PropertyVariable property = (PropertyVariable)propVar;
                if(property.SetAccessor == null)
                    Error(node, "the attribute property is not settable.");
            }
            else if(!propVar.IsField())
                Error(node, "the attribute is not a field nor a settable property.");

            // Check the property visibility
            CheckMemberVisibility(node, propVar);

            // Store the property
            node.SetProperty(propVar);

            // Perform coercion.
            IChelaType propType = propVar.GetVariableType();
            if(valueType != propType &&
               Coerce(propType, valueType) != propType)
                Error(node, "unexistent implicit cast for property assignment.");

            // Make sure the value is constant.
            if(!valueType.IsConstant())
                Error(node, "expected constant value.");

            // Store the coercion type.
            node.SetCoercionType(propType);

            // Store the node type.
            node.SetNodeType(propType);

            return node;
        }

        public override AstNode Visit (AttributeInstance node)
        {
            // Read the attribute type.
            Expression attrExpr = node.GetAttributeExpr();
            if(attrExpr is ObjectReferenceExpression)
            {
                ObjectReferenceExpression refExpr = (ObjectReferenceExpression)attrExpr;
                refExpr.SetAttributeName(true);
            }

            attrExpr.Accept(this);

            // Get his type.
            IChelaType attrType = attrExpr.GetNodeType();
            attrType = ExtractActualType(attrExpr, attrType);
            if(!attrType.IsClass())
                Error(node, "attributes must be classes.");

            // Make sure is derived from the Attribute class.
            Class attributeClass = currentModule.GetAttributeClass();
            Class attrBuilding = (Class)attrType;
            if(!attrBuilding.IsDerivedFrom(attributeClass))
                Error(node, "invalid attribute class.");

            // Store the attribute class.
            node.SetAttributeClass(attrBuilding);

            // Visit the attribute arguments.
            AstNode arg = node.GetArguments();
            List<object> arguments = new List<object> ();
            bool readingProperties = false;
            while(arg != null)
            {
                // Store the attribute type.
                AttributeArgument attrArg = (AttributeArgument)arg;
                attrArg.SetAttributeClass(attrBuilding);

                // Visit the argument.
                arg.Accept(this);

                // Check property/vs constructor argument.
                bool propertyArgument = !string.IsNullOrEmpty(arg.GetName());
                if(propertyArgument)
                {
                    // Start/started reading properties values.
                    readingProperties = true;
                }
                else
                {
                    // Check for unnamed value.
                    if(readingProperties)
                        Error(arg, "expected attribute property value.");

                    // Store the argument.
                    arguments.Add(arg.GetNodeType());
                }

                // Visit the next argument.
                arg = arg.GetNext();
            }

            // Pick the correct attribute constructor.
            FunctionGroup ctorGroup = attrBuilding.GetConstructor();
            Function ctor = PickFunction(node, ctorGroup, false, null, arguments, false);
            if(!ctor.IsMethod())
                Error(node, "invalid constructor."); // Shouldn't reach here.

            // Store the picked constructor.
            node.SetAttributeConstructor((Method)ctor);

            return node;
        }

        private void ProcessAttributes(AstNode node)
        {
            // Get the attributes node.
            AstNode attributes = node.GetAttributes();

            // Visit them.
            VisitList(attributes);
        }

        public override AstNode Visit (BlockNode node)
        {
            // Create the block lexical scope.
            LexicalScope blockScope = CreateLexicalScope(node);
            node.SetScope(blockScope);

            // Push the scope.
            PushScope(blockScope);

            // Visit the block  children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (UnsafeBlockNode node)
        {
            // Create the block lexical scope.
            LexicalScope blockScope = CreateLexicalScope(node);
            node.SetScope(blockScope);

            // Push the unsafe.
            PushUnsafe();

            // Push the scope.
            PushScope(blockScope);

            // Visit the block  children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Pop the unsafe scope.
            PopUnsafe();

            return node;
        }

        public override AstNode Visit(LocalVariablesDeclaration node)
        {
            // Get the type expression and the variables.
            Expression typeExpression = node.GetTypeExpression();
            AstNode variable = node.GetDeclarations();
    
            // Visit the type expression.
            typeExpression.Accept(this);
    
            // Check the type.
            IChelaType type = typeExpression.GetNodeType();
            type = ExtractActualType(typeExpression, type);

            // Class/interfaces instances are held by reference.
            if(type.IsPassedByReference())
                type = ReferenceType.Create(type);
            node.SetNodeType(type);
            
            // Set the variables types, and visit them.
            while(variable != null)
            {
                // Set the variable type.
                variable.SetNodeType(type);
    
                // Visit the variable.
                variable.Accept(this);
    
                // Process the next variable.
                variable = variable.GetNext();
            }

            return node;
        }

        public override AstNode Visit (UsingObjectStatement node)
        {
            // Create the using lexical scope.
            LexicalScope blockScope = CreateLexicalScope(node);
            node.SetScope(blockScope);

            // Push the scope.
            PushScope(blockScope);

            // Get the local declarations.
            LocalVariablesDeclaration decls = node.GetLocals();
            decls.Accept(this);

            // Check if disposing or deleting.
            IChelaType varType = decls.GetNodeType();
            bool pointer = varType.IsPointer();
            if(!pointer)
            {
                // Make sure it could implement IDisposable.
                varType = DeReferenceType(varType);
                if(!varType.IsStructure() && !varType.IsClass() && !varType.IsReference())
                    Error(node, "cannot dispose object that isn't a structure, class or interface.");

                // Make sure it does implement IDisposable.
                Structure building = (Structure) varType;
                Structure idisposable = currentModule.GetLangMember("IDisposable") as Interface;
                if(idisposable == null)
                    Error(node, "couldn't find IDisposable");
                if(building != idisposable && !building.Implements(idisposable))
                    Error(node, "object doesn't implement IDisposable.");
            }

            // Build the finally clause.
            TokenPosition position = node.GetPosition();
            AstNode finallyChildren = null;
            AstNode lastChild = null;
            AstNode decl = decls.GetDeclarations();
            while(decl != null)
            {
                AstNode disposeStmnt = null;
                if(pointer)
                {
                    // Delete the variable
                    Expression variable = new VariableReference(decl.GetName(), position);
                    disposeStmnt = new DeleteStatement(variable, position);
                }
                else
                {
                    // Call the variable dispose.
                    Expression functionExpr = new MemberAccess(new VariableReference(decl.GetName(), position), "Dispose", position);
                    Expression callExpr = new CallExpression(functionExpr, null, position);
                    disposeStmnt = new ExpressionStatement(callExpr, position);
                }

                // If not null.
                Expression notNull = new BinaryOperation(BinaryOperation.OpNEQ, new VariableReference(decl.GetName(), position), new NullConstant(position), position);
                IfStatement ifStmnt = new IfStatement(notNull, disposeStmnt, null, position);

                // Link the call.
                if(lastChild == null)
                {
                    finallyChildren = lastChild = ifStmnt;
                }
                else
                {
                    lastChild.SetNext(ifStmnt);
                    lastChild = ifStmnt;
                }


                // Dispose the next declaration.
                decl = decl.GetNext();
            }

            // Build the try-finally.
            FinallyStatement finallyClause = new FinallyStatement(finallyChildren, position);
            TryStatement tryStatement = new TryStatement(node.GetChildren(), null, finallyClause, position);

            // Substitute the children with the try.
            node.SetChildren(tryStatement);

            // Visit the block  children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (LockStatement node)
        {
            // Create the block lexical scope.
            LexicalScope blockScope = CreateLexicalScope(node);
            node.SetScope(blockScope);

            // Push the scope.
            PushScope(blockScope);

            // Visit the mutex expression.
            Expression mutexExpr = node.GetMutexExpression();
            mutexExpr.Accept(this);

            // Make sure its a reference object
            IChelaType mutexType = mutexExpr.GetNodeType();
            if(!mutexType.IsReference())
                Error(mutexExpr, "expected an object expression.");
            mutexType = DeReferenceType(mutexType);

            // Remove the variable layer.
            if(mutexType.IsReference())
                mutexType = DeReferenceType(mutexType);

            // Must be a class or an interface.
            if(!mutexType.IsClass() && !mutexType.IsInterface())
                Error(mutexExpr, "expected a class/interface instance.");

            // Create the mutex local.
            TokenPosition position = node.GetPosition();
            string mutexName = GenSym();
            AstNode decl = new LocalVariablesDeclaration(new TypeNode(TypeKind.Object, position),
                    new VariableDeclaration(mutexName, node.GetMutexExpression(), position),
                        position);

            // Chela.Threading.Monitor variable.
            TypeNode monitor = new TypeNode(TypeKind.Other, position);
            Structure threadingMonitor = (Structure)currentModule.GetThreadingMember("Monitor", true);
            if(threadingMonitor == null)
                Error(node, "couldn't use lock when the runtime doesn't define Chela.Threading.Monitor" );
            monitor.SetOtherType(threadingMonitor);

            // Enter into the mutex.
            AstNode enterMutex = new CallExpression(new MemberAccess(monitor, "Enter", position),
                                    new VariableReference(mutexName, position), position);
            decl.SetNext(enterMutex);

            // Try finally
            AstNode exitMutex = new CallExpression(new MemberAccess(monitor, "Exit", position),
                                    new VariableReference(mutexName, position), position);
            FinallyStatement finallyStmnt = new FinallyStatement(exitMutex, position);

            // Try statement.
            TryStatement tryStmnt = new TryStatement(node.GetChildren(), null, finallyStmnt, position);
            enterMutex.SetNext(tryStmnt);

            // Replace my children.
            node.SetChildren(decl);

            // Visit the block children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (IfStatement node)
        {
            // Get the condition, then and else.
            Expression cond = node.GetCondition();
            AstNode thenNode = node.GetThen();
            AstNode elseNode = node.GetElse();
            
            // Visit the condition, then and else.
            cond.Accept(this);
            thenNode.Accept(this);
            if(elseNode != null)
                elseNode.Accept(this);
            
            // Check the condition type.
            IChelaType condType = cond.GetNodeType();
            if(condType != ChelaType.GetBoolType() &&
               Coerce(condType, ChelaType.GetBoolType()) != ChelaType.GetBoolType())
                Error(node, "condition must be a boolean expression.");

            return node;
        }

        public override AstNode Visit (SwitchStatement node)
        {
            // Store the old switch statement.
            SwitchStatement oldSwitch = currentSwitch;
            currentSwitch = node;

            // Get and visit the "constant" expression.
            Expression constantExpr = node.GetConstant();
            constantExpr.Accept(this);

            // Get the constant type.
            IChelaType constantType = constantExpr.GetNodeType();
            if(constantType.IsReference())
                constantType = DeReferenceType(constantType);
            if(constantType.IsConstant())
                constantType = DeConstType(constantType);

            // Must be an integer.
            if(!constantType.IsInteger())
            {
                if(constantType.IsStructure())
                {
                    Structure enumBuilding = (Structure)constantType;
                    if(!enumBuilding.IsDerivedFrom(currentModule.GetEnumClass()))
                        Error(node, "expected enumeration expression.");

                    // Get the enumeration base type.
                    FieldVariable valueField = (FieldVariable)enumBuilding.FindMember("__value");
                    if(valueField == null)
                        valueField = (FieldVariable)enumBuilding.FindMember("m_value");
                    if(valueField == null)
                        Error(node, "invalid enumeration.");

                    // Make sure the enumeration is an integer.
                    IChelaType enumType = valueField.GetVariableType();
                    if(!enumType.IsInteger())
                        Error(node, "expected integral enumeration.");
                    constantType = enumType;
                }
                else
                    Error(node, "switch expression must be integer.");
            }

            // Use always integer types.
            IChelaType targetType = null;
            if(constantType.IsUnsigned() && constantType.GetSize() == ChelaType.GetUIntType().GetSize())
                targetType = ChelaType.GetUIntType();
            else
                targetType = ChelaType.GetIntType();

            // Perform the coercion.
            if(Coerce(constantType, targetType) != targetType)
                Error(node, "switch expression type must be int or uint compatible.");
            node.SetCoercionType(targetType);

            // A switch statement cannot be empty.
            AstNode caseNode = node.GetCases();
            if(caseNode == null)
                Error(node, "switch statements cannot be empty.");

            // Now process the cases.
            while(caseNode != null)
            {
                // Cast the case node.
                CaseLabel label = (CaseLabel)caseNode;
                label.SetCoercionType(targetType);

                // Visit the case node.
                label.Accept(this);

                // Check the next case.
                caseNode = caseNode.GetNext();
            }

            // Restore the old switch statement.
            currentSwitch = oldSwitch;

            return node;
        }

        public override AstNode Visit (CaseLabel node)
        {
            // Get the constant expr.
            Expression constantExpr = node.GetConstant();
            if(constantExpr != null)
            {
                // Visit the constant expression.
                constantExpr.Accept(this);

                // Perform coercion.
                IChelaType constantType = constantExpr.GetNodeType();
                IChelaType coercionType = node.GetCoercionType();
                if(Coerce(coercionType, constantType) != coercionType)
                    Error(node, "incompatible constant type {0} -> {1}.", constantType.GetDisplayName(), coercionType.GetDisplayName());

                // Delay the case registration to the constant expansion pass.
            }
            else
            {
                // Make sure there's only one default statement.
                AstNode oldDefault = currentSwitch.GetDefaultCase();
                if(oldDefault != null)
                    Error(node, "only one default case is allowed in a switch statement. Previous definition in {0}", oldDefault.GetPosition().ToString());

                // Register the default case.
                currentSwitch.SetDefaultCase(node);
            }

            // Visit the children statements.
            AstNode children = node.GetChildren();
            VisitList(node.GetChildren());

            // The last case cannot be empty.
            if(children == null && node.GetNext() == null)
                Error(node, "last case in a switch cannot be empty.");

            return node;
        }

        public override AstNode Visit(GotoCaseStatement node)
        {
            Expression labelConstant = node.GetLabel();

            // This statement only can be used inside of a switch.
            if(currentSwitch == null)
                Error(node, "goto {0} only can be used inside of a switch.",
                        labelConstant == null ? "default" : "case");

            // Coerce the label constant.
            if(labelConstant != null)
            {
                // Visit the label expression.
                labelConstant.Accept(this);

                // Coerce the label expression.
                IChelaType labelType = labelConstant.GetNodeType();
                IChelaType coercionType = currentSwitch.GetCoercionType();
                if(Coerce(node, coercionType, labelType, labelConstant.GetNodeValue()) != coercionType)
                    Error(node, "cannot implicitly cast a label of type {0} into {1}",
                           labelType.GetDisplayName(), coercionType.GetDisplayName());
                // Not all of the cases have been registered yet, so perform the
                // existence checks in generation pass.
            }

            // Remove dead code.
            AstNode dead= node.GetNext();
            if(dead != null)
                Warning(dead, "detected unreachable code.");
            node.SetNext(null);

            return node;
        }

        public override AstNode Visit (BreakStatement node)
        {
            // Check for dead code.
            AstNode deadCode = node.GetNext();
            if(deadCode != null)
            {
                // Write a warning.
                Warning(node, "the code after a break is unreachable.");
    
                // Process the dead code for errors.
                VisitList(deadCode);
    
                // Delete the dead code.
                node.SetNext(null);
            }

            return node;
        }
        
        public override AstNode Visit (ContinueStatement node)
        {
            // Check for dead code.
            AstNode deadCode = node.GetNext();
            if(deadCode != null)
            {
                // Write a warning.
                Warning(node, "the code after a continue is unreachable.");
    
                // Process the dead code for errors.
                VisitList(deadCode);
    
                // Delete the dead code.
                node.SetNext(null);
            }

            return node;
        }
        
        public override AstNode Visit (WhileStatement node)
        {
            // Get the condition and the job.
            Expression condition = node.GetCondition();
            AstNode job = node.GetJob();
            
            // Visit the condition and the job.
            condition.Accept(this);
            VisitList(job);
            
            // Check the condition type.
            IChelaType condType = condition.GetNodeType();
            if(condType != ChelaType.GetBoolType() &&
               Coerce(condType, ChelaType.GetBoolType()) != ChelaType.GetBoolType())
                Error(node, "condition must be a boolean expression.");

            return node;
        }
        
        public override AstNode Visit (DoWhileStatement node)
        {
            // Get the condition and the job.
            Expression condition = node.GetCondition();
            AstNode job = node.GetJob();
            
            // Visit the condition and the job.
            condition.Accept(this);
            job.Accept(this);
            
            // Check the condition type.
            IChelaType condType = condition.GetNodeType();
            if(condType != ChelaType.GetBoolType() &&
               Coerce(condType, ChelaType.GetBoolType()) != ChelaType.GetBoolType())
                Error(node, "condition must be a boolean expression.");

            return node;
        }
        
        public override AstNode Visit (ForStatement node)
        {
            // Create the for lexical scope.
            LexicalScope scope = CreateLexicalScope(node);
            node.SetScope(scope);
            
            // Enter into the for scope.
            PushScope(scope);
            
            // Get the for elements.
            AstNode decl = node.GetDecls();
            AstNode cond = node.GetCondition();
            AstNode incr = node.GetIncr();
            
            // Visit his elements.
            if(decl != null)
                decl.Accept(this);
            
            // Check the condition.
            if(cond != null)
            {
                cond.Accept(this);
                
                // Check the condition type.
                IChelaType condType = cond.GetNodeType();
                if(condType != ChelaType.GetBoolType() &&
                   Coerce(condType, ChelaType.GetBoolType()) != ChelaType.GetBoolType())
                    Error(node, "condition must be a boolean expression.");
            }
            
            // Visit the "increment".
            if(incr != null)
                incr.Accept(this);
            
            // Visit his children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (ForEachStatement node)
        {
            // Create the foreach lexical scope.
            LexicalScope scope = CreateLexicalScope(node);
            node.SetScope(scope);

            // Push the lexical scope.
            PushScope(scope);

            // Get the element type expression.
            Expression typeExpr = node.GetTypeExpression();

            // Visit the container expression.
            Expression containerExpr = node.GetContainerExpression();
            containerExpr.Accept(this);

            // Get the container expression type.
            IChelaType containerType = containerExpr.GetNodeType();
            if(!containerType.IsReference())
                Error(containerExpr, "expected an object reference.");

            // Remove type references.
            containerType = DeReferenceType(containerType);
            if(containerType.IsReference())
                containerType = DeReferenceType(containerType);

            // Use primitives associated types.
            if(!containerType.IsStructure() && !containerType.IsClass() && !containerType.IsInterface())
            {
                IChelaType assoc = currentModule.GetAssociatedClass(containerType);
                if(assoc == null)
                    Error(containerExpr, "cannot iterate a container of type {0}", assoc.GetFullName());
                containerType = assoc;
            }

            // Get the GetEnumerator method group.
            Structure building = (Structure)containerType;
            FunctionGroup getEnumeratorGroup = building.FindMemberRecursive("GetEnumerator") as FunctionGroup;
            if(getEnumeratorGroup == null)
                Error(containerExpr, "cannot find GetEnumerator method in {0}", containerType.GetName());

            // Use the first no static member.
            Function getEnumeratorFunction = null;
            foreach(FunctionGroupName gname in getEnumeratorGroup.GetFunctions())
            {
                // Ignore static functions.
                if(gname.IsStatic())
                    continue;

                // Only one argument is supported.
                if(gname.GetFunctionType().GetArgumentCount() == 1)
                {
                    getEnumeratorFunction = gname.GetFunction();
                    break;
                }
            }

            // Make sure the function was found.
            if(getEnumeratorFunction == null)
                Error(containerExpr, "{0} doesn't have a no static GetEnumerator() method.", containerType.GetName());

            // Get the enumerator type.
            TokenPosition position = node.GetPosition();
            TypeNode enumeratorTypeNode = new TypeNode(TypeKind.Other, position);
            enumeratorTypeNode.SetOtherType(getEnumeratorFunction.GetFunctionType().GetReturnType());

            // Get the numerator.
            Expression enumeratorExpr = new CallExpression(
                new MemberAccess(node.GetContainerExpression(), "GetEnumerator", position),
                null, position);

            // Create a variable for the enumerator.
            string enumeratorName = GenSym();
            LocalVariablesDeclaration declEnum = new LocalVariablesDeclaration(enumeratorTypeNode,
                    new VariableDeclaration(enumeratorName, enumeratorExpr, position), position);

            // Create a variable for the current object.
            LocalVariablesDeclaration currentDecl =
                new LocalVariablesDeclaration(node.GetTypeExpression(),
                    new VariableDeclaration(node.GetName(), null, position), position);
            declEnum.SetNext(currentDecl);

            // Get the current member.
            Expression currentProp = new MemberAccess(new VariableReference(enumeratorName, position),
                    "Current", position);

            // Create the while job.
            AstNode whileJob = new ExpressionStatement(
                new AssignmentExpression(new VariableReference(node.GetName(), position),
                    new CastOperation(typeExpr, currentProp, position), position),
                position);
            whileJob.SetNext(node.GetChildren());

            // Create the loop with MoveNext condition.
            Expression moveNext = new CallExpression(
                new MemberAccess(new VariableReference(enumeratorName, position), "MoveNext", position),
                null, position);
            WhileStatement whileStmnt = new WhileStatement(moveNext, whileJob, position);
            currentDecl.SetNext(whileStmnt);

            // Change my children.
            node.SetChildren(declEnum);

            // Visit my children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (FixedVariableDecl node)
        {
            // Get the variable type.
            IChelaType type = node.GetNodeType();

            // Create the variable.
            LocalVariable fixedVar = new LocalVariable(node.GetName(), (LexicalScope)currentScope, type);
            node.SetVariable(fixedVar);

            // Visit the value expression.
            Expression valueExpr = node.GetValueExpression();
            valueExpr.Accept(this);

            // Get the value type.
            IChelaType valueType = valueExpr.GetNodeType();

            // The value must be a reference or a pointer.
            if(!valueType.IsReference() && !valueType.IsPointer())
                Error(valueExpr, "the value expression must be a reference or a pointer.");

            if(valueType.IsPointer())
            {
                if(Coerce(node, type, valueType, valueExpr.GetNodeValue()) != type)
                    Error(valueExpr, "cannot convert {0} into {1}.", valueType.GetName(), type.GetName());

                // Coerce the value.
                node.SetCoercionType(type);
            }
            else
            {
                // Remove references.
                IChelaType refType = DeReferenceType(valueType);
                if(refType.IsReference())
                    refType = DeReferenceType(refType);

                // Only support arrays
                if(refType.IsArray())
                {
                    ArrayType array = (ArrayType)refType;
                    IChelaType pointerType = PointerType.Create(array.GetValueType());
                    if(Coerce(node, type, pointerType, null) != type)
                        Error(valueExpr, "cannot convert {0} into {1}.", pointerType.GetName(), type.GetName());
                }

                // Set the new coercion type.
                node.SetCoercionType(ReferenceType.Create(refType));
            }

            return node;
        }

        public override AstNode Visit (FixedStatement node)
        {
            UnsafeError(node, "cannot use fixed statement under safe contexts.");

            // Create the lexical scope.
            LexicalScope scope = CreateLexicalScope(node);
            node.SetScope(scope);

            // Enter into the scope.
            PushScope(scope);

            // Visit the type expression.
            Expression typeExpr = node.GetTypeExpression();
            typeExpr.Accept(this);

            // Read the type expression.
            IChelaType fixedType = typeExpr.GetNodeType();
            fixedType = ExtractActualType(typeExpr, fixedType);

            // Make sure its a pointer.
            if(!fixedType.IsPointer())
                Error(typeExpr, "expected pointer type.");

            // Vist the declarations.
            AstNode decl = node.GetDeclarations();
            while(decl != null)
            {
                // Notify about the type.
                decl.SetNodeType(fixedType);

                // Visit it.
                decl.Accept(this);

                // Visit the next declaration.
                decl = decl.GetNext();
            }

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit(VariableDeclaration node)
        {
            // Get the variable name.
            string varName = node.GetName();
            
            // Avoid redefinition.
            if(currentContainer.FindMember(varName) != null)
                Error(node, "attempting to redefine a variable in a same scope.");

            // Get the initial value.
            Expression initialValue = node.GetInitialValue();
            
            // Visit the initial value.
            // Do this here to use the state previous to the variable declaration.
            if(initialValue != null)
                initialValue.Accept(this);
            
            // Create the variable.
            LocalVariable local = new LocalVariable(node.GetName(), (LexicalScope)currentContainer,
                                                    node.GetNodeType());
            
            // Store it in the node.
            node.SetVariable(local);

            // Check the compatibility with the initial value.
            if(initialValue == null)
                return node;
            
            // Get the variable type and the initial value type.
            IChelaType varType = node.GetNodeType();
            IChelaType valueType = initialValue.GetNodeType();
            
            // Class instances are held by reference.
            if(varType != valueType && Coerce(node, varType, valueType, initialValue.GetNodeValue()) != varType)
                Error(node, "cannot implicitly cast from {0} to {1}.", valueType.GetFullName(), varType.GetFullName());

            return node;
        }
        
        public override AstNode Visit (ReturnStatement node)
        {
            Expression expr = node.GetExpression();
            
            // Visit the expression.
            if(expr != null)
                expr.Accept(this);

            // Get the expression type
            IChelaType exprType = expr != null ? expr.GetNodeType() : ChelaType.GetVoidType();
            
            // Get the function return type.
            FunctionType functionType = currentFunction.GetFunctionType();
            IChelaType returnType = functionType.GetReturnType();

            // Load the function definition node
            FunctionDefinition functionNode = currentFunction.DefinitionNode;

            // Use the yield type if writin a generator.
            if(node.IsYield)
            {
                // Make sure its a generateable function.
                if(functionNode == null)
                    Error(node, "only can create generators in function definitions.");

                // Don't allow yielding and returning and the same time.
                if(functionNode.HasReturns)
                    Error(node, "cannot mix return and generator style yield return in a same function.");

                // Convert the function into a generator.
                if(!functionNode.IsGenerator)
                    MakeCurrentGenerator(node);

                // Store myself in the interruption point table.
                functionNode.Yields.Add(node);

                // Use the yield type as return type.
                returnType = functionNode.YieldType;

            }
            else if(functionNode != null)
            {
                // Don't allow plain returning in not generators.
                if(functionNode.IsGenerator)
                    Error(node, "cannot perform plain return in a generator function.");

                // Prevent mixing a generator.
                functionNode.HasReturns = true;
            }
            
            // Check for coercion validity.
            if(exprType != returnType)
            {
                IChelaType coercionType = Coerce(node, returnType, exprType, expr.GetNodeValue());
                //System.Console.WriteLine("{0}->{1}={2}", exprType, coercionType, returnType);
                //System.Console.WriteLine("{0} == {1}", coercionType, returnType);
                if(coercionType != returnType)
                    Error(node, "cannot implicitly cast return type '{0}' into '{1}'",
                        exprType.GetDisplayName(), returnType.GetDisplayName());
            }
            
            // Set the coercion type.
            node.SetCoercionType(returnType);
            
            // Check for dead code.
            AstNode deadCode = node.GetNext();
            if(!node.IsYield && deadCode != null)
            {
                // Write a warning.
                Warning(node, "the code after a return is unreachable.");
                
                // Process the dead code for error.
                VisitList(deadCode);
                
                // Delete the dead code.
                node.SetNext(null);
            }

            return node;
        }
        
        public override AstNode Visit (ExpressionStatement node)
        {
            // Get the expression.
            Expression expr = node.GetExpression();
            
            // Visit it.
            expr.Accept(this);

            return node;
        }
        
        public override AstNode Visit (AssignmentExpression node)
        {
            // Get the variable and value expression.
            Expression variable = node.GetVariable();
            Expression value = node.GetValue();
            
            // Visit them.
            variable.Accept(this);
            value.Accept(this);
            
            // Get their types.
            IChelaType variableType = variable.GetNodeType();
            IChelaType valueType = value.GetNodeType();
            
            // Set the node type.
            node.SetNodeType(variableType);
            
            // The variable must be a reference.
            if(!variableType.IsReference())
                Error(node, "trying to set something that isn't a reference.");

            // Check the variable is not a constant.
            variableType = DeReferenceType(variableType);
            if(variableType.IsConstant())
                Error(node, "cannot modify constants.");

            // Check for the read-only constraint.
            Variable variableSlot = variable.GetNodeValue() as Variable;
            if(variableSlot != null && variableSlot.IsReadOnly())
                CheckReadOnlyConstraint(node, variableSlot);

            // Set the coercion type.
            node.SetCoercionType(variableType);
            
            // Check the type compatibility.
            //System.Console.WriteLine("{0}->{1}", valueType, variableType);
            if(variableType != valueType &&
               Coerce(node, variableType, valueType, value.GetNodeValue()) != variableType)
            {
                // Delay integer constants casting.
                if(valueType.IsConstant())
                {
                    valueType = DeConstType(valueType);
                    if(valueType.IsInteger() &&
                        valueType != ChelaType.GetBoolType() &&
                        valueType != ChelaType.GetCharType())
                        node.DelayedCoercion = true;
                }

                if(!node.DelayedCoercion)
                    Error(value, "cannot implicitly cast from {0} to {1}.",
                        valueType.GetDisplayName(), variableType.GetDisplayName());
            }

            return node;
        }
        
        public override AstNode Visit (UnaryOperation node)
        {
            // Get the expression.
            Expression expr = node.GetExpression();
    
            // Visit the expression.
            expr.Accept(this);
    
            // Get the expression type.
            IChelaType type = expr.GetNodeType();

            // Dereference the type.
            type = DeReferenceType(type);
            node.SetCoercionType(type);

            // De-const the type.
            type = DeConstType(type);
            
            // Validate the expression type.
            switch(node.GetOperation())
            {
            case UnaryOperation.OpNop:
                break;
            case UnaryOperation.OpNeg:
                if(!type.IsNumber())
                    Error(node, "expected numeric expression.");

                // Use signed integer when negating.
                if(type.IsInteger() && !type.IsPlaceHolderType())
                {
                    IntegerType intType = (IntegerType)type;
                    intType = intType.GetSignedVersion();
                    if(node.GetCoercionType().IsConstant())
                        node.SetCoercionType(ConstantType.Create(intType));
                    else
                        node.SetCoercionType(intType);
                }
                break;
            case UnaryOperation.OpNot:
                if(type != ChelaType.GetBoolType())
                    Error(node, "expected boolean expression.");
                break;
            case UnaryOperation.OpBitNot:
                if(!type.IsInteger())
                    Error(node, "expected integer expression.");
                break;
            default:
                Error(node, "Compiler bug, unknown unary operation.");
                break;
            }
    
            // Set the node type.
            node.SetNodeType(node.GetCoercionType());

            return node;
        }
        
        public override AstNode Visit (BinaryOperation node)
        {
            // Get the expressions.
            Expression left = node.GetLeftExpression();
            Expression right = node.GetRightExpression();

            // Make sure they are linked, used by operator overloading.
            if(left.GetNext() != null || right.GetNext() != null)
                Error(node, "binary operation arguments linked.");
            left.SetNext(right);
    
            // Visit the expressions.
            left.Accept(this);
            right.Accept(this);
    
            // Check the types.
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();
            IChelaType nullRef = ChelaType.GetNullType();

            // Dereference and de-const types.
            bool leftConstant = leftType.IsConstant();
            leftType = DeConstType(leftType); // Required for string constant.
            if(leftType != nullRef && leftType.IsReference())
            {
                leftType = DeReferenceType(leftType);
                leftConstant = leftConstant || leftType.IsConstant();
                leftType = DeConstType(leftType);
                leftType = DeReferenceType(leftType);
            }

            leftConstant = leftConstant || leftType.IsConstant();
            leftType = DeConstType(leftType);

            // De-reference and de-const the right type.
            bool rightConstant = rightType.IsConstant();
            rightType = DeConstType(rightType);
            if(rightType != nullRef && rightType.IsReference())
            {
                rightType = DeReferenceType(rightType);
                rightConstant = rightConstant || rightType.IsConstant();
                rightType = DeConstType(rightType);
                rightType = DeReferenceType(rightType);
            }
            rightType = DeConstType(rightType);

            // Is this a constant.
            bool isConstant = leftConstant && rightConstant;

            // Copy the deref and deconst types.
            IChelaType actualLeftType = leftType;
            IChelaType actualRightType = rightType;

            // Check for operator overloading.
            string opName = BinaryOperation.GetOpName(node.GetOperation());
            Function overload = null;
            if((leftType.IsStructure() || leftType.IsClass()) &&
               opName != null)
            {
                // Find the operation function.
                Structure building = (Structure)leftType;
                FunctionGroup opGroup = (FunctionGroup)building.FindMember(opName);
                if(opGroup != null)
                {
                    // Pick the function from the group.
                    overload = PickFunction(node, opGroup, true, null, left, true);
                }
            }

            // Now check at the right type.
            if((rightType.IsStructure() || rightType.IsClass()) &&
               opName != null && overload == null)
            {
                // Find the operation function.
                Structure building = (Structure)rightType;
                FunctionGroup opGroup = (FunctionGroup)building.FindMember(opName);
                if(opGroup  != null)
                {
                    // Pick the function from the group.
                    overload = PickFunction(node, opGroup, true, null, left, true);
                }
            }

            // Use the found overload.
            if(overload != null)
            {
                // Set the operator overload.
                FunctionType overloadType = overload.GetFunctionType();
                node.SetNodeType(overloadType.GetReturnType());
                node.SetOverload(overload);
                return node;
            }

            // Check for pointer arithmetic.
            int op = node.GetOperation();
            if((leftType.IsPointer() || rightType.IsPointer()) &&
               (op == BinaryOperation.OpAdd || op == BinaryOperation.OpSub))
            {
                // Only addition and subtraction are accepted.
                if(leftType.IsPointer() && rightType.IsPointer())
                {
                    if(op == BinaryOperation.OpAdd)
                        Error(node, "cannot add two pointers.");

                    // Set the coercion type and pointer arithmetic flags.
                    node.SetNodeType(ChelaType.GetLongType());
                    node.SetCoercionType(leftType);
                    node.SetSecondCoercion(rightType);
                    node.SetPointerArithmetic(true);
                }
                else
                {
                    // Check type compatibility.
                    if(op == BinaryOperation.OpSub && !leftType.IsPointer())
                        Error(node, "cannot perform integer - pointer subtraction.");
                    else if((leftType.IsPointer() && !rightType.IsInteger()) ||
                            (!leftType.IsInteger() && rightType.IsPointer()))
                        Error(node, "only pointers and integers can be used in pointer arithmetic.");

                    // Set the coercion type and pointer arithmetic flags.
                    node.SetNodeType(leftType.IsPointer() ? leftType : rightType);
                    node.SetCoercionType(leftType);
                    node.SetSecondCoercion(rightType);
                    node.SetPointerArithmetic(true);
                }

                // There aren't intermediate operations.
                node.SetOperationType(node.GetNodeType());

                return node;
            }
            else
            {
                // Read the types again.
                leftType = left.GetNodeType();
                rightType = right.GetNodeType();
            }

            // Perform coercion.
            IChelaType destType = Coerce(node, leftType, rightType, right.GetNodeValue());
            if(destType == null)
                destType = Coerce(node, rightType, leftType, left.GetNodeValue());

            // Set the node coercion type.
            node.SetCoercionType(destType);
            node.SetSecondCoercion(destType);

            // Checks for matrix multiplication.
            if(BinaryOperation.OpMul == op)
            {
                // Use the actual types.
                leftType = actualLeftType;
                rightType = actualRightType;

                if(leftType.IsMatrix() && rightType.IsMatrix())
                {
                    // Cast the matrix.
                    MatrixType leftMatrix = (MatrixType)leftType;
                    MatrixType rightMatrix = (MatrixType)rightType;

                    // The number of first matrix columns must be equal
                    // to the number of the second matrix rows.
                    if(leftMatrix.GetNumColumns() != rightMatrix.GetNumRows())
                        Error(node, "not matching number of matrix column and rows");

                    // Coerce the primitive types.
                    IChelaType primCoercion = Coerce(leftMatrix.GetPrimitiveType(), rightMatrix.GetPrimitiveType());
                    if(primCoercion == null)
                        Error(node, "cannot multiply a {0} by a{1}", leftMatrix.GetDisplayName(), rightMatrix.GetDisplayName());
                    node.SetCoercionType(MatrixType.Create(primCoercion, leftMatrix.GetNumRows(), leftMatrix.GetNumColumns()));
                    node.SetSecondCoercion(MatrixType.Create(primCoercion, rightMatrix.GetNumRows(), rightMatrix.GetNumColumns()));

                    // Create the destination type.
                    destType = MatrixType.Create(primCoercion, leftMatrix.GetNumRows(), rightMatrix.GetNumColumns());
                    node.SetMatrixMul(true);
                }
                else if(leftType.IsMatrix() && rightType.IsVector())
                {
                    // Cast the matrix and the vector.
                    MatrixType leftMatrix = (MatrixType)leftType;
                    VectorType rightVector = (VectorType)rightType;

                    // Check the dimensions.
                    if(leftMatrix.GetNumColumns() != rightVector.GetNumComponents())
                        Error(node, "not matching number of matrix column and rows");

                    // Coerce the primitive types.
                    IChelaType primCoercion = Coerce(leftMatrix.GetPrimitiveType(), rightVector.GetPrimitiveType());
                    if(primCoercion == null)
                        Error(node, "cannot multiply a {0} by a {1}", leftMatrix.GetDisplayName(), rightVector.GetDisplayName());
                    node.SetCoercionType(MatrixType.Create(primCoercion, leftMatrix.GetNumRows(), leftMatrix.GetNumColumns()));
                    node.SetSecondCoercion(VectorType.Create(primCoercion, rightVector.GetNumComponents()));

                    // Create the destination type.
                    destType = VectorType.Create(primCoercion, leftMatrix.GetNumRows());
                    node.SetMatrixMul(true);
                }
                else if(rightType.IsMatrix() && leftType.IsVector())
                {
                    // Cast the matrix and the vector.
                    VectorType leftVector = (VectorType)leftType;
                    MatrixType rightMatrix = (MatrixType)rightType;

                    // Check the dimensions.
                    if(leftVector.GetNumComponents() != rightMatrix.GetNumRows())
                        Error(node, "not matching number of matrix column and rows");

                    // Coerce the primitive types.
                    IChelaType primCoercion = Coerce(leftVector.GetPrimitiveType(), rightMatrix.GetPrimitiveType());
                    if(primCoercion == null)
                        Error(node, "cannot multiply a {0} by a {1}", leftVector.GetDisplayName(), rightMatrix.GetDisplayName());
                    node.SetCoercionType(VectorType.Create(primCoercion, leftVector.GetNumComponents()));
                    node.SetSecondCoercion(MatrixType.Create(primCoercion, rightMatrix.GetNumRows(), rightMatrix.GetNumColumns()));

                    // Create the destination type.
                    destType = VectorType.Create(primCoercion, rightMatrix.GetNumColumns());
                    node.SetMatrixMul(true);
                }
            }

            // Check for special operations
            if(destType == null)
            {
                // Use the actual types.
                leftType = actualLeftType;
                rightType = actualRightType;

                // Check for external operations.
                if(leftType.IsVector() && (op == BinaryOperation.OpDiv
                   || op == BinaryOperation.OpMul))
                {
                    // Select the types.
                    VectorType vectorType = (VectorType)leftType;
                    IChelaType primitiveType = rightType;

                    // Coerce the primitive type.
                    IChelaType secondCoercion = vectorType.GetPrimitiveType();
                    if(primitiveType != secondCoercion &&
                       Coerce(secondCoercion, primitiveType) != secondCoercion)
                        Error(node, "failed to perform scalar operation.");

                    // Store the new coercion types.
                    destType = vectorType;
                    node.SetCoercionType(vectorType);
                    node.SetSecondCoercion(secondCoercion);
                }
                else if(rightType.IsVector() && op == BinaryOperation.OpMul)
                {
                    // Select the types.
                    IChelaType primitiveType = leftType;
                    VectorType vectorType = (VectorType)rightType;

                    // Coerce the primitive type.
                    IChelaType firstCoercion = vectorType.GetPrimitiveType();
                    if(primitiveType != firstCoercion &&
                       Coerce(firstCoercion, primitiveType) != firstCoercion)
                        Error(node, "failed to perform scalar operation.");

                    // Store the new coercion types.
                    destType = vectorType;
                    node.SetCoercionType(firstCoercion);
                    node.SetSecondCoercion(vectorType);
                }
                else
                    Error(node, "failed to perform binary operation, invalid operands of type {0} and {1}.",
                        leftType.GetDisplayName(), rightType.GetDisplayName());
            }

            // De-const the type.
            destType = DeConstType(destType);

            // Dest type must be integer, float or boolean.
            IChelaType operationType = destType;
            if(!destType.IsNumber() && !destType.IsPointer() &&
                !BinaryOperation.IsEquality(op))
            {
                // Try to operate with the unboxed types.
                // Only structures are box holders.
                if(!destType.IsStructure())
                    Error(node, "cannot perform binary operations with type {0}.", destType.GetDisplayName());

                // Use the structure associated type, or use his enum type.
                Structure building = (Structure)destType;
                IChelaType assocType = currentModule.GetAssociatedClassPrimitive(building);
                if(assocType == null)
                {
                    // The structure can be an enumeration.
                    Class enumClass = currentModule.GetEnumClass();
                    if(!building.IsDerivedFrom(enumClass))
                        Error(node, "cannot perform operation {0} with object of types {1}.",
                            BinaryOperation.GetOpErrorName(op),
                            building.GetDisplayName());
                }

                // Unbox the structure.
                FieldVariable valueField = building.FindMember("__value") as FieldVariable;
                if(valueField == null)
                    valueField = building.FindMember("m_value") as FieldVariable;
                if(valueField == null)
                    Error(node, "structure {0} is not boxing a primitive.", building.GetDisplayName());
                operationType = valueField.GetVariableType();
                node.SetCoercionType(operationType);
                node.SetSecondCoercion(operationType);
            }

            // Use integers of at least 4 bytes.
            if(!operationType.IsPlaceHolderType() && operationType.IsInteger() &&
                operationType.GetSize() < 4 && BinaryOperation.IsArithmetic(op))
            {
                destType = ChelaType.GetIntType();
                operationType = destType;
                IChelaType coercionType = destType;
                if(isConstant)
                    coercionType = ConstantType.Create(destType);
                node.SetCoercionType(coercionType);
                node.SetSecondCoercion(coercionType);
            }

            // Perform validation
            switch(op)
            {
            case BinaryOperation.OpAdd:
            case BinaryOperation.OpMul:
            case BinaryOperation.OpDiv:
            case BinaryOperation.OpMod:
                if(operationType == ChelaType.GetBoolType())
                    Error(node, "Arithmetic operations aren't available with boolean values.");
                break;

            case BinaryOperation.OpSub:
                if(operationType == ChelaType.GetBoolType())
                    Error(node, "Arithmetic operations aren't available with boolean values.");

                // Use signed integer when subtracting.
                if(operationType.IsInteger() && operationType.IsUnsigned())
                {
                    // Use a new dest type and coercion type.
                    IntegerType intType = (IntegerType)operationType;
                    operationType = intType.GetSignedVersion();

                    IChelaType newCoercion = destType;
                    if(isConstant)
                        newCoercion = ConstantType.Create(intType);

                    // Override the result types.
                    node.SetCoercionType(newCoercion);
                    node.SetSecondCoercion(newCoercion);
                    destType = operationType;
                }
                break;
            case BinaryOperation.OpBitAnd:
            case BinaryOperation.OpBitOr:
            case BinaryOperation.OpBitXor:
            case BinaryOperation.OpBitLeft:
            case BinaryOperation.OpBitRight:
                if(operationType.IsFloatingPoint() || operationType == ChelaType.GetBoolType())
                    Error(node, "Bitwise operations aren't available with floating point and boolean values.");
                break;
            case BinaryOperation.OpLT:
            case BinaryOperation.OpGT:
            case BinaryOperation.OpEQ:
            case BinaryOperation.OpNEQ:
            case BinaryOperation.OpLEQ:
            case BinaryOperation.OpGEQ:
                // Boolean result.
                operationType = ChelaType.GetBoolType();
                destType = operationType;
                break;
            case BinaryOperation.OpLAnd:
            case BinaryOperation.OpLOr:
                // Left and right must be boolean.
                if(operationType != ChelaType.GetBoolType())
                    Error(node, "Expected boolean arguments.");
                break;
            default:
                Error(node, "Compiler bug, unknown binary operation.");
                break;
            }
    
            // Set the destination type.
            if(isConstant)
            {
                node.SetOperationType(ConstantType.Create(operationType));
                node.SetNodeType(ConstantType.Create(destType));
            }
            else
            {
                node.SetOperationType(operationType);
                node.SetNodeType(destType);
            }

            return node;
        }

        public override AstNode Visit (BinaryAssignOperation node)
        {
            // Get the expressions.
            Expression left = node.GetVariable();
            Expression right = node.GetValue();

            // Visit the expressions.
            left.Accept(this);
            right.Accept(this);
    
            // Check the types.
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();

            // The variable must be a reference.
            if(!leftType.IsReference())
                Error(node, "trying to set something that isn't a reference.");
            leftType = DeReferenceType(leftType);

            // Don't modify constants.
            if(leftType.IsConstant())
                Error(node, "cannot modify constants.");

            // Check coercion.
            // TODO: Add operator overloading.

            // Set the variable type.
            node.SetNodeType(left.GetNodeType());

            // Check for pointer arithmetic.
            if(leftType.IsPointer())
            {
                // Only addition and subtraction are accepted.
                int op = node.GetOperation();
                if(op != BinaryOperation.OpAdd && op != BinaryOperation.OpSub)
                    Error(node, "only addition and subtraction of pointers is supported.");

                // Make sure the right type is an integer or another pointer.
                if(rightType.IsReference())
                    rightType = DeReferenceType(rightType);

                // De-const the right operand.
                if(rightType.IsConstant())
                    rightType = DeConstType(rightType);

                // Don't allow right operand pointer.
                if(!rightType.IsInteger())
                    Error(node, "right operand must be integer.");

                // Set the coercion type and pointer arithmetic flags.
                node.SetCoercionType(leftType);
                node.SetSecondCoercion(rightType);
                node.SetPointerArithmetic(true);

                return node;
            }
            else if(leftType.IsReference())
            {
                // Handle the event subscription.
                leftType = DeReferenceType(leftType);

                if(!leftType.IsClass())
                    Error(node, "unsupported operation.");

                // Make sure its actually a delegate.
                Class delegateClass = currentModule.GetDelegateClass();
                Class delegateType = (Class)leftType;
                if(!delegateType.IsDerivedFrom(delegateClass))
                    Error(node, "cannot subscribe no delegates.");

                // Check the operation.
                int op = node.GetOperation();
                if(op != BinaryOperation.OpAdd && op != BinaryOperation.OpSub)
                    Error(node, "invalid event subscription/delegate operation.");

                // Perform coercion.
                leftType = ReferenceType.Create(delegateType);
                if(Coerce(node, leftType, rightType, right.GetNodeValue()) != leftType)
                    Error(node, "unexistent implicit cast for assignment.");

                // Get the overload operator.
                Variable variable = (Variable)left.GetNodeValue();
                if(!variable.IsEvent())
                {
                    string opName = op == BinaryOperation.OpAdd ? "Combine" : "Remove";

                    // Find the first no static function with 2 arguments.
                    FunctionGroup opGroup = (FunctionGroup)delegateClass.FindMember(opName);
                    Function overload = null;
                    foreach(FunctionGroupName gname in opGroup.GetFunctions())
                    {
                        // Ignore no static functions.
                        if(!gname.IsStatic())
                            continue;

                        // Select the first function with 2 arguments.
                        if(gname.GetFunctionType().GetArgumentCount() == 2)
                        {
                            overload = gname.GetFunction();
                            break;
                        }
                    }

                    // Make sure the function was found.
                    if(overload == null)
                        Error(node, "Runtime delegate doesn't have function " + opName);

                    // Use the overload.
                    node.SetOverload(overload);
                }

                node.SetCoercionType(leftType);
                node.SetSecondCoercion(leftType);
                return node;
            }

            if(Coerce(node, leftType, rightType, right.GetNodeValue()) != leftType)
                Error(node, "unexistent implicit cast for assignment.");
            node.SetCoercionType(leftType);
            node.SetSecondCoercion(leftType);

            // Perform validation
            switch(node.GetOperation())
            {
            case BinaryOperation.OpAdd:
            case BinaryOperation.OpSub:
            case BinaryOperation.OpMul:
            case BinaryOperation.OpDiv:
            case BinaryOperation.OpMod:
                if(leftType == ChelaType.GetBoolType())
                    Error(node, "Arithmetic operations aren't available with boolean values.");
                break;
            case BinaryOperation.OpBitAnd:
            case BinaryOperation.OpBitOr:
            case BinaryOperation.OpBitXor:
            case BinaryOperation.OpBitLeft:
            case BinaryOperation.OpBitRight:
                if(leftType.IsFloatingPoint() || leftType == ChelaType.GetBoolType())
                    Error(node, "Bitwise operations aren't available with floating point and boolean values.");
                break;
            case BinaryOperation.OpLT:
            case BinaryOperation.OpGT:
            case BinaryOperation.OpEQ:
            case BinaryOperation.OpNEQ:
            case BinaryOperation.OpLEQ:
            case BinaryOperation.OpGEQ:
            case BinaryOperation.OpLAnd:
            case BinaryOperation.OpLOr:
                Error(node, "Operation without assignment version.");
                break;
            default:
                Error(node, "Compiler bug, unknown binary operation.");
                break;
            }

            // Check for the read-only constraint.
            Variable variableSlot = left.GetNodeValue() as Variable;
            if(variableSlot != null && variableSlot.IsReadOnly())
                CheckReadOnlyConstraint(node, variableSlot);

            return node;
        }

        public override AstNode Visit (PrefixOperation node)
        {
            // Get the variable.
            Expression variableExpr = node.GetVariable();

            // Visit the variable.
            variableExpr.Accept(this);

            // Check the types.
            IChelaType variableType = variableExpr.GetNodeType();

            // The variable must be a reference.
            if(!variableType.IsReference())
                Error(node, "trying to set something that isn't a reference.");

            // Check the operation type.
            IChelaType coercionType = DeReferenceType(variableType);
            if(coercionType.IsConstant())
                Error(node, "cannot modify constants.");

            // TODO: Add operator overloading.
            if(!coercionType.IsInteger() && !coercionType.IsFloat() && !coercionType.IsPointer())
                Error(node, "increment/decrement operator expected an integer/float/pointer variable.");

            // Set the node types.
            node.SetCoercionType(coercionType);
            node.SetNodeType(variableExpr.GetNodeType());

            return node;
        }

        public override AstNode Visit (PostfixOperation node)
        {
            // Get the variable.
            Expression variableExpr = node.GetVariable();

            // Visit the variable.
            variableExpr.Accept(this);

            // Check the types.
            IChelaType variableType = variableExpr.GetNodeType();

            // The variable must be a reference.
            if(!variableType.IsReference())
                Error(node, "trying to set something that isn't a reference.");

            // Check the operation type.
            IChelaType coercionType = DeReferenceType(variableType);
            if(coercionType.IsConstant())
                Error(node, "cannot modify constants.");

            // TODO: Add operator overloading.
            if(!coercionType.IsInteger() && !coercionType.IsFloat() && !coercionType.IsPointer())
                Error(node, "increment/decrement operator expected an integer/float/pointer variable.");

            // Set the node types.
            node.SetCoercionType(coercionType);
            node.SetNodeType(coercionType);

            return node;
        }

        public override AstNode Visit (TernaryOperation node)
        {
            // Get the expressions.
            Expression cond = node.GetCondExpression();
            Expression left = node.GetLeftExpression();
            Expression right = node.GetRightExpression();

            // Visit the expressions.
            cond.Accept(this);
            left.Accept(this);
            right.Accept(this);

            // Check the types.
            IChelaType condType = cond.GetNodeType();
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();

            // The condition must be bool.
            if(condType != ChelaType.GetBoolType() &&
               Coerce(condType, ChelaType.GetBoolType()) != ChelaType.GetBoolType())
                Error(node, "ternary operator condition must be a boolean expression.");

            // Perform coercion.
            IChelaType destType = Coerce(leftType, rightType);

            // Failed to perform ternary coercion.
            if(destType == null)
                Error(node, "failed to perform ternary operation.");

            // Set the node coercion type.
            node.SetCoercionType(destType);
            node.SetNodeType(destType);

            return node;
        }

        private IChelaType GenericCoercion(AstNode where, IChelaType argType, IChelaType argExpType,
                                           GenericPrototype genProto, IChelaType[] genericArgumentsSoFar,
                                           out int argCoercions)
        {
            argCoercions = 0;
            return null;
        }
        
        private Function PickFunction(AstNode node, FunctionGroup fgroup, bool isStatic,
                                      FunctionGroupSelector selector, List<object> arguments, bool forgive)
        {
            // Count the number of arguments.
            int startIndex = isStatic ? 0 : 1;
            int numargs = startIndex + arguments.Count;

            // Store the generic arguments.
            IChelaType[] genericArguments = null;
            if(selector != null)
                genericArguments = selector.GenericParameters;
            
            // Select a function in the group.
            List<Function> bestMatches = new List<Function> ();
            int bestCoercions = -1;
            int numfunctions = fgroup.GetFunctionCount();
            bool nonKernelContext = !currentContainer.IsKernel();
            foreach(FunctionGroupName groupName in fgroup.GetFunctions())
            {
                // Only use the same static-ness.
                if(groupName.IsStatic() != isStatic)
                    continue;
                
                // Get the candidate prototype.
                Function function = groupName.GetFunction();
                FunctionType prototype = groupName.GetFunctionType();

                // Use the kernel entry point when it exists, and calling a kernel from a non-kernel context.
                if(nonKernelContext && function.IsKernel() && function.EntryPoint != null)
                {
                    function = function.EntryPoint;
                    prototype = function.GetFunctionType();
                }

                // Check for generics.
                IChelaType[] genericArgumentsMatched = null;
                GenericPrototype genProto = null;
                if(function != null)
                {
                    genProto = function.GetGenericPrototype();
                    if(genProto.GetPlaceHolderCount() > 0 && genericArguments != null)
                    {
                        // Check if the generic function is matched.
                        if(!CheckGenericArguments(fgroup.GetFunctionCount() == 1 ? node : null,
                                                  genProto, genericArguments))
                            continue;

                        // Matched function, store the arguments and substitute in the prototype
                        genericArgumentsMatched = genericArguments;
                        function = function.InstanceGeneric(new GenericInstance(genProto, genericArguments), currentModule);
                        prototype = function.GetFunctionType();
                    }
                    else if(genProto.GetPlaceHolderCount() > 0)
                    {
                        // Try replacing arguments.
                        genericArgumentsMatched = new IChelaType[genProto.GetPlaceHolderCount()];
                    }
                }

                // TODO: Add variable arguments.
                if(numargs != prototype.GetArgumentCount())
                    continue;

                // Check the arguments.
                int index = startIndex;
                bool match = true;
                int coercions = 0;
                for(int i = 0; i < arguments.Count; ++i)
                {
                    // Get the argument type.
                    object argObject = arguments[i];
                    object argValue = null;
                    AstNode arg = null;
                    IChelaType argExprType = null;
                    if(argObject is IChelaType)
                    {
                        argExprType = (IChelaType)argObject;
                    }
                    else
                    {
                        arg = (AstNode)argObject;
                        argExprType = arg.GetNodeType();
                        argValue = arg.GetNodeValue();
                    }

                    // Get the argument type.
                    IChelaType argType = prototype.GetArgument(index);

                    // Check for reference value.
                    Variable argValueVar = argValue as Variable;
                    bool refValue = argValueVar != null && argValueVar.IsTemporalReferencedSlot();

                    // Reference argument requires exact match.
                    bool cantMatch = false;
                    bool refArg = false;
                    if(argType.IsReference())
                    {
                        IChelaType referencedArg = DeReferenceType(argType);

                        // Check for reference argument.
                        if(!referencedArg.IsPassedByReference() || referencedArg.IsReference())
                        {
                            refArg = true;
                            if(!argExprType.IsReference() || argValueVar == null || !refValue)
                            {
                                cantMatch = true;
                            }
                            else if(argType != argExprType)
                            {
                                // Extract the expression base type.
                                IChelaType exprValueType = DeReferenceType(argExprType);
                                exprValueType = DeConstType(exprValueType);

                                // Check for some type equivalences.
                                IChelaType argValueType = referencedArg;

                                // Check for out-ref differences.
                                if(exprValueType == argValueType)
                                {
                                    // Out-ref difference, so don't match.
                                    cantMatch = true;
                                }
                                else
                                {
                                    // Check primitive-associated pairs.
                                    if(!exprValueType.IsFirstClass() ||
                                        argValueType != currentModule.GetAssociatedClass(exprValueType))
                                    {
                                        if(!argValueType.IsFirstClass() ||
                                            exprValueType != currentModule.GetAssociatedClass(argValueType))
                                            cantMatch = true;
                                    }
                                }
                            }
                        }
                        else if(refValue)
                        {
                            // Ignore not references candidates when the argument is a reference.
                            cantMatch = true;
                        }
                    }
                    else if(refValue)
                    {
                        // Ignore not references candidates when the argument is a reference.
                        cantMatch = true;
                    }

                    // Check coercion.
                    if(argExprType != argType || cantMatch)
                    {
                        int argCoercions = 0;
                        IChelaType coercedArgument = null;

                        // Perform coercion.
                        if(!cantMatch)
                        {
                            if(argType.IsGenericType() && !argType.IsGenericImplicit() && genericArguments == null)
                            {
                                // Perform generic coercion.
                                coercedArgument = GenericCoercion(arg, argType, argExprType,
                                    genProto, genericArgumentsMatched, out argCoercions);
                            }
                            else
                            {
                                // Perform ordinary coercion.
                                if(refArg)
                                    coercedArgument = RefCoerceCounted(arg, argType, argExprType,
                                        argValue, out argCoercions);
                                else
                                    coercedArgument = CoerceCounted(arg, argType, argExprType,
                                        argValue, out argCoercions);
                            }
                        }

                        // Check for coercion success.
                        if(cantMatch || coercedArgument != argType)
                        {
                            // Print early error message.
                            if(numfunctions == 1 && !forgive)
                                Error(node, "cannot implicitly cast argument {0} from {1} to {2}.{3}",
                                    i+1, argExprType.GetDisplayName(), argType.GetDisplayName(), cantMatch ? " Must use ref/out operator." : null);

                            match = false;
                            break;
                        }
                        else
                            coercions += argCoercions;
                    }
                    
                    // Check the next argument.
                    index++;
                }
                
                // Ignore not matching candidates.
                if(!match)
                    continue;

                // Has at least one match.
                if(bestMatches.Count == 0)
                {
                    bestMatches.Add(function);
                    bestCoercions = coercions;
                    continue;
                }
                
                // Prefer the candidates with less coercions.
                if(coercions < bestCoercions || bestCoercions < 0)
                {
                    bestMatches.Clear();
                    bestMatches.Add(function);
                    bestCoercions = coercions;
                }
                else if(coercions == bestCoercions)
                {
                    bestMatches.Add(function);
                }
            }
            
            // Only one function can be the match.
            if(bestMatches.Count == 0 && !forgive)
            {
                string error = "cannot find a matching function for '" + fgroup.GetName() + "'";
                if(fgroup.GetFunctionCount() > 1)
                    error += "\ncandidates are:";
                else
                    error += " the option is:";

                foreach(FunctionGroupName gname in fgroup.GetFunctions())
                {
                    Function function = gname.GetFunction();
                    FunctionType functionType = function.GetFunctionType();
                    if(nonKernelContext && function.IsKernel() && function.EntryPoint != null)
                        functionType = function.EntryPoint.GetFunctionType();
                    error += "\n\t" + functionType.GetDisplayName();
                }

                Error(node, error);
            }
            else if(bestMatches.Count > 1)
            {
                string error = "ambiguous matches for '" + fgroup.GetName() + "'\n"
                                + "candidates are:";
                foreach(Function candidate in bestMatches)
                {
                    error += "\n\t" + candidate.GetFunctionType().GetName();
                }
                Error(node, error);                    
            }

            // Get the selected function.
            Function matched = null;
            if(bestMatches.Count != 0)
            {
                matched = (Function)bestMatches[0];

                // Prevent ambiguity.
                matched.CheckAmbiguity(node.GetPosition());

                // Perform coercion again, required by delegates.
                FunctionType prototype = matched.GetFunctionType();
                int index = startIndex;
                for(int i = 0; i < arguments.Count; ++i)
                {
                    // Get the argument type.
                    object argObject = arguments[i];
                    if(argObject is IChelaType)
                        continue;

                    // Read the argument data.
                    AstNode arg = (AstNode)argObject;
                    IChelaType argExprType = arg.GetNodeType();
                    object argValue = arg.GetNodeValue();

                    // Perform the coercion again.
                    IChelaType argType = prototype.GetArgument(index);
                    if(argExprType != argType)
                    {
                        int argCoercions = 0;
                        CoerceCounted(arg, argType, argExprType, argValue, out argCoercions);
                    }

                    // Coerce the next argument.
                    index++;
                }
            }
            
            // Return the matched function.
            return matched;
        }

        private Function PickFunction(AstNode node, FunctionGroup fgroup, bool isStatic,
                                      FunctionGroupSelector selector, Expression arguments, bool forgive)
        {
            // Store the arguments in a vector.
            AstNode arg = arguments;
            List<object> argList = new List<object> ();
            while(arg != null)
            {
                argList.Add(arg);
                arg = arg.GetNext();
            }

            return PickFunction(node, fgroup, isStatic, selector, argList, forgive);
        }

        private Function PickFunction(AstNode node, FunctionGroup fgroup, FunctionGroupType groupType,
                                      FunctionGroupSelector selector, Expression arguments)
        {
            if(groupType == ChelaType.GetAnyFunctionGroupType())
            {
                Function ret = PickFunction(node, fgroup, false, selector, arguments, true);
                if(ret != null)
                    return ret;
                return PickFunction(node, fgroup, true, selector, arguments, false);
            }
            else if(groupType == ChelaType.GetStaticFunctionGroupType())
            {
                return PickFunction(node, fgroup, true, selector, arguments, false);
            }
            else
            {
                return PickFunction(node, fgroup, false, selector, arguments, false);
            }
        }

        private Function PickFunction(AstNode node, FunctionGroup fgroup, FunctionGroupType groupType,
                                      FunctionGroupSelector selector, List<object> arguments, bool forgive)
        {
            if(groupType == ChelaType.GetAnyFunctionGroupType())
            {
                Function ret = PickFunction(node, fgroup, false, selector, arguments, true);
                if(ret != null)
                    return ret;
                return PickFunction(node, fgroup, true, selector, arguments, false || forgive);
            }
            else if(groupType == ChelaType.GetStaticFunctionGroupType())
            {
                return PickFunction(node, fgroup, true, selector, arguments, false || forgive);
            }
            else
            {
                return PickFunction(node, fgroup, false, selector, arguments, false || forgive);
            }
        }

        private Function PickFunction(AstNode node, FunctionGroup fgroup, FunctionGroupType groupType,
                                      FunctionGroupSelector selector, List<object> arguments)
        {
            return PickFunction(node, fgroup, groupType, selector, arguments, false);
        }

        private Function PickFunction(AstNode node, FunctionGroup fgroup, FunctionGroupType groupType,
                                      List<object> arguments)
        {
            return PickFunction(node, fgroup, groupType, null, arguments, false);
        }

        private Function ProcessCall(Expression node, Expression functionExpr, Expression arguments)
        {
            // Visit the functional expression.
            functionExpr.Accept(this);
            
            // Visit the function arguments.
            VisitList(arguments);
            
            // Get the function group.
            IChelaType functionType = functionExpr.GetNodeType();
            if(functionType.IsMetaType())
            {
                functionType = ExtractActualType(node, functionType);
                if(!functionType.IsStructure() && !functionType.IsVector() && !functionType.IsMatrix())
                    Error(node, "expected function, not a type.");
            }

            // Special treatment for structure construction.
            if(functionType.IsStructure())
            {
                // Get the building.
                Structure building = (Structure)functionType;

                // Select the constructor.
                FunctionGroup ctorGroup = building.GetConstructor();
                Function ctor = PickFunction(node, ctorGroup, ChelaType.GetFunctionGroupType(), null, arguments);

                // Set the node and coercion types.
                node.SetCoercionType(ctor.GetFunctionType());
                node.SetNodeType(building);

                // Return the constructor.
                return ctor;
            }
            else if(functionType.IsVector() || functionType.IsPrimitive() || functionType.IsMatrix())
            {
                // Ignore vector/primitive construction.
                return null;
            }
            
            FunctionType prototype = null;
            Function functionObject = null;
            bool isStatic = false;
            if(functionType.IsFunctionGroup())
            {
                // Get the selector
                FunctionGroupSelector selector = (FunctionGroupSelector)functionExpr.GetNodeValue();
                
                // Get the group
                FunctionGroupType groupType = (FunctionGroupType) functionType;
                FunctionGroup fgroup = selector.GetFunctionGroup();                

                // Pick the function.
                functionObject = PickFunction(node, fgroup, groupType, selector, arguments);
                prototype = functionObject.GetFunctionType();
                
                // Notify the selector.
                selector.Select(functionObject);
            }
            else if(functionType.IsFunction())
            {
                prototype = (FunctionType)functionType;
                functionObject = (Function)functionExpr.GetNodeValue();
                
                if(functionObject != null)
                    isStatic = functionObject.IsStatic();
                
                // Check the arguments.
                AstNode arg = arguments;
                int index = isStatic ? 0 : 1;
                while(arg != null)
                {
                    if(index == prototype.GetArgumentCount())
                        Error(node, "more arguments than expected.");

                    // Get the argument type.
                    IChelaType argExprType = arg.GetNodeType();
                    IChelaType argType = prototype.GetArgument(index);
                    if(argExprType != argType &&
                       Coerce(arg, argType, argExprType, arg.GetNodeValue()) != argType)
                        Error(arg, "incompatible argument type from '{0}' to '{1}'.", argExprType, argType);
                    
                    // Check the next argument.
                    arg = arg.GetNext();
                    index++;
                }
                
                // Check number of arguments.
                if(index != prototype.GetArgumentCount())
                    Error(node, "fewer argument than expected.");
            }
            else if(functionType.IsReference() || functionType.IsPointer())
            {
                if(functionType.IsReference())
                    functionType = DeReferenceType(functionType);
                if(functionType.IsReference())
                    functionType = DeReferenceType(functionType);

                // Check function pointers.
                int startIndex = 0;
                if(functionType.IsPointer())
                {
                    // Make sure its a function pointer.
                    IChelaType pointed = DePointerType(functionType);
                    if(!pointed.IsFunction())
                        Error(node, "cannot invoke a pointer.");

                    // Store the prototype.
                    prototype = (FunctionType)pointed;
                    functionObject = null;
                }
                else if(functionType.IsClass())
                {
                    // Make sure its a delegate.
                    Class functionClass = (Class)functionType;
                    if(!functionClass.IsDerivedFrom(currentModule.GetDelegateClass()))
                        Error(node, "cannot call no delegate object.");

                    // Get the invoke method.
                    FunctionGroup invokeGroup = (FunctionGroup)functionClass.FindMemberRecursive("Invoke");
                    if(invokeGroup == null)
                        Error(node, "cannot call invalid delegate.");

                    // Use the first no static group name in the group.
                    Method invokeMethod = null;
                    foreach(FunctionGroupName gn in invokeGroup.GetFunctions())
                        if(!gn.IsStatic())
                            invokeMethod = (Method)gn.GetFunction();
                    if(invokeMethod == null)
                        Error(node, "cannot call invalid delegate.");

                    // Use the invoke method.
                    functionObject = invokeMethod;
                    prototype = invokeMethod.GetFunctionType();

                    // Ignore the this argument.
                    startIndex = 1;
                }
                else
                {
                    Error(node, "cannot call object.");
                }

                // Check the arguments.
                AstNode arg = arguments;
                int index = startIndex;
                while(arg != null)
                {
                    if(index == prototype.GetArgumentCount())
                        Error(node, "more arguments than expected.");

                    // Get the argument type.
                    IChelaType argExprType = arg.GetNodeType();
                    IChelaType argType = prototype.GetArgument(index);
                    if(argExprType != argType &&
                       Coerce(arg, argType, argExprType, arg.GetNodeValue()) != argType)
                        Error(arg, "incompatible argument type from " + argExprType + " to " + argType + ".");
                    
                    // Check the next argument.
                    arg = arg.GetNext();
                    index++;
                }
                
                // Check number of arguments.
                if(index != prototype.GetArgumentCount())
                    Error(node, "fewer argument than expected.");
            }
            else
            {
                Error(node, "cannot call something that isn't a function.");
            }
            
            // Set the coercion type as the prototype.
            node.SetCoercionType(prototype);
            
            // Set the node type.
            node.SetNodeType(prototype.GetReturnType());
            
            // Return the final function.
            return functionObject;
        }
        
        public override AstNode Visit (CallExpression node)
        {
            Function function = ProcessCall(node, node.GetFunction(), node.GetArguments());

            // Get the function type.
            IChelaType functionType = node.GetFunction().GetNodeType();

            // Perform additional checks when the function is known.
            if(function != null)
            {
                // Set the selected function.
                node.SelectFunction(function);
                
                // Set the presence of a 'this' reference.
                node.SetImplicitArgument(!function.IsStatic());

                // Additional checks for methods.
                if(function.IsMethod())
                {
                    Method method = (Method)function;
                    
                    // Get the virtual slot.
                    node.SetVSlot(method.GetVSlot());
                }                        
            }
            else if(functionType.IsMetaType())
            {
                // This can be a primitive cast, or a vector construction.
                IChelaType targetType = ExtractActualType(node.GetFunction(), functionType);
                if(targetType.IsPrimitive())
                {
                    // Primitive cast.
                    Error(node, "unimplemented primitive construction form.");
                }
                else if(targetType.IsVector() || targetType.IsMatrix())
                {
                    // Vector/Matrix construction.
                    IChelaType coercionType = null;
                    node.SetNodeType(targetType);

                    // Extract the number of components.
                    int typeComps = 0;
                    if(targetType.IsVector())
                    {
                        VectorType vectorType = (VectorType)targetType;
                        coercionType = vectorType.GetPrimitiveType();
                        typeComps = vectorType.GetNumComponents();
                    }
                    else
                    {
                        MatrixType matrixType = (MatrixType)targetType;
                        coercionType = matrixType.GetPrimitiveType();
                        typeComps = matrixType.GetNumColumns() * matrixType.GetNumRows();

                    }
                    node.SetCoercionType(coercionType);

                    node.SetVectorConstruction(true);

                    // Perform argument coercion.
                    int numcomponents = 0;
                    AstNode arg = node.GetArguments();
                    while(arg != null)
                    {
                        IChelaType argType = arg.GetNodeType();
                        if(argType != coercionType &&
                           Coerce(coercionType, argType) != coercionType)
                        {
                            // Deref and deconst the argument.
                            if(argType.IsReference())
                               argType = DeReferenceType(argType);
                            if(argType.IsConstant())
                                argType = DeConstType(argType);

                            // Check if its a vector
                            if(!argType.IsVector())
                                Error(arg, "invalid vector initialization argument: " + argType + " -> " + coercionType);

                            // Check the component compatibility.
                            VectorType argVector = (VectorType)argType;
                            IChelaType argComponent = argVector.GetPrimitiveType();
                            if(argComponent != coercionType &&
                               Coerce(coercionType, argComponent) != coercionType)
                                Error(arg, "incompatible vector components types: " +  argComponent + " -> " + coercionType);

                            // Add the components.
                            numcomponents += argVector.GetNumComponents();
                        }
                        else
                        {
                            // Add one component.
                            ++numcomponents;
                        }

                        // Check the next argument.
                        arg = arg.GetNext();
                    }

                    // Check the number of components is correct.
                    if(numcomponents != typeComps)
                        Error(node, "the number of components is incorrect.");
                }
            }

            return node;
        }

        public override AstNode Visit (ConstructorInitializer node)
        {
            // Visit the function arguments.
            VisitList(node.GetArguments());

            // Pick the function
            Function function = PickFunction(node, node.GetConstructorGroup(), ChelaType.GetFunctionGroupType(), null, (Expression)node.GetArguments());
            node.SetConstructor(function);

            return node;
        }
        
        public override AstNode Visit (NewExpression node)
        {
            // Visit the type expression.
            Expression typeExpression = node.GetTypeExpression();
            typeExpression.SetHints(Expression.TypeHint);
            typeExpression.Accept(this);
            
            // Visit the arguments.
            VisitList(node.GetArguments());
            
            // Get the type.
            IChelaType objectType = typeExpression.GetNodeType();
            objectType = ExtractActualType(typeExpression, objectType);
            node.SetObjectType(objectType);

            // Cannot create abstract objects.
            if(objectType.IsAbstract())
                Error(node, "cannot create abstract objects.");

            // Cannot create reference objects.
            if(objectType.IsReference())
                Error(node, "cannot create references, only object/structures.");
            
            // Create the different objects.
            Structure building = null;
            if(objectType.IsPrimitive() || objectType.IsPointer())
            {
                Error(node, "cannot create primitives with new");
            }
            else if(objectType.IsStructure())
            {
                building = (Structure)objectType;
            }
            else if(objectType.IsClass())
            {
                building = (Structure)objectType;
            }
            else if(objectType.IsInterface())
            {
                Error(node, "cannot instance interfaces.");
            }
            else
                Error(node, "cannot create object of type {0} with new.", objectType);

            // Use the implicit instance.
            if(building != null)
            {
                if(building.IsGeneric() && building.IsGenericImplicit())
                    building = building.GetSelfInstance();

                // Set the node type.
                if(building.IsPassedByReference())
                    node.SetNodeType(ReferenceType.Create(building));
                else
                    node.SetNodeType(building);
            }

            // Get the constructor group.
            FunctionGroup constructorGroup = building.GetConstructor();
            if(constructorGroup == null)
                Error(node, "default constructors unimplemented.");
            
            // Pick the constructor.
            Method constructor = (Method)PickFunction(node, constructorGroup, false, null, node.GetArguments(), false);
            node.SetConstructor(constructor);

            return node;
        }

        public override AstNode Visit (NewRawExpression node)
        {
            // Don't use under safe contexts.
            UnsafeError(node, "cannot use array {0} under safe contexts.",
                    node.IsHeapAlloc() ? "heapalloc" : "stackalloc");

            // Visit the type expression.
            Expression typeExpression = node.GetTypeExpression();
            typeExpression.SetHints(Expression.TypeHint);
            typeExpression.Accept(this);
            
            // Visit the arguments.
            VisitList(node.GetArguments());
            
            // Get the type.
            IChelaType objectType = typeExpression.GetNodeType();
            objectType = ExtractActualType(typeExpression, objectType);
            node.SetObjectType(objectType);

            // Cannot create abstract objects.
            if(objectType.IsAbstract())
                Error(node, "cannot create abstract objects.");

            // Cannot create reference objects.
            if(objectType.IsReference())
                Error(node, "cannot create references, only primitive/pointer/object.");
            
            // Create the different objects.
            Structure building = null;
            if(objectType.IsPrimitive() || objectType.IsPointer())
            {
                if(node.GetArguments() != null)
                    Error(node, "primitives/pointers hasn't constructors.");
                
                // Set the node type and return.
                node.SetNodeType(PointerType.Create(objectType));
                return node;
            }
            else if(objectType.IsStructure())
            {
                building = (Structure)objectType;
                node.SetNodeType(PointerType.Create(objectType));
            }
            else if(objectType.IsClass())
            {
                building = (Structure)objectType;
                node.SetNodeType(ReferenceType.Create(objectType));
            }
            
            // Get the constructor group.
            FunctionGroup constructorGroup = building.GetConstructor();
            if(constructorGroup == null)
                Error(node, "default constructors unimplemented.");
            
            // Pick the constructor.
            Method constructor = (Method)PickFunction(node, constructorGroup, false, null, node.GetArguments(), false);
            node.SetConstructor(constructor);

            return node;
        }

        public override AstNode Visit (NewRawArrayExpression node)
        {
            // Don't use under safe contexts.
            UnsafeError(node, "cannot use array {0} under safe contexts.",
                    node.IsHeapAlloc() ? "heapalloc" : "stackalloc");

            // Visit the type expression.
            Expression typeExpression = node.GetTypeExpression();
            typeExpression.SetHints(Expression.TypeHint);
            typeExpression.Accept(this);

            // Visit the size expression.
            Expression sizeExpression = node.GetSize();
            sizeExpression.Accept(this);

            // Get the type.
            IChelaType objectType = typeExpression.GetNodeType();
            objectType = ExtractActualType(typeExpression, objectType);

            // Check the object type.
            if(objectType.IsConstant())
                Error(node, "cannot create array of constants.");

            // Don't allow references types.
            if(objectType.IsReference() || objectType.IsPassedByReference())
                Error(node, "cannot create raw array of references.");

            // Set the node type.
            node.SetNodeType(PointerType.Create(objectType));
            node.SetObjectType(objectType);

            // Check the size type.
            IChelaType sizeType = sizeExpression.GetNodeType();
            if(sizeType.IsReference())
                sizeType = DeReferenceType(sizeType);
            if(sizeType.IsConstant())
                sizeType = DeConstType(sizeType);
            if(!sizeType.IsInteger())
                Error(node, "array size must be integer.");
            node.SetCoercionType(sizeType);

            return node;
        }

        private void CheckArrayInitializers(AstNode where, int[] sizes, int depth, IChelaType coercionType, AstNode initializers)
        {
            // Common variables during the iteration.
            AstNode current = initializers;
            int size = sizes[depth];
            int currentIndex = 0;
            bool array = depth + 1 < sizes.Length;

            // Check each one of the array initializers.
            while(current != null)
            {
                // Don't get pass of the size.
                if(size >= 0 && currentIndex == size)
                    Error(current, "unexpected extra element in a dimension.");

                // Increase the depth if the node is an array.
                if(current is ArrayExpression)
                {
                    // Make sure an array its expected.
                    if(!array)
                        Error(current, "expected an expression, not a sub-array.");

                    // Process the sub-array.
                    ArrayExpression arrayExpr = (ArrayExpression)current;
                    CheckArrayInitializers(current, sizes, depth+1, coercionType, arrayExpr.GetElements());
                }
                else
                {
                    // Make sure an expression its expected.
                    if(array)
                        Error(current, "expected a sub-aray, not an expression.");

                    // Visit the expression.
                    current.Accept(this);

                    // Perform his coercion.
                    IChelaType currentType = current.GetNodeType();
                    if(currentType != coercionType &&
                       Coerce(current, coercionType, currentType, current.GetNodeValue()) != coercionType)
                        Error(current, "cannot perform implicit cast {0} -> {1}.",
                              currentType.GetDisplayName(), coercionType.GetDisplayName());
                }

                // Process the next expression/array.
                ++currentIndex;
                current = current.GetNext();
            }

            // Make sure the aren't fewer elements.
            if(size >= 0)
            {
                if(currentIndex < size)
                    Error(where, "fewer elements than expected.");
            }
            else
            {
                // Update the size.
                sizes[depth] = currentIndex;
            }

            // Make sure there's at least one element when depth > 0.
            if(depth > 0 && currentIndex == 0)
                Error(where, "expected at least one element.");
        }

        public override AstNode Visit (NewArrayExpression node)
        {
            // Visit the type expression.
            Expression typeExpression = node.GetTypeExpression();
            typeExpression.SetHints(Expression.TypeHint);
            typeExpression.Accept(this);

            // Get the size expression.
            Expression sizeExpression = node.GetSize();
            int dimensions = node.GetDimensions();

            // Count the dimensions-
            if(sizeExpression != null)
            {
                dimensions = 0;
                AstNode currentSize = sizeExpression;
                while(currentSize != null)
                {
                    ++dimensions;
                    currentSize = currentSize.GetNext();
                }
                node.SetDimensions(dimensions);
            }

            // Get the type.
            IChelaType objectType = typeExpression.GetNodeType();
            objectType = ExtractActualType(typeExpression, objectType);

            // Check the object type.
            if(objectType.IsConstant())
                Error(node, "cannot create array of constants.");

            // Compute the init coercion type.
            IChelaType initCoercionType = objectType;
            if(initCoercionType.IsPassedByReference())
                initCoercionType = ReferenceType.Create(initCoercionType);
            node.SetInitCoercionType(initCoercionType);

            // Create the array type.
            ArrayType arrayType = ArrayType.Create(objectType, dimensions, false);
            node.SetArrayType(arrayType);
            node.SetObjectType(objectType);
            node.SetNodeType(ReferenceType.Create(arrayType));

            // Read or guess the sizes
            int[] arraySizes = new int[dimensions];
            bool canInit = true;
            IChelaType sizeCoercionType = null;
            if(sizeExpression != null)
            {
                // Load the sizes.
                int currentSizeIndex = 0;
                while(sizeExpression != null)
                {
                    // Visit the size expression.
                    sizeExpression.Accept(this);

                    // Get the size type.
                    IChelaType sizeType = sizeExpression.GetNodeType();
                    if(sizeType.IsConstant() && canInit)
                    {
                        sizeType = DeConstType(sizeType);
                        if(sizeType.IsInteger() && sizeType.IsPrimitive())
                        {
                            // Read the size constant.
                            ConstantValue sizeConst = sizeExpression.GetNodeValue() as ConstantValue;
                            if(sizeConst != null)
                            {
                                sizeConst = sizeConst.Cast(ConstantValue.ValueType.Int);
                                arraySizes[currentSizeIndex] = sizeConst.GetIntValue();

                            }
                            else
                            {
                                // Cannot initialize here.
                                canInit = false;
                            }
                        }
                        else
                        {
                            // Cannot initialize here.
                            canInit = false;
                        }
                    }
                    else
                    {
                        // Cannot initialize here.
                        canInit = false;
                    }

                    // Coerce the size type.
                    if(sizeCoercionType == null)
                    {
                        if(sizeType.IsReference())
                            sizeType = DeReferenceType(sizeType);
                        if(sizeType.IsConstant())
                            sizeType = DeReferenceType(sizeType);
                        if(sizeType.IsStructure())
                        {
                            IChelaType assoc = currentModule.GetAssociatedClassPrimitive((Structure)sizeType);
                            if(assoc != null)
                                sizeType = assoc;
                        }
                        if(!sizeType.IsInteger() || !sizeType.IsPrimitive())
                            Error(sizeExpression, "expected a size of integer type instead of '{0}'.", sizeType);
                        sizeCoercionType = sizeType;
                    }
                    else if(sizeType != sizeCoercionType)
                    {
                        // Try to get a common coercion type.
                        IChelaType newSizeCoercion = Coerce(sizeExpression, sizeCoercionType, sizeType, sizeExpression.GetNodeValue());
                        if(!newSizeCoercion.IsInteger() || !newSizeCoercion.IsPrimitive())
                            Error(sizeExpression, "expected a size of integer type instead of '{0}'.", sizeType);
                        sizeCoercionType = newSizeCoercion;
                    }

                    // Process the next size.
                    ++currentSizeIndex;
                    sizeExpression = (Expression)sizeExpression.GetNext();
                }
            }
            else
            {
                // Guess the sizes.
                for(int i = 0; i < dimensions; ++i)
                    arraySizes[i] = -1;
            }

            // Use a coercion type.
            if(sizeCoercionType == null) // For implicit sizes.
                sizeCoercionType = ChelaType.GetIntType();
            node.SetCoercionType(sizeCoercionType);

            // Check the initializer.
            AstNode init = node.GetInitializers();
            if(init != null)
            {
                if(!canInit)
                    Error(node, "cannot have variable size arrays with initializers.");

                CheckArrayInitializers(init, arraySizes, 0, initCoercionType, init);
            }

            // Create implicit size expressions.
            sizeExpression = node.GetSize();
            if(sizeExpression == null)
            {
                Expression lastSize = null;
                for(int i = 0; i < dimensions; ++i)
                {
                    Expression nextSize = new IntegerConstant(arraySizes[i], node.GetPosition());
                    if(i == 0)
                        node.SetSize(nextSize);
                    else
                        lastSize.SetNext(nextSize);
                    lastSize = nextSize;
                }
            }

            return node;
        }
        
        public override AstNode Visit (DeleteStatement node)
        {
            // Get the pointer.
            Expression pointer = node.GetPointer();
            pointer.Accept(this);
            
            // Get the pointer type.
            IChelaType pointerType = pointer.GetNodeType();
            
            // Dereference the expression.
            if(pointerType.IsReference())
            {
                ReferenceType refType = (ReferenceType)pointerType;
                pointerType = refType.GetReferencedType();
            }
            
            // The expression must be a pointer expression.
            if(!pointerType.IsPointer())
                Error(node, "cannot delete something without a pointer.");
            
            // Set the coercion type.
            node.SetCoercionType(pointerType);

            return node;
        }

        public override AstNode Visit (DeleteRawArrayStatement node)
        {
            // Get the pointer.
            Expression pointer = node.GetPointer();
            pointer.Accept(this);
            
            // Get the pointer type.
            IChelaType pointerType = pointer.GetNodeType();
            
            // Dereference the expression.
            if(pointerType.IsReference())
            {
                ReferenceType refType = (ReferenceType)pointerType;
                pointerType = refType.GetReferencedType();
            }
            
            // The expression must be a pointer expression.
            if(!pointerType.IsPointer())
                Error(node, "cannot delete something without a pointer.");
            
            // Set the coercion type.
            node.SetCoercionType(pointerType);

            return node;
        }

        public override AstNode Visit (TryStatement node)
        {
            // Read the substatements.
            AstNode tryNode = node.GetTryStatement();
            AstNode catchList = node.GetCatchList();
            AstNode finalNode = node.GetFinallyStatement();

            // Create the exception context.
            ExceptionContext context;
            if(currentExceptionContext != null)
                context = new ExceptionContext(currentExceptionContext);
            else
                context = new ExceptionContext(currentFunction);
            node.SetContext(context);

            // Store the old context and process the try statement.
            ExceptionContext oldContext = currentExceptionContext;
            currentExceptionContext = context;
            tryNode.Accept(this);

            // Restore the context, visit the catch list.
            currentExceptionContext = oldContext;
            AstNode catchNode = catchList;
            while(catchNode != null)
            {
                // Set the exception context.
                CatchStatement catchStmnt = (CatchStatement)catchNode;
                catchStmnt.SetContext(context);

                // Visit it.
                catchStmnt.Accept(this);

                // TODO: Avoid "duplicated" catches.
                catchNode = catchNode.GetNext();
            }

            // Visit the finally statement
            if(finalNode != null)
            {
                // Set the exception context.
                FinallyStatement finalStmnt = (FinallyStatement)finalNode;
                finalStmnt.SetContext(context);

                // Visit it.
                finalStmnt.Accept(this);
            }

            return node;
        }

        public override AstNode Visit (CatchStatement node)
        {
            // Get the type expression.
            Expression typeExpression = node.GetExceptionType();

            // Visit the type node.
            typeExpression.Accept(this);

            // Get the type of the type expression.
            IChelaType type = typeExpression.GetNodeType();
            IChelaType objectType = ExtractActualType(typeExpression, type);

            // Only accept classes.
            if(!objectType.IsClass())
                Error(node, "only class instances can be exceptions.");

            // Set the node type.
            node.SetNodeType(objectType);

            // Create the catch lexical scope.
            LexicalScope scope = CreateLexicalScope(node);
            node.SetScope(scope);

            // Create the exception variable.
            string exceptName = node.GetName();
            if(exceptName != null && exceptName != "")
            {
                LocalVariable exceptLocal = new LocalVariable(exceptName, scope,
                                                              ReferenceType.Create(objectType));
                node.SetVariable(exceptLocal);
            }

            // Enter into the catch scope.
            PushScope(scope);

            // Visit the children.
            node.GetChildren().Accept(this);

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (FinallyStatement node)
        {
            // Visit the children.
            VisitList(node.GetChildren());

            return node;
        }

        public override AstNode Visit (ThrowStatement node)
        {
            // Get the exception.
            Expression exception = node.GetException();
            exception.Accept(this);

            // Get the exception type.
            IChelaType exceptionType = exception.GetNodeType();
            ReferenceType refType;

            // Dereference the exception.
            if(exceptionType.IsReference())
            {
                refType = (ReferenceType)exceptionType;
                IChelaType referencedType = refType.GetReferencedType();
                if(referencedType.IsReference())
                    exceptionType = referencedType;
            }

            // The expression must be an object reference.
            if(!exceptionType.IsReference())
                Error(node, "cannot throw something that isn't a reference.");

            // Get the object type.
            refType = (ReferenceType)exceptionType;
            IChelaType objectType = refType.GetReferencedType();
            if(!objectType.IsClass())
                Error(node, "cannot throw something that isn't a class instance.");

            // Set the coercion type.
            node.SetCoercionType(exceptionType);

            // Remove dead code.
            AstNode dead = node.GetNext();
            if(dead != null)
            {
                Warning(dead, "detected unreachable code.");
                VisitList(dead);
                node.SetNext(null);
            }

            return node;
        }

        public override AstNode Visit (SizeOfExpression node)
        {
            // Visit the type expression.
            Expression typeExpr = node.GetTypeExpression();
            typeExpr.Accept(this);

            // Select the type to use.
            IChelaType theType = typeExpr.GetNodeType();
            if(theType.IsMetaType())
                theType = ExtractActualType(typeExpr, theType);

            if(theType.IsReference() || theType.IsPassedByReference())
                Error(node, "cannot use sizeof with reference types.");

            // Store the type as the coercion one.
            node.SetCoercionType(theType);

            // Select if the type is constant or not.
            if(theType.IsPointer() || theType == ChelaType.GetSizeType() ||
               theType == ChelaType.GetPtrDiffType())
                node.SetNodeType(ChelaType.GetIntType());
            else
                node.SetNodeType(ConstantType.Create(ChelaType.GetIntType()));

            return node;
        }

        public override AstNode Visit (TypeOfExpression node)
        {
            // Visit the type expression.
            Expression typeExpr = node.GetTypeExpression();
            typeExpr.Accept(this);

            // Select the type to use.
            IChelaType theType = typeExpr.GetNodeType();
            theType = ExtractActualType(typeExpr, theType);

            if(theType.IsReference())
                Error(node, "cannot use sizeof with reference types.");

            // Store the type as the coercion one.
            node.SetCoercionType(theType);

            // Set the node type.
            node.SetNodeType(ReferenceType.Create(currentModule.GetTypeClass()));

            return node;
        }

        public override AstNode Visit (DefaultExpression node)
        {
            // Visit the type expression.
            Expression typeExpr = node.GetTypeExpression();
            typeExpr.Accept(this);

            // Select the type to use.
            IChelaType theType = typeExpr.GetNodeType();
            theType = ExtractActualType(typeExpr, theType);

            // If the type is passed by references, use a reference.
            if(theType.IsPassedByReference())
                theType = ReferenceType.Create(theType);

            // Store the type as the coercion and node type.
            node.SetCoercionType(theType);
            node.SetNodeType(theType);

            return node;
        }

        public override AstNode Visit (CastOperation node)
        {
            // Get the target type and value.
            Expression target = node.GetTarget();
            Expression value = node.GetValue();

            // Set the type hint to the target.
            target.SetHints(Expression.TypeHint);
            
            // Visit them.
            target.Accept(this);
            value.Accept(this);
            
            // Get the target type.
            IChelaType targetType = target.GetNodeType();
            targetType = ExtractActualType(target, targetType);
            if(targetType.IsPassedByReference())
                targetType = ReferenceType.Create(targetType);

            // Perform constant result check.
            IChelaType valueType = value.GetNodeType();
            if(valueType.IsConstant() &&
               targetType.IsPrimitive())
            {
                valueType = DeConstType(valueType);
                if(valueType.IsPrimitive())
                    targetType = ConstantType.Create(targetType);
            }
            
            // TODO: Check if it is possible to perform the cast.
            
            // Set the node type.
            node.SetNodeType(targetType);

            return node;
        }

        public override AstNode Visit (ReinterpretCast node)
        {
            // Get the target type and value.
            Expression target = node.GetTargetType();
            Expression value = node.GetValue();

            // Set the type hint to the target.
            target.SetHints(Expression.TypeHint);
            
            // Visit them.
            target.Accept(this);
            value.Accept(this);
            
            // Get the target type.
            IChelaType targetType = target.GetNodeType();
            targetType = ExtractActualType(target, targetType);
            if(targetType.IsPassedByReference())
                targetType = ReferenceType.Create(targetType);

            // Check the source type.
            IChelaType valueType = value.GetNodeType();
            if(valueType.IsReference())
            {
                IChelaType refType = DeReferenceType(valueType);
                if(!refType.IsPassedByReference())
                {
                    // Don't load temporary referenced slots.
                    Variable variable = value.GetNodeValue() as Variable;
                    if(variable == null || !variable.IsTemporalReferencedSlot())
                        valueType = refType;
                }
            }
            node.SetCoercionType(valueType);

            // The target and the source type size must be of the same size.
            bool targetPointer = targetType.IsReference() || targetType.IsPointer() ||
                    targetType == ChelaType.GetSizeType() || targetType == ChelaType.GetPtrDiffType();
            bool sourcePointer = valueType.IsReference() || valueType.IsPointer() ||
                    valueType == ChelaType.GetSizeType() || valueType == ChelaType.GetPtrDiffType();

            // Pointer types must match.
            if(sourcePointer != targetPointer)
                Error(node, "Cannot reinterpret_cast between pointer and non-pointer compatible types.");

            // The type sizes must be the same.
            if(!targetPointer && targetType.GetSize() != valueType.GetSize())
                Error(node, "Cannot perform reinterpret_cast with types of differente sizes:\n{0}->{1}",
                    valueType.GetFullName(), targetType.GetFullName());
            
            // Set the node type.
            node.SetNodeType(targetType);

            return node;
        }

        public override AstNode Visit (AsExpression node)
        {
            // Get the target type and value.
            Expression target = node.GetTarget();
            Expression value = node.GetValue();

            // Visit them.
            target.Accept(this);
            value.Accept(this);

            // The value must be a reference.
            IChelaType valueType = value.GetNodeType();
            node.SetCoercionType(valueType);
            if(!valueType.IsReference())
                Error(node, "expected reference value.");

            // De-ref the value.
            valueType = DeReferenceType(valueType);
            if(valueType.IsReference())
                node.SetCoercionType(valueType);

            // De-ref again.
            valueType = DeReferenceType(valueType);
            if(!valueType.IsPassedByReference())
                Error(node, "expeceted a reference value.");

            // Get the target type.
            IChelaType targetType = target.GetNodeType();
            targetType = ExtractActualType(target, targetType);
            if(!targetType.IsPassedByReference())
                Error(node, "as operator only can be used with reference types.");
            targetType = ReferenceType.Create(targetType);

            // TODO: Check if it is possible to perform the cast.

            // Set the node type.
            node.SetNodeType(targetType);

            return node;
        }

        public override AstNode Visit (IsExpression node)
        {
            // Get the target type and value.
            Expression compare = node.GetCompare();
            Expression value = node.GetValue();

            // Visit them.
            compare.Accept(this);
            value.Accept(this);

            // The value must be a reference.
            IChelaType valueType = value.GetNodeType();

            // De-ref the value.
            valueType = DeReferenceType(valueType);
            valueType = DeReferenceType(valueType);
            if(valueType.IsPassedByReference())
                node.SetCoercionType(ReferenceType.Create(valueType));
            else if(valueType.IsFirstClass() || valueType.IsPlaceHolderType())
                node.SetCoercionType(valueType);
            else
                Error(node, "expected an object instead of {0}", valueType.GetDisplayName());

            // Get the target type.
            IChelaType targetType = compare.GetNodeType();
            targetType = ExtractActualType(compare, targetType);

            // Store the target type.
            node.SetTargetType(targetType);

            // Set the node type.
            node.SetNodeType(ChelaType.GetBoolType());

            return node;
        }

        public override AstNode Visit (RefExpression node)
        {
            // Evaluate the variable reference.
            Expression reference = node.GetVariableExpr();
            reference.Accept(this);

            // Get the reference type.
            IChelaType refType = reference.GetNodeType();
            if(!refType.IsReference())
                Error(node, "expected a variable reference");
            node.SetCoercionType(refType);

            // De-Reference.
            IChelaType referencedType = DeReferenceType(refType);
            if(referencedType.IsPassedByReference() && !referencedType.IsReference())
                Error(node, "expected a variable, not a value.");

            // Create the node type.
            node.SetNodeType(ReferenceType.Create(referencedType, node.IsOut()));

            // Create the node value.
            Variable refVar = (Variable)reference.GetNodeValue();
            if(refVar.IsTemporalReferencedSlot() && node.GetNodeType() == refType)
            {
                Warning(node, "{0} operator is unneeded.", node.IsOut() ? "out" : "ref");
                node.SetNodeValue(refVar);
            }
            else
            {
                node.SetNodeValue(new TemporalReferencedSlot(referencedType));
            }

            return node;
        }

        public override AstNode Visit (AddressOfOperation node)
        {
            UnsafeError(node, "cannot use address-of operator under safe contexts.");

            // Evaluate the base reference.
            Expression reference = node.GetReference();
            reference.Accept(this);

            // Get the reference type.
            IChelaType refType = reference.GetNodeType();
            if(!refType.IsReference() && !refType.IsFunctionGroup())
                Error(node, "expected a reference/function.");

            // Handle function groups.
            if(refType.IsFunctionGroup())
            {
                // Only accept groups with only one function.
                FunctionGroupSelector selector = (FunctionGroupSelector)reference.GetNodeValue();
                FunctionGroup fgroup = selector.GetFunctionGroup();
                if(fgroup.GetFunctionCount() != 1)
                    Error(node, "cannot get the address of a function grop.");

                // Extract the single function in the group.
                Function function = null;
                foreach(FunctionGroupName name in fgroup.GetFunctions())
                    function = name.GetFunction();

                // Make sure its static.
                if(!function.IsStatic())
                    Error(node, "cannot get the addres of a no-static function.");

                // Store it in the node.
                node.SetNodeValue(function);
                node.SetIsFunction(true);

                // Set the node type.
                node.SetNodeType(PointerType.Create(function.GetFunctionType()));

                return node;
            }

            node.SetCoercionType(refType);

            // De-Reference.
            refType = DeReferenceType(refType);
            if(refType.IsReference() || refType.IsPassedByReference())
                Error(node, "cannot use address of operator for managed objects.");

            // Create the pointer type.
            IChelaType pointerType = PointerType.Create(refType);
            node.SetNodeType(pointerType);

            return node;
        }

        public override AstNode Visit (IndirectAccess node)
        {
            // Evaluate the base pointer.
            Expression basePointer = node.GetBasePointer();
            basePointer.Accept(this);
            
            // Get the base pointer type.
            IChelaType pointerType = basePointer.GetNodeType();
            if(pointerType.IsReference())
            {
                // Dereference the variable.
                ReferenceType refType = (ReferenceType) pointerType;
                pointerType = refType.GetReferencedType();
            }
            
            if(!pointerType.IsPointer())
                Error(node, "expected pointer indirection.");
            
            // Set the coercion type.
            node.SetCoercionType(pointerType);
            
            // Get the pointed type.
            PointerType pointer = (PointerType) pointerType;
            IChelaType pointedType = pointer.GetPointedType();
            
            // Only structure type are supported.
            if(!pointedType.IsStructure())
                Error(node, "only structure indirection is supported.");
            
            // Cast the structure type.
            Structure structType = (Structure) pointedType;
            
            // Check that the structure has the member.
            ScopeMember member = structType.FindMemberRecursive(node.GetName());
            if(member == null)
                Error(node, "couldn't find the member '{0}'.", node.GetName());
            
            // Check the flags.
            if((member.GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Static)
                Error(node, "cannot use pointer indirectionto access static members.");
            
            // TODO: Check the access flags.
            
            // Perform checks depending on the type of the member.
            if(member.IsVariable())
            {
                Variable variable = (Variable) member;
                if(!variable.IsField() && !variable.IsProperty())
                    Error(node, "expected field/property member.");

                IChelaType varType = variable.GetVariableType();
                if(varType.IsStructure())
                    varType = ReferenceType.Create(varType); // Required for nested access.
                node.SetNodeType(ReferenceType.Create(varType));
                node.SetNodeValue(variable);

                if(variable.IsField())
                {
                    FieldVariable field = (FieldVariable) variable;
                    node.SetSlot(field.GetSlot());
                }
            }
            else if(member.IsFunction())
            {
                Function function = (Function) member;
                if(!function.IsMethod())
                    Error(node, "expected a method member.");
                
                Method method = (Method) function;
                node.SetSlot(method.GetVSlot());
                node.SetNodeType(method.GetFunctionType());
                node.SetNodeValue(method);
            }
            else if(member.IsFunctionGroup())
            {
                FunctionGroup functionGroup = (FunctionGroup)member;
                FunctionGroupSelector selector = new FunctionGroupSelector(functionGroup);
                node.SetNodeType(ChelaType.GetFunctionGroupType());
                node.SetNodeValue(selector);
            }
            else
            {
                Error(node, "expected field or method member.");
            }

            return node;
        }

        public override AstNode Visit (DereferenceOperation node)
        {
            // Evaluate the pointer.
            Expression pointerExpr = node.GetPointer();
            pointerExpr.Accept(this);

            // Get the pointer type.
            IChelaType pointerType = pointerExpr.GetNodeType();
            if(pointerType.IsReference())
                pointerType = DeReferenceType(pointerType);

            // Constant pointer aren't problems.
            if(pointerType.IsConstant())
                pointerType = DeConstType(pointerType);

            // Must be a pointer.
            if(!pointerType.IsPointer())
                Error(node, "expected pointer expression.");

            // Set the coercion type.
            node.SetCoercionType(pointerType);

            // Create the "variable".
            IChelaType slotType = DePointerType(pointerType);
            PointedSlot slot = new PointedSlot(slotType);
            node.SetNodeType(ReferenceType.Create(slotType));
            node.SetNodeValue(slot);

            return node;
        }

        public override AstNode Visit (SubscriptAccess node)
        {
            // Evaluate the base reference.
            Expression arrayExpr = node.GetArray();
            arrayExpr.Accept(this);
            
            // Get the base reference type.
            IChelaType arrayType =  arrayExpr.GetNodeType();
            IChelaType objectType = null;
            if(arrayType.IsReference())
            {
                // Get the referenced type.
                ReferenceType refType = (ReferenceType) arrayType;
                objectType = refType.GetReferencedType();
                
                // Dereference again (object variable).
                if(objectType.IsReference())
                {
                    refType = (ReferenceType) objectType;
                    arrayType = refType;
                    objectType = refType.GetReferencedType();
                }
                else if(objectType.IsStructure())
                {
                    arrayType = objectType;
                }

                if(objectType.IsPointer())
                {
                    arrayType = objectType;
                }
            }
            
            // Check the array type.
            if(!arrayType.IsPointer() && !arrayType.IsReference() && !arrayType.IsStructure())
                Error(node, "expected pointer/array/object.");
            
            // Get the object type.
            IChelaType[] indexCoercionsType = null;
            IChelaType usedArrayType = arrayType;
            int dimensions = 1;
            bool arraySlot = false;
            if(arrayType.IsPointer())
            {
                PointerType pointer = (PointerType) arrayType;
                objectType = pointer.GetPointedType();
                arraySlot = true;
            }
            else if(objectType != null && objectType.IsArray())
            {
                ArrayType array = (ArrayType)objectType;
                dimensions = array.GetDimensions();
                usedArrayType = array;
                objectType = array.GetValueType();
                if(objectType.IsPassedByReference())
                    objectType = ReferenceType.Create(objectType);
                arraySlot = true;
            }
            else if(objectType.IsClass() || objectType.IsStructure() || objectType.IsInterface())
            {
                // Find the indexer.
                Structure building = (Structure)objectType;

                // TODO: support indexer overloading.
                ScopeMember member = building.FindMemberRecursive("Op_Index");
                if(!member.IsProperty())
                    Error(node, "object has invalid indexer.");

                // Check the indices type.
                PropertyVariable indexer = (PropertyVariable)member;
                dimensions = indexer.GetIndexCount();
                if(dimensions == 0)
                    Error(node, "Expected an intexer with at leas one dimension.");

                // Use the indexer as the node value.
                node.SetNodeValue(indexer);

                // Set the index coercion type.
                indexCoercionsType = new IChelaType[dimensions];
                for(int i = 0; i < dimensions; ++i)
                    indexCoercionsType[i] = indexer.GetIndexType(i);
                node.SetIndexCoercions(indexCoercionsType);

                // Use the indexer type.
                objectType = indexer.GetVariableType();
            }
            else
            {
                Error(node, "unexpected object.");
            }
            
            // Set the coercion type.
            node.SetCoercionType(arrayType);
            
            // Set the node type.
            node.SetNodeType(ReferenceType.Create(objectType));
            
            // Create the "variable" for arrays.
            if(arraySlot)
            {
                ArraySlot variable = new ArraySlot(objectType, usedArrayType);
                node.SetNodeValue(variable);
            }
            
            // Check the index expressions.
            int indexId = 0;
            Expression index = node.GetIndex();
            while(index != null)
            {
                // Don't allow more indices than dimensions.
                if(indexId == dimensions)
                    Error(index, "cannot use more indices than dimensions.");

                // Visit it.
                index.Accept(this);

                // Get the index type.
                IChelaType indexType = index.GetNodeType();
                if(indexCoercionsType != null)
                {
                    // Perform the index coercion.
                    IChelaType coercionType = indexCoercionsType[indexId];
                    if(Coerce(index, coercionType, indexType, index.GetNodeValue()) != coercionType)
                        Error(index, "expected an index of type {0}.", coercionType.GetDisplayName());
                }
                else
                {
                    // Dereference.
                    if(indexType.IsReference())
                    {
                        ReferenceType indexRef = (ReferenceType) indexType;
                        indexType = indexRef.GetReferencedType();
                    }

                    // Deconst.
                    if(indexType.IsConstant())
                        indexType = DeConstType(indexType);

                    if(!indexType.IsInteger())
                        Error(node, "expected integer index.");
                }

                // Check the next index.
                ++indexId;
                index = (Expression)index.GetNext();
            }

            // Don't allow less indices than dimensions.
            if(indexId < dimensions)
                Error(index, "cannot use less indices than dimensions.");

            return node;
        }
        
        public override AstNode Visit (NullStatement node)
        {
            return node;
        }

        private void CheckReadOnlyConstraint(AstNode where, Variable variable)
        {
        }

        private static IChelaType CoerceConstantResult(IChelaType result, bool constant)
        {
            if(constant)
                return result != null ? ConstantType.Create(result) : null;
            return result;
        }

        /// <summary>
        /// Converts T&& into T&
        /// </summary>
        private IChelaType SimplifyReference(IChelaType type)
        {
            if(type.IsReference())
            {
                ReferenceType refType = (ReferenceType)type;
                IChelaType referenced = refType.GetReferencedType();
                if(referenced.IsReference())
                    type = referenced;
            }

            return type;
        }

        public IChelaType RefCoerceCounted(AstNode where, IChelaType leftType, IChelaType rightType, object value, out int count)
        {
            // Initialize the count.
            count = 0;

            // Both types must be references.
            if(!leftType.IsReference())
                return null;
            if(!rightType.IsReference())
                return null;

            // Remove that reference.
            IChelaType left = DeReferenceType(leftType);
            IChelaType right = DeReferenceType(rightType);

            // Check for primitive coercion.
            if(left != right)
            {
                if(left.IsFirstClass() && right == currentModule.GetAssociatedClass(left))
                    return leftType;
                else if(right.IsFirstClass() && left == currentModule.GetAssociatedClass(right))
                    return leftType;
                else
                    return null;
            }

            return leftType;
        }

        private void FixGenericBases(Structure building)
        {
            if(!building.IsTypeInstance())
                return;
            
            StructureInstance instance = (StructureInstance)building;
            instance.FixInheritance();
        }

        private IChelaType CoerceDeReference(IChelaType type, ref bool isConstant)
        {
            if(type.IsReference())
            {
                ReferenceType refType = (ReferenceType)type;
                IChelaType referencedType = refType.GetReferencedType();
                if(referencedType.IsPrimitive() || referencedType.IsStructure() ||
                   referencedType.IsReference() || referencedType.IsPointer() ||
                   referencedType.IsConstant() || referencedType.IsVector() ||
                   referencedType.IsMatrix() || referencedType.IsPlaceHolderType() ||
                   referencedType.IsFirstClass())
                    type = referencedType;

                // De-const again.
                isConstant = type.IsConstant();
                type = DeConstType(type);
            }

            return type;
        }

        public IChelaType CoerceCounted(AstNode where, IChelaType leftType, IChelaType rightType, object value, out int count)
        {
            // Initialize the output to zero.
            count = 0;

            // Remove constant.
            bool leftConstant = leftType.IsConstant();
            leftType = DeConstType(leftType);

            bool rightConstant = rightType.IsConstant();
            rightType = DeConstType(rightType);

            // Compute the result for constants.
            bool resultConstant = leftConstant && rightConstant;

            // Handle null type.
            if(leftType == ChelaType.GetNullType())
            {
                leftType = rightType;
            }
            else if(rightType == ChelaType.GetNullType())
            {
                rightType = leftType;
            }

            // Remove double references
            leftType = SimplifyReference(leftType);
            rightType = SimplifyReference(rightType);
    
            // Dereference.
            leftType = CoerceDeReference(leftType, ref leftConstant);
            rightType = CoerceDeReference(rightType, ref rightConstant);
            resultConstant = leftConstant && rightConstant;

            // If the right type is an integer constant, use his actual type.
            ConstantValue rightValue = value as ConstantValue;
            if(rightConstant && rightType.IsInteger() && rightValue != null &&
                rightType != ChelaType.GetBoolType() &&
                rightType != ChelaType.GetCharType())
            {
                bool positive = rightValue.IsPositive();
                if(leftType.IsUnsigned() && positive)
                {
                    ulong cval = rightValue.GetULongValue();
                    if(cval <= 0xFF)
                        rightType = ChelaType.GetByteType();
                    else if(cval <= 0xFFFF)
                        rightType = ChelaType.GetUShortType();
                    else if(cval <= 0xFFFFFFFFu)
                        rightType = ChelaType.GetUIntType();
                    else
                        rightType = ChelaType.GetULongType();
                }
                else if(!positive)
                {
                    long cval = rightValue.GetLongValue();
                    // Test using the absolute value.
                    if(cval < 0)
                        cval = -cval;

                    if(cval <= 0x7F)
                        rightType = ChelaType.GetSByteType();
                    else if(cval <= 0x7FFF)
                        rightType = ChelaType.GetShortType();
                    else if(cval <= 0x7FFFFFFF)
                        rightType = ChelaType.GetIntType();
                    else
                        rightType = ChelaType.GetLongType();
                }
            }

            /*if(currentFunction != null && currentFunction.GetName() == "GetEnumerator")
            {
                IChelaType left = DeReferenceType(leftType);
                IChelaType right = DeReferenceType(rightType);
                Console.WriteLine("{0}", currentFunction.GetFullName());
                Console.WriteLine("left {0} -> {1}", leftType.GetFullName(), left.GetFullName());
                Console.WriteLine("right {0} -> {1}", rightType.GetFullName() ,right.GetFullName());
                Console.WriteLine("left == right: {0}", left == right);
            }*/

            //System.Console.WriteLine("coerce {0} <-> {1}, eq {2}", leftType, rightType, leftType == rightType);
            // Any pointer can be casted implicitly into a void* or const void*.
            if(leftType == ChelaType.GetConstVoidPtrType() && rightType.IsPointer())
            {
                count++;
                return CoerceConstantResult(ChelaType.GetConstVoidPtrType(), resultConstant);
            }
            else if(leftType == ChelaType.GetVoidPtrType() && rightType.IsPointer())
            {
                count++;
                return CoerceConstantResult(ChelaType.GetVoidPtrType(), resultConstant);
            }

            if(rightType == ChelaType.GetConstVoidPtrType() && leftType.IsPointer())
            {
                count++;
                return CoerceConstantResult(ChelaType.GetConstVoidPtrType(), resultConstant);
            }
            else if(rightType == ChelaType.GetVoidPtrType() && leftType.IsPointer())
            {
                count++;
                return CoerceConstantResult(ChelaType.GetVoidPtrType(), resultConstant);
            }
            
            // Void has special treatment.
            if(leftType == ChelaType.GetVoidType() || rightType == ChelaType.GetVoidType())
                return null; // Invalid coercion.

            IChelaType destType = leftType;
            if(leftType != rightType)
            {
                // Increase the coercion count.
                count++;

                // Vector type checks.
                bool vector = leftType.IsVector() || rightType.IsVector();
                int numcomponents = 1;

                // Vector->Vector coercion rules.
                if(leftType.IsVector() && rightType.IsVector())
                {
                    // Use primitive coercion rules.
                    vector = false;

                    // Check left vector.
                    VectorType leftVector = (VectorType)leftType;
                    leftType = leftVector.GetPrimitiveType();
                    numcomponents = leftVector.GetNumComponents();

                    // Check right vector
                    VectorType rightVector = (VectorType)rightType;
                    rightType = rightVector.GetPrimitiveType();
                    if(numcomponents != rightVector.GetNumComponents())
                        return null;
                }
                else if(vector)
                {
                    // Primitive <-> vector coercion is not allowed.
                    if(leftType.IsPrimitive() || rightType.IsPrimitive())
                        return null; // Not coercion allowed.
                }

                // Coerce the types.
                uint size = Math.Max(leftType.GetSize(), rightType.GetSize());
                bool sameSize = (leftType.GetSize() == size) && (rightType.GetSize() == size);
                bool integer = false;
                bool floating = false;
                bool reference = false;
                bool pointer = false;
                bool structure = false;
                bool functionGroup = false;
                bool firstClass = false;
                bool placeholder = false;
    
                if(leftType.IsInteger())
                    integer = true;
                else if(leftType.IsFloatingPoint())
                    floating = true;
                else if(leftType.IsReference())
                    reference = true;
                else if(leftType.IsPointer())
                    pointer = true;
                else if(leftType.IsFunctionGroup())
                    functionGroup = true;
                if(leftType.IsPlaceHolderType())
                    placeholder = true;
                if(leftType.IsStructure())
                    structure = true;
                if(leftType.IsFirstClass())
                    firstClass = true;
    
                if(rightType.IsInteger())
                    integer = true;
                else if(rightType.IsFloatingPoint())
                    floating = true;
                else if(rightType.IsReference())
                    reference = true;
                else if(rightType.IsPointer())
                    pointer = true;
                else if(rightType.IsFunctionGroup())
                    functionGroup = true;
                if(rightType.IsPlaceHolderType())
                    placeholder = true;
                if(rightType.IsStructure())
                    structure = true;
                if(rightType.IsFirstClass())
                    firstClass = true;
    
                if(reference && !pointer && !firstClass && !placeholder)
                {
                    if(leftType.IsReference() && rightType.IsReference())
                    {
                        // Dereference them.
                        IChelaType left = DeReferenceType(leftType);
                        IChelaType right = DeReferenceType(rightType);
                        IChelaType objectType = currentModule.TypeMap(ChelaType.GetObjectType());
                        destType = null;
                        //System.Console.WriteLine("[{0}]{1} {2} {3}", leftType, left, right, objectType);

                        if(left == right)
                        {
                            destType = leftType;
                        }
                        else if(left == objectType || right == objectType)
                        {
                            destType = ReferenceType.Create(objectType);
                        }
                        else if(left.IsInterface() || right.IsInterface())
                        {
                            destType = null;
                            Structure leftBuilding = (Structure)left;
                            Structure rightBuilding = (Structure)right;
                            // TODO: Check this.
                            if(leftBuilding.IsInterface())
                            {
                                if(rightBuilding.Implements(leftBuilding))
                                    destType = leftType;
                            }
                            if(rightBuilding.IsInterface() && destType == null)
                            {
                                if(leftBuilding.Implements(rightBuilding))
                                    destType = rightType;
                            }
                        }
                        else if(left.IsClass() && right.IsClass())
                        {
                            Structure leftBuilding = (Structure)left;
                            Structure rightBuilding = (Structure)right;

                            // Make sure the generic inheritance relations are loaded.
                            FixGenericBases(leftBuilding);
                            FixGenericBases(rightBuilding);

                            //System.Console.WriteLine("Check base");
                            if(leftBuilding.IsDerivedFrom(rightBuilding))
                                destType = rightType;
                            else if(rightBuilding.IsDerivedFrom(leftBuilding))
                                destType = leftType;
                            else
                                destType = null;
                        }
                        else if(left.IsArray() && right.IsArray())
                        {
                            // Array-array coercion.
                            ArrayType leftArray = (ArrayType)left;
                            ArrayType rightArray = (ArrayType)right;

                            // The dimensions must match.
                            if(leftArray.GetDimensions() != rightArray.GetDimensions())
                            {
                                destType = null;
                            }
                            else
                            {
                                // Get the element types.
                                IChelaType leftElement = leftArray.GetValueType();
                                IChelaType rightElement = rightArray.GetValueType();

                                // Compute the read only flag.
                                bool readOnly = leftArray.IsReadOnly() || rightArray.IsReadOnly();
                                int dimensions = leftArray.GetDimensions();

                                // Coerce the element types.
                                if(leftElement == rightElement)
                                {
                                    destType = ArrayType.Create(leftElement, dimensions, readOnly);
                                }
                                else
                                {
                                    IChelaType coerced = Coerce(where, leftElement, rightElement, null);
                                    if(coerced != null)
                                        destType = ArrayType.Create(coerced, dimensions, readOnly);
                                }

                                // Add the reference layer.
                                if(destType != null)
                                    destType = ReferenceType.Create(destType);
                            }
                        }
                    }
                    else if(functionGroup)
                    {
                        // By default there isn't coercion
                        destType = null;

                        // Function -> Delegate coercion.
                        IChelaType refType = null;
                        FunctionGroupType groupType = null;
                        if(leftType.IsReference())
                        {
                            refType = leftType;
                            groupType = (FunctionGroupType)rightType;
                        }
                        else
                        {
                            refType = rightType;
                            groupType = (FunctionGroupType)leftType;
                        }

                        // One of the types must be a delegate.
                        IChelaType referencedType = DeReferenceType(refType);
                        if(referencedType.IsClass())
                        {
                            // If one of the types is a delegate, coerce to it.
                            Class delegateClass = currentModule.GetDelegateClass();
                            Class delegateType = (Class)referencedType;
                            if(delegateType.IsDerivedFrom(delegateClass))
                            {
                                // Now, select the correct function.
                                FunctionGroup invokeGroup = (FunctionGroup)delegateType.FindMemberRecursive("Invoke");
                                FunctionGroupSelector selector = (FunctionGroupSelector)value;
                                FunctionGroup delegatedGroup = selector.GetFunctionGroup();
                                selector.Select(null);
                                foreach(FunctionGroupName gname in invokeGroup.GetFunctions())
                                {
                                    // Ignore static signatures.
                                    Function invokeFunction = gname.GetFunction();
                                    if(invokeFunction.IsStatic())
                                        continue;

                                    // Only one invoke per delegate is permitted.
                                    List<object> arguments = new List<object> ();
                                    FunctionType invokeType = invokeFunction.GetFunctionType();
                                    for(int i = 1; i < invokeType.GetArgumentCount(); ++i)
                                        arguments.Add(invokeType.GetArgument(i));

                                    // Pick the function. This performs argument covariance.
                                    Function picked = PickFunction(where, delegatedGroup, groupType, null, arguments, true);
                                    if(picked != null)
                                    {
                                        // Make sure the return type is compatible.
                                        FunctionType pickedType = picked.GetFunctionType();
                                        IChelaType delegateReturn = invokeType.GetReturnType();
                                        IChelaType pickedReturn = pickedType.GetReturnType();
                                        if(pickedReturn != delegateReturn &&
                                           Coerce(pickedReturn, delegateReturn) != delegateReturn)
                                            Error(where, "incompatible delegate and invoked function return type.");
    
                                        selector.Select(picked);
                                        break;
                                    }
                                }

                                if(selector.GetSelected() != null)
                                    destType = refType;
                                else
                                    destType = null;
                            }
                        }
                    }
                    else
                    {
                        // Support structure boxing
                        IChelaType referencedType;
                        IChelaType valueType;
                        if(leftType.IsReference())
                        {
                            referencedType = DeReferenceType(leftType);
                            valueType = rightType;
                        }
                        else
                        {
                            referencedType = DeReferenceType(rightType);
                            valueType = leftType;
                        }

                        if(valueType.IsStructure())
                        {
                            Structure referencedClass = referencedType as Structure;
                            Structure building = (Structure)valueType;
                            if(referencedClass != null && building.IsBasedIn(referencedClass))
                                destType = ReferenceType.Create(referencedType);
                            else
                                destType = null;
                        }
                        else
                        {
                            destType = null;
                        }
                    }
                }
                else if(pointer && !reference && !firstClass && !placeholder)
                {
                    if(leftType.IsPointer() && rightType.IsPointer())
                    {
                        // Depointer them.
                        IChelaType left = DePointerType(leftType);
                        IChelaType right = DePointerType(rightType);

                        // De-const.
                        bool pointerToConstant = false;
                        if(left.IsConstant() || right.IsConstant())
                            pointerToConstant = true;

                        left = DeConstType(left);
                        right = DeConstType(right);
                        if(left == right)
                        {
                            destType = left;
                        }
                        else if(left.IsStructure() && right.IsStructure())
                        {
                            Structure leftBuilding = (Structure)left;
                            Structure rightBuilding = (Structure)right;
                            if(leftBuilding.IsDerivedFrom(rightBuilding))
                                destType = right;
                            else if(rightBuilding.IsDerivedFrom(leftBuilding))
                                destType = left;
                            else
                                destType = null;
                        }
                        else if(left.IsStructure() && right.IsFirstClass() ||
                                right.IsStructure() && left.IsFirstClass())
                        {
                            // They could represent the same type.
                            Structure building;
                            IChelaType primitive;
                            if(left.IsStructure())
                            {
                                building = (Structure)left;
                                primitive = right;
                            }
                            else
                            {
                                building = (Structure)right;
                                primitive = left;
                            }

                            // Get the primitive associated class.
                            Structure associated = currentModule.GetAssociatedClass(primitive);

                            // If the structure is the associated class, they are the same type.
                            if(building == associated)
                                destType = left;
                            else
                                destType = null;

                        }
                        else
                        {
                            destType = null;
                        }

                        // Make the pointer.
                        if(destType != null)
                        {
                            if(pointerToConstant)
                                destType = PointerType.Create(ConstantType.Create(destType));
                            else
                                destType = PointerType.Create(destType);
                        }
                    }
                    else
                    {
                        // Invalid coercion.
                        destType = null;
                    }
                }
                else if(firstClass && reference)
                {
                    // Select the primitive and reference type.
                    IChelaType primType;
                    IChelaType refType;
                    if(leftType.IsFirstClass())
                    {
                        primType = leftType;
                        refType = rightType;
                    }
                    else
                    {
                        primType = rightType;
                        refType = leftType;
                    }

                    // Get the referenced type.
                    Structure assoc = currentModule.GetAssociatedClass(primType);
                    IChelaType referencedType = DeReferenceType(refType);
                    if(assoc == null || (!referencedType.IsClass() && !referencedType.IsStructure() && !referencedType.IsInterface()))
                        return null;
                    Structure building = (Structure)referencedType;

                    // Only cast if there are related.
                    if(assoc.IsBasedIn(building))
                        destType = refType;
                    else
                        destType = null;
                }
                else if(firstClass && structure)
                {
                    IChelaType primType;
                    IChelaType structType;
                    if(leftType.IsFirstClass())
                    {
                        primType = leftType;
                        structType = rightType;
                    }
                    else
                    {
                        primType = rightType;
                        structType = leftType;
                    }

                    // Get the associated type
                    Structure building = (Structure)structType;
                    Structure assoc = currentModule.GetAssociatedClass(primType);
                    if(assoc == null || structType != assoc)
                    {
                        destType = null;

                        // Get the value field.
                        FieldVariable valueField = (FieldVariable)building.FindMember("__value");
                        if(valueField == null)
                            valueField = (FieldVariable)building.FindMember("m_value");

                        // Check enumerations
                        if(valueField != null)
                        {
                            IChelaType valueType = valueField.GetVariableType();
                            Class enumClass = currentModule.GetEnumClass();
                            if(building.IsDerivedFrom(enumClass))
                            {
                                if(primType == valueType)
                                    destType = primType;
                            }
                            else
                            {
                                // Generic structure.
                                IChelaType coerced = Coerce(valueType, primType);
                                if(coerced == valueType)
                                    destType = structType;
                                else
                                    destType = coerced;
                            }
                        }
                    }
                    else
                    {
                        // Don't count coercion
                        count = 0;
                        destType = leftType;
                    }
                }
                else if(placeholder && reference)
                {
                    // Check for placeholder->object or constraint.
                    PlaceHolderType placeholderType = null;
                    IChelaType referenceType = null;
                    if(leftType.IsPlaceHolderType())
                    {
                        placeholderType = (PlaceHolderType)leftType;
                        referenceType = rightType;
                    }
                    else
                    {
                        placeholderType = (PlaceHolderType)rightType;
                        referenceType = leftType;
                    }

                    // Get the referenced type.
                    IChelaType referencedType = DeReferenceType(referenceType);

                    // Default result.
                    destType = null;

                    // Use the constraints.
                    if(referencedType.IsInterface() || referencedType.IsClass())
                    {
                        Structure building = (Structure)referencedType;

                        // Check if one constraint implements building.
                        bool compatible = false;

                        // Object is implicit.
                        if(building == currentModule.GetObjectClass())
                            compatible = true;
                        // Value types are deriverd from the ValueType class.
                        else if(placeholderType.IsValueType() &&
                                building == currentModule.GetValueTypeClass())
                            compatible = true;

                        for(int i = 0; i < placeholderType.GetBaseCount() && !compatible; ++i)
                        {
                            Structure baseBuilding = (Structure)placeholderType.GetBase(i);
                            if(baseBuilding == building) // The constraint is the building.
                                compatible = true;
                            // The constraint implements the target type.
                            else if(baseBuilding.IsClass() && baseBuilding.IsDerivedFrom(building))
                                compatible = true;
                            // The constraint implements the interface
                            else if(baseBuilding.IsInterface() && baseBuilding.Implements(building))
                                compatible = true;

                            // Not implemented by the constraint.
                        }

                        // If the type is compatible, perform coercion.
                        if(compatible)
                            destType = referenceType;
                    }
                }
                else if(integer && floating && !placeholder)
                {
                    // int->(float|double)
                    if(size <= 4)
                        destType = ChelaType.GetFloatType();
                    else
                        destType = ChelaType.GetDoubleType();
                }
                else if(integer && !placeholder)
                {
                    // int->long?
    
                    // Find the signs.
                    bool signed = !leftType.IsUnsigned() || !rightType.IsUnsigned();
                    bool unsigned = leftType.IsUnsigned() || rightType.IsUnsigned();
                    bool withSign = signed;

                    // If they have the same size and different signs, increase the size
                    if(sameSize && signed && unsigned)
                        ++size;

                    if(size == 1)
                    {
                        if(withSign)
                            destType = ChelaType.GetSByteType();
                        else
                            destType = ChelaType.GetByteType();
                    }
                    else if(size == 2)
                    {
                        if(withSign)
                            destType = ChelaType.GetShortType();
                        else
                            destType = ChelaType.GetUShortType();
                    }
                    else if(size <= 4)
                    {
                        if(withSign)
                            destType = ChelaType.GetIntType();
                        else
                            destType = ChelaType.GetUIntType();
                    }
                    else if(size == 100)
                    {
                        if(withSign)
                            destType = null;
                        else
                            destType = ChelaType.GetSizeType();
                    }
                    else if(size <= 8)
                    {
                        if(withSign)
                            destType = ChelaType.GetLongType();
                        else
                            destType = ChelaType.GetULongType();
                    }
                }
                else if(floating && !placeholder)
                {
                    // float->double
                    if(size <= 4)
                        destType = ChelaType.GetFloatType();
                    else
                        destType = ChelaType.GetDoubleType();
                }
                else
                {
                    // Failed to coerce.
                    destType = null;
                }

                // Vector coercion.
                if(destType != null && numcomponents > 1 && destType.IsPrimitive())
                    destType = VectorType.Create(destType, numcomponents);
            }
            return CoerceConstantResult(destType, resultConstant);
        }

        public IChelaType Coerce(AstNode where, IChelaType leftType, IChelaType rightType, object value)
        {
            int count;
            return CoerceCounted(where, leftType, rightType, value, out count);
        }

        public IChelaType Coerce(IChelaType leftType, IChelaType rightType)
        {
            return Coerce(null, leftType, rightType, null);
        }
    }
}
