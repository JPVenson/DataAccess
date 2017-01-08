using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IConditionalColumnQuery{TPoco}" />
	public class ConditionalColumnQuery<TPoco> : QueryBuilderX, IConditionalColumnQuery<TPoco>
	{
		/// <summary>
		/// The state of the current bilder
		/// </summary>
		public readonly CondtionBuilderState State;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionalColumnQuery{TPoco}"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="state">The state.</param>
		public ConditionalColumnQuery(IQueryBuilder database, CondtionBuilderState state) : base(database)
		{
			State = state;
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Is
		{
			get
			{
				return new ConditionalOperatorQuery<TPoco>(this, State.ToOperator(Operator.Is));
			}
		}

		///// <summary>
		///// Prepaires an Conditional Query that targets an single Column
		///// </summary>
		//public ConditionalEvalQuery<TPoco> Is(object value)
		//{
		//	if (value == null)
		//	{
		//		return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"), State);
		//	}

		//	return QueryOperatorValue("=", value);
		//}

		///// <summary>
		///// Defines an condition that should be inverted
		///// </summary>
		//public ConditionalEvalQuery<TPoco> Not(object value)
		//{
		//	if (value == null)
		//	{
		//		return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NOT NULL"), State);
		//	}
		//	return QueryOperatorValue("<>", value);
		//}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> IsQueryOperatorValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(value), State);
		}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{1} @m_val{0}", nextParameterId, operators), new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)), State);
		}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> IsQueryValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ("= " + value), State);
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		public ConditionalEvalQuery<TPoco> NotQueryValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
					.QueryQ("<> " + value), State);
		}
	}
}
