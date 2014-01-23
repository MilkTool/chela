namespace Chela.Compiler.Ast
{
    public class UsingObjectStatement: ScopeNode
    {
        private LocalVariablesDeclaration locals;

        public UsingObjectStatement (LocalVariablesDeclaration locals, AstNode children, TokenPosition position)
            : base(children, position)
        {
            this.locals = locals;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public LocalVariablesDeclaration GetLocals()
        {
            return locals;
        }
    }
}

