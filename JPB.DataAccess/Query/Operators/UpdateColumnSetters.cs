using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Defines mehtods for an UPDATE query
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class UpdateColumnSetters<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public UpdateColumnSetters(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public UpdateColumnSetters(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public UpdateColumnSetters(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public UpdateColumnSetters(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public UpdateColumnSetters(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///		Syntax for setting the Entitys
		/// </summary>
		public SetValueForUpdateQuery<TPoco> Set
		{
			get
			{
				return new SetValueForUpdateQuery<TPoco>(this.QueryD("SET"));
			}
		}
	}
}