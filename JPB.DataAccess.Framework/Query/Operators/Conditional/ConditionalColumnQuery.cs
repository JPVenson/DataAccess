#region

using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	public class ConditionalColumnQuery<TPoco> : IConditionalColumnQuery<TPoco>
	{
		private readonly IQueryBuilder _conditionalQuery;
		private readonly ExpressionConditionPart _expression;

		internal ConditionalColumnQuery(IQueryBuilder conditionalQuery, ExpressionConditionPart expression)
		{
			_conditionalQuery = conditionalQuery;
			_expression = expression;
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Is
		{
			get { return new ConditionalOperatorQuery<TPoco>(_conditionalQuery, _expression, Operator.Is); }
		}
	}
}