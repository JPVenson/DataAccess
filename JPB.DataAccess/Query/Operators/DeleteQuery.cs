using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;
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
		public DeleteQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public DeleteQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public DeleteQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public DeleteQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public DeleteQuery(DbAccessLayer database) : base(database)
		{
		}


		/// <summary>
		///     Gets the current identifier.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

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
