#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	/// </summary>
	public static class QueryBuilderExtentions
	{
		/// <summary>
		///     Adds a QueryCommand part to the Local collection
		/// </summary>
		/// <returns></returns>
		public static TQuery Add<TQuery>(this TQuery query, GenericQueryPart part)
			where TQuery : IQueryBuilder
		{
			if (query.ContainerObject.AllowParamterRenaming)
				foreach (var queryParameter in part.QueryParameters)
				{
					var fod =
						query.ContainerObject.Parts.SelectMany(s => s.QueryParameters).FirstOrDefault(s => s.Name == queryParameter.Name);

					if (fod == null)
						continue;

					//parameter is existing ... renaming new Parameter to Auto gen and renaming all ref in the QueryCommand
					var name = fod.Name;
					var newName = query.ContainerObject.GetNextParameterId().ToString().CheckParamter();
					part.Prefix = part.Prefix.Replace(name, newName);
					queryParameter.Name = newName;
				}
			query.ContainerObject.Parts.Add(part);
			return query;
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static E QueryQ<E>(this E builder, string query, params IQueryParameter[] parameters)
			where E : IQueryBuilder
		{
			return builder.Add(new GenericQueryPart(query, parameters, builder));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static TQuery QueryD<TQuery>(this TQuery builder, string query, dynamic paramerters)
			where TQuery : IQueryBuilder
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return builder.Add(new GenericQueryPart(query, parameters, builder));
			}

			return builder.Add(new GenericQueryPart(query));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static TQuery QueryD<TQuery>(this TQuery builder, string query)
			where TQuery : IQueryBuilder
		{
			return QueryD(builder, query, null);
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static TQuery QueryCommand<TQuery>(this TQuery builder, IDbCommand command)
			where TQuery : IQueryBuilder
		{
			return builder.Add(GenericQueryPart.FromCommand(command, null));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static TQuery QueryText<TQuery>(this TQuery builder, string query, params object[] args)
			where TQuery : IQueryBuilder
		{
			return builder.QueryQ(string.Format(query, args));
		}

		/// <summary>
		///     Inserts and Appends brackets after the delegate
		/// </summary>
		/// <returns></returns>
		public static TQuery InBracket<TQuery>(this TQuery query,
			Action<TQuery> header)
			where TQuery : IQueryBuilder
		{
			query.QueryText("(");
			header(query);
			query.QueryText(")");
			return query;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IDataPager AsPager<TPoco>(this IQueryContainer query, int pageSize)
		{
			var targetQuery = query.Compile();
			var dbAccess = query.AccessLayer.Database.CreatePager<TPoco>();
			dbAccess.AppendedComands.Add(targetQuery);
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IWrapperDataPager<TPoco, TVm> AsPagerViewModel<TPoco, TVm>(this IQueryContainer query, int pageSize)
		{
			var targetQuery = query.Compile();
			var dbAccess = query.AccessLayer.Database.CreatePager<TPoco, TVm>();
			dbAccess.BaseQuery = targetQuery;
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Adds Parameter to the QueryCommand object without adding a Statement
		/// </summary>
		/// <returns></returns>
		public static TQuery WithParamerters<TQuery>(this TQuery query, dynamic paramerters)
			where TQuery : IQueryBuilder
		{
			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
			query.QueryQ(string.Empty, parameters.ToArray());
			return query;
		}
	}
}