using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class ConditionalEvalQuery<TPoco> : ElementProducer<TPoco>, IConditionalEvalQuery<TPoco>
	{
		public readonly CondtionBuilderState State;

		public ConditionalEvalQuery(IQueryBuilder database, CondtionBuilderState state) : base(database)
		{
			State = state;
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalQuery<TPoco> And()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("AND"), State);
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Or()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("OR"), State);
		}

		/// <summary>
		/// Closes an Parenthesis if any is open. When not leading ( is found noting happens
		/// </summary>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> ParenthesisClose()
		{
			if (State.InBreaket)
				return new ConditionalEvalQuery<TPoco>(this.QueryText(")"), State.ToInBreaket(true));
			return new ConditionalEvalQuery<TPoco>(this, State);
		}
	}
}
