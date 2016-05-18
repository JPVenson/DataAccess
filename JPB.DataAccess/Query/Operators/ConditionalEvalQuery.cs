using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class ConditionalEvalQuery<TPoco> : ElementProducer<TPoco>, IConditionalEvalQuery<TPoco>
	{
		public ConditionalEvalQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public ConditionalEvalQuery(IQueryContainer database) : base(database)
		{
		}

		public ConditionalEvalQuery(IQueryBuilder database) : base(database)
		{
		}

		public ConditionalEvalQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public ConditionalEvalQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalQuery<TPoco> And()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("AND"));
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Or()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("OR"));
		}
	}
}
