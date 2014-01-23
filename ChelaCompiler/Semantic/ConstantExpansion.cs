using System.Collections.Generic;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;

namespace Chela.Compiler.Semantic
{
    public class ConstantExpansion: ObjectDeclarator
    {
        protected SwitchStatement currentSwitch;

        public ConstantExpansion ()
        {
            this.currentSwitch = null;
        }

        public override AstNode Visit (ModuleNode node)
        {
            // Extract the constants.
            ConstantGrouping grouping = new ConstantGrouping();
            node.Accept(grouping);

            Dictionary<FieldVariable, ConstantData> constants = grouping.GetConstants();

            // Build constant dependency graph and perform a topolgical sort.
            ConstantDependencies deps = new ConstantDependencies(constants);
            List<ConstantData> sortedConstants = deps.BuildGraph();

            // Expand the constants itself.
            foreach(ConstantData constant in sortedConstants)
            {
                // Visit the initializer.
                Expression initializer = constant.GetInitializer();
                initializer = (Expression)initializer.Accept(this);

                // The initializer must be a constant.
                if(!initializer.IsConstant())
                    Error(initializer, "is not a constant expression.");

                // The constant expression value must be the constant itself.
                FieldVariable field = constant.GetVariable();
                ConstantValue initValue = (ConstantValue)initializer.GetNodeValue();
                field.SetInitializer(initValue.Cast(field.GetVariableType()));
            }

            // Now, perform module constant expansion.
            return base.Visit (node);
        }


        public override AstNode Visit (StructDefinition node)
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
        
        public override AstNode Visit (ClassDefinition node)
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

        public override AstNode Visit (InterfaceDefinition node)
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

        public override AstNode Visit (DelegateDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            return node;
        }

        public override AstNode Visit (FieldDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            return node;
        }

        public override AstNode Visit (CallExpression node)
        {
            return node;
        }

        public override AstNode Visit (PropertyDefinition node)
        {
            // Visit the property accessors.
            if(node.GetAccessor != null)
               node.GetAccessor.Accept(this);

            if(node.SetAccessor != null)
               node.SetAccessor.Accept(this);

            return node;
        }

        public override AstNode Visit (FunctionDefinition node)
        {
            // Process attributes.
            ProcessAttributes(node);

            // Ignore pure declarations.
            if(node.GetChildren() == null)
                return node;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (GetAccessorDefinition node)
        {
            // Ignore pure declarations.
            if(node.GetChildren() == null)
                return node;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (SetAccessorDefinition node)
        {
            // Ignore pure declarations.
            if(node.GetChildren() == null)
                return node;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (AttributeArgument node)
        {
            // Expand the argument value.
            Expression argValue = node.GetValueExpression();
            argValue = (Expression)argValue.Accept(this);
            node.SetValueExpression(argValue);

            return node;
        }

        public override AstNode Visit (AttributeInstance node)
        {
            // Visit the arguments.
            VisitList(node.GetArguments());

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
            // Ignore pure declarations.
            if(node.GetChildren() == null)
                return node;

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (UnsafeBlockNode node)
        {
            // Ignore pure declarations.
            if(node.GetChildren() == null)
                return node;

            // Push the unsafe.
            PushUnsafe();

            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Restore the scope.
            PopScope();

            // Pop the unsafe scope.
            PopUnsafe();

            return node;
        }

        public override AstNode Visit (ExpressionStatement node)
        {
            return node;
        }

        public override AstNode Visit (IfStatement node)
        {
            // Visit the then node.
            AstNode thenNode = node.GetThen();
            thenNode.Accept(this);

            // Visit the else node.
            AstNode elseNode = node.GetElse();
            if(elseNode != null)
                elseNode.Accept(this);

            return node;
        }

        public override AstNode Visit (SwitchStatement node)
        {
            // Store the old switch statement.
            SwitchStatement oldSwitch = currentSwitch;
            currentSwitch = node;

            // Visit the cases.
            VisitList(node.GetCases());

            // Restore the switch statement.
            currentSwitch = oldSwitch;

            return node;
        }

        public override AstNode Visit (CaseLabel node)
        {
            // Get the constant expression.
            Expression constant = node.GetConstant();

            // Expand it.
            if(constant != null)
            {
                constant = (Expression)constant.Accept(this);
                node.SetConstant(constant);

                // Get the constant value.
                IChelaType coercionType = currentSwitch.GetCoercionType();
                ConstantValue constantValue = (ConstantValue)constant.GetNodeValue();
                constantValue = constantValue.Cast(coercionType);

                // Make sure the case definition is unique.
                IDictionary<ConstantValue, CaseLabel> caseDict = currentSwitch.CaseDictionary;
                CaseLabel oldCase;
                if(caseDict.TryGetValue(constantValue, out oldCase))
                    Error(node, "previous definition of the same case in {0}.", oldCase.GetPosition().ToString());

                // Register the case.
                caseDict.Add(constantValue, node);
            }

            // Visit the children.
            VisitList(node.GetChildren());

            return node;
        }

        public override AstNode Visit(GotoCaseStatement node)
        {
            // Store the current switch in the node.
            node.SetSwitch(currentSwitch);

            // Get the constant expression.
            Expression constant = node.GetLabel();

            // Expand it.
            CaseLabel targetLabel;
            if(constant != null)
            {
                constant = (Expression)constant.Accept(this);
                node.SetLabel(constant);

                // Get the constant value.
                IChelaType coercionType = currentSwitch.GetCoercionType();
                node.SetCoercionType(coercionType);
            }
            else
            {
                // Make sure the current switch contains a default case.
                targetLabel = currentSwitch.GetDefaultCase();
                if(targetLabel == null)
                    Error(node, "current switch doesn't contain a default case.");
                node.SetTargetLabel(targetLabel);
            }

            return node;
        }

        public override AstNode Visit (WhileStatement node)
        {
            // Visit children.
            VisitList(node.GetJob());

            return node;
        }

        public override AstNode Visit (DoWhileStatement node)
        {
            // Visit children.
            VisitList(node.GetJob());

            return node;
        }

        public override AstNode Visit (ForStatement node)
        {
            // Push the scope.
            PushScope(node.GetScope());

            // Visit children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (ForEachStatement node)
        {
            // Push the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (FixedStatement node)
        {
            // Push the scope.
            PushScope(node.GetScope());

            // Visit children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (UsingObjectStatement node)
        {
            // Push the scope.
            PushScope(node.GetScope());

            // Visit the declarations.
            LocalVariablesDeclaration decls = node.GetLocals();
            decls.Accept(this);

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (LockStatement node)
        {
            // Push the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

            // Pop the scope.
            PopScope();

            return node;
        }

        public override AstNode Visit (TryStatement node)
        {
            // Visit the try clause.
            AstNode tryStatement = node.GetTryStatement();
            tryStatement.Accept(this);

            // Visit the catch clause.
            VisitList(node.GetCatchList());

            // Visit the finally clause.
            AstNode finallyStament = node.GetFinallyStatement();
            if(finallyStament != null)
                finallyStament.Accept(this);

            return node;
        }

        public override AstNode Visit (CatchStatement node)
        {
            // Update the scope.
            PushScope(node.GetScope());

            // Visit the children.
            VisitList(node.GetChildren());

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

        public override AstNode Visit (SizeOfExpression node)
        {
            IChelaType type = node.GetCoercionType();
            IChelaType nodeType = node.GetNodeType();
            if(nodeType.IsConstant())
            {
                return (new ConstantValue((int)type.GetSize())).ToAstNode(node.GetPosition());
            }
            return node;
        }

        public override AstNode Visit (TypeOfExpression node)
        {
            return node;
        }

        public override AstNode Visit (DefaultExpression node)
        {
            return node;
        }

        public override AstNode Visit (CastOperation node)
        {
            // First expand the value.
            Expression value = node.GetValue();
            value = (Expression)value.Accept(this);
            node.SetValue(value);

            // Check if result is constant.
            IChelaType nodeType = node.GetNodeType();
            if(!nodeType.IsConstant())
                return node;

            // Cast the constant.
            IChelaType valueType = value.GetNodeType();
            if(valueType == nodeType)
                return value;

            ConstantValue valueConstant = (ConstantValue)value.GetNodeValue();
            return valueConstant.Cast(nodeType).ToAstNode(node.GetPosition());
        }

        public override AstNode Visit (UnaryOperation node)
        {
            // Remove the nop expression.
            if(node.GetOperation() == UnaryOperation.OpNop)
                return node.GetExpression();

            // Process the base expression.
            Expression expr = node.GetExpression();
            expr = (Expression)expr.Accept(this);
            node.SetExpression(expr);

            // Get the node type.
            IChelaType nodeType = node.GetNodeType();
            if(!nodeType.IsConstant())
                return node;

            // Perform the base coercion.
            ConstantValue baseConstant = (ConstantValue)expr.GetNodeValue();
            IChelaType baseType = expr.GetNodeType();
            IChelaType baseCoercion = node.GetCoercionType();
            if(baseType != baseCoercion)
                baseConstant = baseConstant.Cast(baseCoercion);

            // Perform the constant operation.
            ConstantValue res = null;
            switch(node.GetOperation())
            {
            case UnaryOperation.OpNop:
                res = baseConstant;
                break;
            case UnaryOperation.OpNeg:
                res = -baseConstant;
                break;
            case UnaryOperation.OpNot:
                res = !baseConstant;
                break;
            case UnaryOperation.OpBitNot:
                res = ~baseConstant;
                break;
            }

            return res.ToAstNode(node.GetPosition());
        }

        public override AstNode Visit (BinaryOperation node)
        {
            // Process the left expression.
            Expression left = node.GetLeftExpression();
            left = (Expression)left.Accept(this);
            node.SetLeftExpression(left);

            // Process the right expression.
            Expression right = node.GetRightExpression();
            right = (Expression)right.Accept(this);
            node.SetRightExpression(node);

            // Get the node type.
            IChelaType nodeType = node.GetNodeType();
            if(!nodeType.IsConstant())
                return node;

            // Perform left constant coercion.
            ConstantValue leftConstant = (ConstantValue)left.GetNodeValue();
            IChelaType leftType = left.GetNodeType();
            IChelaType leftCoercion = node.GetCoercionType();
            if(leftType != leftCoercion)
                leftConstant = leftConstant.Cast(leftCoercion);

            // Perform right constant coercion.
            ConstantValue rightConstant = (ConstantValue)right.GetNodeValue();
            IChelaType rightType = right.GetNodeType();
            IChelaType rightCoercion = node.GetSecondCoercion();
            if(rightType != rightCoercion)
                rightConstant = rightConstant.Cast(rightCoercion);

            // Perform the constant operation.
            ConstantValue res = null;
            switch(node.GetOperation())
            {
            case BinaryOperation.OpAdd:
                res = leftConstant + rightConstant;
                break;
            case BinaryOperation.OpSub:
                res = leftConstant - rightConstant;
                break;
            case BinaryOperation.OpMul:
                res = leftConstant * rightConstant;
                break;
            case BinaryOperation.OpDiv:
                res = leftConstant / rightConstant;
                break;
            case BinaryOperation.OpMod:
                res = leftConstant % rightConstant;
                break;
            case BinaryOperation.OpEQ:
                res = ConstantValue.Equals(leftConstant, rightConstant);
                break;
            case BinaryOperation.OpNEQ:
                res = ConstantValue.NotEquals(leftConstant, rightConstant);
                break;
            case BinaryOperation.OpLT:
                res = leftConstant < rightConstant;
                break;
            case BinaryOperation.OpLEQ:
                res = leftConstant <= rightConstant;
                break;
            case BinaryOperation.OpGT:
                res = leftConstant > rightConstant;
                break;
            case BinaryOperation.OpGEQ:
                res = leftConstant >= rightConstant;
                break;
            case BinaryOperation.OpBitAnd:
                res = leftConstant & rightConstant;
                break;
            case BinaryOperation.OpBitOr:
                res = leftConstant | rightConstant;
                break;
            case BinaryOperation.OpBitXor:
                res = leftConstant ^ rightConstant;
                break;
            case BinaryOperation.OpBitLeft:
                res = ConstantValue.BitLeft(leftConstant, rightConstant);
                break;
            case BinaryOperation.OpBitRight:
                res = ConstantValue.BitRight(leftConstant, rightConstant);
                break;
            case BinaryOperation.OpLAnd:
                res = new ConstantValue(leftConstant.GetBoolValue() && rightConstant.GetBoolValue());
                break;
            case BinaryOperation.OpLOr:
                res = new ConstantValue(leftConstant.GetBoolValue() || rightConstant.GetBoolValue());
                break;
            default:
                throw new System.NotImplementedException();
            }

            // Return the result node.
            return res.ToAstNode(node.GetPosition());
        }

        public override AstNode Visit (ReturnStatement node)
        {
            return node;
        }

        public override AstNode Visit (BreakStatement node)
        {
            return node;
        }

        public override AstNode Visit (ContinueStatement node)
        {
            return node;
        }

        public override AstNode Visit (LocalVariablesDeclaration node)
        {
            return node;
        }

        public override AstNode Visit (ThrowStatement node)
        {
            return node;
        }

        public override AstNode Visit (NewExpression node)
        {
            return node;
        }

        public override AstNode Visit (NewArrayExpression node)
        {
            return node;
        }

        public override AstNode Visit (NewRawArrayExpression node)
        {
            return node;
        }

        public override AstNode Visit (DeleteStatement node)
        {
            return node;
        }

        public override AstNode Visit (DeleteRawArrayStatement node)
        {
            return node;
        }

        public override AstNode Visit (VariableReference node)
        {
           // Get the node type.
            IChelaType variableType = node.GetNodeType();

            // Ignore type references, namespaces and functions.
            if(variableType.IsMetaType() || variableType.IsNamespace() ||
               variableType.IsFunctionGroup() || variableType.IsFunction())
                return node;

            // The type must be a reference.
            variableType = DeReferenceType(variableType);

            // Now, it can be a constant
            if(variableType.IsConstant())
            {
                // Perform constant expansion.
                FieldVariable field = (FieldVariable)node.GetNodeValue();
                if(!field.IsExternal())
                    return field.GetInitializer().ToAstNode(node.GetPosition());
            }

            return node;
        }

        public override AstNode Visit (MemberAccess node)
        {
           // Get the node type.
            IChelaType variableType = node.GetNodeType();

            // Ignore type references, namespaces and functions.
            if(variableType.IsMetaType() || variableType.IsNamespace() ||
               variableType.IsFunctionGroup() || variableType.IsFunction())
                return node;

            // The type must be a reference.
            variableType = DeReferenceType(variableType);

            // Now, it can be a constant
            if(variableType.IsConstant())
            {
                // Perform constant expansion.
                FieldVariable field = (FieldVariable)node.GetNodeValue();
                if(!field.IsExternal())
                    return field.GetInitializer().ToAstNode(node.GetPosition());
            }

            return node;
        }

        public override AstNode Visit (IndirectAccess node)
        {
            return node;
        }

    }
}
