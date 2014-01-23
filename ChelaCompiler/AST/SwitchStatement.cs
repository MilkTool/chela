using System.Collections.Generic;
using Chela.Compiler.Module;

namespace Chela.Compiler.Ast
{
    /// <summary>
    /// Switch statement.
    /// </summary>
    public class SwitchStatement: Statement
    {
        private Expression constant;
        private AstNode cases;
        private CaseLabel defaultCase;
        private IDictionary<ConstantValue, CaseLabel> caseDictionary;

        public SwitchStatement (Expression constant, AstNode cases, TokenPosition position)
            : base(position)
        {
            this.constant = constant;
            this.cases = cases;
            this.defaultCase = null;
            this.caseDictionary = new Dictionary<ConstantValue, CaseLabel> ();
        }

        public override AstNode Accept (AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Expression GetConstant()
        {
            return this.constant;
        }

        public void SetConstant(Expression constant)
        {
            this.constant = constant;
        }

        public AstNode GetCases()
        {
            return this.cases;
        }

        public CaseLabel GetDefaultCase()
        {
            return this.defaultCase;
        }

        public void SetDefaultCase(CaseLabel defaultCase)
        {
            this.defaultCase = defaultCase;
        }

        /// <summary>
        /// Gets the case dictionary.
        /// </summary>
        /// <value>
        /// The case dictionary.
        /// </value>
        public IDictionary<ConstantValue, CaseLabel> CaseDictionary {
            get {
                return caseDictionary;
            }
        }
    }
}

