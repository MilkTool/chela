namespace Chela.Compiler.Ast
{
    /// <summary>
    /// Goto case statement.
    /// </summary>
    public class GotoCaseStatement: Statement
    {
        private Expression label;
        private CaseLabel targetLabel;
        private SwitchStatement switchStatement;

        public GotoCaseStatement(Expression label, TokenPosition position)
            : base(position)
        {
            this.label = label;
        }

        public override AstNode Accept(AstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        /// <summary>
        /// Gets the label expression.
        /// </summary>
        /// <returns>
        /// The label.
        /// </returns>
        public Expression GetLabel()
        {
            return label;
        }

        /// <summary>
        /// Sets the label expression.
        /// </summary>
        /// <param name='label'>
        /// Label.
        /// </param>
        public void SetLabel(Expression label)
        {
            this.label = label;
        }

        /// <summary>
        /// Gets the target label.
        /// </summary>
        /// <returns>
        /// The target label.
        /// </returns>
        public CaseLabel GetTargetLabel()
        {
            return targetLabel;
        }

        /// <summary>
        /// Sets the target label.
        /// </summary>
        /// <param name='targetLabel'>
        /// Target label.
        /// </param>
        public void SetTargetLabel(CaseLabel targetLabel)
        {
            this.targetLabel = targetLabel;
        }

        /// <summary>
        /// Gets the switch holding this statement.
        /// </summary>
        /// <returns>
        /// The switch.
        /// </returns>
        public SwitchStatement GetSwitch()
        {
            return switchStatement;
        }

        /// <summary>
        /// Sets the switch statement holding this statement..
        /// </summary>
        /// <param name='switchStatement'>
        /// Switch statement.
        /// </param>
        public void SetSwitch(SwitchStatement switchStatement)
        {
            this.switchStatement = switchStatement;
        }
    }
}

