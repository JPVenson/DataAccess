#region

using System;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Operators.Orders
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IOrderdColumnElementProducer{TPoco}" />
	public class OrderByColumn<TPoco> : ElementProducer<TPoco>, IOrderdColumnElementProducer<TPoco>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="OrderByColumn{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public OrderByColumn(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     returns an Descending orderd collection
		/// </summary>
		/// <returns></returns>
		public OrderByColumn<TPoco> Descending
		{
			get
			{
				ContainerObject.Search<OrderByColumnQueryPart>().Descending = true;
				return new OrderByColumn<TPoco>(this);
			}
		}

		/// <summary>
		///     Creates an Order By statement that is ether Ascending or Descending
		/// </summary>
		/// <param name="ascending">if set to <c>true</c> [ascending].</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> OrderBy(bool ascending)
		{
			if (ascending)
			{
				return this;
			}
			return Descending;
		}

		/// <summary>
		///     Appents another order statement
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> ThenBy(string columnName)
		{
			ContainerObject.Search<OrderByColumnQueryPart>().Columns.Add(columnName);
			return new OrderByColumn<TPoco>(this);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> ThenBy<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLamdba();
			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return ThenBy(propName.DbName);
		}
	}
}