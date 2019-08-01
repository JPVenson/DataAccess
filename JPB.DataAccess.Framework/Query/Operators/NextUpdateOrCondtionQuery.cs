using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.Operators.Conditional;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Framework.Query.Operators
{
	/// <summary>
	///		Defines an End-Point of an Conditional Query where the query can ether be executed or other conditons can be attached
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class NextUpdateOrCondtionQuery<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(IQueryBuilder database) : base(database)
		{
		}
		
		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public SetValueForUpdateQuery<TPoco> And
		{
			get { return new SetValueForUpdateQuery<TPoco>(this); }
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where
		{
			get
			{
				return new ConditionalQuery<TPoco>(Add(new ConditionStatementQueryPart()));
			}
		}
	}
}