using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
	public class ModuleObjectDeclarator: ObjectDeclarator
	{
        private void CheckMainCandidate(AstNode node, Function main)
        {
            // Ignore the main type if the module is not executable.
            if(currentModule.GetModuleType() != ModuleType.Executable)
                return;

            // Get the function type.
            FunctionType type = main.GetFunctionType();

            // Check the return type.
            IChelaType returnType = type.GetReturnType();
            if(returnType != ChelaType.GetVoidType() &&
               returnType != ChelaType.GetIntType())
                return;

            // Check the argument count.
            if(type.GetArgumentCount() > 1)
                return;

            // Check the argument.
            if(type.GetArgumentCount() == 1)
            {
                IChelaType strArrayType = ReferenceType.Create(ArrayType.Create(currentModule.TypeMap(ChelaType.GetStringType())));
                if(strArrayType != type.GetArgument(0))
                   return;
            }

            // Make usure it could be.
            string fullName = main.GetFullName();
            string mainName = currentModule.GetMainName();
            if(mainName != null && fullName != mainName)
                return;

            // This is a main candidate.
            Function oldMain = currentModule.GetMainFunction();
            if(oldMain != null && oldMain != main)
                Error(node, "multiple main functions defined");

            // This is the main.
            currentModule.SetMainFunction(main);
        }

        private void CreateKernelEntryPoint(AstNode where, Function function)
        {
            // Only create once
            if(function.EntryPoint != null)
                return;

            // Get the function type.
            FunctionType functionType = function.GetFunctionType();

            // The return type must be void.
            if(functionType.GetReturnType() != ChelaType.GetVoidType())
                return;

            // Check the parameters for input and output streams, and not references.
            bool inputStream = false;
            bool outputStream = false;
            for(int i = 0; i < functionType.GetArgumentCount(); ++i)
            {
                IChelaType argType = functionType.GetArgument(i);

                // Streams are references.
                if(!argType.IsReference())
                    continue;

                // Cast the argument.
                ReferenceType argRef = (ReferenceType)argType;

                // Only stream and array references are supported in kernel entry points.
                if(!argRef.IsStreamReference())
                {
                    argType = argRef.GetReferencedType();
                    if(argType.IsArray())
                        continue;
                    return;
                }

                // Check the stream flow.
                ReferenceFlow flow = argRef.GetReferenceFlow();
                if(flow == ReferenceFlow.In || flow == ReferenceFlow.InOut)
                    inputStream = true;
                if(flow == ReferenceFlow.Out || flow == ReferenceFlow.InOut)
                    outputStream = true;

                // Don't stop the loop to check for the non-stream references.
            }

            // Both types of streams must be present.
            if(!inputStream || !outputStream)
                return;

            // Now create the entry point function type.
            List<IChelaType> arguments = new List<IChelaType> ();
            for(int i = 0; i < functionType.GetArgumentCount(); ++i)
            {
                IChelaType argType = functionType.GetArgument(i);

                // Select the adecuate holder.
                Class holder = null;
                if(!argType.IsReference())
                {
                    // Use uniform holder for non-stream and non-array arguments.
                    holder = currentModule.GetUniformHolderClass();
                }
                else
                {
                    // De-Reference the argument type.
                    ReferenceType refType = (ReferenceType)argType;
                    argType = refType.GetReferencedType();

                    // Get the array dimensions.
                    int dimensions = 0;
                    if(argType.IsArray())
                    {
                        ArrayType arrayType = (ArrayType)argType;
                        argType = arrayType.GetValueType();
                        dimensions = arrayType.GetDimensions();
                    }
                    else
                    {
                        // TODO: Read the dimensions from the reference type.
                    }

                    // Select the stream holder according to the dimensions.
                    switch(dimensions)
                    {
                    case 0:
                        holder = currentModule.GetStreamHolderClass();
                        break;
                    case 1:
                        holder = currentModule.GetStreamHolder1DClass();
                        break;
                    case 2:
                        holder = currentModule.GetStreamHolder2DClass();
                        break;
                    case 3:
                        holder = currentModule.GetStreamHolder3DClass();
                        break;
                    default:
                        Error(where, "unsupported array with more than 3 dimensions in kernels.");
                        break;
                    }
                }

                // Create an stream holder instance.
                GenericInstance instanceData = new GenericInstance(holder.GetGenericPrototype(),
                    new IChelaType[]{argType});
                IChelaType holderType = holder.InstanceGeneric(instanceData, currentModule);

                // Store the stream holder type.
                if(holderType.IsPassedByReference())
                    holderType = ReferenceType.Create(holderType);
                arguments.Add(holderType);
            }

            // Create the kernel binder type.
            IChelaType computeBindingType = ReferenceType.Create(currentModule.GetComputeBindingDelegate());
            FunctionType kernelBinderType = FunctionType.Create(computeBindingType, arguments);

            // Create the kernel entry point.
            function.EntryPoint = new KernelEntryPoint(function, kernelBinderType);
        }

        public override AstNode Visit (AliasDeclaration node)
        {
            // Visit the using member.
            Expression member = node.GetMember();
            member.Accept(this);

            // Cast the pseudo-scope.
            PseudoScope scope = (PseudoScope)node.GetScope();

            // Get the member type.
            IChelaType memberType = member.GetNodeType();
            if(memberType.IsMetaType())
            {
                // Get the actual type.
                memberType = ExtractActualType(node, memberType);

                // Only structure relatives.
                if(memberType.IsStructure() || memberType.IsClass() || memberType.IsInterface())
                {
                    scope.AddAlias(node.GetName(), (ScopeMember)memberType);
                }
                else
                {
                    Error(node, "unexpected type.");
                }
            }
            else if(memberType.IsReference())
            {
                // Only static members supported.
                ScopeMember theMember = (ScopeMember)node.GetNodeValue();
                MemberFlags instanceFlags = MemberFlags.InstanceMask & theMember.GetFlags();
                if(instanceFlags != MemberFlags.Static)
                    Error(node, "unexpected member type.");

                // Store the member.
                scope.AddAlias(node.GetName(), theMember);

            }
            else if(memberType.IsNamespace())
            {
                // Set the namespace chain.
                Namespace space = (Namespace)node.GetNodeValue();
                scope.AddAlias(node.GetName(), space);
            }

            base.Visit(node);

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
            // Push the scope.
            PushScope(node.GetScope());

            // Use a default type of const int.
            IChelaType baseType = ChelaType.GetIntType();
            IChelaType enumType = ConstantType.Create(baseType);

            // Declare the value field.
            Structure building = node.GetStructure();
            FieldVariable valueField =
                new FieldVariable("m_value", MemberFlags.Public, baseType, building);
            building.AddField(valueField);

            // Used for implicit prev + 1.
            IChelaType enumConst = ConstantType.Create(building);
            TypeNode enumIntExpr = new TypeNode(TypeKind.Other, node.GetPosition());
            enumIntExpr.SetOtherType(enumType);

            // Visit the constant defininitions.
            EnumConstantDefinition prev = null;
            AstNode child = node.GetChildren();
            while(child != null)
            {
                // Cast the child.
                EnumConstantDefinition constDef = (EnumConstantDefinition)child;

                // Set the child type.
                constDef.SetCoercionType(enumType);
                constDef.SetNodeType(enumConst);

                // If there isn't a constant expression, use previous + 1 or 0.
                if(constDef.GetValue() == null)
                {
                    TokenPosition constPos = constDef.GetPosition();
                    if(prev != null)
                    {
                        // Previous + 1
                        Expression prevExpr = new CastOperation(enumIntExpr,
                                                                new VariableReference(prev.GetName(), constPos),
                                                                constPos);
                        Expression implicitVal = new BinaryOperation(BinaryOperation.OpAdd,
                                                                     prevExpr,
                                                                     new ByteConstant(1, constPos),
                                                                     constPos);
                        constDef.SetValue(implicitVal);
                    }
                    else
                    {
                        // First element is 0
                        constDef.SetValue(new ByteConstant(0, constPos));
                    }
                }

                // Visit the constant definition.
                constDef.Accept(this);

                // Process the next constant.
                prev = constDef;
                child = child.GetNext();
            }

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (EnumConstantDefinition node)
        {
            // Use the same flags for all of the constants.
            MemberFlags flags = MemberFlags.Static | MemberFlags.Public;

            // The current container must be a structure.
            Structure building = (Structure)currentContainer;

            // Create the field.
            FieldVariable field = new FieldVariable(node.GetName(), flags, node.GetNodeType(), building);
            node.SetVariable(field);

            // Add it to the enumeration.
            building.AddField(field);

            // Return the node.
            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            // Check the security.
            bool isUnsafe = (node.GetFlags() & MemberFlags.SecurityMask) == MemberFlags.Unsafe;
            if(isUnsafe)
                PushUnsafe();

            // Get the delegate building.
            Structure building = node.GetStructure();

            // Use the generic prototype.
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Visit the argument list.
            VisitList(node.GetArguments());

            // Visit the return type.
            Expression returnTypeExpr = node.GetReturnType();
            returnTypeExpr.Accept(this);

            IChelaType returnType = returnTypeExpr.GetNodeType();
            returnType = ExtractActualType(returnTypeExpr, returnType);

            // Use references for class/interface.
            if(returnType.IsPassedByReference())
                returnType = ReferenceType.Create(returnType);

            // Create the invoke function type.
            List<IChelaType> arguments = new List<IChelaType> ();

            // Add the argument types.
            AstNode argNode = node.GetArguments();
            while(argNode != null)
            {
                arguments.Add(argNode.GetNodeType());
                argNode = argNode.GetNext();
            }

            // Create the function type.
            FunctionType functionType = FunctionType.Create(returnType, arguments);

            // Check the type security.
            if(functionType.IsUnsafe())
                UnsafeError(node, "safe delegate with unsafe type.");

            // Create the invoke type.
            List<IChelaType> invokeArguments = new List<IChelaType> ();
            invokeArguments.Add(ReferenceType.Create(building));
            for(int i = 0; i < arguments.Count; ++i)
                invokeArguments.Add(arguments[i]);
            FunctionType invokeType = FunctionType.Create(returnType, invokeArguments);

            // Create the constructor type.
            List<IChelaType> ctorArguments = new List<IChelaType> ();
            ctorArguments.Add(ReferenceType.Create(building));
            ctorArguments.Add(ReferenceType.Create(currentModule.TypeMap(ChelaType.GetObjectType())));
            ctorArguments.Add(ReferenceType.Create(functionType));
            FunctionType ctorType = FunctionType.Create(ChelaType.GetVoidType(), ctorArguments);

            // Create the constructor method.
            Method ctorMethod = new Method("<ctor>", MemberFlags.Public | MemberFlags.Runtime | MemberFlags.Constructor, building);
            ctorMethod.SetFunctionType(ctorType);
            building.AddFunction("<ctor>", ctorMethod);

            // Create the invoke function.
            Method invokeMethod = new Method("Invoke", MemberFlags.Public | MemberFlags.Runtime, building);
            invokeMethod.SetFunctionType(invokeType);
            building.AddFunction("Invoke", invokeMethod);

            // Restore the scope.
            if(genScope != null)
                PopScope();

            // Restore the security level.
            if(isUnsafe)
                PushUnsafe();

            return node;
        }

		public override AstNode Visit (FieldDefinition node)
		{
            // Push the unsafe context.
            bool isUnsafe = (node.GetFlags() & MemberFlags.SecurityMask) == MemberFlags.Unsafe;
            if(isUnsafe)
                PushUnsafe();

			// Get the type node.
			Expression typeExpression = node.GetTypeNode();
			
			// Visit the type node.
			typeExpression.Accept(this);
			
			// Get the type of the type expression.
			IChelaType type = typeExpression.GetNodeType();
			type = ExtractActualType(typeExpression, type);

            // Check field type security.
            if(type.IsUnsafe())
                UnsafeError(node, "safe field with unsafe type.");

		    // Class/interfaces instances are held by reference.
            if(type.IsPassedByReference())
                type = ReferenceType.Create(type);

            // Constants are always static.
            if(type.IsConstant())
                node.SetFlags((node.GetFlags() & ~MemberFlags.InstanceMask) | MemberFlags.Static);

            // Process the declarations.
			FieldDeclaration decl = node.GetDeclarations();
			while(decl != null)
			{
				// Find an existent member.
				ScopeMember oldField = currentContainer.FindMember(decl.GetName());
				if(oldField != null)
					Error(node, "already declared a member with the same name.");
				
				// Create the field.
				FieldVariable field = new FieldVariable(decl.GetName(), node.GetFlags(), type, currentContainer);
                field.Position = decl.GetPosition();
				decl.SetVariable(field);
                decl.SetNodeType(type);
				
				// Add the field into the scope.
				if(currentContainer.IsStructure() || currentContainer.IsClass())
                {
    				// Add the field.
    				Structure building = (Structure)currentContainer;
    				building.AddField(field);
                }
                else if(currentContainer.IsNamespace())
                {
                    // Make sure its a global variable.
                    if(!field.IsStatic())
                        Error(node, "namespaces only can have static fields.");

                    Namespace space = (Namespace)currentContainer;
                    space.AddMember(field);
                }
                else
                {
                    Error(node, "a field must be declared inside an structure or class.");
                }
				
				// Process the next declaration.
				decl = (FieldDeclaration)decl.GetNext();
			}

            if(isUnsafe)
                PopUnsafe();

            return node;
		}
		
		public override AstNode Visit (FunctionDefinition node)
		{
			// Get the prototype.
			FunctionPrototype prototype = node.GetPrototype();
			
			// Visit the prototype.
			prototype.Accept(this);
			
			// Set the node type.
			node.SetNodeType(prototype.GetNodeType());
			
			// Get and link the function with the definition.
			Function function = prototype.GetFunction();
			node.SetFunction(function);

            // Create finalizers as macros.
            AstNode children = node.GetChildren();
            if(prototype.GetDestructorName() != null && children != null)
            {
                TokenPosition position = node.GetPosition();
                // Get the base finalizer.
                Expression baseFinalizer = new MemberAccess(new BaseExpression(position), "Finalize", position);

                // Invoke it.
                AstNode baseInvoke = new CallExpression(baseFinalizer, null, position);
                FinallyStatement finalStmnt = new FinallyStatement(baseInvoke, position);

                // Try the finalizer, always invoke the parent finalizer.
                TryStatement tryStmnt = new TryStatement(children, null, finalStmnt, position);
                node.SetChildren(tryStmnt);
            }

            return node;
		}
		
		public override AstNode Visit (FunctionArgument node)
		{
			// Get the type node.
			Expression typeExpression = node.GetTypeNode();
			
			// Visit the type node.
			typeExpression.Accept(this);
			
			// Get the type of the type expression.
			IChelaType type = typeExpression.GetNodeType();

            // Extract the actual type.
            IChelaType objectType = ExtractActualType(typeExpression, type);
            if(objectType.IsPassedByReference())
                objectType = ReferenceType.Create(objectType);

            // Cannot have argument of void type.
            if(objectType == ChelaType.GetVoidType())
                Error(node, "cannot have parameters of void type.");
			
			// Set the node type.
			node.SetNodeType(objectType);

            // Checks for params arguments.
            if(node.IsParams())
            {
                // It must be the last argument.
                if(node.GetNext() != null)
                    Error(node, "only the last argument can have the params modifier.");

                // It must be a managed array.
                IChelaType paramsArg = DeReferenceType(objectType);
                if(!paramsArg.IsArray())
                    Error(node, "only array parameters can have the params modifier.");
            }

            return node;
		}
		
		public override AstNode Visit (FunctionPrototype node)
		{
            MemberFlags callingConvention = node.GetFlags() & MemberFlags.LanguageMask;
            bool isCdecl = callingConvention == MemberFlags.Cdecl;
            bool isUnsafe = (node.GetFlags() & MemberFlags.SecurityMask) == MemberFlags.Unsafe;

            // Use the unsafe scope.
            if(isUnsafe)
                PushUnsafe();

            // Generate a name for explicit contracts.
            if(node.GetNameExpression() != null)
                node.SetName(GenSym());

			// Find an existing group.
			ScopeMember oldGroup = currentContainer.FindMember(node.GetName());
			if(oldGroup != null && (oldGroup.IsType() || !oldGroup.IsFunctionGroup()))
			{
                if(oldGroup.IsFunction() && isCdecl)
                {
                    // TODO: Add additional checks.
                    Function oldDefinition = (Function)oldGroup;
                    node.SetFunction(oldDefinition);
                    node.SetNodeType(oldDefinition.GetFunctionType());
                    oldGroup = null;
                    return node;
                }
                else
				    Error(node, "cannot override something without a function group.");
			}
			
			FunctionGroup functionGroup = null;
			if(oldGroup != null) // Cast the group member.
				functionGroup = (FunctionGroup)oldGroup;

            // Make sure the destructor name is correct.
            if(node.GetDestructorName() != null)
            {
                // Get the building scope.
                Scope buildingScope = currentScope;
                while(buildingScope != null && (buildingScope.IsFunction() || buildingScope.IsPseudoScope() || buildingScope.IsLexicalScope()))
                    buildingScope = buildingScope.GetParentScope();

                // Make sure we are in a class.
                if(buildingScope == null || !buildingScope.IsClass())
                    Error(node, "only classes can have finalizers.");

                // Make sure the destructor and class name are the same.
                if(node.GetDestructorName() != buildingScope.GetName())
                    Error(node, "finalizers must have the same name as their class.");
            }

			// Read the instance flag.
			MemberFlags instanceFlags = node.GetFlags() & MemberFlags.InstanceMask;

            // Don't allow overloading with cdecl.
            if(isCdecl && functionGroup != null)
                Error(node, "overloading and cdecl are incompatible.");

            // Make sure static constructor doesn't have modifiers.
            if(instanceFlags == MemberFlags.StaticConstructor &&
               node.GetFlags() != MemberFlags.StaticConstructor)
                Error(node, "static constructors cannot have modifiers.");

            // Static constructors cannot have paraemeters.
            if(instanceFlags == MemberFlags.StaticConstructor &&
               node.GetArguments() != null)
                Error(node, "static constructors cannot have parameters.");

			// Add the this argument.
			if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
			{
				// Cast the scope.
				Structure building = (Structure)currentContainer;
				
				// Add the this argument.
				if(instanceFlags != MemberFlags.Static &&
                   instanceFlags != MemberFlags.StaticConstructor)
				{
                    // Instance the building.
                    building = building.GetSelfInstance();

					// Use the structure type.
					TypeNode thisType = new TypeNode(TypeKind.Reference, node.GetPosition());
					thisType.SetOtherType(building);
					
					// Create the this argument.
					FunctionArgument thisArg = new FunctionArgument(thisType, "this",
					                                                node.GetPosition());
					
					// Add to the begin of the argument list.
					thisArg.SetNext(node.GetArguments());
					node.SetArguments(thisArg);
				}
			}

            // Parse the generic signature.
            GenericSignature genericSign = node.GetGenericSignature();
            GenericPrototype genProto = null;
            PseudoScope protoScope = null;
            if(genericSign != null)
            {
                // Visit the generic signature.
                genericSign.Accept(this);

                // Create the placeholders scope.
                genProto = genericSign.GetPrototype();
                protoScope = new PseudoScope(currentScope, genProto);
                node.SetScope(protoScope);
            }

            // Use the prototype scope.
            if(protoScope != null)
                PushScope(protoScope);

			// Visit the arguments.
			VisitList(node.GetArguments());
			
			// Visit the return type.
			Expression returnTypeExpr = node.GetReturnType();
			IChelaType returnType;
			if(returnTypeExpr != null)
			{
                // Don't allow the same name as the container.
                if((currentContainer.IsStructure() || currentContainer.IsClass() ||
                   currentContainer.IsInterface()) &&
                   node.GetName() == currentContainer.GetName())
                    Error(node, "constructors cannot have a return type.");

				returnTypeExpr.Accept(this);
				returnType = returnTypeExpr.GetNodeType();
                returnType = ExtractActualType(returnTypeExpr, returnType);

                // Use references for class/interface.
                if(returnType.IsPassedByReference())
                    returnType = ReferenceType.Create(returnType);
			}
            else if(node.GetDestructorName() != null)
            {
                returnType = ChelaType.GetVoidType();
            }
			else
			{
				returnType = ChelaType.GetVoidType();
				if(!currentContainer.IsStructure() && !currentContainer.IsClass())
					Error(node, "only classes and structures can have constructors.");
				
				if(node.GetName() != currentContainer.GetName())
					Error(node, "constructors must have the same name as their parent.");
			}

            // Restore the prototype scope.
            if(protoScope != null)
                PopScope();
			
			// Create his function type.
			List<IChelaType> arguments = new List<IChelaType> ();
            bool variableArgs = false;
			
			// Add the argument types.
			AstNode argNode = node.GetArguments();
			while(argNode != null)
			{
                // Cast the argument node.
                FunctionArgument funArg = (FunctionArgument)argNode;

                // Store the argument type.
				arguments.Add(funArg.GetNodeType());

                // Set the variable argument flags.
                variableArgs = funArg.IsParams();

                // Check the next argument.
				argNode = argNode.GetNext();
			}
			
			// Set the function type.
			FunctionType type = FunctionType.Create(returnType, arguments,
                                                    variableArgs, callingConvention);
			node.SetNodeType(type);

            // Check the function safetyness.
            if(type.IsUnsafe())
                UnsafeError(node, "safe function with unsafe type.");
			
			// Avoid collisions.
			if(functionGroup != null)
            {
                Function ambiguos = functionGroup.Find(type, instanceFlags == MemberFlags.Static);
                if(ambiguos != null)
				    Error(node, "adding ambiguous overload for " + ambiguos.GetFullName());
            }
			
			// Create and add the method into his scope.
			Function function = null;
			if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
			{
				// Cast the scope.
				Structure building = (Structure) currentContainer;
				
				// Create the function according to his instance type.
				if(instanceFlags == MemberFlags.Static ||
                   instanceFlags == MemberFlags.StaticConstructor)
				{
					function = new Function(node.GetName(), node.GetFlags(), currentContainer);
				}
				else
				{
					Method method = new Method(node.GetName(), node.GetFlags(), currentContainer);
                    function = method;

                    // Check the constructor initializer.
                    ConstructorInitializer ctorInit = node.GetConstructorInitializer();
                    if(ctorInit == null || ctorInit.IsBaseCall())
                        method.SetCtorLeaf(true);
				}

                // Set the function position.
                function.Position = node.GetPosition();
				
				// Set the function type.
				function.SetFunctionType(type);
				
				// Add the function.
                try
                {
				    building.AddFunction(node.GetName(), function);
                }
                catch(ModuleException error)
                {
                    Error(node, error.Message);
                }
			}
			else if(currentContainer.IsNamespace())
			{
				// Create the function.
				function = new Function(node.GetName(), node.GetFlags(), currentContainer);
				function.SetFunctionType(type);

                // Set the function position.
                function.Position = node.GetPosition();
				
				// Add the method into his namespace.
				Namespace space = (Namespace) currentContainer;
				
				// Create the function group.
				if(functionGroup == null && !isCdecl)
				{
					functionGroup = new FunctionGroup(node.GetName(), currentContainer);
					oldGroup = functionGroup;
					space.AddMember(functionGroup);
				}
				
				// Add the function into the function group or the namespace.
                if(!isCdecl)
				    functionGroup.Insert(function);
                else
                    space.AddMember(function);
			}
            else
            {
                Error(node, "a function cannot be added here.");
            }

            // Store the generic prototype.
            if(genProto != null)
                function.SetGenericPrototype(genProto);
			
			// Set the node function.
			node.SetFunction(function);

            // Check for main function.
            if(function.IsStatic() && function.GetName() == "Main")
                CheckMainCandidate(node, function);

            // Create kernel entry point.
            if(function.IsKernel())
                CreateKernelEntryPoint(node, function);

            // Restore the safety scope.
            if(isUnsafe)
                PopUnsafe();

            return node;
		}

        public override AstNode Visit (PropertyDefinition node)
        {
            // Check the property security.
            bool isUnsafe = (node.GetFlags() & MemberFlags.SecurityMask) == MemberFlags.Unsafe;
            if(isUnsafe)
                PushUnsafe();

            // Generate a name for explicit contracts.
            if(node.GetNameExpression() != null)
                node.SetName(GenSym());

            // Checks for indexers.
            if(node.GetNameExpression() == null)
            {
                if(node.GetName() == "this" && node.GetIndices() == null)
                    Error(node, "indexers must have parameters.");
                else if(node.GetName() != "this" && node.GetIndices() != null)
                    Error(node, "indexers name must be 'this'");
            }

            // Use a special name for indexers.
            if(node.GetName() == "this")
                node.SetName("Op_Index");

            // Find an existing property.
            ScopeMember oldProperty = currentContainer.FindMember(node.GetName());
            if(oldProperty != null && !oldProperty.IsProperty())
                Error(node, "trying to override something with a property.");

            // Visit the type expression.
            Expression typeExpr = node.GetPropertyType();
            typeExpr.Accept(this);

            // Extract the meta type.
            IChelaType propType = typeExpr.GetNodeType();
            propType = ExtractActualType(typeExpr, propType);

            // Use references for class/interface.
            if(propType.IsPassedByReference())
                propType = ReferenceType.Create(propType);

            // Store the indices.
            List<IChelaType> indices = new List<IChelaType> ();
            AstNode index = node.GetIndices();
            while(index != null)
            {
                // Visit the index.
                index.Accept(this);

                // Store his type.
                indices.Add(index.GetNodeType());

                // Process the next index.
                index = index.GetNext();
            }

            // Make sure it is the same property.
            PropertyVariable property = null;
            if(oldProperty != null)
            {
                // Compare the property types.
                property = (PropertyVariable) oldProperty;
                if(property.GetVariableType() != propType)
                    Error(node, "trying to overwrite property.");

                // TODO: Check indices.

                // Avoid collisions.
                if(node.GetAccessor != null && property.GetAccessor != null)
                    Error(node.GetAccessor, "multiples definitions.");
                if(node.SetAccessor != null && property.SetAccessor != null)
                    Error(node.GetAccessor, "multiples definitions.");
            }
            else
            {
                // Create the property.
                property = new PropertyVariable(node.GetName(), node.GetFlags(),
                                                propType, indices.ToArray(), currentContainer);

                // Add it into the current scope.
                if(currentContainer.IsNamespace())
                {
                    Namespace space = (Namespace) currentContainer;
                    space.AddMember(property);
                }
                else if(currentContainer.IsClass() || currentContainer.IsStructure() || currentContainer.IsInterface())
                {
                    Structure building = (Structure) currentContainer;
                    building.AddProperty(property);
                }
                else
                    Error(node, "a property cannot be defined here.");
            }

            // Store the property.
            node.SetProperty(property);

            // Set the property in the accessors.
            if(node.GetAccessor != null)
            {
                node.GetAccessor.SetProperty(property);
                node.GetAccessor.SetIndices(node.GetIndices());
            }

            if(node.SetAccessor != null)
            {
                node.SetAccessor.SetProperty(property);
                node.SetAccessor.SetIndices(node.GetIndices());
            }

            // Visit the accessors.
            VisitList(node.GetAccessors());

            // Restore the property security.
            if(isUnsafe)
                PopUnsafe();
            
            return node;
        }

        public override AstNode Visit (GetAccessorDefinition node)
        {
            PropertyVariable property = node.GetProperty();

            // Check for property declarations.
            if(node.GetChildren() == null && property.GetAccessor != null)
                return node;

            if(node.GetChildren() != null && property.GetAccessor != null)
                Error(node, "multiples definitions of the get accessor.");

            // Build the property flags.
            MemberFlags flags = node.GetFlags();
            if(flags == MemberFlags.ImplicitVis)
            {
                flags = property.GetFlags();
            }
            else
            {
                // TODO: Check compatibility.
                flags |= property.GetFlags() & ~MemberFlags.VisibilityMask;
            }

            // Get the instance type.
            MemberFlags instanceFlags = MemberFlags.InstanceMask & flags;

            // Abstract properties cannot have a body.
            if((instanceFlags == MemberFlags.Abstract ||
               instanceFlags == MemberFlags.Contract) && node.GetChildren() != null)
                Error(node, "abstract properties cannot have a definition body.");

            // Create the get accesor.
            if(property.GetAccessor == null)
            {
                // Create the argument list.
                List<IChelaType> arguments = new List<IChelaType> ();

                // Add the this argument.
                IChelaType selfType = null;
                if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
                {
                    Structure building = (Structure) currentContainer;
                    if(instanceFlags != MemberFlags.Static)
                    {
                        // Instance the building.
                        building = building.GetSelfInstance();

                        // Create the self type
                        selfType = ReferenceType.Create(building);
                        arguments.Add(selfType);
                    }
                }

                // Append the indices.
                foreach(IChelaType indexType in property.Indices)
                    arguments.Add(indexType);

                // Create the function type.
                FunctionType functionType = FunctionType.Create(property.GetVariableType(), arguments, false);

                // Create the function.
                Function accessor;
                if(selfType != null)
                    accessor = new Method("get_" + property.GetName(), flags, currentContainer);
                else
                    accessor = new Function("get_" + property.GetName(), flags, currentContainer);

                // Set the arguments.
                accessor.SetFunctionType(functionType);

                // Set the functionposition.
                accessor.Position = node.GetPosition();

                // Store the function in the property.
                property.GetAccessor = accessor;

                // Store the function in his scope.
                if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
                {
                    Structure building = (Structure)currentContainer;
                    building.AddFunction("get_" + property.GetName(), accessor);
                }
                else if(currentContainer.IsNamespace())
                {
                    Namespace space = (Namespace)currentContainer;
                    space.AddMember(accessor);
                }
                else
                {
                    Error(node, "a property cannot be declared here.");
                }

                // If there's a body, declare the "argument" variables
                if(node.GetChildren() != null)
                {
                    LexicalScope scope = CreateLexicalScope(node, accessor);
                    if(selfType != null)
                    {
                        node.ArgumentVariables.Add(
                            new ArgumentVariable(0, "this", scope, selfType)
                        );
                    }

                    // Add the indices variables.
                    AstNode index = node.GetIndices();
                    List<LocalVariable> indexerVars = new List<LocalVariable> ();
                    while(index != null)
                    {
                        // Create the local
                        LocalVariable local = new LocalVariable(index.GetName(), scope, index.GetNodeType());
                        indexerVars.Add(local);

                        // Process the next.
                        index = index.GetNext();
                    }
                    node.SetIndexerVariables(indexerVars);

                    // Store the scope.
                    node.SetScope(scope);
                }
            }
            else
            {
                // Check for declaration compatibility.
                if(flags != property.GetAccessor.GetFlags())
                    Error(node, "incompatible get accessor declarations.");
            }

            // Set the node function to the accessor.
            node.SetFunction(property.GetAccessor);

            return node;
        }

        public override AstNode Visit (SetAccessorDefinition node)
        {
            PropertyVariable property = node.GetProperty();

            if(node.GetChildren() == null && property.SetAccessor != null)
                return node;

            if(node.GetChildren() != null && property.SetAccessor != null)
                Error(node, "multiples definitions of the set accessor.");

            // Build the property flags.
            MemberFlags flags = node.GetFlags();
            if(flags == MemberFlags.ImplicitVis)
            {
                flags = property.GetFlags();
            }
            else
            {
                // TODO: Check compatibility.
                flags |= property.GetFlags() & ~MemberFlags.VisibilityMask;
            }

            // Get the instance type.
            MemberFlags instanceFlags = MemberFlags.InstanceMask & flags;

            // Abstract properties cannot have a body.
            if((instanceFlags == MemberFlags.Abstract ||
               instanceFlags == MemberFlags.Contract) && node.GetChildren() != null)
                Error(node, "abstract properties cannot have a definition body.");

            // Create the set accesor.
            if(property.SetAccessor == null)
            {
                // Create the argument list.
                List<IChelaType> arguments = new List<IChelaType> ();

                // Add the this argument.
                IChelaType selfType = null;
                if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
                {
                    Structure building = (Structure) currentContainer;
                    if(instanceFlags != MemberFlags.Static)
                    {
                        // Instance the building.
                        building = building.GetSelfInstance();

                        // Create the self parameter.
                        selfType = ReferenceType.Create(building);
                        arguments.Add(selfType);
                    }
                }

                // Append the indices.
                foreach(IChelaType indexType in property.Indices)
                    arguments.Add(indexType);

                // Add the value argument.
                arguments.Add(property.GetVariableType());

                // Create the function type.
                FunctionType functionType = FunctionType.Create(ChelaType.GetVoidType(), arguments, false);

                // Create the function.
                Function accessor;
                if(selfType != null)
                    accessor = new Method("set_" + property.GetName(), flags, currentContainer);
                else
                    accessor = new Function("set_" + property.GetName(), flags, currentContainer);

                // Set the arguments.
                accessor.SetFunctionType(functionType);

                // Set the function position.
                accessor.Position = node.GetPosition();

                // Store the function in the property.
                property.SetAccessor = accessor;

                // Store the function in his scope.
                if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
                {
                    Structure building = (Structure)currentContainer;
                    building.AddFunction("set_" + property.GetName(), accessor);
                }
                else if(currentContainer.IsNamespace())
                {
                    Namespace space = (Namespace)currentContainer;
                    space.AddMember(accessor);
                }
                else
                {
                    Error(node, "a property cannot be declared here.");
                }

                // If there's a body, declare the "argument" variables.
                if(node.GetChildren() != null)
                {
                    LexicalScope scope = CreateLexicalScope(node, accessor);
    
                    // Add the "this" local.
                    if(selfType != null)
                        new ArgumentVariable(0, "this", scope, selfType);
    
                    // Add the "value" local
                    LocalVariable valueVar = new LocalVariable("value", scope, property.GetVariableType());
                    node.SetValueLocal(valueVar);

                    // Add the indices variables.
                    AstNode index = node.GetIndices();
                    List<LocalVariable> indexerVars = new List<LocalVariable> ();
                    while(index != null)
                    {
                        // Create the local
                        LocalVariable local = new LocalVariable(index.GetName(), scope, index.GetNodeType());
                        indexerVars.Add(local);

                        // Process the next.
                        index = index.GetNext();
                    }
                    node.SetIndexerVariables(indexerVars);
                    node.SetScope(scope);
                }
            }
            else
            {
                // Check for declaration compatibility.
                if(flags != property.SetAccessor.GetFlags())
                    Error(node, "incompatible set accessor declarations.");
            }

            // Set the node function to the accessor.
            node.SetFunction(property.SetAccessor);

            return node;
        }

        public override AstNode Visit (EventDefinition node)
        {
            // Find an existing event.
            ScopeMember oldEvent = currentContainer.FindMember(node.GetName());
            if(oldEvent != null)
            {
                if(oldEvent.IsEvent())
                    Error(node, "trying to redefine an event.");
                else
                    Error(node, "trying to override something with an event.");
            }

            // Visit the type expression.
            Expression typeExpr = node.GetEventType();
            typeExpr.Accept(this);

            // Extract the meta type.
            IChelaType eventType = typeExpr.GetNodeType();
            eventType = ExtractActualType(typeExpr, eventType);

            // Make sure the event type is a class..
            if(!eventType.IsClass())
                Error(typeExpr, "event types must be delegates.");

            // Store the delegate type.
            Class delegateType = (Class)eventType;
            node.SetDelegateType(delegateType);

            // The actual event type is a reference.
            eventType = ReferenceType.Create(eventType);

            // Create the event.
            EventVariable eventVariable =
                new EventVariable(node.GetName(), node.GetFlags(),
                                  eventType, currentContainer);

            // Add it into the current scope.
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace) currentContainer;
                space.AddMember(eventVariable);
            }
            else if(currentContainer.IsClass() || currentContainer.IsStructure() || currentContainer.IsInterface())
            {
                Structure building = (Structure) currentContainer;
                building.AddEvent(eventVariable);
            }
            else
                Error(node, "an event cannot be defined here.");

            // Set the event variable in the accessors.
            node.SetEvent(eventVariable);
            if(node.AddAccessor != null)
                node.AddAccessor.SetEvent(eventVariable);

            if(node.RemoveAccessor != null)
                node.RemoveAccessor.SetEvent(eventVariable);

            // Visit the accessors.
            VisitList(node.GetAccessors());

            // Declare simplified event.
            if(node.GetAccessors() == null)
                DeclareSimplifiedEvent(node);

            return node;
        }

        private void DeclareSimplifiedEvent(EventDefinition node)
        {
            // Get the event variable.
            EventVariable eventVariable = node.GetEvent();

            // Get the flags.
            MemberFlags flags = node.GetFlags();

            // Get the instance type.
            MemberFlags instanceFlags = MemberFlags.InstanceMask & flags;

            // Create the argument list.
            List<IChelaType> arguments = new List<IChelaType> ();

            // Add the this argument.
            IChelaType selfType = null;
            if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
            {
                Structure building = (Structure) currentContainer;
                if(instanceFlags != MemberFlags.Static)
                {
                    // Instance the building.
                    building = building.GetSelfInstance();

                    // Create the self argument.
                    selfType = ReferenceType.Create(building);
                    arguments.Add(selfType);
                }
            }

            // Add the value argument.
            arguments.Add(eventVariable.GetVariableType());

            // Create the function type.
            FunctionType functionType = FunctionType.Create(ChelaType.GetVoidType(), arguments, false);

            // Create the add function.
            CreateSimplifiedEventFunction(node, eventVariable, functionType, true);

            // Create the remove function
            CreateSimplifiedEventFunction(node, eventVariable, functionType, false);

            // Create the delegate field.
            string delFieldName = "_evdel_" + eventVariable.GetName();
            MemberFlags delFieldFlags = flags & ~MemberFlags.VisibilityMask;
            if(eventVariable.IsPrivate())
                delFieldFlags |= MemberFlags.Private;
            else
                delFieldFlags |= MemberFlags.Protected;

            // Store the delegate field in the current container.
            FieldVariable delField = new FieldVariable(delFieldName, delFieldFlags, eventVariable.GetVariableType(), currentContainer);
            delField.Position = node.GetPosition();
            eventVariable.AssociatedField = delField;
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace) currentContainer;
                space.AddMember(delField);
            }
            else if(currentContainer.IsClass() || currentContainer.IsStructure() || currentContainer.IsInterface())
            {
                Structure building = (Structure) currentContainer;
                building.AddField(delField);
            }
            else
                Error(node, "an event cannot be defined here.");
        }

        private void CreateSimplifiedEventFunction(AstNode node, EventVariable eventVariable, FunctionType functionType, bool isAdd)
        {
            // Create the function name.
            string functionName = (isAdd ? "add_" : "remove_") + eventVariable.GetName();

            // Create the function.
            Function function;
            if(functionType.GetArgumentCount() > 1)
                function = new Method(functionName, eventVariable.GetFlags(), currentContainer);
            else
                function = new Function(functionName, eventVariable.GetFlags(), currentContainer);

            // Set the function type.
            function.SetFunctionType(functionType);

            // Set the function position.
            function.Position = node.GetPosition();

            // Store the function in the event.
            if(isAdd)
                eventVariable.AddModifier = function;
            else
                eventVariable.RemoveModifier = function;

            // Store the function in his container.
            if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
            {
                Structure building = (Structure)currentContainer;
                building.AddFunction(functionName, function);
            }
            else if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddMember(function);
            }
            else
            {
                Error(node, "an event cannot be declared here.");
            }
        }

        public override AstNode Visit (EventAccessorDefinition node)
        {
            EventVariable eventVariable = node.GetEvent();

            // Build the event flags.
            MemberFlags flags = node.GetFlags();
            if(flags == MemberFlags.ImplicitVis)
            {
                flags = eventVariable.GetFlags();
            }
            else
            {
                // TODO: Check compatibility.
                flags |= eventVariable.GetFlags() & ~MemberFlags.VisibilityMask;
            }

            // Get the instance type.
            MemberFlags instanceFlags = MemberFlags.InstanceMask & flags;

            // Abstract properties cannot have a body.
            if((instanceFlags == MemberFlags.Abstract ||
               instanceFlags == MemberFlags.Contract) && node.GetChildren() != null)
                Error(node, "abstract events cannot have a definition body.");

            // Create the argument list.
            List<IChelaType> arguments = new List<IChelaType> ();

            // Add the this argument.
            IChelaType selfType = null;
            if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
            {
                Structure building = (Structure) currentContainer;
                if(instanceFlags != MemberFlags.Static)
                {
                    // Instance the building.
                    building = building.GetSelfInstance();

                    // Create the self argument.
                    selfType = ReferenceType.Create(building);
                    arguments.Add(selfType);
                }
            }

            // Add the value argument.
            arguments.Add(eventVariable.GetVariableType());

            // Create the function type.
            FunctionType functionType = FunctionType.Create(ChelaType.GetVoidType(), arguments, false);

            // Build the function name.
            string functionName = node.GetName() + "_" + eventVariable.GetName();

            // Create the function.
            Function accessor;
            if(selfType != null)
                accessor = new Method(functionName, flags, currentContainer);
            else
                accessor = new Function(functionName, flags, currentContainer);

            // Set the arguments.
            accessor.SetFunctionType(functionType);

            // Set the function position.
            accessor.Position = node.GetPosition();

            // Store the function in the event.
            if(node.GetName() == "add")
                eventVariable.AddModifier = accessor;
            else
                eventVariable.RemoveModifier = accessor;

            // Store the function in his scope.
            if(currentContainer.IsStructure() || currentContainer.IsClass() || currentContainer.IsInterface())
            {
                Structure building = (Structure)currentContainer;
                building.AddFunction(functionName, accessor);
            }
            else if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddMember(accessor);
            }
            else
            {
                Error(node, "an event cannot be declared here.");
            }

            // If there's a body, declare the "argument" variables.
            if(node.GetChildren() != null)
            {
                LexicalScope scope = CreateLexicalScope(node, accessor);

                // Add the "this" local.
                if(selfType != null)
                    new ArgumentVariable(0, "this", scope, selfType);

                // Add the "value" local
                LocalVariable valueVar = new LocalVariable("value", scope, eventVariable.GetVariableType());
                node.SetValueLocal(valueVar);

                // Store the scope
                node.SetScope(scope);
            }

            // Set the node function to the accessor.
            node.SetFunction(accessor);

            return node;
        }
	}
}

