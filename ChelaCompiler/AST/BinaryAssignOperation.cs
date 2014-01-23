using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class BinaryAssignOperation: Expression
    {
        private Expression variable;
        private Expression value;
        private bool pointerArithmetic;
        private int operation;
        private IChelaType secondCoercion;
        private Function overload;

        public BinaryAssignOperation (int operation, Expression variable, Expression value, TokenPosition position)
            : base(position)
        {
            this.operation = operation;
            this.variable = variable;
            this.value = value;
            this.pointerArithmetic = false;
            this.secondCoercion = null;
            this.overload = null;
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public int GetOperation()
        {
            return this.operation;
        }

        public Expression GetVariable()
        {
            return this.variable;
        }
        
        public Expression GetValue()
        {
            return this.value;
        }

        public bool IsPointerArithmetic()
        {
            return pointerArithmetic;
        }

        public void SetPointerArithmetic(bool pointerArithmetic)
        {
            this.pointerArithmetic = pointerArithmetic;
        }

        public Function GetOverload()
        {
            return this.overload;
        }

        public void SetOverload(Function overload)
        {
            this.overload = overload;
        }

        public IChelaType GetSecondCoercion()
        {
            return this.secondCoercion;
        }

        public void SetSecondCoercion(IChelaType secondCoercion)
        {
            this.secondCoercion = secondCoercion;
        }
    }
}

