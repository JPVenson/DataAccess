#region

using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IConditionalEvalQuery{TPoco}" />
	public class ConditionalEvalQuery<TPoco> : ElementProducer<TPoco>, IConditionalEvalQuery<TPoco>
	{
		/// <summary>
		///     The current query state
		/// </summary>
		public CondtionBuilderState State { get; private set; }

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalEvalQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="state">The state.</param>
		public ConditionalEvalQuery(ConditionalEvalQuery<TPoco> database) : base(database)
		{
			State = database.State;
		}

		public ConditionalEvalQuery(IQueryBuilder builder) : base(builder)
		{
			State = new CondtionBuilderState(null);
			if (builder is IStateQuery)
			{
				State = ((IStateQuery)builder).State;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalEvalQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="state">The state.</param>
		public ConditionalEvalQuery(IQueryBuilder database, CondtionBuilderState state) : base(database)
		{
			State = state;
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> And
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("AND"), State); }
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Or
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("OR"), State); }
		}

		/// <summary>
		///     Closes an Parenthesis if any is open. When not leading ( is found noting happens
		/// </summary>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> ParenthesisClose
		{
			get
			{
				if (State.InBreaket)
					return new ConditionalEvalQuery<TPoco>(this.QueryText(")"), State.ToInBreaket(true));
				return new ConditionalEvalQuery<TPoco>(this, State);
			}
		}
	}
}