using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class UpdateQuery<TPoco> : ElementProducer<TPoco>, IUpdateQuery<TPoco>
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
	}
}