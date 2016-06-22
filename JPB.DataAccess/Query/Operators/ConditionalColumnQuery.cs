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
		public ConditionalOperatorQuery<TPoco> Is()
		{
			return new ConditionalOperatorQuery<TPoco>(this, Operator.Is);
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		public ConditionalEvalQuery<TPoco> Is(object value)
		{
			if (value == null)
			{
				return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"));
			}

			return QueryOperatorValue("=", value);
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		public ConditionalEvalQuery<TPoco> Not(object value)
		{
			if (value == null)
			{
				return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NOT NULL"));
			}
			return QueryOperatorValue("<>", value);
		}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> IsQueryOperatorValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(value));
		}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{1} @m_val{0}", nextParameterId, operators), new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)));
		}

		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> IsQueryValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ("= " + value));
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		public ConditionalEvalQuery<TPoco> NotQueryValue(string value)
		{
			return new ConditionalEvalQuery<TPoco>(this
					.QueryQ("<> " + value));
		}
	}
}
