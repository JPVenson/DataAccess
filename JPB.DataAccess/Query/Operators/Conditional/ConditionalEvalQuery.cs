#region

using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IConditionalEvalQuery{TPoco}" />
	public class ConditionalEvalQuery<TPoco> : ElementResultQuery<TPoco>, IConditionalEvalQuery<TPoco>
	{
		/// <summary>
		///     The current query state
		/// </summary>
		public CondtionBuilderState State { get; private set; }

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalEvalQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public ConditionalEvalQuery(ConditionalEvalQuery<TPoco> database) : base(database)
		{
			State = database.State;
		}

		/// <inheritdoc />
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
			get
			{
				ContainerObject.Search<ConditionStatementQueryPart>().Conditions.Add(new ConditionStructurePart(ConditionStructurePart.LogicalOperator.And));
				return new ConditionalQuery<TPoco>(this);
			}
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Or
		{
			get
			{
				ContainerObject.Search<ConditionStatementQueryPart>().Conditions.Add(new ConditionStructurePart(ConditionStructurePart.LogicalOperator.Or));
				return new ConditionalQuery<TPoco>(this);
			}
		}
	}
}