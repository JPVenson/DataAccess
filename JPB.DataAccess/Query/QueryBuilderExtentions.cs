using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query
{
	public static class QueryBuilderExtentions
	{
		/// <summary>
		///     Adds a QueryCommand part to the Local collection
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> Add<T>(this IQueryBuilder<T> query, GenericQueryPart part)
			where T : IQueryElement
		{
			if (query.ContainerObject.AllowParamterRenaming)
			{
				foreach (var queryParameter in part.QueryParameters)
				{
					var fod = query.ContainerObject.Parts.SelectMany(s => s.QueryParameters).FirstOrDefault(s => s.Name == queryParameter.Name);

					if (fod == null)
						continue;

					//parameter is existing ... renaming new Parameter to Auto gen and renaming all ref in the QueryCommand
					var name = fod.Name;
					var newName = query.ContainerObject.GetNextParameterId().ToString().CheckParamter();
					part.Prefix = part.Prefix.Replace(name, newName);
					queryParameter.Name = newName;
				}
			}
			query.ContainerObject.Parts.Add(part);
			return query;
		}

		/// <summary>
		///     Adds a QueryCommand part to the Local collection
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> Add<T, E>(this IQueryBuilder<E> query, GenericQueryPart part)
			where T : IQueryElement
			where E : IQueryElement
		{
			if (query.ContainerObject.AllowParamterRenaming)
			{
				foreach (var queryParameter in part.QueryParameters)
				{
					var fod = query.ContainerObject.Parts.SelectMany(s => s.QueryParameters).FirstOrDefault(s => s.Name == queryParameter.Name);

					if (fod == null)
						continue;

					//parameter is existing ... renaming new Parameter to Auto gen and renaming all ref in the QueryCommand
					var name = fod.Name;
					var newName = query.ContainerObject.GetNextParameterId().ToString().CheckParamter();
					part.Prefix = part.Prefix.Replace(name, newName);
					queryParameter.Name = newName;
				}
			}
			query.ContainerObject.Parts.Add(part);
			return query.ChangeType<T>();
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryQ<T>(this IQueryBuilder<T> builder, string query, params IQueryParameter[] parameters)
			where T : IQueryElement
		{
			return builder.Add(new GenericQueryPart(query, parameters));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryD<T>(this IQueryBuilder<T> builder, string query, dynamic paramerters) where T : IQueryElement
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return builder.Add(new GenericQueryPart(query, parameters));
			}

			return builder.Add(new GenericQueryPart(query));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryD<T>(this IQueryBuilder<T> builder, string query) where T : IQueryElement
		{
			return QueryD(builder, query, null);
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryCommand<T, E>(this IQueryBuilder<E> builder, IDbCommand command)
			where T : IQueryElement
			where E : IQueryElement
		{
			return builder.Add(GenericQueryPart.FromCommand(command)).ChangeType<T>();
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryCommand<T>(this IQueryBuilder<T> builder, IDbCommand command)
			where T : IQueryElement
		{
			return builder.Add<T>(GenericQueryPart.FromCommand(command));
		}

		/// <summary>
		///     Adds a QueryCommand part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> QueryText<T>(this IQueryBuilder<T> builder, string query, params object[] args)
			where T : IQueryElement
		{
			return builder.QueryQ(string.Format(query, args));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> Select<T>(this IQueryBuilder<IRootQuery> query)
		{
			return query.Select(typeof(T));
		}


		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> Select(this IQueryBuilder<IRootQuery> query, Type type)
		{
			IDbCommand cmd = query.ContainerObject.AccessLayer.CreateSelectQueryFactory(type.GetClassInfo(), query.ContainerObject.AccessLayer.Database);
			return query.QueryCommand<ISelectQuery, IRootQuery>(cmd);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> Select<E>(this IQueryBuilder<E> query, Type type)
			where E : IRootQuery
		{
			IDbCommand cmd = query.ContainerObject.AccessLayer.CreateSelectQueryFactory(type.GetClassInfo(), query.ContainerObject.AccessLayer.Database);
			return query.QueryCommand(cmd).ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> Select<E>(this IQueryBuilder<E> query)
			where E : IRootQuery
		{
			query.QueryText("SELECT");
			return query.ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IQueryBuilder<IUpdateQuery> Update<T, E>(this IQueryBuilder<E> query, T obj)
			where E : IQueryElement
		{
			return query.Update<E>(typeof(T), obj);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IUpdateQuery> Update<T>(this IQueryBuilder<T> query, Type type, object obj)
			where T : IQueryElement
		{
			return query.QueryCommand<IUpdateQuery, T>(query.ContainerObject.AccessLayer._CreateUpdate(type.GetClassInfo(), obj));
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> InBracket<T>(this IQueryBuilder<T> query,
			Action<IQueryBuilder<INestedRoot>> header)
			where T : IQueryElement
		{
			query.QueryText("(");
			header(query.ChangeType<INestedRoot>());
			query.QueryText(")");
			return query;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     Accepts only Where statements
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IDataPager AsPager<T>(this IQueryContainer query, int pageSize)
		{
			var targetQuery = query.Compile();
			var dbAccess = query.AccessLayer.Database.CreatePager<T>();
			dbAccess.AppendedComands.Add(targetQuery);
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     Accepts only Where statements
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IWrapperDataPager<T, TE> AsPagerViewModel<T, TE>(this IQueryContainer query, int pageSize)
		{
			var targetQuery = query.Compile();
			var dbAccess = query.AccessLayer.Database.CreatePager<T, TE>();
			dbAccess.BaseQuery = targetQuery;
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Adds a SQL WHERE Condition
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Where<T>(this IQueryBuilder<T> query, string condition, dynamic paramerters = null)
			where T : IElementProducer
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ(string.Format("WHERE {0}", condition), parameters.ToArray()).ChangeType<IConditionalQuery>();
			}
			return query.QueryText("WHERE {0}", condition).ChangeType<IConditionalQuery>();
		}

		/// <summary>
		///     Adds And Condition
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> And(this IQueryBuilder<IConditionalQuery> query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ(string.Format("AND {0}", condition), parameters.ToArray());
			}
			return query.QueryText("AND {0}", condition);
		}

		/// <summary>
		///     Adds And Condition
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> And(this IQueryBuilder<IConditionalQuery> query)
		{
			return query.QueryText("AND");
		}

		/// <summary>
		///     Adds Or Condition
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Or(this IQueryBuilder<IConditionalQuery> query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ("OR " + condition, parameters.ToArray());
			}
			return query.QueryText("OR {0}", condition);
		}

		/// <summary>
		///     Adds Parameter to the QueryCommand object without adding a Statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> WithParamerters<T>(this IQueryBuilder<T> query, dynamic paramerters)
			where T : IQueryElement
		{
			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
			query.QueryQ(string.Empty, parameters.ToArray());
			return query;
		}
	}
}