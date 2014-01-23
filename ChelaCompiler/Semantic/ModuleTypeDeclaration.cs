using System.Collections.Generic;
using System.Text;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
	public class ModuleTypeDeclaration: ObjectDeclarator
	{
		private Dictionary<string, IChelaType> typeMaps;
        private Dictionary<string, IChelaType> assocClasses;
		
		public ModuleTypeDeclaration ()
		{
			typeMaps = new Dictionary<string, IChelaType> ();
			typeMaps.Add("Chela.Lang.Object", ChelaType.GetObjectType());
			typeMaps.Add("Chela.Lang.String", ChelaType.GetStringType());

            assocClasses = new Dictionary<string, IChelaType> ();
            assocClasses.Add("Chela.Lang.Boolean", ChelaType.GetBoolType());
            assocClasses.Add("Chela.Lang.Char", ChelaType.GetCharType());
            assocClasses.Add("Chela.Lang.SByte", ChelaType.GetSByteType());
            assocClasses.Add("Chela.Lang.Byte", ChelaType.GetByteType());
            assocClasses.Add("Chela.Lang.Int16", ChelaType.GetShortType());
            assocClasses.Add("Chela.Lang.UInt16", ChelaType.GetUShortType());
            assocClasses.Add("Chela.Lang.Int32", ChelaType.GetIntType());
            assocClasses.Add("Chela.Lang.UInt32", ChelaType.GetUIntType());
            assocClasses.Add("Chela.Lang.Int64", ChelaType.GetLongType());
            assocClasses.Add("Chela.Lang.UInt64", ChelaType.GetULongType());
            assocClasses.Add("Chela.Lang.IntPtr", ChelaType.GetPtrDiffType());
            assocClasses.Add("Chela.Lang.UIntPtr", ChelaType.GetSizeType());
            assocClasses.Add("Chela.Lang.Single", ChelaType.GetFloatType());
            assocClasses.Add("Chela.Lang.Double", ChelaType.GetDoubleType());

            // Vector associated classes
            assocClasses.Add("Chela.Lang.Single2", VectorType.Create(ChelaType.GetFloatType(), 2));
            assocClasses.Add("Chela.Lang.Single3", VectorType.Create(ChelaType.GetFloatType(), 3));
            assocClasses.Add("Chela.Lang.Single4", VectorType.Create(ChelaType.GetFloatType(), 4));
            assocClasses.Add("Chela.Lang.Double2", VectorType.Create(ChelaType.GetDoubleType(), 2));
            assocClasses.Add("Chela.Lang.Double3", VectorType.Create(ChelaType.GetDoubleType(), 3));
            assocClasses.Add("Chela.Lang.Double4", VectorType.Create(ChelaType.GetDoubleType(), 4));
            assocClasses.Add("Chela.Lang.Int32x2", VectorType.Create(ChelaType.GetIntType(), 2));
            assocClasses.Add("Chela.Lang.Int32x3", VectorType.Create(ChelaType.GetIntType(), 3));
            assocClasses.Add("Chela.Lang.Int32x4", VectorType.Create(ChelaType.GetIntType(), 4));
            assocClasses.Add("Chela.Lang.Boolean2", VectorType.Create(ChelaType.GetBoolType(), 2));
            assocClasses.Add("Chela.Lang.Boolean3", VectorType.Create(ChelaType.GetBoolType(), 3));
            assocClasses.Add("Chela.Lang.Boolean4", VectorType.Create(ChelaType.GetBoolType(), 4));
		}
		
		public override AstNode Visit(ModuleNode node)
		{
			// Store the old module.
			ChelaModule oldModule = this.currentModule;
			currentModule = node.GetModule();
			
			// Visit the children.
			PushScope(currentModule.GetGlobalNamespace());
			VisitList(node.GetChildren());
			PopScope();
			
			// Restore the module.
			currentModule = oldModule;
            return node;
		}

        public override AstNode Visit (FileNode node)
        {
            VisitList(node.GetChildren());
            return node;
        }

		public override AstNode Visit(NamespaceDefinition node)
		{
			// Handle nested names.
			string fullname = node.GetName();
			StringBuilder nameBuilder = new StringBuilder();
			
			int numscopes = 0;
			
			for(int i = 0; i < fullname.Length; i++)
			{
				char c = fullname[i];
				if(c != '.')
				{
					nameBuilder.Append(c);
					if(i+1 < fullname.Length)
						continue;
				}
				
				// Expect full name.
				if(c == '.' && i+1 == fullname.Length)
					Error(node, "expected complete namespace name.");
				
				// Find an already declared namespace.
				string name = nameBuilder.ToString();
				nameBuilder.Length = 0;
				ScopeMember old = currentContainer.FindMember(name);
				if(old != null)
				{
					if(!old.IsNamespace())
						Error(node, "defining namespace collides with another thing.");
					
					// Store a reference in the node.
					node.SetNamespace((Namespace)old);
				}
				else
				{
					// Cast the current scope.
					Namespace space = (Namespace)currentContainer;
					
					// Create, name and add the new namespace.
					Namespace newNamespace = new Namespace(space);
					newNamespace.SetName(name);
					space.AddMember(newNamespace);
					
					// Store a reference in the node.
					node.SetNamespace(newNamespace);
				}
				
				// Update the scope.
				PushScope(node.GetNamespace());
				
				// Increase the number of scopes.
				numscopes++;
			}
							
			// Visit the children.
			VisitList(node.GetChildren());
			
			// Restore the scopes.
			for(int i = 0; i < numscopes; i++)
				PopScope();

            return node;
		}

        public override AstNode Visit (UsingStatement node)
        {
            // Create the pseudo-scope.
            PseudoScope scope = new PseudoScope(currentScope);

            // Store it in the node.
            node.SetScope(scope);

            // Change the ast layout to reflect the new scope hierarchy.
            node.SetChildren(node.GetNext());
            node.SetNext(null);

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
            PseudoScope scope = new PseudoScope(currentScope);

            // Store it in the node.
            node.SetScope(scope);

            // Change the ast layout to reflect the new scope hierarchy.
            node.SetChildren(node.GetNext());
            node.SetNext(null);

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }
		
		public override AstNode Visit(StructDefinition node)
		{
            // Parse the generic signature.
            GenericSignature genericSign = node.GetGenericSignature();
            GenericPrototype genProto = GenericPrototype.Empty;
            PseudoScope protoScope = null;
            if(genericSign != null)
            {
                // Visit the generic signature.
                genericSign.Accept(this);

                // Connect the generic prototype.
                genProto = genericSign.GetPrototype();

                // Create the placeholders scope.
                protoScope = new PseudoScope(currentScope, genProto);
                node.SetGenericScope(protoScope);
            }

			// Prevent redefinition.
			ScopeMember old = currentContainer.FindType(node.GetName(), genProto);
			if(old != null)
				Error(node, "trying to redefine a struct.");
			
			// Create the structure
			Structure building = new Structure(node.GetName(), node.GetFlags(), currentContainer);
            building.SetGenericPrototype(genProto);
			node.SetStructure(building);
            building.Position = node.GetPosition();

            // Use the prototype scope.
            if(protoScope != null)
                PushScope(protoScope);

            // Register type association.
            string fullName = building.GetFullName();
            IChelaType assoc;
            if(assocClasses.TryGetValue(fullName, out assoc))
                currentModule.RegisterAssociatedClass(assoc, building);

			// Add into the current scope.
			if(currentContainer.IsNamespace())
			{
				Namespace space = (Namespace)currentContainer;
				space.AddType(building);
			}
			else if(currentContainer.IsStructure() || currentContainer.IsInterface() || currentContainer.IsClass())
			{
				Structure parent = (Structure)currentContainer;
                parent.AddType(building);
			}
			else
			{
				Error(node, "unexpected place for a structure.");
			}

            // Push the building unsafe.
            if(building.IsUnsafe())
                PushUnsafe();
			
			// Update the scope.
			PushScope(building);
			
			// Visit the building children.
			VisitList(node.GetChildren());
			
			// Restore the scope.
			PopScope();

            // Pop the building unsafe.
            if(building.IsUnsafe())
                PopUnsafe();

            // Restore the prototype scope.
            if(protoScope != null)
                PopScope();

            return node;
		}
		
		public override AstNode Visit(ClassDefinition node)
		{
            // Check the partial flag.
            bool isPartial = (node.GetFlags() & MemberFlags.ImplFlagMask) == MemberFlags.Partial;

            // Add the parent static flag.
            if(currentContainer.IsStatic())
            {
                MemberFlags oldInstance = node.GetFlags() & MemberFlags.InstanceMask;
                if(oldInstance == MemberFlags.Static || oldInstance == MemberFlags.Instanced)
                {
                    MemberFlags flags = node.GetFlags() & ~MemberFlags.InstanceMask;
                    node.SetFlags(flags | MemberFlags.Static);
                }
                else
                    Error(node, "static class member cannot be {0}.", oldInstance.ToString().ToLower());
            }

            // Static classes are automatically sealed.
            if((node.GetFlags() & MemberFlags.InstanceMask) == MemberFlags.Static)
            {
                MemberFlags flags = node.GetFlags() & ~MemberFlags.InheritanceMask;
                node.SetFlags(flags | MemberFlags.Sealed);
            }

            // Parse the generic signature.
            GenericSignature genericSign = node.GetGenericSignature();
            GenericPrototype genProto = GenericPrototype.Empty;
            PseudoScope protoScope = null;
            if(genericSign != null)
            {
                // Visit the generic signature.
                genericSign.Accept(this);

                // Connect the generic prototype.
                genProto = genericSign.GetPrototype();

                // Create the placeholders scope.
                protoScope = new PseudoScope(currentScope, genProto);
                node.SetGenericScope(protoScope);
            }

			// Prevent redefinition.
			ScopeMember old = currentContainer.FindType(node.GetName(), genProto);
			if(old != null && (!isPartial || !old.IsClass()))
				Error(node, "trying to redefine a class.");
			
			// Create the structure
            Class clazz = (Class)old;
            if(clazz != null)
            {
                if(!clazz.IsPartial())
                    Error(node, "incompatible partial class definitions.");
            }
            else
            {
			    clazz = new Class(node.GetName(), node.GetFlags(), currentContainer);
                clazz.Position = node.GetPosition();
                clazz.SetGenericPrototype(genProto);
            }
			node.SetStructure(clazz);

            // Use the prototype scope.
            if(protoScope != null)
                PushScope(protoScope);

			// Register type maps.
            string fullName = clazz.GetFullName();
			IChelaType alias;
			if(typeMaps.TryGetValue(fullName, out alias))
				currentModule.RegisterTypeMap(alias, clazz);

            // Register type association.
            IChelaType assoc;
            if(assocClasses.TryGetValue(fullName, out assoc))
                currentModule.RegisterAssociatedClass(assoc, clazz);

            // Array base class is special.
            if(fullName == "Chela.Lang.Array")
                currentModule.RegisterArrayClass(clazz);
            else if(fullName == "Chela.Lang.Attribute")
                currentModule.RegisterAttributeClass(clazz);
            else if(fullName == "Chela.Lang.ValueType")
                currentModule.RegisterValueTypeClass(clazz);
            else if(fullName == "Chela.Lang.Enum")
                currentModule.RegisterEnumClass(clazz);
            else if(fullName == "Chela.Lang.Type")
                currentModule.RegisterTypeClass(clazz);
            else if(fullName == "Chela.Lang.Delegate")
                currentModule.RegisterDelegateClass(clazz);
            else if(fullName == "Chela.Compute.StreamHolder")
                currentModule.RegisterStreamHolderClass(clazz);
            else if(fullName == "Chela.Compute.StreamHolder1D")
                currentModule.RegisterStreamHolder1DClass(clazz);
            else if(fullName == "Chela.Compute.StreamHolder2D")
                currentModule.RegisterStreamHolder2DClass(clazz);
            else if(fullName == "Chela.Compute.StreamHolder3D")
                currentModule.RegisterStreamHolder3DClass(clazz);
            else if(fullName == "Chela.Compute.UniformHolder")
                currentModule.RegisterUniformHolderClass(clazz);
			
			// Add into the current scope.
			if(currentContainer.IsNamespace())
			{
				Namespace space = (Namespace)currentContainer;
                if(old == null)
				    space.AddType(clazz);
			}
			else if(currentContainer.IsStructure() || currentContainer.IsInterface() || currentContainer.IsClass())
			{
				Structure parent = (Structure)currentContainer;
                if(old == null)
                    parent.AddType(clazz);
			}
			else
			{
				Error(node, "unexpected place for a class.");
			}

            // Push the class unsafe.
            if(clazz.IsUnsafe())
                PushUnsafe();
			
			// Update the scope.
			PushScope(clazz);
			
			// Visit the building children.
			VisitList(node.GetChildren());
			
			// Restore the scope.
			PopScope();

            // Restore the scope.
            if(protoScope != null)
                PopScope();

            // Pop the class unsafe.
            if(clazz.IsUnsafe())
                PopUnsafe();

            return node;
		}

	    public override AstNode Visit(InterfaceDefinition node)
        {
            // Parse the generic signature.
            GenericSignature genericSign = node.GetGenericSignature();
            GenericPrototype genProto = GenericPrototype.Empty;
            PseudoScope protoScope = null;
            if(genericSign != null)
            {
                // Visit the generic signature.
                genericSign.Accept(this);

                // Connect the generic prototype.
                genProto = genericSign.GetPrototype();

                // Create the placeholders scope.
                protoScope = new PseudoScope(currentScope, genProto);
                node.SetGenericScope(protoScope);
            }

            // Prevent redefinition.
            ScopeMember old = currentContainer.FindType(node.GetName(), genProto);
            if(old != null)
                Error(node, "trying to redefine a class.");
            
            // Create the structure
            Interface iface = new Interface(node.GetName(), node.GetFlags(), currentContainer);
            iface.SetGenericPrototype(genProto);
            node.SetStructure(iface);
            iface.Position = node.GetPosition();

            // Register type maps.
            string fullName = iface.GetFullName();
            IChelaType alias;
            if(typeMaps.TryGetValue(fullName, out alias))
                currentModule.RegisterTypeMap(alias, ReferenceType.Create(iface));

            // Register some important interfaces.
            if(fullName == "Chela.Lang.NumberConstraint")
                currentModule.RegisterNumberConstraint(iface);
            else if(fullName == "Chela.Lang.IntegerConstraint")
                currentModule.RegisterIntegerConstraint(iface);
            else if(fullName == "Chela.Lang.FloatingPointConstraint")
                currentModule.RegisterFloatingPointConstraint(iface);
            else if(fullName == "Chela.Collections.IEnumerable")
                currentModule.RegisterEnumerableIface(iface);
            else if(fullName == "Chela.Collections.IEnumerator")
                currentModule.RegisterEnumeratorIface(iface);
            else if(fullName.StartsWith("Chela.Collections.Generic.IEnumerable<"))
                currentModule.RegisterEnumerableGIface(iface);
            else if(fullName.StartsWith("Chela.Collections.Generic.IEnumerator<"))
                currentModule.RegisterEnumeratorGIface(iface);

            // Use the prototype scope.
            if(protoScope != null)
                PushScope(protoScope);

            // Add into the current scope.
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddType(iface);
            }
            else if(currentContainer.IsStructure() || currentContainer.IsInterface() || currentContainer.IsClass())
            {
                Structure parent = (Structure)currentContainer;
                parent.AddType(iface);
            }
            else
            {
                Error(node, "unexpected place for a class.");
            }

            // Push the interface unsafe.
            if(iface.IsUnsafe())
                PushUnsafe();
            
            // Update the scope.
            PushScope(iface);
            
            // Visit the building children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            // Restore the generic scope.
            if(protoScope != null)
                PopScope();

            // Pop the interface unsafe.
            if(iface.IsUnsafe())
                PopUnsafe();

            return node;
        }


        public override AstNode Visit (EnumDefinition node)
        {
            // Prevent redefinition.
            ScopeMember old = currentContainer.FindType(node.GetName(), GenericPrototype.Empty);
            if(old != null)
                Error(node, "trying to redefine a enum.");
            
            // Create the structure
            Structure building = new Structure(node.GetName(), node.GetFlags(), currentContainer);
            building.Position = node.GetPosition();
            node.SetStructure(building);

            // Add into the current scope.
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddType(building);
            }
            else if(currentContainer.IsStructure() || currentContainer.IsInterface() || currentContainer.IsClass())
            {
                Structure parent = (Structure)currentContainer;
                parent.AddType(building);
            }
            else
            {
                Error(node, "unexpected place for a structure.");
            }

            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            // Parse the generic signature.
            GenericSignature genericSign = node.GetGenericSignature();
            GenericPrototype genProto = GenericPrototype.Empty;
            PseudoScope protoScope = null;
            if(genericSign != null)
            {
                // Visit the generic signature.
                genericSign.Accept(this);

                // Connect the generic prototype.
                genProto = genericSign.GetPrototype();

                // Create the placeholders scope.
                protoScope = new PseudoScope(currentScope, genProto);
                node.SetGenericScope(protoScope);
            }

            // Prevent redefinition.
            ScopeMember old = currentContainer.FindType(node.GetName(), GenericPrototype.Empty);
            if(old != null)
                Error(node, "trying to redefine a delegate.");

            // Create the structure
            MemberFlags flags = node.GetFlags();
            // TODO: Check the flags.
            Class building = new Class(node.GetName(), flags, currentContainer);
            building.SetGenericPrototype(genProto);
            building.Position = node.GetPosition();
            node.SetStructure(building);

            // Add into the current scope.
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddType(building);
            }
            else if(currentContainer.IsStructure() || currentContainer.IsInterface() || currentContainer.IsClass())
            {
                Structure parent = (Structure)currentContainer;
                parent.AddType(building);
            }
            else
            {
                Error(node, "unexpected place for a delegate.");
            }

            // Register the compute binding delegate.
            if(building.GetFullName() == "Chela.Compute.ComputeBinding")
                currentModule.RegisterComputeBindingDelegate(building);

            return node;
        }

        public override AstNode Visit (TypedefDefinition node)
        {
            // Prevent redefinition.
            ScopeMember old = currentContainer.FindMember(node.GetName());
            if(old != null)
                Error(node, "trying to redefine a typedef.");

            // Create the type name.
            TypeNameMember typeName = new TypeNameMember(node.GetFlags(), node.GetName(), currentContainer);
            typeName.SetTypedefNode(node);
            node.SetTypeName(typeName);

            // Register the type name.
            if(currentContainer.IsNamespace())
            {
                Namespace space = (Namespace)currentContainer;
                space.AddMember(typeName);
            }
            else if(currentContainer.IsStructure() || currentContainer.IsClass())
            {
                Structure parent = (Structure)currentContainer;
                parent.AddTypeName(typeName);
            }
            else
            {
                Error(node, "unexpected place for a typedef.");
            }

            return node;
        }

		public override AstNode Visit (FunctionPrototype node)
		{
            // Add the parent static flag.
            if(currentContainer.IsStatic())
            {
                MemberFlags oldInstance = node.GetFlags() & MemberFlags.InstanceMask;
                if(oldInstance == MemberFlags.Static || oldInstance == MemberFlags.Instanced)
                {
                    MemberFlags flags = node.GetFlags() & ~MemberFlags.InstanceMask;
                    node.SetFlags(flags | MemberFlags.Static);
                }
                else
                    Error(node, "static class member cannot be {0}.", oldInstance.ToString().ToLower());
            }

            // Add the parent unsafe flag.
            if(IsUnsafe)
            {
                MemberFlags flags = node.GetFlags() & ~MemberFlags.SecurityMask;
                node.SetFlags(flags | MemberFlags.Unsafe);
            }

            return node;
		}

		public override AstNode Visit(FunctionDefinition node)
		{
			// Visit the prototype.
            AstNode proto = node.GetPrototype();
            proto.Accept(this);
            return node;
		}
		
		public override AstNode Visit(FieldDefinition node)
		{
            // Add the parent static flag.
            if(currentContainer.IsStatic())
            {
                MemberFlags oldInstance = node.GetFlags() & MemberFlags.InstanceMask;
                if(oldInstance == MemberFlags.Static || oldInstance == MemberFlags.Instanced)
                {
                    MemberFlags flags = node.GetFlags() & ~MemberFlags.InstanceMask;
                    node.SetFlags(flags | MemberFlags.Static);
                }
                else
                    Error(node, "static class member cannot {0}.", oldInstance.ToString().ToLower());
            }

            // Add the parent unsafe flag.
            if(IsUnsafe)
            {
                MemberFlags flags = node.GetFlags() & ~MemberFlags.SecurityMask;
                node.SetFlags(flags | MemberFlags.Unsafe);
            }

            return node;
		}

        public override AstNode Visit (PropertyDefinition node)
        {
            // Add the parent static flag.
            if(currentContainer.IsStatic())
            {
                MemberFlags oldInstance = node.GetFlags() & MemberFlags.InstanceMask;
                if(oldInstance == MemberFlags.Static || oldInstance == MemberFlags.Instanced)
                {
                    MemberFlags flags = node.GetFlags() & ~MemberFlags.InstanceMask;
                    node.SetFlags(flags | MemberFlags.Static);
                }
                else
                    Error(node, "static class member cannot {0}.", oldInstance.ToString().ToLower());
            }

            // Add the parent unsafe flag.
            if(IsUnsafe)
            {
                MemberFlags flags = node.GetFlags() & ~MemberFlags.SecurityMask;
                node.SetFlags(flags | MemberFlags.Unsafe);
            }

            // Link the accessors.
            AstNode acc = node.GetAccessors();
            while(acc != null)
            {
                PropertyAccessor pacc = (PropertyAccessor)acc;
                if(pacc.IsGetAccessor())
                {
                    if(node.GetAccessor != null)
                        Error(node, "a property only can have one get accessor.");
                    node.GetAccessor = (GetAccessorDefinition)pacc;
                }
                else
                {
                    if(node.SetAccessor != null)
                        Error(node, "a property only can have one set accessor.");
                    node.SetAccessor = (SetAccessorDefinition)pacc;
                }
                
                acc = acc.GetNext();
            }

            return node;
        }

        public override AstNode Visit (EventDefinition node)
        {
            // Add the parent static flag.
            if(currentContainer.IsStatic())
            {
                MemberFlags oldInstance = node.GetFlags() & MemberFlags.InstanceMask;
                if(oldInstance == MemberFlags.Static || oldInstance == MemberFlags.Instanced)
                {
                    MemberFlags flags = node.GetFlags() & ~MemberFlags.InstanceMask;
                    node.SetFlags(flags | MemberFlags.Static);
                }
                else
                    Error(node, "static class member cannot {0}.", oldInstance.ToString().ToLower());
            }

            // Link the accessors.
            AstNode acc = node.GetAccessors();
            while(acc != null)
            {
                EventAccessorDefinition eacc = (EventAccessorDefinition)acc;
                string accName = eacc.GetName();
                if(accName == "add")
                {
                    if(node.AddAccessor != null)
                        Error(node, "an event only can have one add accessor.");
                    node.AddAccessor = eacc;
                }
                else if(accName == "remove")
                {
                    if(node.RemoveAccessor != null)
                        Error(node, "an event only can have one remove accessor.");
                    node.RemoveAccessor = eacc;
                }
                else
                {
                    Error(acc, "invalid event accessor name.");
                }

                acc = acc.GetNext();
            }

            return node;
        }
	}
}

