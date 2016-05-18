using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class ConditionalQuery<TPoco> : QueryBuilderX, IConditionalQuery<TPoco>
	{
		public ConditionalQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public ConditionalQuery(IQueryContainer database) : base(database)
		{
		}

		public ConditionalQuery(IQueryBuilder database) : base(database)
		{
		}

		public ConditionalQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public ConditionalQuery(DbAccessLayer database) : base(database)
		{
		}
		
		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column(string columnName)
		{
			return new ConditionalColumnQuery<TPoco>(this.QueryText(columnName));
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column<TA>(
								Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetFullPropertyInfoFromLabda();
			return new ConditionalColumnQuery<TPoco>(this.QueryText(member.DbName));
		}

		
	}
}
