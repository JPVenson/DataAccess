#region

using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="ElementProducer{TPoco}" />
	/// <seealso cref="IConditionalEvalQuery{T}" />
	public class ConditionalEvalQuery<TPoco> : ElementResultQuery<TPoco>, IConditionalEvalQuery<TPoco>
	{
		/// <inheritdoc />
		public ConditionalEvalQuery(IQueryBuilder builder) : base(builder)
		{
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> And
		{
			get
			{
				ContainerObject.SearchLast<ConditionStatementQueryPart>().Conditions.Add(new ConditionStructurePart(ConditionStructurePart.LogicalOperator.And));
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
				ContainerObject.SearchLast<ConditionStatementQueryPart>().Conditions.Add(new ConditionStructurePart(ConditionStructurePart.LogicalOperator.Or));
				return new ConditionalQuery<TPoco>(this);
			}
		}
	}
}