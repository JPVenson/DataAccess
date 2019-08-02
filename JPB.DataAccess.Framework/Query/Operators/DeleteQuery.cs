using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Defines a Delete query root
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class DeleteQuery<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public DeleteQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where
		{
			get { return new ConditionalQuery<TPoco>(Add(new ConditionStatementQueryPart())); }
		}
	}
}
