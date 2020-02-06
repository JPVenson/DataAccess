#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Operators.Orders
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

		private OrderByColumn<TPoco> CreateByPath(
			IReadOnlyCollection<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>> columnPath)
		{
			var columnDefinitionPart =
				ConditionalQuery<TPoco>.TraversePropertyPathToColumn(columnPath, ContainerObject);

			ContainerObject.SearchLast<OrderByColumnQueryPart>().Columns.Add(columnDefinitionPart);
			return new OrderByColumn<TPoco>(this);
		}

		/// <summary>
		///     Appents another order statement
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> ThenBy(string columnName)
		{
			return CreateByPath(PropertyPath<TPoco>
				.Get(ContainerObject.AccessLayer.Config, columnName)
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					if (dbClassInfoCache.Propertys.ContainsKey(e.Name))
					{
						return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache, dbClassInfoCache.Propertys[e.Name]);
					}
					else
					{
						throw new InvalidOperationException($"The expected property '{e.Name}' was not found.")
						{
							Data =
							{
								{"Class", dbClassInfoCache }
							}
						};
					}
				}).ToArray());
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> ThenBy<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			return CreateByPath(PropertyPath<TPoco>
				.Get(columnName)
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					if (dbClassInfoCache.Propertys.ContainsKey(e.Name))
					{
						return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache, dbClassInfoCache.Propertys[e.Name]);
					}
					else
					{
						throw new InvalidOperationException($"The expected property '{e.Name}' was not found.")
						{
							Data =
							{
								{"Class", dbClassInfoCache }
							}
						};
					}
				})
				.ToArray());
		}
	}
}