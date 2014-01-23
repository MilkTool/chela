using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ModuleInheritance: ObjectDeclarator
    {
        public ModuleInheritance ()
        {
        }

        private void CreateDefaultConstructor(StructDefinition node)
        {
            // Get the structure.
            Structure building = node.GetStructure();

            // Check for an user defined constructor.
            if(building.GetConstructor() != null)
                return;

            // Instance the building.
            GenericPrototype contGenProto = building.GetGenericPrototype();
            int templateArgs = contGenProto.GetPlaceHolderCount();
            Structure buildingInstance = building;
            if(templateArgs != 0)
            {
                IChelaType[] thisArgs = new IChelaType[templateArgs];
                for(int i = 0; i < templateArgs; ++i)
                    thisArgs[i] = contGenProto.GetPlaceHolder(i);
                buildingInstance = (Structure)building.InstanceGeneric(
                    new GenericInstance(contGenProto, thisArgs), currentModule);
            }

            // Create the default constructor function type.
            List<IChelaType> arguments = new List<IChelaType> ();
            arguments.Add(ReferenceType.Create(buildingInstance));
            FunctionType ctorType = FunctionType.Create(ChelaType.GetVoidType(), arguments);

            // Create the constructor method.
            MemberFlags flags = MemberFlags.Public | MemberFlags.Constructor;
            Method constructor = new Method(building.GetName(), flags, building);
            constructor.SetFunctionType(ctorType);

            // Store it.
            building.AddFunction("<ctor>", constructor);
            node.SetDefaultConstructor(constructor);
        }

        public override AstNode Visit (StructDefinition node)
        {
            // Fix the vtable.
            try
            {
                node.GetStructure().FixInheritance();
            }
            catch(ModuleException error)
            {
                Error(node, error.Message);
            }

            // Create default constructor.
            CreateDefaultConstructor(node);

            // Update the scope.
            PushScope(node.GetScope());
            
            // Visit his children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            return node;
        }
        
        public override AstNode Visit (ClassDefinition node)
        {
            // Fix the vtable.
            try
            {
                node.GetStructure().FixInheritance();
            }
            catch(ModuleException error)
            {
                Error(node, error.Message);
            }

            // Create default constructor.
            CreateDefaultConstructor(node);

            // Update the scope.
            PushScope(node.GetScope());
            
            // Visit his children.
            VisitList(node.GetChildren());
            
            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (InterfaceDefinition node)
        {
            // Fix the vtable.
            node.GetStructure().FixInheritance();

            // Update the scope.
            PushScope(node.GetScope());

            // Visit his children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }


        public override AstNode Visit (EnumDefinition node)
        {
            // Use the base class for enumerations.
            Structure building = node.GetStructure();
            building.SetBase(currentModule.GetEnumClass());

            try
            {
                building.FixInheritance();
            }
            catch(ModuleException error)
            {
                Error(node, error.Message);
            }

            
            return node;
        }

        public override AstNode Visit (DelegateDefinition node)
        {
            // Use the base class for delegates.
            Structure building = node.GetStructure();
            building.SetBase(currentModule.GetDelegateClass());

            try
            {
                building.FixInheritance();
            }
            catch(ModuleException error)
            {
                Error(node, error.Message);
            }

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
    }
}

