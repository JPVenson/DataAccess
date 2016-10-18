using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class QueryElement : QueryBuilderX, IQueryElement
	{
		public QueryElement(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public QueryElement(IQueryContainer database) : base(database)
		{
		}

		public QueryElement(IQueryBuilder database) : base(database)
		{
		}

		public QueryElement(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public QueryElement(DbAccessLayer database) : base(database)
		{
		}
	}
}
