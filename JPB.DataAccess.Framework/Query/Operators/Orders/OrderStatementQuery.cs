#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Operators.Orders
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="QueryBuilderX" />
	/// <seealso cref="IOrderdElementProducer{T}" />
	public class OrderStatementQuery<TPoco> : IOrderdElementProducer<TPoco>
	{
		private readonly IQueryBuilder _queryBuilder;

		internal OrderStatementQuery(IQueryBuilder queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		private OrderByColumn<TPoco> CreateByPath
			(IReadOnlyCollection<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>> columnPath)
		{
			var columnDefinitionPart =
				ConditionalQuery<TPoco>.TraversePropertyPathToColumn(columnPath, _queryBuilder.ContainerObject);

			_queryBuilder.ContainerObject.SearchLast<OrderByColumnQueryPart>().Columns.Add(columnDefinitionPart);
			return new OrderByColumn<TPoco>(_queryBuilder);
		}

		/// <summary>
		///     Uses
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By(string columnName)
		{
			var cache = _queryBuilder.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco));
			return CreateByPath(new[]
			{
				new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(cache, cache
					.Propertys[columnName]),
			});
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By<TA>(
			Expression<Func<TPoco, TA>> columnName)
		{
			return CreateByPath(PropertyPath<TPoco>
				.Get(columnName)
				.Select(e =>
				{
					var dbClassInfoCache = _queryBuilder.ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache,
						dbClassInfoCache.Propertys[e.Name]);
				})
				.ToArray());
		}
	}
}