namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class CondtionBuilderState
	{
		//internal CondtionBuilderState()
		//{
			
		//}

		public string Identifier { get; private set; }
		public Operator Operator { get; private set; }
		public bool InBreaket { get; private set; }
		private int breaketCounter = 0;

		internal CondtionBuilderState(string currentIdentifier)
		{
			this.Identifier = currentIdentifier;
		}

		internal CondtionBuilderState ToOperator(Operator op)
		{
			this.Operator = op;
			return this;
		}

		internal CondtionBuilderState ToInBreaket(bool op)
		{
			if (op)
			{
				breaketCounter++;
			}
			else
			{
				breaketCounter--;
			}
			if (breaketCounter > 0)
			{
				this.InBreaket = false;
			}
			else
			{
				this.InBreaket = true;
			}

			return this;
		}

		public CondtionBuilderState ToAlias(string @alias)
		{
			Identifier = alias;
			return this;
		}
	}
}