using System;
using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class FunctionGenerator: ObjectDeclarator
    {
        private BlockBuilder builder;
        private BasicBlock currentBreak;
        private BasicBlock currentContinue;
        protected ExceptionContext currentExceptionContext;
        
        public FunctionGenerator ()
        {
            this.currentFunction = null;
            this.currentBreak = null;
            this.currentContinue = null;
            this.currentExceptionContext = null;
            this.builder = new BlockBuilder();
        }

        public override AstNode Visit (ModuleNode node)
        {
            // This is for the global namespace.
            currentModule = node.GetModule();
            currentScope = currentModule.GetGlobalNamespace();
            currentContainer = currentScope;

            // Create the static constructor.
            CreateStaticConstructor(node, currentModule.GetGlobalNamespace());

            // Visit the children.
            VisitList(node.GetChildren());

            return node;
        }

        public override AstNode Visit (NamespaceDefinition node)
        {
            // Handle nested namespaces.
            LinkedList<Namespace> namespaceChain = new LinkedList<Namespace>();
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

            // Create the static constructors.
            CreateStaticConstructor(node, node.GetNamespace());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            for(int i = 0; i < namespaceChain.Count; i++)
                PopScope();

            return node;
        }

        public override AstNode Visit (StructDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Create default constructors.
            CreateDefaultConstructor(node);
            CreateDefaultStaticConstructor(node);

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

            // Create default constructors.
            CreateDefaultConstructor(node);
            CreateDefaultStaticConstructor(node);

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

        public override AstNode Visit (EnumDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            return node;
        }

        public override AstNode Visit (FieldDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            return node;
        }
        
        public override AstNode Visit (FunctionPrototype node)
        {
            // Do nothing. ???
            return node;
        }

        private void GenerateFieldInitializations(AstNode node, Structure building)
        {
            foreach(FieldDeclaration decl in building.GetFieldInitializations())
            {
                // Ignore static fields.
                FieldVariable field = (FieldVariable)decl.GetVariable();
                if(field.IsStatic())
                    continue;

                // Load self.
                builder.CreateLoadArg(0);

                // Visit the value expression.
                Expression value = decl.GetDefaultValue();
                value.Accept(this);

                // Cast the value type.
                IChelaType valueType = value.GetNodeType();
                IChelaType coercionType = decl.GetCoercionType();
                if(valueType != coercionType)
                    Cast(decl, value.GetNodeValue(), valueType, coercionType);

                // Store the value.
                builder.CreateStoreField(field);
            }
        }

        private void GenerateStaticFieldInitializations(AstNode node, List<FieldDeclaration> staticFields)
        {
            foreach(FieldDeclaration decl in staticFields)
            {
                // Ignore static fields.
                FieldVariable field = (FieldVariable)decl.GetVariable();
                if(!field.IsStatic())
                    continue;

                // Begin field node.
                builder.BeginNode(decl);

                // Visit the value expression.
                Expression value = decl.GetDefaultValue();
                value.Accept(this);

                // Cast the value type.
                IChelaType valueType = value.GetNodeType();
                IChelaType coercionType = decl.GetCoercionType();
                if(valueType != coercionType)
                    Cast(decl, value.GetNodeValue(), valueType, coercionType);

                // Store the value.
                builder.CreateStoreGlobal(field);

                // End field node.
                builder.EndNode();
            }
        }

        private void GenerateStaticFieldInitializations(AstNode node, Function ctor, Structure building)
        {
            // Get the static initialization fields.
            List<FieldDeclaration> staticInitedField = new List<FieldDeclaration> ();
            bool isUnsafe = false;
            foreach(FieldDeclaration decl in building.GetFieldInitializations())
            {
                Variable variable = decl.GetVariable();
                if(variable.IsStatic())
                {
                    staticInitedField.Add(decl);
                    if(variable.IsUnsafe())
                        isUnsafe = true;
                }
            }

            // If there aren't field to initialize, don't create the static constructor.
            if(staticInitedField.Count == 0)
                return;

            // Check if unsafe its required.
            if(isUnsafe && !ctor.IsUnsafe())
                Error(node, "static constructor must be unsafe due to implicit unsafe static field initialization.");

            // Initialize the static fields.
            GenerateStaticFieldInitializations(node, staticInitedField);
        }

        private void CreateImplicitBaseConstructor(AstNode node, Method function)
        {
            Function parentConstructor = function.GetCtorParent();
            if(parentConstructor != null &&
               function.GetParentScope().IsStructure() == parentConstructor.GetParentScope().IsStructure())
            {
                builder.CreateLoadArg(0);
                builder.CreateCall(parentConstructor, 1);
            }
        }

        private void CreateDefaultConstructor(StructDefinition node)
        {
            // Get the structure, and the default constructor.
            Structure building = node.GetStructure();
            Method constructor = node.GetDefaultConstructor();

            // Ignore structures without a default constructor.
            if(constructor == null)
                return;

            // Crete the self scope.
            LexicalScope selfScope = CreateLexicalScope(node, constructor);
            new ArgumentVariable(0, "this", selfScope, ReferenceType.Create(building));
            PushScope(selfScope);

            // Create the top basic block.
            BasicBlock topBlock = new BasicBlock(constructor);
            builder.SetBlock(topBlock);

            // Add implicit construction expressions.
            CreateImplicitBaseConstructor(node, constructor);

            // Initialize some field.
            GenerateFieldInitializations(node, building);

            // Restore the scope.
            PopScope();

            // Return void.
            builder.CreateRetVoid();
        }

        private Method CreateTrivialConstructor(Structure building, Structure buildingInstance)
        {
            // Create the constructor function type.
            FunctionType type = FunctionType.Create(ChelaType.GetVoidType(),
                new IChelaType[]{ReferenceType.Create(buildingInstance)}, false);

            // Create the constructor.
            Method ctor = new Method(".ctor", MemberFlags.Public | MemberFlags.Constructor, building);
            ctor.SetFunctionType(type);
            building.AddFunction(".ctor", ctor);

            // Create the constructor block.
            BasicBlock top = new BasicBlock(ctor);
            top.SetName("top");
            top.Append(new Instruction(OpCode.RetVoid, null));

            return ctor;
        }

        private void CreateDefaultStaticConstructor(StructDefinition node)
        {
            // Get the structure.
            Structure building = node.GetStructure();

            // Check for an user defined constructor.
            if(building.GetStaticConstructor() != null)
                return;

            // Get the static initialization fields.
            List<FieldDeclaration> staticInitedField = new List<FieldDeclaration> ();
            bool isUnsafe = false;
            foreach(FieldDeclaration decl in building.GetFieldInitializations())
            {
                Variable variable = decl.GetVariable();
                if(variable.IsStatic())
                {
                    staticInitedField.Add(decl);
                    if(variable.IsUnsafe())
                        isUnsafe = true;
                }
            }

            // If there aren't field to initialize, don't create the static constructor.
            if(staticInitedField.Count == 0)
                return;

            // Begin the node.
            builder.BeginNode(node);

            // Create the static constructor function type.
            FunctionType ctorType = FunctionType.Create(ChelaType.GetVoidType());

            // Create the constructor method.
            MemberFlags flags = MemberFlags.StaticConstructor;
            if(isUnsafe)
                flags |= MemberFlags.Unsafe;
            Method constructor = new Method("<sctor>", flags, building);
            constructor.SetFunctionType(ctorType);

            // Store it.
            building.AddFunction("<sctor>", constructor);

            // Create the constructor scope.
            LexicalScope ctorScope = CreateLexicalScope(node, constructor);
            PushScope(ctorScope);

            // Create the top basic block.
            BasicBlock topBlock = new BasicBlock(constructor);
            builder.SetBlock(topBlock);

            // Initialize the static fields.
            GenerateStaticFieldInitializations(node, staticInitedField);

            // Restore the scope.
            PopScope();

            // Return void.
            builder.CreateRetVoid();

            // End the node.
            builder.EndNode();
        }

        private void CreateStaticConstructor(AstNode node, Namespace space)
        {
            // Check for a previous definition.
            if(space.GetStaticConstructor() != null || space.GetGlobalInitializations() == null)
                return;

            // Get the static initialization fields.
            List<FieldDeclaration> staticInitedField = new List<FieldDeclaration> ();
            bool isUnsafe = false;
            foreach(FieldDeclaration decl in space.GetGlobalInitializations())
            {
                Variable variable = decl.GetVariable();
                if(variable.IsStatic())
                {
                    staticInitedField.Add(decl);
                    if(variable.IsUnsafe())
                        isUnsafe = true;
                }
            }

            // If there aren't field to initialize, don't create the static constructor.
            if(staticInitedField.Count == 0)
                return;

            // Create the static constructor function type.
            FunctionType ctorType = FunctionType.Create(ChelaType.GetVoidType());

            // Create the constructor method.
            MemberFlags flags = MemberFlags.StaticConstructor;
            if(isUnsafe)
                flags |= MemberFlags.Unsafe;
            Function constructor = new Function("<sctor>", flags, space);
            constructor.SetFunctionType(ctorType);

            // Store it.
            space.AddMember(constructor);

            // Create the constructor scope.
            LexicalScope ctorScope = CreateLexicalScope(node, constructor);
            PushScope(ctorScope);

            // Create the top basic block.
            BasicBlock topBlock = new BasicBlock(constructor);
            builder.SetBlock(topBlock);

            // Initialize the static fields.
            GenerateStaticFieldInitializations(null, staticInitedField);

            // Restore the scope.
            PopScope();

            // Return void.
            builder.CreateRetVoid();
        }

        public override AstNode Visit (AttributeArgument node)
        {
            // Set the node value.
            Expression valueExpr = node.GetValueExpression();
            node.SetNodeValue(valueExpr.GetNodeValue());

            return node;
        }

        public override AstNode Visit (AttributeInstance node)
        {
            // Create the attribute constant.
            Class attributeClass = node.GetAttributeClass();
            Method attributeCtor = node.GetAttributeConstructor();
            AttributeConstant attrConstant = new AttributeConstant(attributeClass, attributeCtor);

            // Set the attribute constant as the node value.
            node.SetNodeValue(attrConstant);

            // Store the arguments.
            AstNode arg = node.GetArguments();
            while(arg != null)
            {
                // Vist the argument.
                arg.Accept(this);

                // Add the argument.
                ConstantValue value = (ConstantValue)arg.GetNodeValue();
                AttributeArgument attrArg = (AttributeArgument)arg;
                Variable property = attrArg.GetProperty();
                if(property != null)
                    attrConstant.AddPropertyValue(property, value);
                else
                    attrConstant.AddArgument(value);

                // Check the next argument.
                arg = arg.GetNext();
            }
            return node;
        }

        private void ProcessAttributes(AstNode node)
        {
            // Get the attributes node.
            AstNode attrNode = node.GetAttributes();

            // Ignore nodes without attributes.
            if(attrNode == null)
                return;

            // Retrieve the node member.
            List<ScopeMember> memberList = new List<ScopeMember> ();
            FunctionDefinition defunNode = node as FunctionDefinition;
            if(defunNode != null)
            {
                memberList.Add(defunNode.GetFunction());
            }
            else
            {
                FunctionPrototype protoNode = node as FunctionPrototype;
                if(protoNode != null)
                {
                    memberList.Add(protoNode.GetFunction());
                }
                else
                {
                    FieldDefinition fieldNode = node as FieldDefinition;
                    if(fieldNode != null)
                    {
                        // Add each one of the fields declared.
                        AstNode declNode = fieldNode.GetDeclarations();
                        while(declNode != null)
                        {
                            FieldDeclaration decl = (FieldDeclaration)declNode;
                            memberList.Add(decl.GetVariable());
                            declNode = declNode.GetNext();
                        }
                    }
                    else
                    {
                        ScopeNode scopeNode = node as ScopeNode;
                        if(scopeNode != null)
                            memberList.Add(scopeNode.GetScope());
                        else
                            Error(node, "unimplemented member node type.");
                    }
                }
            }

            // Visit the attributes, and add them to the member.
            while(attrNode != null)
            {
                // Visit it.
                attrNode.Accept(this);

                // Store the attribute.
                AttributeConstant attrConstant = (AttributeConstant)attrNode.GetNodeValue();
                foreach(ScopeMember member in memberList)
                    member.AddAttribute(attrConstant);

                // Check the next attribute.
                attrNode = attrNode.GetNext();
            }
        }

        private void PrepareReturning(AstNode where, Function function, LexicalScope scope)
        {
            // Get the definition node and check for generator.
            FunctionDefinition defNode = function.DefinitionNode;
            bool isGenerator = false;
            if(defNode != null)
                isGenerator = defNode.IsGenerator;

            // Use standard return.
            if(function.GetExceptionContext() == null && !isGenerator)
                return;

            // Create the return block.
            BasicBlock returnBlock = new BasicBlock(function);
            returnBlock.SetName("retBlock");
            function.ReturnBlock = returnBlock;

            // Create the return value local.
            FunctionType functionType = function.GetFunctionType();
            IChelaType retType = functionType.GetReturnType();
            if(retType != ChelaType.GetVoidType() && !isGenerator)
                function.ReturnValueVar = new LocalVariable(GenSym() + "_retval", scope, retType);

            // Create the is returning flag.
            function.ReturningVar = new LocalVariable(GenSym() + "_isret", scope, ChelaType.GetBoolType(), isGenerator);
            function.ReturningVar.Type = LocalType.Return;

            // Create the field for generators.
            if(isGenerator)
            {
                FieldVariable returningField = new FieldVariable("returning", MemberFlags.Private,
                    ChelaType.GetBoolType(), defNode.GeneratorClass);
                defNode.GeneratorClass.AddField(returningField);
                function.ReturningVar.ActualVariable = returningField;
            }

            // Initializer the returning flag to false.
            builder.CreateLoadBool(false);
            PerformAssignment(where, function.ReturningVar);
        }

        private void FinishReturn(AstNode node, Function function)
        {
            // Get the return block.
            BasicBlock returnBlock = function.ReturnBlock;

            // Get the definition node and check for generator.
            FunctionDefinition defNode = function.DefinitionNode;
            bool isGenerator = false;
            if(defNode != null)
                isGenerator = defNode.IsGenerator;

            // Add implicit void return.
            FunctionType functionType = function.GetFunctionType();
            IChelaType retType = functionType.GetReturnType();
            IChelaType voidType = ChelaType.GetVoidType();
            if(!builder.IsLastTerminator())
            {
                if(retType == voidType || isGenerator)
                {
                    if(returnBlock != null)
                    {
                        builder.CreateJmp(returnBlock);
                    }
                    else if(isGenerator)
                    {
                        builder.CreateLoadBool(false);
                        builder.CreateRet();
                    }
                    else
                    {
                        builder.CreateRetVoid();
                    }
                }
                else
                {
                    //function.Dump();
                    Error(node, "not all of the function code paths returns something.");
                }
            }

            // Build the return block.
            if(returnBlock != null)
            {
                builder.SetBlock(returnBlock);

                // Perform return.
                if(isGenerator)
                {
                    // Set the disposed state.
                    builder.CreateLoadArg(0);
                    builder.CreateLoadInt32(1);
                    builder.CreateStoreField(defNode.GeneratorState);

                    // Return false in MoveNext.
                    builder.CreateLoadBool(false);
                    builder.CreateRet();
                }
                else if(retType == voidType)
                {
                    builder.CreateRetVoid();
                }
                else
                {
                    builder.CreateLoadLocal(function.ReturnValueVar);
                    builder.CreateRet();
                }
            }
        }

        private void DefineFunctionBody(FunctionDefinition node, Function function, LexicalScope topScope)
        {
            // Create the top basic block.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // Prepare returning.
            PrepareReturning(node, function, topScope);

            // Store the arguments into local variables.
            FunctionPrototype prototype = node.GetPrototype();
            AstNode argument = prototype.GetArguments();
            byte index = 0;
            while(argument != null)
            {
                FunctionArgument argNode = (FunctionArgument) argument;
                LocalVariable argVar = argNode.GetVariable();
                if(argVar != null && !argVar.IsPseudoScope())
                {
                    // Load the argument.
                    builder.CreateLoadArg(index);

                    // Store it into the local.
                    builder.CreateStoreLocal(argVar);
                }

                // Process the next argument.
                argument = argument.GetNext();
                index++;
            }

            // Generate constructor initialization.
            if(function.IsConstructor())
            {
                Method ctor = (Method)function;
                ConstructorInitializer ctorInit = prototype.GetConstructorInitializer();
                if(ctorInit != null)
                    ctorInit.Accept(this);
                else
                    CreateImplicitBaseConstructor(node, ctor);

                // Initialize some field.
                if(ctor.IsCtorLeaf())
                {
                    Structure building = (Structure)ctor.GetParentScope();
                    GenerateFieldInitializations(node, building);
                }
            }
            else if(function.IsStaticConstructor())
            {
                // Generate static field initialization.
                Scope parent = function.GetParentScope();
                Structure pbuilding = parent as Structure;
                if(pbuilding != null)
                    GenerateStaticFieldInitializations(node, function, pbuilding);
            }

            // Visit his children.
            VisitList(node.GetChildren());

            // Finish return.
            FinishReturn(node, function);
        }

        private void DefineGeneratorBody(FunctionDefinition node)
        {
            // Get entry function.
            Function entryFunction = node.GetFunction();

            // Rebase his locals.
            entryFunction.RebaseLocals();

            // Create the generator class.
            Scope spaceScope = entryFunction.GetParentScope();
            Class generatorClass =
                new Class(GenSym(), MemberFlags.Internal, spaceScope);
            node.GeneratorClass = generatorClass;

            // Use the same generic prototype as the entry point function.
            generatorClass.SetGenericPrototype(entryFunction.GetGenericPrototype());

            // Add the generator class to the same scope as the function.
            if(spaceScope.IsClass() || spaceScope.IsStructure())
            {
                Structure parentClass = (Structure)spaceScope;
                parentClass.AddType(generatorClass);
            }
            else if(spaceScope.IsNamespace())
            {
                Namespace parentSpace = (Namespace)spaceScope;
                parentSpace.AddMember(generatorClass);
            }
            else
            {
                Error(node, "Cannot create generator class in {0}", spaceScope.GetFullName());
            }

            // Add the enumerable interface.
            Structure enumerableIface = null;
            if(node.IsEnumerable)
            {
                enumerableIface = currentModule.GetEnumerableIface();
                if(node.IsGenericIterator)
                {
                    enumerableIface = currentModule.GetEnumerableGIface();
                    GenericInstance gargs = new GenericInstance(enumerableIface.GetGenericPrototype(),
                        new IChelaType[]{node.YieldType});
                    enumerableIface = (Structure)enumerableIface.InstanceGeneric(gargs, currentModule);
                }

                generatorClass.AddInterface(enumerableIface);
            }

            // Add the enumerator interface.
            Structure enumeratorIface = currentModule.GetEnumeratorIface();
            if(node.IsGenericIterator)
            {
                enumeratorIface = currentModule.GetEnumeratorGIface();
                GenericInstance gargs = new GenericInstance(enumeratorIface.GetGenericPrototype(),
                    new IChelaType[]{node.YieldType});
                enumeratorIface = (Structure)enumeratorIface.InstanceGeneric(gargs, currentModule);
            }
            generatorClass.AddInterface(enumeratorIface);

            // Create the yielded field.
            FieldVariable yieldedValue = new FieldVariable("yielded", MemberFlags.Private, node.YieldType, generatorClass);
            generatorClass.AddField(yieldedValue);
            node.YieldedValue = yieldedValue;

            // Create the generator state variable.
            FieldVariable generatorState = new FieldVariable("state", MemberFlags.Private, ChelaType.GetIntType(), generatorClass);
            generatorClass.AddField(generatorState);
            node.GeneratorState = generatorState;

            // Encapsulate the locals in fields.
            foreach(LocalVariable local in entryFunction.GetLocals())
            {
                if(!local.IsPseudoLocal)
                    continue;

                // Variables containing arguments must be public.
                MemberFlags access = MemberFlags.Private;
                if(local.ArgumentIndex >= 0)
                    access = MemberFlags.Public;

                // Create the field to hold the state.
                FieldVariable localField = new FieldVariable(GenSym(), access, local.GetVariableType(), generatorClass);
                generatorClass.AddField(localField);
                local.ActualVariable = localField;
            }

            // Create an instance of the generator class.
            Structure generatorClassInstance = generatorClass.GetClassInstance();
            if(generatorClass.GetGenericPrototype().GetPlaceHolderCount() != 0)
            {
                // Create an instance using the same placeholders.
                GenericPrototype proto = generatorClass.GetGenericPrototype();
                int numargs = proto.GetPlaceHolderCount();
                IChelaType[] protoInstance = new IChelaType[numargs];
                for(int i = 0; i < numargs; ++i)
                    protoInstance[i] = proto.GetPlaceHolder(i);

                // Instance the generic class.
                GenericInstance instance = new GenericInstance(proto, protoInstance);
                generatorClassInstance = (Structure)generatorClassInstance.InstanceGeneric(instance, currentModule);
            }
            node.GeneratorClassInstance = generatorClassInstance;

            // Create the trivial constructor.
            Function ctor = CreateTrivialConstructor(generatorClass, generatorClassInstance);
            if(generatorClass.IsGeneric())
                ctor = FindFirstConstructor(generatorClassInstance);

            // Create a local to hold the created closure.
            LexicalScope topScope = (LexicalScope)node.GetScope();
            LocalVariable closureLocal = new LocalVariable("closure", topScope, ReferenceType.Create(generatorClassInstance));

            // Create the entry point content.
            BasicBlock entryBlock = CreateBasicBlock();
            entryBlock.SetName("entry");
            builder.SetBlock(entryBlock);

            // Create the closure and store it in his local.
            builder.CreateNewObject(generatorClassInstance, ctor, 0);
            builder.CreateStoreLocal(closureLocal);

            // Load the closure.
            builder.CreateLoadLocal(closureLocal);

            // Store the arguments into the closure.
            FunctionPrototype prototype = node.GetPrototype();
            AstNode argument = prototype.GetArguments();
            byte index = 0;
            while(argument != null)
            {
                FunctionArgument argNode = (FunctionArgument) argument;

                // TODO: Forbid ref, out, stream arguments here.

                // Store the argument in the closure.
                LocalVariable argVar = argNode.GetVariable();
                if(argVar != null)
                {
                    // Load the closure
                    builder.CreateDup1();

                    // Load the argument.
                    builder.CreateLoadArg(index);

                    // Store it into the field.
                    builder.CreateStoreField((FieldVariable)argVar.ActualVariable);
                }

                // Process the next argument.
                argument = argument.GetNext();
                index++;
            }

            // Encapsulate the argument variables.
            foreach(ArgumentVariable argVar in node.ArgumentVariables)
            {
                if(!argVar.IsPseudoArgument)
                    continue;

                // Create the argument field.
                FieldVariable argField = new FieldVariable(GenSym(), MemberFlags.Public, argVar.GetVariableType(), generatorClass);
                generatorClass.AddField(argField);
                argVar.ActualVariable = argField;

                // Store the self field.
                if(!currentFunction.IsStatic() && argVar.GetArgumentIndex() == 0)
                    node.GeneratorSelf = argField;

                // Load the closure.
                builder.CreateDup1();

                // Load the argument.
                builder.CreateLoadArg((byte)argVar.GetArgumentIndex());

                // Store it into the closure.
                builder.CreateStoreField(argField);
            }

            // Return the generator.
            builder.CreateRet();

            // Notify the yields about their states.
            int stateIndex = 2;
            foreach(ReturnStatement yieldStmtn in node.Yields)
            {
                yieldStmtn.YieldState = stateIndex;
                stateIndex += 2;
            }

            // Implement IEnumerator.
            if(node.IsEnumerable)
            {
                // Create the object GetEnumerator method.
                CreateGenerator_GetEnumerator(node, currentModule.GetEnumeratorIface(), false);

                // Create the generic GetEnumerator method
                if(node.IsGenericIterator)
                    CreateGenerator_GetEnumerator(node, enumeratorIface, true);
            }

            // Create the Current property.
            CreateGenerator_Current(node, false);
            if(node.IsGenericIterator)
                CreateGenerator_Current(node, true);

            // Create the Reset method.
            CreateGenerator_Reset(node);

            // Create the MoveNext method.
            Function moveNext = CreateGenerator_MoveNext(node);

            // Create the Dispose method.
            CreateGenerator_Dispose(node, moveNext);

            // Fix the inheritance.
            generatorClass.FixInheritance();
        }

        private Function FindFirstConstract(Structure building, string name)
        {
            // Find the contract declaration.
            FunctionGroup group = (FunctionGroup)building.FindMember(name);
            foreach(FunctionGroupName gname in group.GetFunctions())
            {
                if(!gname.IsStatic())
                    return gname.GetFunction();
            }

            return null;
        }

        private Function FindFirstConstructor(Structure building)
        {
            // Find the contract declaration.
            FunctionGroup group = building.GetConstructor();
            foreach(FunctionGroupName gname in group.GetFunctions())
            {
                if(!gname.IsStatic())
                    return gname.GetFunction();
            }

            return null;
        }

        private Function GetExceptionCtor(AstNode where, Structure building)
        {
            FunctionGroup group = building.GetConstructor();
            foreach(FunctionGroupName gname in @group.GetFunctions())
            {
                if(!gname.IsStatic() && gname.GetFunctionType().GetArgumentCount() == 2)
                    return gname.GetFunction();
            }

            return null;
        }

        private void CreateGenerator_GetEnumerator(FunctionDefinition node, Structure enumeratorType, bool generic)
        {
            // Get the generator class.
            Class genClass = node.GeneratorClass;

            // Use the GetEnumerator name for the most specific version.
            string name = "GetEnumerator";
            Function contract = null;
            if(!generic && node.IsGenericIterator)
            {
                name = GenSym();
                contract = FindFirstConstract(currentModule.GetEnumerableIface(), "GetEnumerator");
            }

            // Create the function type.
            FunctionType functionType =
                FunctionType.Create(ReferenceType.Create(enumeratorType),
                    new IChelaType[]{ReferenceType.Create(node.GeneratorClassInstance)}, false);

            // Create the method.
            Method method = new Method(name, MemberFlags.Public, genClass);
            method.SetFunctionType(functionType);
            genClass.AddFunction(name, method);

            // Set the explicit contract.
            if(contract != null)
                method.SetExplicitContract(contract);

            // Create the method block.
            BasicBlock block = new BasicBlock(method);
            block.SetName("top");

            // Return this.
            builder.SetBlock(block);
            builder.CreateLoadArg(0);
            builder.CreateRet();
        }

        private void CreateGenerator_Current(FunctionDefinition node, bool generic)
        {
            // Get the generator class.
            Class genClass = node.GeneratorClass;

            // Use the get_Current name for the most specific version.
            string name = "get_Current";
            Function contract = null;
            if(!generic && node.IsGenericIterator)
            {
                name = GenSym();
                contract = FindFirstConstract(currentModule.GetEnumeratorIface(), "get_Current");
            }

            // Select the current type.
            IChelaType currentType = node.YieldType;
            if(!generic)
                currentType = ReferenceType.Create(currentModule.GetObjectClass());

            // Create the function type.
            FunctionType functionType =
                FunctionType.Create(currentType,
                    new IChelaType[]{ReferenceType.Create(node.GeneratorClassInstance)}, false);

            // Create the method.
            Method method = new Method(name, MemberFlags.Public, genClass);
            method.SetFunctionType(functionType);
            genClass.AddFunction(name, method);

            // Set the explicit contract.
            if(contract != null)
                method.SetExplicitContract(contract);

            // Create the method block.
            BasicBlock block = new BasicBlock(method);
            block.SetName("top");

            // Create the return and error blocks.
            BasicBlock retBlock = new BasicBlock(method);
            retBlock.SetName("ret");

            BasicBlock errorBlock = new BasicBlock(method);
            errorBlock.SetName("error");

            // Make sure reset was called before the first Current.
            builder.SetBlock(block);
            builder.CreateLoadArg(0);
            builder.CreateLoadField(node.GeneratorState);
            builder.CreateLoadInt32(0);
            builder.CreateCmpEQ();
            builder.CreateBr(errorBlock, retBlock);

            // Raise an error if reset wasn't called.
            builder.SetBlock(errorBlock);

            Class excpt = (Class)ExtractClass(node, currentModule.GetLangMember("InvalidOperationException"));
            builder.CreateLoadString("IEnumerator.MoveNext must be called before than IEnumerator.Current.");
            builder.CreateNewObject(excpt, GetExceptionCtor(node, excpt), 1);
            builder.CreateThrow();

            // Load the yielded value.
            builder.SetBlock(retBlock);
            builder.CreateLoadArg(0);
            builder.CreateLoadField(node.YieldedValue);

            // Cast the yielded value.
            Cast(node, null, node.YieldType, currentType);

            // Return.
            builder.CreateRet();
        }

        private Function CreateGenerator_MoveNext(FunctionDefinition node)
        {
            // Get the generator class.
            Class genClass = node.GeneratorClass;

            // Create the function type.
            FunctionType functionType =
                FunctionType.Create(ChelaType.GetBoolType(),
                    new IChelaType[]{ReferenceType.Create(node.GeneratorClassInstance)}, false);

            // Create the method.
            Method method = new Method("MoveNext", MemberFlags.Public, genClass);
            method.SetFunctionType(functionType);
            method.DefinitionNode = node;
            genClass.AddFunction("MoveNext", method);

            // Swap the exception contexts.
            method.SwapExceptionContexts(currentFunction);

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = method;

            // Create the jump table block.
            BasicBlock jmpBlock = CreateBasicBlock();
            jmpBlock.SetName("jmp");

            // Generate the main code.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // Prepare returning.
            LexicalScope topScope = (LexicalScope) node.GetScope();
            PrepareReturning(node, currentFunction, topScope);

            // Create the function body.
            VisitList(node.GetChildren());

            // Finish returning.
            FinishReturn(node, currentFunction);

            // Create the state jump table.
            builder.SetBlock(jmpBlock);

            // Load the current state.
            builder.CreateLoadArg(0);
            builder.CreateLoadField(node.GeneratorState);

            // Build the jump table.
            int numstates = node.Yields.Count*2+3;
            int[] stateConstants = new int[numstates];
            BasicBlock[] stateEntries = new BasicBlock[numstates];

            // The default case is return.
            stateConstants[0] = -1;
            stateEntries[0] = currentFunction.ReturnBlock;

            // The first state is the top block.
            stateConstants[1] = 0;
            stateEntries[1] = topBlock;

            // The second state is the return block.
            stateConstants[2] = 1;
            stateEntries[2] = currentFunction.ReturnBlock;

            // The next states are the yield merges followed by yield disposes.
            for(int i = 0; i < node.Yields.Count; ++i)
            {
                ReturnStatement yieldStmnt = node.Yields[i];

                // Emit the merge state.
                int stateId = i*2+2;
                int entryIndex = stateId+1;
                stateConstants[entryIndex] = stateId;
                stateEntries[entryIndex] = yieldStmnt.MergeBlock;

                // Emit the dispose state.
                stateConstants[entryIndex+1] = stateId+1;
                stateEntries[entryIndex+1] = yieldStmnt.DisposeBlock;
            }

            builder.CreateSwitch(stateConstants, stateEntries);

            // Restore the old function.
            currentFunction = oldFunction;

            return method;
        }

        private void CreateGenerator_Dispose(FunctionDefinition node, Function moveNext)
        {
            // Get the generator class.
            Class genClass = node.GeneratorClass;

            // Create the function type.
            FunctionType functionType =
                FunctionType.Create(ChelaType.GetVoidType(),
                    new IChelaType[]{ReferenceType.Create(node.GeneratorClassInstance)}, false);

            // Create the method.
            Method method = new Method("Dispose", MemberFlags.Public, genClass);
            method.SetFunctionType(functionType);
            genClass.AddFunction("Dispose", method);

            // Create the top block.
            BasicBlock top = new BasicBlock(method);
            builder.SetBlock(top);

            // Create the return and dispose blocks.
            BasicBlock justReturn = new BasicBlock(method);
            BasicBlock disposeAndReturn = new BasicBlock(method);

            // Load the current state.
            builder.CreateLoadArg(0);
            builder.CreateDup1();
            builder.CreateDup1();
            builder.CreateLoadField(node.GeneratorState);
            builder.CreateDup1();
            builder.CreateLoadInt32(1);
            builder.CreateCmpEQ();
            builder.CreateBr(justReturn, disposeAndReturn);

            // Dispose and return implementation.
            builder.SetBlock(disposeAndReturn);

            // Increase the state.
            builder.CreateLoadInt32(1);
            builder.CreateAdd();
            builder.CreateStoreField(node.GeneratorState);

            // Call move next.
            builder.CreateCall(moveNext, 1);
            builder.CreateRetVoid();

            // Just return implementation.
            builder.SetBlock(justReturn);
            builder.CreateRetVoid();
        }

        private void CreateGenerator_Reset(FunctionDefinition node)
        {
            // Get the generator class.
            Class genClass = node.GeneratorClass;

            // Create the function type.
            FunctionType functionType =
                FunctionType.Create(ChelaType.GetVoidType(),
                    new IChelaType[]{ReferenceType.Create(node.GeneratorClassInstance)}, false);

            // Create the method.
            Method method = new Method("Reset", MemberFlags.Public, genClass);
            method.SetFunctionType(functionType);
            genClass.AddFunction("Reset", method);

            // Create the top block.
            BasicBlock top = new BasicBlock(method);
            builder.SetBlock(top);

            // Throw an exception.
            Class excpt = (Class)ExtractClass(node, currentModule.GetLangMember("InvalidOperationException"));
            builder.CreateLoadString("IEnumerator.Reset cannot be called in generators.");
            builder.CreateNewObject(excpt, GetExceptionCtor(node, excpt), 1);
            builder.CreateThrow();
        }

        private void LoadCurrentSelf()
        {
            // Load the current instance or closure.
            builder.CreateLoadArg(0);

            // Extract the instance from the generator closure.
            FunctionDefinition defNode = currentFunction.DefinitionNode;
            if(defNode != null && defNode.IsGenerator)
                builder.CreateLoadField(defNode.GeneratorSelf);
        }

        public override AstNode Visit (FunctionDefinition node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Process attributes.
            ProcessAttributes(node);

            // Get the prototype.
            FunctionPrototype prototype = node.GetPrototype();
            
            // Get the function.
            Function function = prototype.GetFunction();

            // Ignore functions without a body.
            if(node.GetChildren() == null)
                return builder.EndNode();
            
            // Store the old function.
            // TODO: Add closures
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Update the securiry.
            if(function.IsUnsafe())
                PushUnsafe();

            // Get the function lexical scope.
            LexicalScope topScope = (LexicalScope)node.GetScope();

            // Update the scope.
            PushScope(topScope);

            // Define the generator or function body
            if(node.IsGenerator)
                DefineGeneratorBody(node);
            else
                DefineFunctionBody(node, function, topScope);

            // Restore the scope.
            PopScope();

            // Restore the security.
            if(function.IsUnsafe())
                PopUnsafe();

            // Restore the current function.
            currentFunction = oldFunction;

            return builder.EndNode();
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
            // Begin the node.
            builder.BeginNode(node);

            // Get the function.
            Function function = node.GetFunction();

            // Ignore declarations.
            if(node.GetChildren() == null)
                return builder.EndNode();

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Get and update the lexical scope.
            LexicalScope topScope = (LexicalScope)node.GetScope();
            PushScope(topScope);

            // Create the top basic block.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // Prepare returning, required by exception handling.
            PrepareReturning(node, function, topScope);

            // Store the indices in local variables.
            int index = function.IsStatic() ? 0 : 1;
            foreach(LocalVariable indexLocal in node.GetIndexerVariables())
            {
                builder.CreateLoadArg((byte)index++);
                builder.CreateStoreLocal(indexLocal);
            }

            // Visit his children.
            VisitList(node.GetChildren());

            // Finish return.
            FinishReturn(node, function);

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return builder.EndNode();
        }

        public override AstNode Visit (SetAccessorDefinition node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the function.
            Function function = node.GetFunction();

            // Ignore declarations.
            if(node.GetChildren() == null)
                return builder.EndNode();

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Get and update lexical scope.
            LexicalScope topScope = (LexicalScope)node.GetScope();
            PushScope(topScope);

            // Create the top basic block.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // Prepare returning, required by exception handling.
            PrepareReturning(node, function, topScope);

            // Store the indices in local variables.
            int index = function.IsStatic() ? 0 : 1;
            foreach(LocalVariable indexLocal in node.GetIndexerVariables())
            {
                builder.CreateLoadArg((byte)index++);
                builder.CreateStoreLocal(indexLocal);
            }

            // Store the "value" argument in a local variable.
            LocalVariable valueLocal = node.GetValueLocal();
            builder.CreateLoadArg((byte)index++);
            builder.CreateStoreLocal(valueLocal);

            // Visit his children.
            VisitList(node.GetChildren());

            // Finish return.
            FinishReturn(node, function);

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return builder.EndNode();
        }

        public override AstNode Visit (EventDefinition node)
        {
            // Visit the add accessors.
            if(node.AddAccessor != null)
                node.AddAccessor.Accept(this);

            // Visit the remove accessors.
            if(node.RemoveAccessor != null)
                node.RemoveAccessor.Accept(this);

            // Define simplified events.
            if(node.GetAccessors() == null)
                CreateSimplifiedEventFunctions(node);
            return node;
        }

        private void CreateSimplifiedEventFunctions(EventDefinition node)
        {
            // Create the add function.
            CreateSimplifiedEventFunction(node, true);

            // Create the remove function
            CreateSimplifiedEventFunction(node, false);
        }

        private void CreateSimplifiedEventFunction(EventDefinition node, bool isAdd)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the event variable.
            EventVariable eventVariable = node.GetEvent();

            // Get the function.
            Function function = isAdd ? eventVariable.AddModifier : eventVariable.RemoveModifier;

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Create the top basic block.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // TODO: Add multithreading safety.

            // Load the delegate field.
            FieldVariable delegateField = eventVariable.AssociatedField;
            if(delegateField.IsStatic())
            {
                builder.CreateLoadGlobal(delegateField);
            }
            else
            {
                builder.CreateLoadArg(0);
                builder.CreateDup1();
                builder.CreateLoadField(delegateField);
            }

            // Load the value.
            if(function.IsScope())
                builder.CreateLoadArg(0);
            else
                builder.CreateLoadArg(1);

            // Combine/Remove delegates.
            string modificatorName = isAdd ? "Combine" : "Remove";
            Class delegateClass = currentModule.GetDelegateClass();
            ScopeMember modificatorMember = delegateClass.FindMemberRecursive(modificatorName);
            if(modificatorMember == null || !modificatorMember.IsFunctionGroup())
                Error(node, modificatorName + " is not a member of Delegate");

            // Use the first static function with two arguments.
            Function modificator = null;
            FunctionGroup modificatorGroup = (FunctionGroup)modificatorMember;
            foreach(FunctionGroupName gname in modificatorGroup.GetFunctions())
            {
                if(gname.IsStatic() && gname.GetFunctionType().GetArgumentCount() == 2)
                {
                    modificator = gname.GetFunction();
                    break;
                }
            }

            if(modificator == null)
                Error(node, "couldn't find a suitable " + modificatorName + " in Delegate");

            // Perform the call.
            builder.CreateCall(modificator, 2);

            // Reference cast the delegate.
            builder.CreateCast(eventVariable.GetVariableType());

            // Store the new delegate.
            if(function.IsStatic())
                builder.CreateStoreGlobal(delegateField);
            else
                builder.CreateStoreField(delegateField);

            // Return void.
            builder.CreateRetVoid();

            // Restore the current function.
            currentFunction = oldFunction;

            // End the node.
            builder.EndNode();
        }

        public override AstNode Visit (EventAccessorDefinition node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the function.
            Function function = node.GetFunction();

            // Ignore declarations.
            if(node.GetChildren() == null)
                return builder.EndNode();

            // Store the old function.
            Function oldFunction = currentFunction;
            currentFunction = function;

            // Get the function lexical scope.
            LexicalScope topScope = (LexicalScope)node.GetScope();
            PushScope(topScope);

            // Create the top basic block.
            BasicBlock topBlock = CreateBasicBlock();
            topBlock.SetName("top");
            builder.SetBlock(topBlock);

            // Prepare returning, required by exception handling.
            PrepareReturning(node, function, topScope);

            // Store the "value" argument in a local variable.
            int index = function.IsStatic() ? 0 : 1;
            LocalVariable valueLocal = node.GetValueLocal();
            builder.CreateLoadArg((byte)index++);
            builder.CreateStoreLocal(valueLocal);

            // Visit his children.
            VisitList(node.GetChildren());

            // Finish return.
            FinishReturn(node, function);

            // Restore the scope.
            PopScope();

            // Restore the current function.
            currentFunction = oldFunction;

            return builder.EndNode();
        }

        public override AstNode Visit (BlockNode node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Create the block lexical scope.
            LexicalScope blockScope = (LexicalScope) node.GetScope();
            node.SetScope(blockScope);

            // Create the new basic block.
            BasicBlock block = CreateBasicBlock();
            block.SetName("block");

            // Jump into the basic block.
            builder.CreateJmp(block);
            builder.SetBlock(block);

            // Push the scope.
            PushScope(blockScope);

            // Visit the block  children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            // Create a merge block and jump into it.
            if(!builder.IsLastTerminator())
            {
                BasicBlock mergeBlock = CreateBasicBlock();
                mergeBlock.SetName("merge");

                builder.CreateJmp(mergeBlock);
                builder.SetBlock(mergeBlock);
            }

            return builder.EndNode();
        }

        public override AstNode Visit (UnsafeBlockNode node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Push the unsafe scope.
            PushUnsafe();

            // Create the block lexical scope.
            LexicalScope blockScope = (LexicalScope) node.GetScope();
            node.SetScope(blockScope);
    
            // Create the new basic block.
            BasicBlock block = CreateBasicBlock();
            block.SetName("block");
    
            // Jump into the basic block.
            builder.CreateJmp(block);
            builder.SetBlock(block);
    
            // Push the scope.
            PushScope(blockScope);
    
            // Visit the block  children.
            VisitList(node.GetChildren());
    
            // Pop the scope.
            PopScope();

            // Create a merge block and jump into it.
            if(!builder.IsLastTerminator())
            {
                BasicBlock mergeBlock = CreateBasicBlock();
                mergeBlock.SetName("merge");
    
                builder.CreateJmp(mergeBlock);
                builder.SetBlock(mergeBlock);
            }

            // Pop the unsafe scope.
            PopUnsafe();
    
            return builder.EndNode();
        }
        
        public override AstNode Visit(LocalVariablesDeclaration node)
        {
            // Get the declarations.
            AstNode declarations = node.GetDeclarations();
            
            // Visit them.
            VisitList(declarations);

            return node;
        }

        public override AstNode Visit (UsingObjectStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Push the scope.
            PushScope(node.GetScope());

            // Visit the declarations.
            LocalVariablesDeclaration decls = node.GetLocals();
            decls.Accept(this);

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return builder.EndNode();
        }

        public override AstNode Visit (LockStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Push the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return builder.EndNode();
        }
        
        public override AstNode Visit (IfStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the condition, then and else.
            Expression condition = node.GetCondition();
            AstNode thenNode = node.GetThen();
            AstNode elseNode = node.GetElse();

            IChelaType condType = condition.GetNodeType();

            // Visit the condition.
            condition.Accept(this);
            if(condType != ChelaType.GetBoolType()) // Read variables values.
                Cast(node, condition.GetNodeValue(), condType, ChelaType.GetBoolType());
            
            // Create the basic blocks.
            BasicBlock thenBlock = CreateBasicBlock();
            BasicBlock elseBlock = CreateBasicBlock();
            thenBlock.SetName("then");
            if(elseNode != null)
                elseBlock.SetName("else");
            else
                elseBlock.SetName("merge");
            
            // Perform the branch.
            builder.CreateBr(thenBlock, elseBlock);
            builder.SetBlock(thenBlock);
            
            // Visit the then statement.
            thenNode.Accept(this);
            
            // Update the then block.
            thenBlock = builder.GetBlock();
            
            // Write the else block.
            if(elseNode != null)
            {
                // Set the current block.
                builder.SetBlock(elseBlock);
                
                // Visit the else statement.
                elseNode.Accept(this);
                
                // Update the else block.
                elseBlock = builder.GetBlock();
            }
            
            if(thenBlock.IsLastTerminator() && elseBlock.IsLastTerminator())
                return builder.EndNode();
            
            // Get or create the merge block.
            BasicBlock mergeBlock = null;
            if(elseNode != null)
            {
                mergeBlock = CreateBasicBlock();
                mergeBlock.SetName("merge");
            }
            else
            {
                mergeBlock = elseBlock;
            }
            
            // Write the block terminators.
            if(!thenBlock.IsLastTerminator())
            {
                builder.SetBlock(thenBlock);
                builder.CreateJmp(mergeBlock);
            }
            
            if(elseNode != null && !elseBlock.IsLastTerminator())
            {
                builder.SetBlock(elseBlock);
                builder.CreateJmp(mergeBlock);
            }
            
            // Continue.
            builder.SetBlock(mergeBlock);

            return builder.EndNode();
        }

        public override AstNode Visit (SwitchStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Process the switch expression.
            Expression switchExpr = node.GetConstant();
            switchExpr.Accept(this);

            // Perform coercion.
            IChelaType switchType = switchExpr.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(switchType != coercionType)
                Cast(node, switchExpr.GetNodeValue(), switchType, coercionType);

            // Create the merge block.
            BasicBlock merge = CreateBasicBlock();
            merge.SetName("smerge");

            // Store the old break.
            BasicBlock oldBreak = currentBreak;
            currentBreak = merge;

            // Initialize the default block to the merge.
            BasicBlock defaultBlock = merge;

            // Get the cases dictionary.
            IDictionary<ConstantValue, CaseLabel> caseDictionary = node.CaseDictionary;

            // Create the jump table.
            int tableSize = caseDictionary.Count + 1;
            int[] constants = new int[tableSize];
            BasicBlock[] blocks = new BasicBlock[tableSize];

            // The first element is always the default block.
            int i = 0;
            constants[i] = -1;
            blocks[i] = defaultBlock; ++i;

            // Add the other elements.
            BasicBlock caseBlock = null;
            CaseLabel label = (CaseLabel)node.GetCases();
            while(label != null)
            {
                // Create the label block.
                if(caseBlock == null)
                {
                    caseBlock = CreateBasicBlock();
                    caseBlock.SetName("caseBlock");
                }

                // Set the label block.
                label.SetBlock(caseBlock);

                // Store the default block.
                if(label.GetConstant() == null)
                {
                    defaultBlock = caseBlock;
                    blocks[0] = caseBlock;
                }
                else
                {
                    // Append it to the jump table.
                    Expression caseExpr = label.GetConstant();
                    ConstantValue caseConstant = (ConstantValue)caseExpr.GetNodeValue();
                    constants[i] = caseConstant.GetIntValue();
                    blocks[i] = caseBlock;
                    ++i;
                }

                // Create a new block if the last case wasn't empty.
                if(label.GetChildren() != null)
                    caseBlock = null;

                // Process the next label.
                label = (CaseLabel)label.GetNext();
            }

            // Perform the switch.
            BasicBlock switchBlock = builder.GetBlock();
            builder.CreateSwitch(constants, blocks);

            // Generate the cases blocks.
            VisitList(node.GetCases());

            // Continue with the normal flow.
            currentBreak = oldBreak;

            // Remove the merge block if its unreachable.
            if(merge.GetPredsCount() == 0)
            {
                builder.SetBlock(switchBlock);
                merge.Destroy();
                if(node.GetNext() != null)
                    Warning(node.GetNext(), "detected unreachable code.");
                node.SetNext(null);
            }
            else
            {
                builder.SetBlock(merge);
            }

            return builder.EndNode();
        }

        public override AstNode Visit (CaseLabel node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Set the insert point.
            builder.SetBlock(node.GetBlock());

            // Generate the children.
            AstNode children = node.GetChildren();
            VisitList(children);

            // Jump to the next node or end the switch.
            if(children != null && !builder.IsLastTerminator())
                Error(node, "a non-empty case control flow must end in a terminator such as break, goto case/default or return.");

            return builder.EndNode();
        }

        public override AstNode Visit(GotoCaseStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the constant expression.
            Expression constant = node.GetLabel();

            // Expand it.
            if(constant != null)
            {
                // Get the constant value.
                IChelaType coercionType = node.GetCoercionType();
                ConstantValue constantValue = (ConstantValue)constant.GetNodeValue();
                constantValue = constantValue.Cast(coercionType);

                // Make sure the case is in the current switch.
                IDictionary<ConstantValue, CaseLabel> caseDict = node.GetSwitch().CaseDictionary;
                CaseLabel targetLabel;
                if(!caseDict.TryGetValue(constantValue, out targetLabel))
                    Error(node, "current switch doesn't contain a case for [{1}]{0}.", constantValue.ToString(), constantValue.GetHashCode());
                node.SetTargetLabel(targetLabel);
            }

            // Jump to the target case.
            CaseLabel targetCase = node.GetTargetLabel();
            builder.CreateJmp(targetCase.GetBlock());

            // End the node.
            return builder.EndNode();
        }

        public override AstNode Visit (BreakStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Check the current break point.
            if(currentBreak == null)
                Error(node, "cannot perform break here.");

            // Perform break.
            builder.CreateJmp(currentBreak);

            // Finish the node.
            return builder.EndNode();
        }

        public override AstNode Visit (ContinueStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Check the current continue point.
            if(currentContinue == null)
                Error(node, "cannot perform continue here.");

            // Perform continue.
            builder.CreateJmp(currentContinue);

            // Finish the node.
            return builder.EndNode();
        }
        
        public override AstNode Visit (WhileStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the condition and the job.
            Expression cond = node.GetCondition();
            AstNode job = node.GetJob();
            
            // Create the basic blocks.
            BasicBlock condBlock = CreateBasicBlock();
            BasicBlock contentBlock = CreateBasicBlock();
            BasicBlock endBlock = CreateBasicBlock();
            
            // Set the blocks names.
            condBlock.SetName("cond");
            contentBlock.SetName("content");
            endBlock.SetName("end");
            
            // Store the old break and continue.
            BasicBlock oldBreak = currentBreak;
            BasicBlock oldContinue = currentContinue;
            
            // Set the new break and continue.
            currentBreak = endBlock;
            currentContinue = condBlock;            
            
            // Jump into the condition.
            builder.CreateJmp(condBlock);
            
            // Write the condition.
            builder.SetBlock(condBlock);
            cond.Accept(this);

            // Coerce the condition.
            IChelaType condType = cond.GetNodeType();
            if(condType != ChelaType.GetBoolType())
                Cast(node, cond.GetNodeValue(), condType, ChelaType.GetBoolType());

            // Perform the loop conditional branch.
            builder.CreateBr(contentBlock, endBlock);
            
            // Write the loop content.
            builder.SetBlock(contentBlock);
            VisitList(job);
            
            // Close the loop.
            if(!builder.IsLastTerminator())
                builder.CreateJmp(condBlock);
            
            // Continue as normal.
            builder.SetBlock(endBlock);
            
            // Restore the break and continue blocks.
            currentBreak = oldBreak;
            currentContinue = oldContinue;

            return builder.EndNode();
        }
        
        public override AstNode Visit (DoWhileStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the condition and the job.
            Expression cond = node.GetCondition();
            AstNode job = node.GetJob();
            
            // Create the basic blocks.
            BasicBlock condBlock = CreateBasicBlock();
            BasicBlock contentBlock = CreateBasicBlock();
            BasicBlock endBlock = CreateBasicBlock();
            
            // Set the blocks names.
            condBlock.SetName("cond");
            contentBlock.SetName("content");
            endBlock.SetName("end");
            
            // Store the old break and continue.
            BasicBlock oldBreak = currentBreak;
            BasicBlock oldContinue = currentContinue;
            
            // Set the new break and continue.
            currentBreak = endBlock;
            currentContinue = condBlock;            
            
            // Branch into the content.
            builder.CreateJmp(contentBlock);
            
            // Write the content.
            builder.SetBlock(contentBlock);
            job.Accept(this);
            
            // Branch into the condition.
            if(!builder.IsLastTerminator())
                builder.CreateJmp(condBlock);
        
            // Write the condition.
            builder.SetBlock(condBlock);
            cond.Accept(this);

            // Cast the condition.
            IChelaType condType = cond.GetNodeType();
            if(condType != ChelaType.GetBoolType())
                Cast(node, cond.GetNodeValue(), condType, ChelaType.GetBoolType());
            
            // Close the loop.
            builder.CreateBr(contentBlock, endBlock);
            
            // Continue with the normal flow.
            builder.SetBlock(endBlock);
            
            // Restore the break and continue blocks.
            currentBreak = oldBreak;
            currentContinue = oldContinue;

            return builder.EndNode();
        }
        
        public override AstNode Visit (ForStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the for elements.
            AstNode decl = node.GetDecls();
            AstNode cond = node.GetCondition();
            AstNode incr = node.GetIncr();
            
            // Write the declaration.
            if(decl != null)
                decl.Accept(this);
            
            // Create the basics blocks.
            BasicBlock forStart = CreateBasicBlock();
            BasicBlock forEnd = CreateBasicBlock();
            BasicBlock forBreak = CreateBasicBlock();
            BasicBlock forCond = forStart;
            
            // Set the blocks names.
            forStart.SetName("for_start");
            forEnd.SetName("for_end");
            forBreak.SetName("for_break");
            
            // Store and update the break and continue points.
            BasicBlock oldBreak = currentBreak;
            BasicBlock oldContinue = currentContinue;
            currentBreak = forBreak;
            currentContinue = forEnd;
            
            // Write the condition.
            if(cond != null)
            {
                // Create the condition block.
                forCond = CreateBasicBlock();
                forCond.SetName("for_cond");
                
                // Jump into the condition.
                builder.CreateJmp(forCond);
                builder.SetBlock(forCond);
                
                // Write the condition.
                cond.Accept(this);

                // Cast the condition.
                IChelaType condType = cond.GetNodeType();
                if(condType != ChelaType.GetBoolType())
                    Cast(node, cond.GetNodeValue(), condType, ChelaType.GetBoolType());
                
                // Perform the conditional branch.
                builder.CreateBr(forStart, forBreak);
            }
            else
            {
                // Jump into the beginning of the loop.
                builder.CreateJmp(forStart);
            }
            
            // Write the loop content.
            builder.SetBlock(forStart);
            VisitList(node.GetChildren());
            
            // Branch into the loop end.
            if(!builder.IsLastTerminator())
                builder.CreateJmp(forEnd);
            
            // Write the for end.
            builder.SetBlock(forEnd);
            
            // Write the increment.
            if(incr != null)
                incr.Accept(this);
            
            // Branch into the condition.
            if(!builder.IsLastTerminator())
                builder.CreateJmp(forCond);
            
            // Continue with the rest of the program.
            builder.SetBlock(forBreak);
            
            // Restore the break and continue point.
            currentBreak = oldBreak;
            currentContinue = oldContinue;

            return builder.EndNode();
        }

        public override AstNode Visit (ForEachStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Push the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return builder.EndNode();
        }

        public override AstNode Visit (FixedVariableDecl node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the fixed local.
            LocalVariable local = node.GetVariable();

            // Get the value expression.
            Expression valueExpr = node.GetValueExpression();
            valueExpr.Accept(this);

            // Get the associated types.
            IChelaType type = node.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            IChelaType valueType = valueExpr.GetNodeType();

            // Perform coercion.
            if(valueType != coercionType )
                Cast(node, valueExpr.GetNodeValue(), valueType, coercionType);

            // Pin references, cast pointers.
            if(coercionType.IsPointer())
            {
                // Cast the pointer.
                if(coercionType != type)
                    Cast(node, null, coercionType, type);

                // Store the pointer in a local.
                builder.CreateStoreLocal(local);
            }
            else
            {
                // References.
                valueType = DeReferenceType(coercionType);

                // Give a null value for null references.
                builder.CreateDup1();

                // Create the blocks for the comparison
                BasicBlock notNullBlock = CreateBasicBlock();
                notNullBlock.SetName("notNull");

                BasicBlock nullBlock = CreateBasicBlock();
                nullBlock.SetName("null");

                BasicBlock nullMerge = CreateBasicBlock();
                nullMerge.SetName("nullMerge");

                // Compare to null.
                builder.CreateLoadNull();
                builder.CreateCmpEQ();
                builder.CreateBr(nullBlock, notNullBlock);

                // Set the null block content.
                builder.SetBlock(nullBlock);
                builder.CreatePop();
                builder.CreateLoadNull();
                builder.CreateStoreLocal(local);
                builder.CreateJmp(nullMerge);

                // Set the not null content.
                builder.SetBlock(notNullBlock);

                // Use the first element of the array.
                if(valueType.IsArray())
                {
                    ArrayType array = (ArrayType)valueType;
                    for(int i = 0; i < array.GetDimensions(); ++i)
                        builder.CreateLoadUInt8(0);
                    builder.CreateLoadArraySlotAddr(array);

                    // Cast the result.
                    IChelaType slotType = PointerType.Create(array.GetValueType());
                    if(slotType != type)
                        Cast(node, null, slotType, type);
                }
                else
                    Error(node, "unsupported fixed object of type {0}.", valueType.GetName());

                // Store the value.
                builder.CreateStoreLocal(local);

                // Merge the results.
                builder.CreateJmp(nullMerge);
                builder.SetBlock(nullMerge);
            }

            // End.
            return builder.EndNode();
        }

        public override AstNode Visit (FixedStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Push the scope.
            PushScope(node.GetScope());

            // Create a block for the fixed statement.
            BasicBlock fixedBlock = CreateBasicBlock();
            fixedBlock.SetName("fixed");

            // Enter into the fixed block.
            builder.CreateJmp(fixedBlock);
            builder.SetBlock(fixedBlock);

            // Visit the declarations.
            VisitList(node.GetDeclarations());

            // Visit the children.
            VisitList(node.GetChildren());

            // Remove unreachable code.
            if(builder.IsLastTerminator())
            {
                AstNode next = node.GetNext();
                if(next != null)
                    Warning(next, "found unreachable code.");
                node.SetNext(null);
            }
            else
            {
                // Merge the block.
                BasicBlock merge = CreateBasicBlock();
                merge.SetName("fmerge");
                builder.CreateJmp(merge);
                builder.SetBlock(merge);
            }

            // Restore the scope.
            PopScope();

            return builder.EndNode();
        }

        public override AstNode Visit(VariableDeclaration node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the initial value.
            Expression initial = node.GetInitialValue();
            if(initial == null)
                return builder.EndNode();
            
            // Visit the initial value.
            initial.Accept(this);
            
            // Get the initial value type and the node type.
            object value = initial.GetNodeValue();
            IChelaType valueType = initial.GetNodeType();
            IChelaType varType = node.GetNodeType();
            
            // Cast the initial value.
            if(valueType != varType)
                Cast(node, value, valueType, varType);
            
            // Store the value.
            PerformAssignment(node, node.GetVariable());

            return builder.EndNode();
        }
        
        public override AstNode Visit (ReturnStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the return value expression
            Expression expr = node.GetExpression();
            IChelaType coercionType = node.GetCoercionType();
            if(expr != null)
            {
                // Visit the expression.
                expr.Accept(this);
                
                // Get the expression type.
                object exprValue = expr.GetNodeValue();
                IChelaType exprType = expr.GetNodeType();
                
                // Perform coercion.
                if(exprType != coercionType)
                    Cast(node, exprValue, exprType, coercionType);
            }

            // Compute the return block.
            BasicBlock returnBlock = null;
            if(currentFunction.ReturnBlock != null)
            {
                ExceptionContext context = currentExceptionContext;
                while(context != null)
                {
                    returnBlock = context.GetCleanup();
                    if(returnBlock != null)
                        break;
                    context = context.GetParentContext();
                }

                if(returnBlock == null)
                    returnBlock = currentFunction.ReturnBlock;
            }

            // Perform yielding.
            if(node.IsYield)
            {
                // Get the generator data.
                FunctionDefinition defNode = currentFunction.DefinitionNode;

                // Store the yielded value.
                builder.CreateLoadArg(0);
                builder.CreatePush(1);
                builder.CreateStoreField(defNode.YieldedValue);
                builder.CreatePop();

                // Store the new state.
                builder.CreateLoadArg(0);
                builder.CreateLoadInt32(node.YieldState);
                builder.CreateStoreField(defNode.GeneratorState);

                // Return the yielded value.
                builder.CreateLoadBool(true);
                builder.CreateRet();

                // Create the merge block.
                BasicBlock mergeBlock = CreateBasicBlock();
                mergeBlock.SetName("merge");
                builder.SetBlock(mergeBlock);

                // Store the merge and dispose blocks.
                node.MergeBlock = mergeBlock;
                node.DisposeBlock = returnBlock;
                return builder.EndNode();
            }

            // Return the value.
            bool isVoid = coercionType == ChelaType.GetVoidType();
            if(currentFunction.ReturnBlock != null)
            {
                // Store the return value.
                if(!isVoid)
                    builder.CreateStoreLocal(currentFunction.ReturnValueVar);

                // Store the returning flag.
                builder.CreateLoadBool(true);
                builder.CreateStoreLocal(currentFunction.ReturningVar);

                // Perform delayed returning.
                builder.CreateJmp(returnBlock);
            }
            else
            {
                if(isVoid)
                    builder.CreateRetVoid();
                else
                    builder.CreateRet();
            }

            return builder.EndNode();
        }
        
        public override AstNode Visit (ExpressionStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expression.
            Expression expr = node.GetExpression();
            
            // Visit it.
            expr.Accept(this);
            
            // Get his type.
            IChelaType type = expr.GetNodeType();
            
            // Ignore void expression.
            if(type == ChelaType.GetVoidType())
                return builder.EndNode();
            
            // Special treatment for reference expressions.
            if(type.IsReference())
            {
                ReferenceType refType = (ReferenceType) type;
                type = refType.GetReferencedType();
                if(expr.GetNodeValue() != null)
                {
                    // Remove the variable reference.
                    Variable variable = (Variable)expr.GetNodeValue();
                    uint args = GetVariableParameters(node, variable);
                    for(uint i = 0; i < args; i++)
                        builder.CreatePop();
                }
                else
                {
                    // Remove the reference.
                    builder.CreatePop();
                }
            }
            else
            {
                // Pop the expression value.
                builder.CreatePop();
            }

            return builder.EndNode();
        }

        // Only stack parameters.
        private uint GetVariableParameters(AstNode node, Variable variable)
        {
            if(variable.IsLocal())
            {
                return 0u;
            }
            else if(variable.IsField())
            {
                FieldVariable field = (FieldVariable) variable;
                if(!field.IsStatic())
                    return 1u;
                else
                    return 0u;
            }
            else if(variable.IsReferencedSlot() || variable.IsPointedSlot())
            {
                return 1u;
            }
            else if(variable.IsSwizzleVariable())
            {
                return 1u;
            }
            else if(variable.IsArraySlot())
            {
                ArraySlot slot = (ArraySlot)variable;
                return (uint) (slot.Dimensions + 1);
            }
            else if(variable.IsProperty())
            {
                PropertyVariable property = (PropertyVariable)variable;
                if(!property.IsStatic())
                    return (uint)(property.GetIndexCount() + 1);
                else
                    return (uint)property.GetIndexCount();
            }
            else if(variable.IsEvent())
            {
                EventVariable eventVar = (EventVariable)variable;
                if(eventVar.IsStatic())
                    return 0;
                else
                    return 1;
            }
            else
                Error(node, "unimplemented variable type.");

            // Shouldn't reach here.
            return 0u;
        }

        private void DuplicateReference(AstNode node, Variable variable)
        {
            // Get the parameter count.
            uint args = GetVariableParameters(node, variable);
            if(args == 0u)
                return;
            else if(args == 1u)
                builder.CreateDup1();
            else if(args == 2u)
                builder.CreateDup2();
            else
                builder.CreateDup(args);
        }

        private void PerformAssignment(AstNode node, Variable variable)
        {
            // Store the value into the variable.
            if(variable.IsLocal())
            {
                LocalVariable local = (LocalVariable)variable;
                if(local.IsPseudoLocal)
                {
                    // Store the closure local.
                    builder.CreateLoadArg(0);
                    builder.CreatePush(1);
                    builder.CreateStoreField((FieldVariable)local.ActualVariable);
                    builder.CreatePop();
                }
                else
                {
                    builder.CreateStoreLocal(local);
                }
            }
            else if(variable.IsField())
            {
                FieldVariable field = (FieldVariable) variable;
                if(field.IsStatic())
                    builder.CreateStoreGlobal(field);
                else
                    builder.CreateStoreField(field);
            }
            else if(variable.IsArraySlot())
            {
                ArraySlot slot = (ArraySlot)variable;
                builder.CreateStoreArraySlot(slot.GetArrayType());
            }
            else if(variable.IsReferencedSlot() || variable.IsPointedSlot())
            {
                builder.CreateStoreValue();
            }
            else if(variable.IsSwizzleVariable())
            {
                SwizzleVariable swizzle = (SwizzleVariable)variable;
                if(!swizzle.IsSettable())
                    Error(node, "cannot set vector members.");
                builder.CreateStoreSwizzle(swizzle.Components, swizzle.Mask);
            }
            else if(variable.IsProperty() || variable.IsDirectPropertySlot())
            {
                // Get the property.
                PropertyVariable property;
                bool direct = false;
                if(variable.IsDirectPropertySlot())
                {
                    DirectPropertySlot directProp = (DirectPropertySlot)variable;
                    property = directProp.Property;
                    direct = true;
                }
                else
                {
                    property = (PropertyVariable)variable;
                }

                if(property.SetAccessor == null)
                    Error(node, "cannot set property without a set accessor.");

                Function setAccessor = property.SetAccessor;
                uint argCount = (uint)setAccessor.GetFunctionType().GetArgumentCount();
                if(setAccessor.IsMethod())
                {
                    Method method = (Method)setAccessor;

                    if((method.IsOverride() || method.IsVirtual() || method.IsAbstract() || method.IsContract())
                       && !method.GetParentScope().IsStructure() && !direct)
                        builder.CreateCallVirtual(method, argCount);
                    else
                        builder.CreateCall(method, argCount);
                }
                else
                {
                    builder.CreateCall(setAccessor, argCount);
                }
            }
            else if(variable.IsEvent())
            {
                // Make sure the field is accessible.
                EventVariable eventVar = (EventVariable)variable;
                FieldVariable field = eventVar.AssociatedField;
                if(field == null)
                    Error(node, "cannot invoke/modify custom explicit event.");

                // Check the field access.
                CheckMemberVisibility(node, field);

                // Assign the field.
                PerformAssignment(node, field);
            }
            else
                Error(node, "unimplemented variable type.");
        }

        public override AstNode Visit (AssignmentExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the variable and his value.
            Expression variable = node.GetVariable();
            Expression value = node.GetValue();
            
            // Get the types.
            //IChelaType variableType = variable.GetNodeType();
            IChelaType valueType = value.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();

            // Get the variable data.
            Variable variableData = (Variable)variable.GetNodeValue();

            // Visit the variable.
            variable.Accept(this);

            // Duplicate the reference.
            DuplicateReference(node, variableData);

            // Visit the value.
            value.Accept(this);

            // Check for delayed coercion
            if(node.DelayedCoercion)
            {
                ConstantValue constant = (ConstantValue) value.GetNodeValue();
                uint size = coercionType.GetSize();
                if(coercionType.IsUnsigned())
                {
                    // Check unsigned ranges.
                    ulong cval = constant.GetULongValue();
                    if(size <= 1 && cval > 0xFFu ||
                       size <= 2 && cval > 0xFFFFu ||
                       size <= 4 && cval > 0xFFFFFFFFu)
                        Error(node, "cannot implicitly convert {0} into a {1}.", cval, coercionType.GetName());
                }
                else
                {
                    // Check for unsigned constraint.
                    long cval = constant.GetLongValue();
                    if(cval < 0)
                    {
                        if(coercionType.IsUnsigned())
                            Error(node, "cannot convert signed {0} into {1}.", cval, coercionType.GetName());
                        cval = -cval;
                    }

                    // Check signed ranges.
                    if(size <= 1 && cval > 0x7Fu ||
                       size <= 2 && cval > 0x7FFFu ||
                       size <= 4 && cval > 0x7FFFFFFFu)
                        Error(node, "cannot implicitly convert {0} into a {1}.", cval, coercionType.GetName());
                }
            }

            // Cast the value.
            if(valueType != coercionType)
                Cast(node, value.GetNodeValue(), valueType, coercionType);

            // Store the value into the variable.
            PerformAssignment(node, variableData);

            // Set the node value.
            node.SetNodeValue(variableData);

            return builder.EndNode();
        }
        
        public override AstNode Visit (UnaryOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Evaluate first the expression.
            Expression expr = node.GetExpression();
            expr.Accept(this);

            // Perform coercion.
            IChelaType exprType = expr.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(exprType != coercionType)
                Cast(node, expr.GetNodeValue(), exprType, coercionType);
            
            // Perform the operation.
            switch(node.GetOperation())
            {
            case UnaryOperation.OpNop:
                // Do nothing.
                break;
            case UnaryOperation.OpNeg:
                builder.CreateNeg();
                break;
            case UnaryOperation.OpNot:
            case UnaryOperation.OpBitNot:
                builder.CreateNot();
                break;
            }

            return builder.EndNode();
        }
        
        public override AstNode Visit (BinaryOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expressions.
            Expression left = node.GetLeftExpression();
            Expression right = node.GetRightExpression();
            
            // Get the types.
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            IChelaType secondCoercion = node.GetSecondCoercion();
            IChelaType operationType = node.GetOperationType();
            IChelaType destType = node.GetNodeType();

            // Special pipeline for operator overloading.
            Function overload = node.GetOverload();
            if(overload != null)
            {
                FunctionType overloadType = overload.GetFunctionType();
                coercionType = overloadType.GetArgument(0);
                secondCoercion = overloadType.GetArgument(1);

                // Send first the left operand.
                left.Accept(this);
                if(leftType != coercionType)
                    Cast(node, left.GetNodeValue(), leftType, coercionType);

                // Now send the right operand.
                right.Accept(this);
                if(rightType != secondCoercion)
                    Cast(node, right.GetNodeValue(), rightType, secondCoercion);

                // Perform the call.
                builder.CreateCall(overload, 2);

                // Return the node.
                return builder.EndNode();
            }

            // Send the left operand.
            left.Accept(this);
            if(leftType != coercionType)
                Cast(node, left.GetNodeValue(), leftType, coercionType);

            // Short circuit operations has special evaluation orders.
            if(node.GetOperation() == BinaryOperation.OpLAnd ||
               node.GetOperation() == BinaryOperation.OpLOr)
            {
                BasicBlock continueBlock = CreateBasicBlock();
                continueBlock.SetName("shc");

                BasicBlock stopBlock = CreateBasicBlock();
                stopBlock.SetName("shs");

                BasicBlock mergeBlock = CreateBasicBlock();
                mergeBlock.SetName("shm");

                // Perform left branch.
                if(node.GetOperation() == BinaryOperation.OpLAnd)
                    builder.CreateBr(continueBlock, stopBlock);
                else
                    builder.CreateBr(stopBlock, continueBlock);

                // Build stop.
                builder.SetBlock(stopBlock);
                builder.CreateLoadBool(node.GetOperation() == BinaryOperation.OpLOr);
                builder.CreateJmp(mergeBlock);

                // Build continue block.
                builder.SetBlock(continueBlock);

                // Send the right operand verbatim.
                right.Accept(this);
                if(rightType != secondCoercion)
                    Cast(node, right.GetNodeValue(), rightType, secondCoercion);
                builder.CreateJmp(mergeBlock);

                // Continue with the control flow.
                builder.SetBlock(mergeBlock);
                return builder.EndNode();
            }

            // Send the right operand.
            right.Accept(this);
            if(rightType != secondCoercion)
                Cast(node, right.GetNodeValue(), rightType, secondCoercion);
    
            switch(node.GetOperation())
            {
            case BinaryOperation.OpAdd:
                builder.CreateAdd();
                break;
            case BinaryOperation.OpSub:
                builder.CreateSub();
                break;
            case BinaryOperation.OpMul:
                if(node.IsMatrixMul())
                    builder.CreateMatMul();
                else
                    builder.CreateMul();
                break;
            case BinaryOperation.OpDiv:
                builder.CreateDiv();
                break;
            case BinaryOperation.OpMod:
                builder.CreateMod();
                break;
            case BinaryOperation.OpBitAnd:
                builder.CreateAnd();
                break;
            case BinaryOperation.OpBitOr:
                builder.CreateOr();
                break;
            case BinaryOperation.OpBitXor:
                builder.CreateXor();
                break;
            case BinaryOperation.OpBitLeft:
                builder.CreateShLeft();
                break;
            case BinaryOperation.OpBitRight:
                builder.CreateShRight();
                break;
            case BinaryOperation.OpLT:
                builder.CreateCmpLT();
                break;
            case BinaryOperation.OpGT:
                builder.CreateCmpGT();
                break;
            case BinaryOperation.OpEQ:
                builder.CreateCmpEQ();
                break;
            case BinaryOperation.OpNEQ:
                builder.CreateCmpNE();
                break;
            case BinaryOperation.OpLEQ:
                builder.CreateCmpLE();
                break;
            case BinaryOperation.OpGEQ:
                builder.CreateCmpGE();
                break;
            case BinaryOperation.OpLAnd:
            case BinaryOperation.OpLOr:
                // Shouldn't reach here.
                break;
            }

            // Cast the result.
            if(operationType != destType)
                Cast(node, null, operationType, destType);

            return builder.EndNode();
        }

        public override AstNode Visit (BinaryAssignOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expressions.
            Expression left = node.GetVariable();
            Expression right = node.GetValue();

            // Get the types.
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            IChelaType secondCoercion = node.GetSecondCoercion();

            // Get the variable.
            Variable variable = (Variable)left.GetNodeValue();
            left.Accept(this);

            // Duplicate the variable reference.
            DuplicateReference(node, variable);

            // If the variable is an event,
            if(variable.IsEvent())
            {
                // Use event functions.
                EventVariable eventVar = (EventVariable)variable;

                // Send the argument.
                right.Accept(this);
                Cast(node, right.GetNodeValue(), rightType, secondCoercion);

                // Select the correct function.
                Function modifier = null;
                if(node.GetOperation() == BinaryOperation.OpAdd)
                    modifier = eventVar.AddModifier;
                else
                    modifier = eventVar.RemoveModifier;

                // Invoke the modifier.
                if(!modifier.IsStatic())
                {
                    Method method = (Method)modifier;
                    if(method.IsVirtual())
                        builder.CreateCallVirtual(method, 2);
                    else
                        builder.CreateCall(method, 2);
                }
                else
                {
                    builder.CreateCall(modifier, 1);
                }

                // Return the node.
                return builder.EndNode();
            }

            // Duplicate again.
            DuplicateReference(node, variable);

            // Read the variable.
            Cast(node, variable, leftType, coercionType);

            // Send the right operand.
            right.Accept(this);
            if(rightType != secondCoercion)
                Cast(node, right.GetNodeValue(), rightType, secondCoercion);

            // Get the overloaded operator.
            Function op = node.GetOverload();

            // Perform the operation
            if(op != null)
            {
                // Use the overloaded operation.
                builder.CreateCall(op, 2);

                // Coerce the result.
                IChelaType opResult = op.GetFunctionType().GetReturnType();
                if(opResult != coercionType)
                    builder.CreateCast(coercionType);
            }
            else
            {
                switch(node.GetOperation())
                {
                case BinaryOperation.OpAdd:
                    builder.CreateAdd();
                    break;
                case BinaryOperation.OpSub:
                    builder.CreateSub();
                    break;
                case BinaryOperation.OpMul:
                    builder.CreateMul();
                    break;
                case BinaryOperation.OpDiv:
                    builder.CreateDiv();
                    break;
                case BinaryOperation.OpMod:
                    builder.CreateMod();
                    break;
                case BinaryOperation.OpBitAnd:
                    builder.CreateAnd();
                    break;
                case BinaryOperation.OpBitOr:
                    builder.CreateOr();
                    break;
                case BinaryOperation.OpBitXor:
                    builder.CreateXor();
                    break;
                case BinaryOperation.OpBitLeft:
                    builder.CreateShLeft();
                    break;
                case BinaryOperation.OpBitRight:
                    builder.CreateShRight();
                    break;
                case BinaryOperation.OpLT:
                case BinaryOperation.OpGT:
                case BinaryOperation.OpEQ:
                case BinaryOperation.OpNEQ:
                case BinaryOperation.OpLEQ:
                case BinaryOperation.OpGEQ:
                case BinaryOperation.OpLAnd:
                case BinaryOperation.OpLOr:
                    // Shouldn't reach here.
                    break;
                }
            }

            // Now, perform the assignment.
            PerformAssignment(node, variable);

            // Set the node value.
            node.SetNodeValue(variable);

            return builder.EndNode();
        }

        public override AstNode Visit (PrefixOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expressions.
            Expression variableExpr = node.GetVariable();

            // Get the types.
            IChelaType variableType = variableExpr.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();

            // Get the variable.
            Variable variable = (Variable)variableExpr.GetNodeValue();
            variableExpr.Accept(this);

            // Duplicate the variable twice reference.
            DuplicateReference(node, variable);
            DuplicateReference(node, variable);

            // Read the variable.
            Cast(node, variable, variableType, coercionType);

            // Send the right operand.
            if(coercionType.IsPointer())
            {
                builder.CreateLoadUInt32(1);
            }
            else
            {
                builder.CreateLoadInt32(1);
                if(ChelaType.GetIntType() != coercionType)
                    Cast(node, null, ChelaType.GetIntType(), coercionType);
            }

            // Perform the operation
            switch(node.GetOperation())
            {
            case PrefixOperation.Increment:
                builder.CreateAdd();
                break;
            case PrefixOperation.Decrement:
                builder.CreateSub();
                break;
            default:
                Error(node, "invalid prefix operation.");
                break;
            }

            // Now, perform the assignment.
            PerformAssignment(node, variable);

            // Set the node value.
            node.SetNodeValue(variable);

            return builder.EndNode();
        }

        public override AstNode Visit (PostfixOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expressions.
            Expression variableExpr = node.GetVariable();

            // Get the types.
            IChelaType variableType = variableExpr.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();

            // Get the variable.
            Variable variable = (Variable)variableExpr.GetNodeValue();
            variableExpr.Accept(this);

            // Duplicate the variable reference.
            DuplicateReference(node, variable);

            // Read the variable.
            Cast(node, variable, variableType, coercionType);

            // Duplicate the value and reference.
            uint args = GetVariableParameters(node, variable) + 1u;
            if(args == 1u)
                builder.CreateDup1();
            else if(args == 2u)
                builder.CreateDup2();
            else
                builder.CreateDup(args);

            // Send the right operand.
            if(coercionType.IsPointer())
            {
                builder.CreateLoadUInt32(1);
            }
            else
            {
                builder.CreateLoadInt32(1);
                if(ChelaType.GetIntType() != coercionType)
                    Cast(node, null, ChelaType.GetIntType(), coercionType);
            }

            // Perform the operation
            switch(node.GetOperation())
            {
            case PostfixOperation.Increment:
                builder.CreateAdd();
                break;
            case PostfixOperation.Decrement:
                builder.CreateSub();
                break;
            default:
                Error(node, "invalid prefix operation.");
                break;
            }

            // Now, perform the assignment.
            PerformAssignment(node, variable);

            // Remove the extra reference.
            for(uint i = 1; i < args; i++)
                builder.CreateRemove(1u);

            return builder.EndNode();
        }

        public override AstNode Visit (TernaryOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the expressions.
            Expression cond = node.GetCondExpression();
            Expression left = node.GetLeftExpression();
            Expression right = node.GetRightExpression();

            // Get the types.
            IChelaType condType = cond.GetNodeType();
            IChelaType leftType = left.GetNodeType();
            IChelaType rightType = right.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();

            // Create the basic blocks.
            BasicBlock trueBlock = CreateBasicBlock();
            trueBlock.SetName("ttrue");
            BasicBlock falseBlock = CreateBasicBlock();
            falseBlock.SetName("tfalse");
            BasicBlock mergeBlock = CreateBasicBlock();
            mergeBlock.SetName("tmerge");

            // Check the condition.
            cond.Accept(this);
            if(condType != ChelaType.GetBoolType())
                Cast(node, cond.GetNodeValue(), condType, ChelaType.GetBoolType());

            // Perform branching.
            builder.CreateBr(trueBlock, falseBlock);

            // Send the "true" operand.
            builder.SetBlock(trueBlock);
            left.Accept(this);
            if(leftType != coercionType)
                Cast(node, left.GetNodeValue(), leftType, coercionType);

            builder.CreateJmp(mergeBlock); // merge.

            // Send the "false" operand
            builder.SetBlock(falseBlock);
            right.Accept(this);
            if(rightType != coercionType)
                Cast(node, right.GetNodeValue(), rightType, coercionType);

            builder.CreateJmp(mergeBlock); // merge.

            // Continue as normal.
            builder.SetBlock(mergeBlock);

            return builder.EndNode();
        }

        public override AstNode Visit (ConstructorInitializer node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the constructor and his type.
            Method constructor = (Method)node.GetConstructor();
            FunctionType functionType = constructor.GetFunctionType();

            // Push the implicit this.
            builder.CreateLoadArg(0);

            // Push the arguments.
            AstNode argExpr = node.GetArguments();
            int index = 1; // For the implicit this
            while(argExpr != null)
            {
                // Visit the argument.
                argExpr.Accept(this);

                // Coerce it.
                IChelaType argExprType = argExpr.GetNodeType();
                IChelaType argType = functionType.GetArgument(index++);
                if(argType != argExprType)
                    Cast(node, argExpr.GetNodeValue(), argExprType, argType);

                argExpr = argExpr.GetNext();
            }

            // Perform the constructor call.
            int numargs = index;
            builder.CreateCall(constructor, (uint)numargs);

            return builder.EndNode();
        }

        private AstNode VectorConstruction(CallExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Retrieve the types.
            IChelaType targetType = node.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();

            // Push the arguments.
            AstNode argExpr = node.GetArguments();
            while(argExpr != null)
            {
                // Visit the argument.
                argExpr.Accept(this);
                
                // Coerce it.
                IChelaType argExprType = argExpr.GetNodeType();
                if(argExprType != coercionType)
                {
                    // De-ref and de-const
                    IChelaType argCoercionType = argExprType;
                    if(argCoercionType.IsReference())
                        argCoercionType = DeReferenceType(argCoercionType);
                    if(argCoercionType.IsConstant())
                        argCoercionType = DeConstType(argCoercionType);

                    if(argCoercionType.IsPrimitive())
                    {
                        // Use normal casting.
                        Cast(node, argExpr.GetNodeValue(), argExprType, coercionType);
                    }
                    else
                    {
                        // Coercion to a vector with the same size.
                        VectorType argVector = (VectorType)argCoercionType;
                        argCoercionType = VectorType.Create(coercionType, argVector.GetNumComponents());
                        if(argCoercionType != argExprType)
                            Cast(node, argExpr.GetNodeValue(), argExprType, argCoercionType);
                    }

                }

                // Process the next argument.
                argExpr = argExpr.GetNext();
            }

            // Create the vector.
            if(targetType.IsVector())
                builder.CreateNewVector(targetType);
            else
                builder.CreateNewMatrix(targetType);
            return builder.EndNode();
        }

        public override AstNode Visit (CallExpression node)
        {
            if(node.IsVectorConstruction())
                return VectorConstruction(node);

            // Begin the node.
            builder.BeginNode(node);

            // Get the functional expression.
            Expression functionalExpression = node.GetFunction();
            
            // Get the function type.
            FunctionType functionType = (FunctionType)node.GetCoercionType(); 
            
            // Visit the functional expression.
            functionalExpression.Accept(this);

            // Check if suppressing virtual calls.
            FunctionGroupSelector selector = functionalExpression.GetNodeValue() as FunctionGroupSelector;
            bool implicitThis = node.HasImplicitArgument();
            bool suppressVirtual = false;
            Function function = node.GetSelectedFunction();
            if(selector != null)
            {
                suppressVirtual = selector.SuppressVirtual;
                implicitThis = implicitThis || (selector.ImplicitThis && !function.IsStatic());
            }

            // Cast the function expression.
            IChelaType exprType = functionalExpression.GetNodeType();
            IChelaType functionCoercion = exprType;

            // Load variables.
            if(functionCoercion.IsReference())
               functionCoercion = DeReferenceType(exprType);
            if(exprType != functionCoercion)
                Cast(node, functionalExpression.GetNodeValue(), exprType, functionCoercion);

            // Push the arguments.
            AstNode argExpr = node.GetArguments();
            int index = implicitThis ? 1 : 0;
            while(argExpr != null)
            {
                // Visit the argument.
                argExpr.Accept(this);

                // Don't cast reference argument
                Variable argVariable = argExpr.GetNodeValue() as Variable;
                bool refArg = argVariable != null && argVariable.IsTemporalReferencedSlot();

                // Coerce it.
                IChelaType argExprType = argExpr.GetNodeType();
                IChelaType argType = functionType.GetArgument(index++);
                if(argType != argExprType && !refArg)
                    Cast(node, argExpr.GetNodeValue(), argExprType, argType);

                // Push the next argument.
                argExpr = argExpr.GetNext();
            }

            // Store the number of arguments.
            int numargs = index;

            // Perform a call if the function is known, otherwise an indirect call.
            if(function != null)
            {
                // Use different opcode for virtual calls.
                if(function.IsKernelEntryPoint())
                {
                    KernelEntryPoint entryPoint = (KernelEntryPoint)function;
                    builder.CreateBindKernel(DeReferenceType(node.GetNodeType()), entryPoint.Kernel);
                }
                else if(function.IsConstructor())
                    builder.CreateNewStruct(node.GetNodeType(), function, (uint)(numargs-1));
                else if(!function.IsVirtualCallable() || function.GetParentScope().IsStructure() || suppressVirtual)
                    builder.CreateCall(function, (uint)numargs);
                else
                    builder.CreateCallVirtual(function, (uint)numargs);
            }
            else
            {
                // Call the object.
                builder.CreateCallIndirect((uint)numargs);
            }

            return builder.EndNode();
        }

        public override AstNode Visit (NewExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the functional expression.
            Method constructor = node.GetConstructor();

            // Create the object.
            IChelaType objectType = node.GetObjectType();
            if(constructor == null)
            {
                if(objectType.IsStructure())
                    builder.CreateNewStruct(objectType, null, 0);
                else
                    builder.CreateNewObject(node.GetObjectType(), null, 0);
                return builder.EndNode();
            }

            // Get the function type.
            FunctionType functionType = (FunctionType)constructor.GetFunctionType();

            // Push the arguments.
            AstNode argExpr = node.GetArguments();
            int index = 1;
            while(argExpr != null)
            {
                // Visit the argument.
                argExpr.Accept(this);

                // Coerce it.
                IChelaType argExprType = argExpr.GetNodeType();
                IChelaType argType = functionType.GetArgument(index++);
                if(argType != argExprType)
                    Cast(node, argExpr.GetNodeValue(), argExprType, argType);

                // Read the next argument.
                argExpr = argExpr.GetNext();
            }

            // Store the number of arguments.
            int numargs = index-1;

            // Create the object.
            if(objectType.IsStructure())
                builder.CreateNewStruct(objectType, constructor, (uint)numargs);
            else
                builder.CreateNewObject(objectType, constructor, (uint)numargs);

            return builder.EndNode();
        }

        public override AstNode Visit (NewRawExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the functional expression.
            Method constructor = node.GetConstructor();
        
            // Emit a create new object for primitives.
            if(constructor == null)
            {
                builder.CreateNewRawObject(node.GetObjectType(), null, 0);
                return builder.EndNode();
            }
            
            // Get the function type.
            FunctionType functionType = (FunctionType)constructor.GetFunctionType();
            
            // Push the arguments.
            AstNode argExpr = node.GetArguments();
            int index = 1;
            while(argExpr != null)
            {
                // Visit the argument.
                argExpr.Accept(this);
                
                // Coerce it.
                IChelaType argExprType = argExpr.GetNodeType();
                IChelaType argType = functionType.GetArgument(index++);
                if(argType != argExprType)
                    Cast(node, argExpr.GetNodeValue(), argExprType, argType);

                // Read the next argument.
                argExpr = argExpr.GetNext();
            }

            // Store the number of arguments.
            int numargs = index-1;

            // Create the object.
            if(node.IsHeapAlloc())
                builder.CreateNewRawObject(node.GetObjectType(), constructor, (uint)numargs);
            else
                builder.CreateNewStackObject(node.GetObjectType(), constructor, (uint)numargs);

            return builder.EndNode();
        }

        public override AstNode Visit (NewRawArrayExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Visit the size expresion.
            Expression sizeExpr = node.GetSize();
            sizeExpr.Accept(this);

            // Coerce the size.
            IChelaType sizeType = sizeExpr.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(sizeType != coercionType)
                Cast(node, sizeExpr.GetNodeValue(), sizeType, coercionType);

            // Create the array.
            if(node.IsHeapAlloc())
                builder.CreateNewRawArray(node.GetObjectType());
            else
                builder.CreateNewStackArray(node.GetObjectType());

            return builder.EndNode();
        }

        private void InitializeArrayElements(AstNode where, int dimensions, int depth, IChelaType arrayType, IChelaType coercionType, Expression init)
        {
            // My children are arrays?
            bool array = depth + 1 < dimensions;

            // Append each children.
            int currentIndex = 0;
            AstNode current = init;
            while(current != null)
            {
                // Duplicate the previous arguments.
                if(!array)
                {
                    if(dimensions == 1)
                        builder.CreateDup1();
                    else if(dimensions == 2)
                        builder.CreateDup2();
                    else
                        builder.CreateDup((uint)dimensions);
                }

                // Add the index.
                builder.CreateLoadInt32(currentIndex);

                // Process the current expression.
                if(current is ArrayExpression)
                {
                    // Make sure its an array.
                    if(!array)
                        Error(current, "expected an expression.");

                    // Initialize the array type.
                    ArrayExpression arrayExpr = (ArrayExpression)current;
                    InitializeArrayElements(current, dimensions, depth+1, arrayType, coercionType, arrayExpr.GetElements());
                }
                else
                {
                    // Make sure its an expression.
                    if(array)
                        Error(current, "expected an array.");

                    // Visit the expression.
                    current.Accept(this);

                    // Coerce the value.
                    IChelaType currentType = current.GetNodeType();
                    if(currentType != coercionType)
                        Cast(where, current.GetNodeValue(), currentType, coercionType);

                    // Store the element.
                    builder.CreateStoreArraySlot(arrayType);
                }

                // Remove the index.
                if(array)
                    builder.CreatePop();

                // Add the next children.
                ++currentIndex;
                current = (Expression)current.GetNext();
            }
        }

        public override AstNode Visit (NewArrayExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Visit each size expresion.
            Expression sizeExpr = node.GetSize();
            IChelaType coercionType = node.GetCoercionType();
            while(sizeExpr != null)
            {
                // Visit the size expression.
                sizeExpr.Accept(this);

                // Coerce the size.
                IChelaType sizeType = sizeExpr.GetNodeType();
                if(sizeType != coercionType)
                    Cast(node, sizeExpr.GetNodeValue(), sizeType, coercionType);

                // Process the next size.
                sizeExpr = (Expression)sizeExpr.GetNext();
            }

            // Create the array
            IChelaType arrayType = node.GetArrayType();
            builder.CreateNewArray(arrayType);

            // Initialize elements.
            Expression init = (Expression)node.GetInitializers();
            if(init != null)
                InitializeArrayElements(init, node.GetDimensions(), 0, arrayType, node.GetInitCoercionType(), init);

            return builder.EndNode();
        }

        public override AstNode Visit (DeleteStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the pointer.
            Expression pointer = node.GetPointer();
            pointer.Accept(this);
            
            // Cast the pointer.
            IChelaType pointerType = pointer.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(pointerType != coercionType)
                Cast(node, pointer.GetNodeValue(), pointerType, coercionType);
            
            // Delete the object.
            builder.CreateDeleteObject();

            return builder.EndNode();
        }

        public override AstNode Visit (DeleteRawArrayStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the pointer.
            Expression pointer = node.GetPointer();
            pointer.Accept(this);
            
            // Cast the pointer.
            IChelaType pointerType = pointer.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(pointerType != coercionType)
                Cast(node, pointer.GetNodeValue(), pointerType, coercionType);
            
            // Delete the object.
            builder.CreateDeleteRawArray();

            return builder.EndNode();
        }

        public override AstNode Visit (TryStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Read the substatements.
            AstNode tryNode = node.GetTryStatement();
            AstNode catchList = node.GetCatchList();
            AstNode finalNode = node.GetFinallyStatement();

            // Read the exception context.
            ExceptionContext context = node.GetContext();

            // Create the finally block.
            BasicBlock finalBlock = null;
            if(finalNode != null)
            {
                finalBlock = CreateBasicBlock();
                finalBlock.SetName("finally");
                context.SetCleanup(finalBlock);
            }

            // Create the merge block.
            BasicBlock merge = finalBlock;
            if(merge == null)
            {
                merge = CreateBasicBlock();
                merge.SetName("trymerge");
            }

            // Store the old context.
            ExceptionContext oldContext = currentExceptionContext;
            currentExceptionContext = context;

            // Create the try block.
            BasicBlock tryBlock = CreateBasicBlock();
            tryBlock.SetName("try");
            builder.CreateJmp(tryBlock);
            builder.SetBlock(tryBlock);

            // Process the try statement.
            tryNode.Accept(this);

            // Jump to finally/merge.
            bool tryReturn = builder.IsLastTerminator();
            if(!builder.IsLastTerminator())
                builder.CreateJmp(merge);

            // Restore the context, visit the catch list.
            currentExceptionContext = oldContext;
            AstNode catchNode = catchList;
            while(catchNode != null)
            {
                // Visit it.
                catchNode.Accept(this);

                // Jump to finally/merge.
                if(!builder.IsLastTerminator())
                    builder.CreateJmp(merge);

                // TODO: Avoid "duplicated" catches.
                catchNode = catchNode.GetNext();
            }

            // Visit the finally statement
            if(finalNode != null)
            {
                // Generate the finally block.
                builder.SetBlock(finalBlock);

                // Visit it.
                finalNode.Accept(this);

                // Jump/resume to merge.
                if(builder.IsLastTerminator())
                    Error(finalNode, "finally cannot return.");

                // Create propagate return block.
                BasicBlock propRet = CreateBasicBlock();
                propRet.SetName("propRet");
                builder.CreateJumpResume(propRet);
                builder.SetBlock(propRet);

                // Find the parent return.
                BasicBlock parentRetBlock = null;
                ExceptionContext parentContext = context.GetParentContext();
                while(parentContext != null)
                {
                    parentRetBlock = parentContext.GetCleanup();
                    if(parentRetBlock != null)
                        break;
                    parentContext = parentContext.GetParentContext();
                }

                // Couldn't find a parent cleanup, return.
                if(parentRetBlock == null)
                    parentRetBlock = currentFunction.ReturnBlock;

                // If try returns and theres not catch, just return.
                if(tryReturn && catchNode == null)
                {
                    builder.CreateJmp(parentRetBlock);
                    AstNode next = node.GetNext();
                    if(next != null)
                        Warning(next, "unreachable code detected.");
                    node.SetNext(null);
                }
                else
                {
                    // Merge with the other blocks.
                    merge = CreateBasicBlock();
                    merge.SetName("trymerge");

                    // Return/finally or merge.
                    builder.CreateLoadLocal(currentFunction.ReturningVar);
                    builder.CreateBr(parentRetBlock, merge);
                }
            }

            // Remove unused code.
            if(merge.GetPredsCount() == 0)
            {
                if(node.GetNext() != null)
                    Warning(node, "detected unreachable code at {0}", node.GetNext().GetPosition());
                node.SetNext(null);
                merge.Destroy();
            }
            else
            {
                // Continue without try.
                builder.SetBlock(merge);
            }

            return builder.EndNode();
        }

        public override AstNode Visit (CatchStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Read the exception.
            Structure exception = (Structure)node.GetNodeType();

            // Create the block.
            BasicBlock block = CreateBasicBlock();
            block.SetName("catch");
            builder.SetBlock(block);

            // Add the catch to the context.
            ExceptionContext context = node.GetContext();
            context.AddCatch(exception, block);

            // Load the exception.
            LocalVariable local = node.GetVariable();
            if(local != null)
                builder.CreateStoreLocal(local);
            else
                builder.CreatePop();

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return builder.EndNode();
        }

        public override AstNode Visit (FinallyStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Move to the finally block.
            BasicBlock block = node.GetContext().GetCleanup();
            block.SetName("finally");
            builder.SetBlock(block);

            // Visit the children.
            node.GetChildren().Accept(this);

            return builder.EndNode();
        }

        public override AstNode Visit (ThrowStatement node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the exception.
            Expression exception = node.GetException();
            exception.Accept(this);

            // Cast the exception.
            IChelaType exceptionType = exception.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(exceptionType != coercionType)
                Cast(node, exception.GetNodeValue(), exceptionType, coercionType);

            // Throw the exception.
            builder.CreateThrow();

            return builder.EndNode();
        }

        public override AstNode Visit (SizeOfExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Perform operation.
            builder.CreateSizeOf(node.GetCoercionType());

            // Cast into integer.
            builder.CreateCast(ChelaType.GetIntType());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (TypeOfExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Perform operation.
            builder.CreateTypeOf(node.GetCoercionType());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (DefaultExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Perform the operation.
            builder.CreateLoadDefault(node.GetCoercionType());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (CastOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the target type and value.
            //Expression target = node.GetTarget();
            Expression value = node.GetValue();

            // Visit them.
            //target.Accept(this);
            value.Accept(this);

            // Get their types.
            IChelaType targetType = node.GetNodeType();
            IChelaType valueType = value.GetNodeType();
            if(targetType != valueType)
                Cast(node, value.GetNodeValue(), valueType, targetType, true);

            return builder.EndNode();
        }

        public override AstNode Visit (ReinterpretCast node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the target type and value.
            //Expression target = node.GetTarget();
            Expression value = node.GetValue();

            // Visit them.
            //target.Accept(this);
            value.Accept(this);

            // Perform the coercion.
            IChelaType coercionType = node.GetCoercionType();
            IChelaType valueType = value.GetNodeType();
            if(coercionType != valueType)
                Cast(node, value.GetNodeValue(), valueType, coercionType, true);

            // Perform the reinterpret_cast.
            IChelaType targetType = node.GetNodeType();
            if(valueType != targetType)
                builder.CreateBitCast(targetType);

            return builder.EndNode();
        }

        public override AstNode Visit (AsExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the target type and value.
            //Expression target = node.GetTarget();
            Expression value = node.GetValue();

            // Visit them.
            //target.Accept(this);
            value.Accept(this);

            // Get their types.
            IChelaType targetType = node.GetNodeType();
            IChelaType valueType = value.GetNodeType();
            if(targetType != valueType)
                Cast(node, value.GetNodeValue(), valueType, targetType, false);

            return builder.EndNode();
        }

        public override AstNode Visit (IsExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the target type and value.
            //Expression cmp = node.GetCompare();
            Expression value = node.GetValue();

            // Visit them.
            //target.Accept(this);
            value.Accept(this);

            // Perform coercion.
            IChelaType valueType = value.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(valueType != coercionType)
                Cast(node, value.GetNodeValue(), valueType, coercionType);

            // Perform the type comparison.
            IChelaType targetType = node.GetTargetType();
            if(targetType != valueType)
            {
                builder.CreateIsA(targetType);
            }
            else
            {
                builder.CreatePop();
                builder.CreateLoadBool(true);
            }

            return builder.EndNode();
        }

        public override AstNode Visit (RefExpression node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the reference expression.
            Expression reference = node.GetVariableExpr();

            // Visit it.
            reference.Accept(this);

            // Get the reference variable.
            Variable variable = (Variable) reference.GetNodeValue();
            if(variable.IsLocal())
            {
                builder.CreateLoadLocalRef((LocalVariable) variable);
            }
            else if(variable.IsField())
            {
                FieldVariable field = (FieldVariable) variable;
                if(field.IsStatic())
                    builder.CreateLoadGlobalRef(field);
                else
                    builder.CreateLoadFieldRef(field);
            }
            else if(variable.IsArraySlot())
            {
                ArraySlot slot = (ArraySlot)variable;
                builder.CreateLoadArraySlotRef(slot.GetArrayType());
            }
            else if(variable.IsPointedSlot())
            {
                builder.CreateCast(node.GetNodeType());
            }
            else if(variable.IsProperty())
            {
                Error(node, "cannot use ref operator for properties.");
            }
            else if(variable.IsReferencedSlot())
            {
                // Do nothing.
            }
            else
            {
                Error(node, "unsupported variable reference type");
            }

            return builder.EndNode();
        }

        public override AstNode Visit (AddressOfOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Functions are statically handler.
            if(node.IsFunction())
            {
                builder.CreateLoadFunctionAddr((Function)node.GetNodeValue());
                return builder.EndNode();
            }

            // Get the reference expression.
            Expression reference = node.GetReference();

            // Visit it.
            reference.Accept(this);

            // Get the reference variable.
            Variable variable = (Variable) reference.GetNodeValue();
            if(variable.IsLocal())
            {
                LocalVariable local = (LocalVariable) variable;
                if(local.IsPseudoLocal)
                {
                    builder.CreateLoadArg(0);
                    builder.CreateLoadFieldAddr((FieldVariable)local.ActualVariable);
                }
                else
                {
                    builder.CreateLoadLocalAddr(local);
                }
            }
            else if(variable.IsField())
            {
                FieldVariable field = (FieldVariable) variable;
                if(field.IsStatic())
                    builder.CreateLoadGlobalAddr(field);
                else
                    builder.CreateLoadFieldAddr(field);
            }
            else if(variable.IsArraySlot())
            {
                ArraySlot slot = (ArraySlot)variable;
                builder.CreateLoadArraySlotAddr(slot.GetArrayType());
            }
            else if(variable.IsPointedSlot())
            {
                // Do nothing.
            }
            else if(variable.IsReferencedSlot())
            {
                builder.CreateBitCast(node.GetNodeType());
            }
            else if(variable.IsProperty())
            {
                Error(node, "cannot use address-of operator for properties.");
            }

            return builder.EndNode();
        }

        private Scope GetMemberParent(AstNode node)
        {
            // Get the member.
            ScopeMember member = (ScopeMember)node.GetNodeValue();

            // Get the member parent.
            Scope memberParent = null;

            // Function groups don't have parent.
            if(member.IsFunctionGroupSelector())
            {
                FunctionGroupSelector selector = (FunctionGroupSelector)member;
                Function selected = selector.GetSelected();
                if(selected != null)
                    memberParent = selected.GetParentScope();
            }
            else
                memberParent = member.GetParentScope();

            return memberParent;
        }
        
        public override AstNode Visit (MemberAccess node)
        {
            // Begin the node
            builder.BeginNode(node);

            // Get the reference expression.
            Expression baseRef = node.GetReference();
            
            // Visit it.
            baseRef.Accept(this);
            
            // Get the reference and coercion type.
            IChelaType refType = baseRef.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(refType != coercionType)
                Cast(node, baseRef.GetNodeValue(), refType, coercionType);

            // Cast generic place holders into something more appropiate.
            Scope memberParent = GetMemberParent(node);
            if(coercionType.IsPlaceHolderType() && memberParent.IsType())
            {
                IChelaType targetType = (IChelaType)memberParent;
                if(targetType.IsPassedByReference())
                    targetType = ReferenceType.Create(targetType);

                // Cast the generic placeholder.
                builder.CreateGCast(targetType);
            }

            // Pop redundant reference.
            ScopeMember member = (ScopeMember)node.GetNodeValue();
            if((coercionType.IsReference() || coercionType.IsStructure())
                && member.IsStatic())
                builder.CreatePop();

            // Load the implicit this.
            if(node.ImplicitSelf)
                LoadCurrentSelf();

            // Next checks are only for functions.
            IChelaType nodeType = node.GetNodeType();
            if(!nodeType.IsFunctionGroup() && !nodeType.IsFunction())
                return builder.EndNode();

            // Handle virtual suppression.
            if(nodeType.IsFunctionGroup() && refType.IsMetaType())
            {
                FunctionGroupSelector selector = (FunctionGroupSelector)node.GetNodeValue();
                Function selected = selector.GetSelected();

                // Load implicit this.
                if(!selected.IsStatic() && selector.ImplicitThis)
                    LoadCurrentSelf();
            }

            // Only primitives and structure can be boxed.
            IChelaType referencedType = DeReferenceType(coercionType);
            bool firstClass = referencedType.IsFirstClass();
            bool structure = referencedType.IsStructure();
            if(!firstClass && !structure)
                return builder.EndNode();

            // Box structures when calling virtual methods.
            Method method = null;
            if(nodeType.IsFunctionGroup())
            {
                // Set the must box flag.
                FunctionGroupSelector selector = (FunctionGroupSelector)node.GetNodeValue();
                Function selected = selector.GetSelected();
                if(selected == null)
                    Error(node, "Trying to get unselected function.");
                if(selected.IsMethod())
                    method = (Method)selected;
            }
            else //if(nodeType.IsFunction())
            {
                Function function = (Function)node.GetNodeValue();
                if(function.IsMethod())
                    method = (Method)function;
            }

            // Only methods may require boxing.
            if(method != null)
            {
                if(!method.GetParentScope().IsStructure())
                {
                    // Must box.
                    IChelaType boxType;
                    if(firstClass)
                        boxType = currentModule.GetAssociatedClass(referencedType);
                    else
                        boxType = referencedType;
                    builder.CreateBox(boxType);
                }
                else if(firstClass) // Light primitive boxing.
                {
                    builder.CreatePrimBox(currentModule.GetAssociatedClass(referencedType));
                }
            }

            return builder.EndNode();
        }
        
        public override AstNode Visit (IndirectAccess node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the pointer expression.
            Expression basePointer = node.GetBasePointer();
            
            // Visit it.
            basePointer.Accept(this);
            
            // Get the pointer and coercion type.
            IChelaType pointerType = basePointer.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(pointerType != coercionType)
                Cast(node, basePointer.GetNodeValue(), pointerType, coercionType);

            return builder.EndNode();
        }

        public override AstNode Visit (DereferenceOperation node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the pointer expression.
            Expression pointer = node.GetPointer();
            pointer.Accept(this);

            // Perform pointer coercion.
            IChelaType pointerType = pointer.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            if(pointerType != coercionType)
                Cast(node, pointer.GetNodeValue(), pointerType, coercionType);

            return builder.EndNode();
        }
        
        public override AstNode Visit (SubscriptAccess node)
        {
            // Begin the node
            builder.BeginNode(node);

            // Get the array expression.
            Expression array = node.GetArray();
            
            // Visit it.
            array.Accept(this);

            // Get the array and coercion type.
            IChelaType arrayType = array.GetNodeType();
            IChelaType coercionType = node.GetCoercionType();
            
            // Perform array coercion.
            if(arrayType != coercionType)
                Cast(node, array.GetNodeValue(), arrayType, coercionType);

            // Visit and coerce each index.
            IChelaType[] indexCoercions = node.GetIndexCoercions();
            Expression index = node.GetIndex();
            int indexId = 0;
            while(index != null)
            {
                // Visit it.
                index.Accept(this);

                // Get the index type.
                IChelaType indexType = index.GetNodeType();

                // Get or compute the index coercion type.
                IChelaType indexCoercion = null;
                if(indexCoercions != null && indexCoercions.Length != 0)
                {
                    indexCoercion = indexCoercions[indexId];
                }
                else
                {
                    // Remove the reference and constant layer.
                    indexCoercion = indexType;
                    if(indexCoercion.IsReference())
                        indexCoercion = DeReferenceType(indexCoercion);
                    if(indexCoercion.IsConstant())
                        indexCoercion = DeConstType(indexCoercion);
                }

                if(indexType != indexCoercion)
                    Cast(index, index.GetNodeValue(), indexType, indexCoercion);

                // Process the next index.
                ++indexId;
                index = (Expression)index.GetNext();
            }

            return builder.EndNode();
        }
        
        public override AstNode Visit (VariableReference node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Get the variable value.
            ScopeMember value = (ScopeMember)node.GetNodeValue();
            
            if(value.IsVariable())
            {
                // Load the aliased variable.
                Variable aliased = node.GetAliasVariable();
                Variable variable = null;
                if(aliased != null)
                    variable = aliased;
                else
                    variable = (Variable) value;

                if((variable.IsField() || variable.IsEvent() || variable.IsProperty()) && !variable.IsStatic())
                {
                    // Load the 'this' reference.
                    LoadCurrentSelf();
                }

                // Perform actual alias load.
                if(aliased != null)
                    LoadVariableValue(node, aliased);
            }
            else if(value.IsFunction())
            {
                // Cast into a function.
                Function function = (Function) value;
                if(function.IsMethod())
                {
                    // Cast into a method.
                    Method method = (Method) function;
                    
                    // Load the 'this' reference.
                    if(!method.IsStatic())
                        LoadCurrentSelf();
                }
            }
            else if(value.IsFunctionGroupSelector())
            {
                // Cast into a selector.
                FunctionGroupSelector selector = (FunctionGroupSelector)value;
                if(selector.GetSelected().IsMethod())
                {
                    // Cast into a method.
                    Method method = (Method)selector.GetSelected();

                    // Load the 'this' reference.
                    if(!method.IsStatic())
                        LoadCurrentSelf();
                }
            }

            return builder.EndNode();
        }
        
        public override AstNode Visit (BoolConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadBool(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (SByteConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadInt8(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (ByteConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadUInt8(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (CharacterConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadChar(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (ShortConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadInt16(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (UShortConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadUInt16(node.GetValue());

            // Finish.
            return builder.EndNode();
        }
        
        public override AstNode Visit (IntegerConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadInt32(node.GetValue());

            // Finish.
            return builder.EndNode();
        }
        
        public override AstNode Visit (UIntegerConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadUInt32(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (LongConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadInt64(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (ULongConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadUInt64(node.GetValue());

            // Finish.
            return builder.EndNode();
        }
        
        public override AstNode Visit (FloatConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadFp32(node.GetValue());

            // Finish.
            return builder.EndNode();
        }
        
        public override AstNode Visit (DoubleConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadFp64(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (CStringConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadCString(node.GetValue());

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (StringConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadString(node.GetValue());

            // Finish.
            return builder.EndNode();
        }
        
        public override AstNode Visit (NullConstant node)
        {
            // Begin the node.
            builder.BeginNode(node);

            // Load the constant.
            builder.CreateLoadNull();

            // Finish.
            return builder.EndNode();
        }

        public override AstNode Visit (NullStatement node)
        {
            return node;
        }

        private BasicBlock CreateBasicBlock()
        {
            BasicBlock ret = new BasicBlock(currentFunction);
            ret.IsUnsafe = IsUnsafe;
            if(currentExceptionContext != null)
                currentExceptionContext.AddBlock(ret);
            return ret;
        }

        private void LoadVariableValue(AstNode where, Variable variable)
        {
            if(variable.IsLocal())
            {
                LocalVariable local = (LocalVariable) variable;
                if(local.IsPseudoLocal)
                {
                    // Load the variable in the generator closure.
                    builder.CreateLoadArg(0);
                    builder.CreateLoadField((FieldVariable)local.ActualVariable);
                }
                else
                {
                    builder.CreateLoadLocal(local);
                }
            }
            else if(variable.IsArgument())
            {
                ArgumentVariable arg = (ArgumentVariable)variable;
                if(arg.IsPseudoArgument)
                {
                    // Load the variable in the generator closure.
                    builder.CreateLoadArg(0);
                    builder.CreateLoadField((FieldVariable)arg.ActualVariable);
                }
                else
                {
                    builder.CreateLoadArg((byte)arg.GetArgumentIndex());
                }
            }
            else if(variable.IsField())
            {
                FieldVariable field = (FieldVariable) variable;
                if(field.IsStatic())
                    builder.CreateLoadGlobal(field);
                else
                    builder.CreateLoadField(field);
            }
            else if(variable.IsArraySlot())
            {
                ArraySlot slot = (ArraySlot)variable;
                builder.CreateLoadArraySlot(slot.GetArrayType());
            }
            else if(variable.IsReferencedSlot() || variable.IsPointedSlot())
            {
                builder.CreateLoadValue();
            }
            else if(variable.IsTemporalReferencedSlot())
            {
                Error(where, "cannot use here ref/out value.");
            }
            else if(variable.IsSwizzleVariable())
            {
                SwizzleVariable swizzle = (SwizzleVariable)variable;
                builder.CreateLoadSwizzle(swizzle.Components, swizzle.Mask);
            }
            else if(variable.IsProperty() || variable.IsDirectPropertySlot())
            {
                PropertyVariable property;
                bool direct = false;
                if(variable.IsDirectPropertySlot())
                {
                    DirectPropertySlot directProp = (DirectPropertySlot)variable;
                    property = directProp.Property;
                    direct = true;
                }
                else
                {
                    // Cast into a property.
                    property= (PropertyVariable)variable;
                }

                // Check the presence of a get accessor.
                if(property.GetAccessor == null)
                    Error(where, "cannot get value from property without get accessor.");
    
                // Use the get accessor.
                Function getAccessor = property.GetAccessor;
                uint argCount = (uint)getAccessor.GetFunctionType().GetArgumentCount();
                if(getAccessor.IsMethod())
                {
                    // Use the correct method invocation.
                    Method method = (Method)getAccessor;
                    if((method.IsOverride() || method.IsVirtual() || method.IsAbstract() || method.IsContract())
                       && !method.GetParentScope().IsStructure() && !direct)
                        builder.CreateCallVirtual(method, argCount);
                    else
                        builder.CreateCall(method, argCount);
                }
                else
                {
                    builder.CreateCall(getAccessor, argCount);
                }
            }
            else if(variable.IsEvent())
            {
                // Make sure the field is accessible.
                EventVariable eventVar = (EventVariable)variable;
                FieldVariable field = eventVar.AssociatedField;
                if(field == null)
                    Error(where, "cannot invoke/modify custom explicit event.");
    
                // Check the field access.
                CheckMemberVisibility(where, field);
    
                // Now, load the field.
                LoadVariableValue(where, field);
            }
        }

        private void Cast(AstNode where, object original, IChelaType originalType, IChelaType destType)
        {
            Cast(where, original, originalType, destType, false);
        }

        private void Cast(AstNode where, object original, IChelaType originalType, IChelaType destType, bool isChecked)
        {
            // De-const.
            originalType = DeConstType(originalType);
            destType = DeConstType(destType);
            
            // Don't perform casting if not needed.
            if(originalType == destType)
                return;

            // Dereference.
            if(original != null && originalType.IsReference() && originalType != ChelaType.GetNullType())
            {
                ReferenceType refType = (ReferenceType) originalType;
                IChelaType referencedType = refType.GetReferencedType();
    
                bool load = false;
                if(destType.IsReference() == referencedType.IsReference())
                {
                    load = true;
                }
                else if(!destType.IsPointer() || referencedType.IsStructure() ||
                        referencedType.IsPointer() || referencedType.IsReference() ||
                        referencedType.IsVector())
                {
                    load = true;
                }
                
                if(load)
                {
                    ScopeMember member = (ScopeMember) original;
                    if(member.IsVariable())
                    {
                        Variable variable = (Variable) member;
                        LoadVariableValue(where, variable);
                    }
                }
                originalType = DeConstType(referencedType);
            }

            if(originalType == destType)
                return;
            
            //bool decrease = destType.GetSize() < originalType.GetSize();
            //System.Console.WriteLine("Cast {0} -> {1}", originalType, destType);
            IChelaType intermediateCast = null;
            if((originalType.IsInteger() && destType.IsFloatingPoint()) ||
               (originalType.IsFloatingPoint() && destType.IsInteger()) ||
               (originalType.IsFloatingPoint() && destType.IsFloatingPoint()) ||
               (originalType.IsInteger() && destType.IsInteger()) ||
               (destType.IsPointer() && originalType.IsPointer()) )
            {
                builder.CreateCast(destType);
            }
            else if((destType == ChelaType.GetSizeType() && originalType.IsPointer()) ||
                    (destType.IsPointer() && (originalType == ChelaType.GetSizeType() || originalType == ChelaType.GetPtrDiffType()))
                   )
            {
                Warning(where, "reinterpret_cast is preffered to cast from {0} into {1}", originalType.GetFullName(), destType.GetFullName());
                builder.CreateCast(destType);
            }
            else if(destType.IsReference() && originalType.IsReference())
            {
                IChelaType rsource = DeReferenceType(originalType);
                IChelaType rdest = DeReferenceType(originalType);
                if(rdest != null && rdest.IsStructure())
                {
                    if(rsource == null)
                        Error(where, "trying to unbox a null reference.");

                    if(isChecked)
                        builder.CreateChecked();
                    builder.CreateUnbox(rdest);
                }
                else
                {
                    if(rsource != null && rsource.IsStructure())
                        builder.CreateBox(rsource);
                    if(isChecked)
                        builder.CreateChecked();
                    builder.CreateCast(destType);
                }
            }
            else if(destType.IsReference() && originalType.IsFunctionGroup())
            {
                // Cast the delegate type.
                IChelaType referencedType = DeReferenceType(destType);
                Class delegateType = (Class)referencedType;

                // Get the selected function.
                FunctionGroupSelector selector = (FunctionGroupSelector)original;

                // Create the delegate.
                builder.CreateNewDelegate(delegateType, selector.GetSelected());
            }
            else if(destType.IsReference() && originalType.IsStructure())
            {
                builder.CreateBox(originalType);
                builder.CreateCast(destType);
            }
            else if((destType.IsReference() || destType.IsPointer()) &&
                    originalType == ChelaType.GetNullType())
            {
                builder.CreateCast(destType);
            }
            else if(destType.IsPointer() && originalType.IsStructure())
            {
                // Get the structure type.
                Structure building = (Structure)originalType;

                // Building must be IntPtr or UIntPtr.
                if(building != currentModule.GetAssociatedClass(ChelaType.GetSizeType()) &&
                    building != currentModule.GetAssociatedClass(ChelaType.GetPtrDiffType()))
                    Error(where, "cannot perform structure -> pointer cast.");

                // Unbox the structure.
                FieldVariable field = (FieldVariable)building.FindMember("__value");
                if(field == null)
                    field = (FieldVariable)building.FindMember("m_value");
                if(field == null)
                    Error(where, "cannot unbox {0}", building.GetFullName());
                builder.CreateExtractPrim();

                // Cast into the pointer.
                builder.CreateCast(destType);
            }
            else if(destType.IsStructure() && originalType.IsPointer())
            {
                // Get the structure type.
                Structure building = (Structure)destType;

                // Building must be IntPtr or UIntPtr.
                if(building != currentModule.GetAssociatedClass(ChelaType.GetSizeType()) &&
                    building != currentModule.GetAssociatedClass(ChelaType.GetPtrDiffType()))
                    Error(where, "cannot perform pointer -> structure cast.");

                // Get the structure pointer type.
                FieldVariable field = (FieldVariable)building.FindMember("__value");
                if(field == null)
                    field = (FieldVariable)building.FindMember("m_value");
                if(field == null)
                    Error(where, "cannot unbox {0}", building.GetFullName());

                // Cast the pointer into the pointer used by the structure.
                builder.CreateCast(field.GetVariableType());

                // Box the pointer.
                builder.CreatePrimBox(building);
            }
            else if(destType.IsStructure() && originalType.IsReference())
            {
                // Don't unbox null.
                IChelaType rsource = DeReferenceType(originalType);
                if(rsource == null)
                    Error(where, "trying to unbox a null reference.");

                if(isChecked)
                    builder.CreateChecked();
                builder.CreateUnbox(destType);
            }
            else if(destType.IsFirstClass() && originalType.IsStructure())
            {
                // Check dest/source relation.
                Structure bsource = (Structure)originalType;
                // TODO: Check the source type its a box.
                if(!bsource.IsDerivedFrom(currentModule.GetEnumClass()) &&
                    currentModule.GetAssociatedClassPrimitive(bsource) == null)
                    Error(where, "structure cannot be casted into a primitive value.");

                // Get the value field.
                FieldVariable valueField = (FieldVariable)bsource.FindMember("__value");
                if(valueField == null)
                    valueField = (FieldVariable)bsource.FindMember("m_value");
                if(valueField == null)
                    Error(where, "invalid enum definition, bug in the compiler.");

                // Use the intermediate cast.
                IChelaType boxedType = valueField.GetVariableType();
                if(destType != boxedType)
                   intermediateCast = boxedType;

                // Extract the primitive.
                builder.CreateExtractPrim();
            }
            else if(destType.IsStructure() && originalType.IsFirstClass())
            {
                // Check dest/source relation.
                Structure bdest = (Structure)destType;
                if(!bdest.IsDerivedFrom(currentModule.GetEnumClass()) &&
                    currentModule.GetAssociatedClassPrimitive(bdest) == null)
                    Error(where, "primitive value cannot be casted into structure.");

                // Get the value field.
                FieldVariable valueField = (FieldVariable)bdest.FindMember("__value");
                if(valueField == null)
                    valueField = (FieldVariable)bdest.FindMember("m_value");
                if(valueField == null)
                    Error(where, "invalid enum definition, bug in the compiler.");

                // Perform the intermediate cast.
                IChelaType enumType = valueField.GetVariableType();
                if(originalType != enumType)
                    Cast(where, null, originalType, enumType);

                // Create the structure.
                builder.CreatePrimBox(destType);
            }
            else if(destType.IsFirstClass() && originalType.IsReference())
            {
                // Get the associated class.
                Structure assoc = currentModule.GetAssociatedClass(destType);
                IChelaType rsource = DeReferenceType(originalType);
                if(rsource == null)
                    Error(where, "trying to unbox null.");
                if(!rsource.IsStructure() && !rsource.IsClass() && !rsource.IsInterface())
                    Error(where, "expected class/interface reference.");

                if(!assoc.IsBasedIn((Structure)rsource))
                    Error(where, "cannot perform cast " + originalType + " -> " + destType);
                if(isChecked)
                    builder.CreateChecked();
                builder.CreateUnbox(assoc);

                // Extract the primitive.
                builder.CreateExtractPrim();
            }
            else if(destType.IsReference() && originalType.IsFirstClass())
            {
                // Get the associated class.
                Structure assoc = currentModule.GetAssociatedClass(originalType);
                IChelaType rdest = DeReferenceType(destType);

                // Make sure the cast is valid.
                if(!assoc.IsBasedIn((Structure)rdest))
                    Error(where, "cannot perform cast " + originalType + " -> " + destType);

                builder.CreateBox(assoc);
                if(rdest != assoc)
                    builder.CreateCast(destType);
            }
            else if(destType.IsStructure() && originalType.IsStructure())
            {
                // Cast the structures.
                Structure originalBuilding = (Structure)originalType;
                Structure destBuilding = (Structure)destType;

                // Only boxed types are supported in this cast.
                if(originalBuilding.GetSlotCount() != 1 ||
                    destBuilding.GetSlotCount() != 1)
                    Error(where, "cannot perform cast structure {0} -> {1}.", originalBuilding.GetFullName(), destBuilding.GetFullName());

                // Get the original field.
                FieldVariable originalField = (FieldVariable)originalBuilding.FindMember("__value");
                if(originalField == null)
                    originalField = (FieldVariable)originalBuilding.FindMember("m_value");

                // Get the destination field.
                FieldVariable destField = (FieldVariable)destBuilding.FindMember("__value");
                if(destField == null)
                    destField = (FieldVariable)destBuilding.FindMember("m_value");

                // Make sure they exist.
                if(originalField == null || destField == null)
                    Error(where, "cannot perform cast structure {0} -> {1}.", originalBuilding.GetFullName(), destBuilding.GetFullName());

                // Unbox the source.
                Cast(where, original, originalType, originalField.GetVariableType());

                // Cast the field.
                Cast(where, null, originalField.GetVariableType(), destField.GetVariableType());

                // Box the casted source.
                Cast(where, null, destField.GetVariableType(), destType);
            }
            else if(destType.IsReference() && originalType.IsPlaceHolderType())
            {
                // TODO: Add type checking.
                builder.CreateGCast(destType);
            }
            else if((destType.IsFirstClass() || destType.IsStructure()) &&
                    originalType.IsPlaceHolderType())
            {
                // TODO: Add type checking.
                builder.CreateGCast(destType);
            }
            else if(destType.IsPlaceHolderType() && originalType.IsReference())
            {
                // TODO: Add type checking.
                builder.CreateGCast(destType);
            }
            else if(destType.IsPlaceHolderType() &&
                    (originalType.IsFirstClass() || originalType.IsStructure()))
            {
                // TODO: Add type checking.
                builder.CreateGCast(destType);
            }

            else
                Error(where, "cannot perform cast " + originalType + " -> " + destType);

            // Cast the intermediate type.
            if(intermediateCast != null)
                Cast(where, null, intermediateCast, destType);
        }        
    }
}

