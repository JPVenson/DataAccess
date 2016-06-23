using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class ConditionalEvalQuery<TPoco> : ElementProducer<TPoco>, IConditionalEvalQuery<TPoco>
	{
		private readonly CondtionBuilderState _state;

		public ConditionalEvalQuery(IQueryBuilder database, CondtionBuilderState state) : base(database)
		{
			_state = state;
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
			return new ConditionalQuery<TPoco>(this.QueryText("AND"), _state);
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
			return new ConditionalQuery<TPoco>(this.QueryText("OR"), _state);
		}
	}
}
