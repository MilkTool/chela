using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ModuleReadBases: ObjectDeclarator
    {
        public ModuleReadBases()
        {
        }

        private void ProcessBases(StructDefinition node)
        {
            // Process the bases.
            int numbases = 0;
            AstNode baseNode = node.GetBases();
            Structure building = node.GetStructure();
            for(; baseNode != null; baseNode = baseNode.GetNext())
            {
                // Visit the base.
                baseNode.Accept(this);
             
                // Get the node type.
                IChelaType baseType = baseNode.GetNodeType();
                baseType = ExtractActualType(node, baseType);

                // Check base security.
                if(baseType.IsUnsafe())
                    UnsafeError(node, "safe class/struct/interface with unsafe base/interface.");
             
                // Check the class/struct/iface matches.
                if(!baseType.IsInterface() &&
                   baseType.IsClass() != building.IsClass())
                    Error(baseNode, "incompatible base type.");

                // Check for interface inheritance.
                if(building.IsInterface() && !baseType.IsInterface())
                    Error(baseNode, "interfaces only can have interfaces as bases.");
             
                // Only single inheritance, multiple interfaces.
                if(numbases >= 1 && !baseType.IsInterface())
                    Error(baseNode, "only single inheritance and multiples interfaces is supported.");
             
                // Set the base building.
                if(baseType.IsInterface())
                {
                    building.AddInterface((Structure)baseType);
                }
                else
                {
                    Structure oldBase = building.GetBase();
                    Structure baseBuilding = (Structure)baseType;
                    if(oldBase != null && oldBase != baseBuilding)
                        Error(node, "incompatible partial class bases.");

                    building.SetBase(baseBuilding);
                    numbases++;
                }
            }

            // Notify the building.
            building.CompletedBases();
        }

        public override AstNode Visit(StructDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Process the base structures.
            node.GetStructure().SetBase(currentModule.GetValueTypeClass());

            // Process the base interfaces.
            ProcessBases(node);
         
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
     
        public override AstNode Visit(ClassDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Process the base structures.
            ProcessBases(node);

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

        public override AstNode Visit(InterfaceDefinition node)
        {
            // Use the generic scope.
            PseudoScope genScope = node.GetGenericScope();
            if(genScope != null)
                PushScope(genScope);

            // Process the base structures.
            ProcessBases(node);

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
    }
}

