#region

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Orders;
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
			var queryIdentifier = query.ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.SubQuery);
			var part = new SubSelectQueryPart(queryIdentifier, subSelect().ContainerObject.Parts, typeof(T));
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
			var cteName = query.ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.Cte);
			var item = new CteDefinitionQueryPart.CteInfo()
			{
				Name = cteName
			};
			item.CteContentParts.AddRange(query.ContainerObject.Parts.ToArray());

			IQueryBuilder newQuery = new RootQuery(query.ContainerObject.AccessLayer);
			var cteQueryPart = query.ContainerObject.Search<CteDefinitionQueryPart>();
			newQuery = newQuery.Add(cteQueryPart ?? (cteQueryPart = new CteDefinitionQueryPart()))
				.Add(cteQueryPart.AddCte(item));

			return new ElementProducer<TOut>(newQuery
				.Add(new CountTargetQueryPart(cteName, query.ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.SubQuery))));
		}
	}
}