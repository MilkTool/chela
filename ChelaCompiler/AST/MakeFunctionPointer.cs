using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class MakeFunctionPointer: Expression
    {
        private Expression returnType;
        private AstNode arguments;
        private MemberFlags flags;

        public MakeFunctionPointer (Expression returnType, AstNode arguments, MemberFlags flags, TokenPosition position)
            : base(position)
        {
            this.returnType = returnType;
            this.arguments = arguments;
            this.flags = flags;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetReturnType()
        {
            return returnType;
        }

        public AstNode GetArguments()
        {
            return arguments;
        }

        public MemberFlags GetFlags()
        {
            return flags;
        }
    }
}

