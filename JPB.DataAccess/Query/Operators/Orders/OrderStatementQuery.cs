using System;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Orders
{
	public class OrderStatementQuery<TPoco> : QueryBuilderX, IOrderdElementProducer<TPoco>
	{
		public OrderStatementQuery(IQueryBuilder database) : base(database)
		{
		}

		public OrderByColumn<TPoco> By(string columnName)
		{
			return new OrderByColumn<TPoco>(this.QueryText(columnName));
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By<TA>(
								Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return By(propName.DbName);
		}
	}
}
