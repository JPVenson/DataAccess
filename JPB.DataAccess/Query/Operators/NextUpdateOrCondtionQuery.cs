using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Defines an End-Point of an Conditional Query where the query can ether be executed or other conditons can be attached
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class NextUpdateOrCondtionQuery<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public NextUpdateOrCondtionQuery(DbAccessLayer database) : base(database)
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
			get { return new SetValueForUpdateQuery<TPoco>(this.QueryText(", ")); }
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(null)); }
		}
	}
}