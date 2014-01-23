using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    public class ReturnStatement: Statement
    {
        private Expression expression;
        private bool yielding;
        private int yieldState;
        private BasicBlock mergeBlock;
        private BasicBlock disposeBlock;
        
        public ReturnStatement (Expression expr, bool yielding, TokenPosition position)
            : base(position)
        {
            this.expression = expr;
            this.yielding = yielding;
        }

        public ReturnStatement (Expression expr, TokenPosition position)
            : this(expr, false, position)
        {
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetExpression()
        {
            return this.expression;
        }

        public bool IsYield {
            get {
                return yielding;
            }
            set {
                yielding = true;
            }
        }

        public int YieldState {
            get {
                return yieldState;
            }
            set {
                yieldState = value;
            }
        }

        public BasicBlock MergeBlock {
            get {
                return mergeBlock;
            }
            set {
                mergeBlock = value;
            }
        }

        public BasicBlock DisposeBlock {
            get {
                return disposeBlock;
            }
            set {
                disposeBlock = value;
            }
        }
    }
}

