using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class ElementProducer<TPoco> : QueryBuilderX, IElementProducer<TPoco>
	{
		public ElementProducer(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public ElementProducer(IQueryContainer database) : base(database)
		{
		}

		public ElementProducer(IQueryBuilder database) : base(database)
		{
		}

		public ElementProducer(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public ElementProducer(DbAccessLayer database) : base(database)
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
