using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class UpdateQuery<TPoco> : QueryBuilderX, IUpdateQuery<TPoco>
	{
		public UpdateQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public UpdateQuery(IQueryContainer database) : base(database)
		{
		}

		public UpdateQuery(IQueryBuilder database) : base(database)
		{
		}

		public UpdateQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public UpdateQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///		does not emit any conditional statement
		///		should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("WHERE"));
		}
	}
}