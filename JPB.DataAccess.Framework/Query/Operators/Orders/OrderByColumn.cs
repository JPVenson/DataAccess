#region

using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.Framework.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Framework.Contacts.Pager;
using JPB.DataAccess.Framework.MetaApi;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Framework.Query.Operators.Orders
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="ElementProducer{TPoco}" />
	/// <seealso cref="IOrderdColumnElementProducer{T}" />
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
				ContainerObject.SearchLast<OrderByColumnQueryPart>().Descending = true;
				return new OrderByColumn<TPoco>(this);
			}
		}

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns></returns>
		public IDataPager<TPoco> ForPagedResult(int page, int pageSize)
		{
			if (ContainerObject.SearchLast<OrderByColumnQueryPart>() == null)
			{
				throw new InvalidOperationException("To use the Pagination you have to define an Order.By()");
			}

			var pager = ContainerObject.AccessLayer.Database.CreatePager<TPoco>();
			pager.CommandQuery = this;
			pager.PageSize = pageSize;
			pager.CurrentPage = page;
			pager.LoadPage(ContainerObject.AccessLayer);
			return pager;
		}

		/// <summary>
		///		Returns a Query that is will skip N items and return M items
		/// </summary>
		/// <returns></returns>
		public OrderByColumn<TPoco> AsPagedQuery(int page, int pageSize)
		{
			return new OrderByColumn<TPoco>(Add(new PaginationPart()
			{
				Page = page,
				PageSize = pageSize
			}));
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
			var columnInfos = ContainerObject.SearchLast<ISelectableQueryPart>()
				.Columns.ToArray();
			var columnDefinitionPart = columnInfos.FirstOrDefault(e => e.IsEquivalentTo(columnName));
			if (columnDefinitionPart == null)
			{
				throw new InvalidOperationException($"You have tried to create an expression for the column '{columnName}' on table '{typeof(TPoco)}' that does not exist.");
			}

			ContainerObject.SearchLast<OrderByColumnQueryPart>().Columns.Add(columnDefinitionPart);
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