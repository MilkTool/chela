using System.Collections.Generic;
using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class AstVisitor
    {
        protected Scope currentScope;
        protected Scope currentContainer;
        protected LinkedList<Scope> scopeStack;
        protected int unsafeCount;
        
        public AstVisitor ()
        {
            this.currentScope = null;
            this.currentContainer = null;
            this.unsafeCount = 0;
            this.scopeStack = new LinkedList<Scope> ();
        }

        public virtual void BeginPass()
        {
        }

        public virtual void EndPass()
        {
        }
        
        public virtual AstNode Visit(AstNode node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(AttributeArgument node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(AttributeInstance node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(BaseExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FileNode node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(BlockNode node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(UnsafeBlockNode node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(BoolConstant node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(BreakStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ContinueStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ModuleNode node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(NamespaceDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(UsingStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(UsingObjectStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(AliasDeclaration node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(TypedefDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(StructDefinition node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ClassDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ConstructorInitializer node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(InterfaceDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(EnumDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DelegateDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(EnumConstantDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DoubleConstant node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(FunctionArgument node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(FunctionDefinition node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(FunctionPrototype node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FieldDeclaration node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FieldDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GenericSignature node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GenericParameter node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GenericConstraint node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GenericInstanceExpr node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(PropertyDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GetAccessorDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(SetAccessorDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(EventDefinition node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(EventAccessorDefinition node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(FloatConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(CharacterConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ByteConstant node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(SByteConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ShortConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(UShortConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(IntegerConstant node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(UIntegerConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(LongConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ULongConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NullConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(CStringConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(StringConstant node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(IfStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(SwitchStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(CaseLabel node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(GotoCaseStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(WhileStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(DoWhileStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ForStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ForEachStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FixedStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FixedVariableDecl node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(TypeNode node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(MakePointer node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(MakeReference node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(MakeConstant node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(MakeArray node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(MakeFunctionPointer node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ReturnStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(VariableReference node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(ExpressionStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(AssignmentExpression node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(UnaryOperation node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(BinaryOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(BinaryAssignOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(PrefixOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(PostfixOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(TernaryOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(SizeOfExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(TypeOfExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DefaultExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(CallExpression node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(CastOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ReinterpretCast node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(AsExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(IsExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NewExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NewArrayExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NewRawExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NewRawArrayExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DeleteStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DeleteRawArrayStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(CatchStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(FinallyStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(TryStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(ThrowStatement node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(LocalVariablesDeclaration node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(LockStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(VariableDeclaration node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(AddressOfOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(RefExpression node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(MemberAccess node)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual AstNode Visit(IndirectAccess node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(DereferenceOperation node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(SubscriptAccess node)
        {
            throw new System.NotImplementedException();
        }

        public virtual AstNode Visit(NullStatement node)
        {
            throw new System.NotImplementedException();
        }
        
        protected void VisitList(AstNode list)
        {
            try
            {
                while(list != null)
                {
                    list.Accept(this);
                    list = list.GetNext();
                }
            }
            catch(System.SystemException sysException)
            {
                System.Console.WriteLine("Compiler bug near {0}", list.GetPosition().ToString());
                System.Console.WriteLine(sysException.ToString());
                System.Console.WriteLine(sysException.StackTrace);
                System.Environment.Exit(-1);
            }
        }
        
        protected void Error(AstNode where, string what)
        {
            throw new CompilerException(what, where.GetPosition());
        }

        protected void Error(AstNode where, string what, params object[] args)
        {
            Error(where, string.Format(what, args));
        }

        protected bool TryError(AstNode where, string what, params object[] args)
        {
            if(where == null)
                return false;

            Error(where, string.Format(what, args));
            return true;
        }

        protected void Warning(AstNode where, string what)
        {
            System.Console.Error.Write(where.GetPosition().ToString() +
                                       ": warning: " + what + "\n");
        }

        protected void Warning(AstNode where, string what, params object[] args)
        {
            Warning(where, string.Format(what, args));
        }

        protected void UnsafeError(AstNode where, string what, params object[] args)
        {
            if(!IsUnsafe)
                Error(where, what, args);
        }

        protected void UnsafeError(AstNode where, string what)
        {
            if(!IsUnsafe)
                Error(where, what);
        }

        protected void PushUnsafe()
        {
            ++unsafeCount;
        }

        protected void PopUnsafe()
        {
            --unsafeCount;
            if(unsafeCount < 0)
                throw new ModuleException("unbalanced unsafe scopes.");
        }

        protected bool IsUnsafe {
            get {
                return unsafeCount > 0;
            }
        }

        protected void PushScope(Scope scope)
        {
            scopeStack.AddFirst(currentScope);
            currentScope = scope;

            // Differentiate between pseudo-scopes.
            if(!currentScope.IsPseudoScope())
                currentContainer = currentScope;

            // Push the unsafe context.
            if(scope.IsUnsafe())
                PushUnsafe();
        }
        
        protected void PopScope()
        {
            // Restore the unsafe context.
            if(currentScope != null && currentScope.IsUnsafe())
                PopUnsafe();

            currentScope = scopeStack.First.Value;
            scopeStack.RemoveFirst();

            if(currentScope == null || !currentScope.IsPseudoScope())
            {
                currentContainer = currentScope;
            }
            else
            {
                // Find the closest container.
                currentContainer = null;
                foreach(Scope scope in scopeStack)
                {
                    if(!scope.IsPseudoScope())
                    {
                        currentContainer = scope;
                        break;
                    }
                }

                // Couldn't find a container.
                if(currentContainer == null)
                    throw new System.ApplicationException("couldn't find a container scope.");
            }
        }
    }
}

