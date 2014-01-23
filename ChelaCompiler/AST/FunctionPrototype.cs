using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class FunctionPrototype: ScopeNode
    {
        private MemberFlags flags;
        private Expression nameExpression;
        private Expression returnType;
        private FunctionArgument arguments;
        private Function function;
        private ConstructorInitializer ctorInitializer;
        private string destructorName;
        private GenericSignature genericSignature;
     
        public FunctionPrototype (MemberFlags flags, Expression returnType,
                                  FunctionArgument arguments, string name,
                                  GenericSignature genericSignature,
                                  TokenPosition position)
         : base(null, position)
        {
            SetName (name);
            this.flags = flags;
            this.returnType = returnType;
            this.arguments = arguments;
            this.function = null;
            this.ctorInitializer = null;
            this.destructorName = null;
            this.genericSignature = genericSignature;
        }

        public FunctionPrototype (MemberFlags flags, Expression returnType,
                               FunctionArgument arguments, Expression nameExpression,
                                  GenericSignature genericSignature,
                                  TokenPosition position)
         : this(flags, returnType, arguments, "", genericSignature, position)
        {
            this.nameExpression = nameExpression;
        }

        public FunctionPrototype (MemberFlags flags, Expression returnType,
                                  FunctionArgument arguments, string name,
                                  TokenPosition position)
            : this(flags, returnType, arguments, name, null, position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit (this);
        }
     
        public FunctionArgument GetArguments ()
        {
            return arguments;
        }
     
        public void SetArguments (FunctionArgument arguments)
        {
            this.arguments = arguments;
        }

        public Expression GetNameExpression()
        {
            return nameExpression;
        }

        public Expression GetReturnType ()
        {
            return returnType;
        }

        public MemberFlags GetFlags ()
        {
            return flags;
        }

        public void SetFlags(MemberFlags flags)
        {
            this.flags = flags;
        }

        public Function GetFunction ()
        {
            return this.function;
        }

        public void SetFunction (Function function)
        {
            this.function = function;
        }

        public ConstructorInitializer GetConstructorInitializer ()
        {
            return this.ctorInitializer;
        }

        public void SetConstructorInitializer (ConstructorInitializer ctorInitializer)
        {
            this.ctorInitializer = ctorInitializer;
        }

        public string GetDestructorName ()
        {
            return destructorName;
        }

        public void SetDestructorName (string dtorName)
        {
            destructorName = dtorName;
        }

        public GenericSignature GetGenericSignature ()
        {
            return genericSignature;
        }
    }
}

