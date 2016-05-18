using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class ConditionalColumnQuery<TPoco> : QueryBuilderX, IConditionalColumnQuery<TPoco>
	{
		public ConditionalColumnQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public ConditionalColumnQuery(IQueryContainer database) : base(database)
		{
		}

		public ConditionalColumnQuery(IQueryBuilder database) : base(database)
		{
		}

		public ConditionalColumnQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public ConditionalColumnQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> Is(object value)
		{
			if (value == null)
			{
				return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"));
			}
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ("= @m_val", new QueryParameter(string.Format("@m_val"), value)));
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> Not(object value)
		{
			if (value == null)
			{
				return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NOT NULL"));
			}
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ("<> @m_val", new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)));
		}
	}
}
