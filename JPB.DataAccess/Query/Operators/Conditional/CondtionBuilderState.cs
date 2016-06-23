namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class CondtionBuilderState
	{
		internal CondtionBuilderState()
		{
			
		}

		public Operator Operator { get; private set; }
		public bool InBreaket { get; private set; }

		public CondtionBuilderState ToOperator(Operator op)
		{
			this.Operator = op;
			return this;
		}

		public CondtionBuilderState ToInBreaket(bool op)
		{
			this.InBreaket = op;
			return this;
		}
	}
}