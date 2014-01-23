namespace Chela.Compiler.Ast
{
	public class MemberAccess: ObjectReferenceExpression
	{
		private Expression reference;
		private int slot;
        private bool implicitThis;
		
		public MemberAccess (Expression reference, string name, TokenPosition position)
			: base(position)
		{
			SetName(name);
			this.reference = reference;
			this.slot = -1;
		}
		
		public override AstNode Accept (AstVisitor visitor)
		{
			return visitor.Visit(this);
		}
		
		public Expression GetReference()
		{
			return this.reference;
		}
		
		public int GetSlot()
		{
			return this.slot;
		}
		
		public void SetSlot(int slot)
		{
			this.slot = slot;
		}

        public bool ImplicitSelf {
            get {
                return implicitThis;
            }
            set {
                implicitThis = value;
            }
        }
	}
}

