using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class SelectQuery<TPoco> : ElementProducer<TPoco>, ISelectQuery<TPoco>
	{
		public SelectQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public SelectQuery(IQueryContainer database) : base(database)
		{
		}

		public SelectQuery(IQueryBuilder database) : base(database)
		{
		}

		public SelectQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public SelectQuery(DbAccessLayer database) : base(database)
		{
			
		}
	}
}
