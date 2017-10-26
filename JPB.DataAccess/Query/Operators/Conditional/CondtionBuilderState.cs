namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	///     Holds informations about the current query state. No historical data.
	///     For Internal Usage only
	/// </summary>
	public class CondtionBuilderState
	{
		private int _breaketCounter;

		internal CondtionBuilderState(string currentIdentifier)
		{
			Identifier = currentIdentifier;
		}

		//internal CondtionBuilderState()
		//{

		//}

		/// <summary>
		///     If Used the last Identifier for Tables or columns
		/// </summary>
		public string Identifier { get; private set; }

		/// <summary>
		///     If currently build, the Operator for the Conditional Query
		/// </summary>
		public Operator Operator { get; private set; }

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public bool InBreaket { get; private set; }

		internal CondtionBuilderState ToOperator(Operator op)
		{
			Operator = op;
			return this;
		}

		internal CondtionBuilderState ToInBreaket(bool op)
		{
			if (op)
			{
				_breaketCounter++;
			}
			else
			{
				_breaketCounter--;
			}
			if (_breaketCounter > 0)
			{
				InBreaket = false;
			}
			else
			{
				InBreaket = true;
			}

			return this;
		}

		/// <summary>
		///     Changes the current used Alias. Does not update the alias in other query elements
		///     For Internal Usage only
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public CondtionBuilderState ToAlias(string alias)
		{
			Identifier = alias;
			return this;
		}
	}
}