#region

using System;
using System.Linq;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Provides A set of extentions for Microsoft SQL Serve
	/// </summary>
	public static class MsQueryBuilderExtentions
	{
		/// <summary>
		///     Creates an closed sub select
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="subSelect">The sub select.</param>
		/// <returns></returns>
		public static ElementProducer<T> SubSelect<T>(this RootQuery query,
			Func<ElementResultQuery<T>> subSelect)
		{
			var classInfo = query.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(T));
			var queryIdentifier = query.ContainerObject.CreateTableAlias(classInfo.TableName);
			var part = new SubSelectQueryPart(queryIdentifier, subSelect().ContainerObject.Parts, query.ContainerObject);
			return new ElementProducer<T>(query.Add(part));
		}

		/// <summary>
		///     Creates an TSQL Count(1) statement
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static ElementProducer<int> CountInt<TPoco>(this IElementProducer<TPoco> query)
		{
			return query.Count<TPoco, int>();
		}

		/// <summary>
		///     Creates an TSQL Count(1) statement
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <typeparam name="TOut">The type of the out.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static ElementProducer<TOut> Count<TPoco, TOut>(this IElementProducer<TPoco> query)
		{
			IQueryBuilder newQuery = new RootQuery(query.ContainerObject.AccessLayer);
			//in case there is a grouping in the query, we must use a SubQuery

			var ordering = query.ContainerObject.SearchLast<OrderByColumnQueryPart>();

			var cteName = query.ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.Cte);
			var item = new CteDefinitionQueryPart.CteInfo()
			{
				Name = cteName
			};
			item.CteContentParts.AddRange(query.ContainerObject.Parts.Except(new IQueryPart[] { ordering }).ToArray());

			var cteQueryPart = query.ContainerObject.SearchLast<CteDefinitionQueryPart>();
			newQuery = newQuery.Add(cteQueryPart ?? (cteQueryPart = new CteDefinitionQueryPart()))
				.Add(cteQueryPart.AddCte(item));

			var subQueryId = query.ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.SubQuery);
			var countQueryPart = newQuery
				.Add(new CountTargetQueryPart(cteName, subQueryId));
			if (ordering != null)
			{
				var orderByColumnQueryPart = new OrderByColumnQueryPart();
				orderByColumnQueryPart.Descending = ordering.Descending;
				orderByColumnQueryPart.Columns = orderByColumnQueryPart.Columns
					.Select(f => new ColumnInfo(f.ColumnName, f, subQueryId, query.ContainerObject)).ToList();
				countQueryPart.Add(orderByColumnQueryPart);
			}

			return new ElementProducer<TOut>(countQueryPart);
		}
	}
}