using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
	public class BinaryOperation: Expression
	{
		public const int OpAdd = 0;
		public const int OpSub = 1;
		public const int OpMul = 2;
		public const int OpDiv = 3;
		public const int OpMod = 4;
		public const int OpBitAnd = 5;
		public const int OpBitOr = 6;
		public const int OpBitXor = 7;
		public const int OpBitLeft = 8;
		public const int OpBitRight = 9;
		public const int OpLT = 10;
		public const int OpGT = 11;
		public const int OpEQ = 12;
		public const int OpNEQ = 13;
		public const int OpLEQ = 14;
		public const int OpGEQ = 15;
		public const int OpLAnd = 16;
		public const int OpLOr = 17;

		private int operation;
		private Expression left;
		private Expression right;
        private bool pointerArithmetic;
        private IChelaType secondCoercion;
        private IChelaType operationType;
        private Function overload;
        private bool matrixMul;
		
		public BinaryOperation (int operation, Expression left, Expression right,
		                        TokenPosition position)
			: base(position)
		{
			this.operation = operation;
			this.left = left;
			this.right = right;
            this.pointerArithmetic = false;
            this.secondCoercion = null;
            this.operationType = null;
            this.overload = null;
            this.matrixMul = false;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public int GetOperation()
		{
			return this.operation;
		}

        public void SetLeftExpression(Expression left)
        {
            this.left = left;
        }

		public Expression GetLeftExpression()
		{
			return this.left;
		}

        public void SetRightExpression(Expression right)
        {
            this.right = right;
        }
		
		public Expression GetRightExpression()
		{
			return this.right;
		}

        public bool IsPointerArithmetic()
        {
            return pointerArithmetic;
        }

        public void SetPointerArithmetic(bool pointerArithmetic)
        {
            this.pointerArithmetic = pointerArithmetic;
        }

        public IChelaType GetSecondCoercion()
        {
            return this.secondCoercion;
        }

        public void SetSecondCoercion(IChelaType secondCoercion)
        {
            this.secondCoercion = secondCoercion;
        }

        public IChelaType GetOperationType()
        {
            return this.operationType;
        }

        public void SetOperationType(IChelaType operationType)
        {
            this.operationType = operationType;
        }

        public static bool IsArithmetic(int op)
        {
            return op <= OpMod || op == OpBitLeft;
        }

        public static bool IsEquality(int op)
        {
            return op == OpEQ || op == OpNEQ;
        }

        public Function GetOverload()
        {
            return this.overload;
        }

        public void SetOverload(Function overload)
        {
            this.overload = overload;
        }

        public void SetMatrixMul(bool value)
        {
            this.matrixMul = value;
        }

        public bool IsMatrixMul()
        {
            return this.matrixMul;
        }

        public static string GetOpName(int op)
        {
            switch(op)
            {
            case OpAdd: return "Op_Add";
            case OpSub: return "Op_Sub";
            case OpMul: return "Op_Mul";
            case OpDiv: return "Op_Div";
            case OpMod: return "Op_Mod";
            case OpBitAnd: return "Op_And";
            case OpBitOr: return "Op_Or";
            case OpBitXor: return "Op_Xor";
            case OpBitLeft: return "Op_ShiftLeft";
            case OpBitRight: return "Op_ShiftRight";
            case OpLT: return "Op_Lt";
            case OpLEQ: return "Op_Leq";
            case OpGT: return "Op_Gt";
            case OpGEQ: return "Op_Geq";
            case OpEQ: return "Op_Eq";
            case OpNEQ: return "Op_Neq";
            case OpLAnd: return null;
            case OpLOr: return null;
            default:
                throw new System.NotImplementedException();
            }
        }

        public static string GetOpErrorName(int op)
        {
            switch(op)
            {
            case OpAdd: return "'+'";
            case OpSub: return "'-'";
            case OpMul: return "'*'";
            case OpDiv: return "'/'";
            case OpMod: return "'%'";
            case OpBitAnd: return "'&'";
            case OpBitOr: return "'|'";
            case OpBitXor: return "'^'";
            case OpBitLeft: return "'<<'";
            case OpBitRight: return "'>>'";
            case OpLT: return "'<'";
            case OpLEQ: return "'<='";
            case OpGT: return "'>'";
            case OpGEQ: return "'>='";
            case OpEQ: return "'=='";
            case OpNEQ: return "'!='";
            case OpLAnd: return "'&&'";
            case OpLOr: return "'||'";
            default:
                throw new System.NotImplementedException();
            }
        }
	}
}

