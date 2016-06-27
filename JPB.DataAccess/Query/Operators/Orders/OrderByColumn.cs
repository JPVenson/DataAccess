using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Orders
{
	public class OrderByColumn<TPoco> : ElementProducer<TPoco>, IOrderdColumnElementProducer<TPoco>
	{
		public OrderByColumn(IQueryBuilder database) : base(database)
		{
		}

		public OrderByColumn<TPoco> Order(bool ascending)
		{
			if (ascending)
				return this;
			return this.Descending();
		}

		public OrderByColumn<TPoco> Descending()
		{
			return new OrderByColumn<TPoco>(this.QueryText("DESC"));
		}

		public OrderByColumn<TPoco> ThenBy(string columnName)
		{
			return new OrderByColumn<TPoco>(this.QueryText("," + columnName));
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public OrderByColumn<TPoco> ThenBy<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return ThenBy(propName.DbName);
		}
	}
}
