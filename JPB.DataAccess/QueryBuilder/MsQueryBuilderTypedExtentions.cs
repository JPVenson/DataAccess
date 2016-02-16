/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;

namespace JPB.DataAccess.QueryBuilder
{
	public static partial class MsQueryBuilderExtentions
	{
		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> QueryQ<T>(this QueryBuilder<T> builder, string query,
			params IQueryParameter[] parameters)
		{
			return builder.Add(new GenericQueryPart(query, parameters));
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> QueryD<T>(this QueryBuilder<T> builder, string query, dynamic paramerters)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return builder.Add(new GenericQueryPart(query, parameters));
			}

			return builder.Add(new GenericQueryPart(query));
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> QueryD<T>(this QueryBuilder<T> builder, string query)
		{
			return QueryD(builder, query, null);
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Query<T>(this QueryBuilder<T> builder, IDbCommand command)
		{
			return builder.Add(GenericQueryPart.FromCommand(command));
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Query<T>(this QueryBuilder<T> builder, string query, params object[] args)
		{
			return builder.QueryQ(string.Format(query, args));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static QueryBuilder<T> Select<T>(this QueryBuilder<T> query)
		{
			return query.Select(typeof (T));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Select<T>(this QueryBuilder<T> query, Type type)
		{
			return query.Query(query.AccessLayer.CreateSelectQueryFactory(type.GetClassInfo(), query.AccessLayer.Database));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static QueryBuilder<T> Update<T>(this QueryBuilder<T> query, T obj)
		{
			return query.Update(typeof (T), obj);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Update<T>(this QueryBuilder<T> query, Type type, object obj)
		{
			return query.Query(query.AccessLayer._CreateUpdate(type.GetClassInfo(), obj));
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> WithCteForType<T>(this QueryBuilder<T> query, Type target, string cteName,
			bool useStarOperator = false)
		{
			var cteBuilder = new StringBuilder();
			cteBuilder.Append("WITH ");
			cteBuilder.Append(cteName);
			cteBuilder.Append(" (");
			cteBuilder.Append(!useStarOperator ? target.GetClassInfo().CreatePropertyCsv() : "*");
			cteBuilder.Append(") AS (");
			cteBuilder.Append(DbAccessLayer.CreateSelect(target));
			cteBuilder.Append(")");
			query.Query(cteBuilder.ToString());
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> InBracket<T>(this QueryBuilder<T> query, Action<QueryBuilder<T>> header)
		{
			query.Query("(");
			header(query);
			query.Query(")");
			return query;
		}

		/// <summary>
		///     Creates a Subselect
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use the InBracket Mehtod", true)]
		public static QueryBuilder<T> SubSelect<T>(this QueryBuilder<T> query, Action<QueryBuilder<T>> subSelect)
		{
			query.Query("(");
			subSelect(query);
			query.Query(")");
			return query;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     Accepts only Where statements
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IDataPager AsPager<T>(this QueryBuilder<T> query, int pageSize)
		{
			var targetQuery = query.Compile();
			var dbAccess = query.AccessLayer.Database.CreatePager<T>();
			dbAccess.AppendedComands.Add(targetQuery);
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> SubSelect<T>(this QueryBuilder<T> query, Type type)
		{
			query.Query("(");
			query.Select(type);
			query.Query(")");
			return query;
		}

		/// <summary>
		///     Append an AS part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> As<T>(this QueryBuilder<T> query, string alias)
		{
			return query.Query("AS " + alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Contains<T>(this QueryBuilder<T> query, string alias)
		{
			return query.Query("CONTAINS ({0})", alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Contains<T>(this QueryBuilder<T> query, object alias)
		{
			var paramaterAutoId = query.GetParamaterAutoId();
			return query.QueryQ(string.Format("CONTAINS (@{0})", paramaterAutoId),
				new QueryParameter(paramaterAutoId.ToString(CultureInfo.InvariantCulture), alias));
		}

		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> RowNumberOrder<T>(this QueryBuilder<T> query, string over, bool Desc = false)
		{
			return query.Query("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, Desc ? "DESC" : "ASC");
		}

		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> RowNumberOrder<T, TProp>(this QueryBuilder<T> query, Expression<Func<T, TProp>> exp,
			bool Desc = false)
		{
			return query.Query("ROW_NUMBER() OVER (ORDER BY {0} {1})", MetaInfoStoreExtentions.GetPropertyInfoFromLabda(exp),
				Desc ? "DESC" : "ASC");
		}

		/// <summary>
		///     Adds a SQL WHERE Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Where<T>(this QueryBuilder<T> query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ(string.Format("WHERE {0}", condition), parameters.ToArray());
			}
			return query.Query("WHERE {0}", condition);
		}

		/// <summary>
		///     Adds And Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> And<T>(this QueryBuilder<T> query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ(string.Format("AND {0}", condition), parameters.ToArray());
			}
			return query.Query("AND {0}", condition);
		}

		/// <summary>
		///     Adds Or Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Or<T>(this QueryBuilder<T> query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ("OR " + condition, parameters.ToArray());
			}
			return query.Query("OR {0}", condition);
		}

		/// <summary>
		///     Adds a LEFT JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Join<T>(this QueryBuilder<T> query, Type source, Type target)
		{
			var sourcePK = source.GetFK(target);
			var targetPK = target.GetPK();
			var targetTable = target.GetClassInfo().TableName;
			var sourceTable = source.GetClassInfo().TableName;
			return query.Query("JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Join<T>(this QueryBuilder<T> query, JoinMode mode, Type source, Type target)
		{
			return Join(query, mode.JoinType, source, target);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Join<T>(this QueryBuilder<T> query, string mode, Type source, Type target)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (mode == null) throw new ArgumentNullException("mode");
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");

			var sourcePK = source.GetFK(target);
			var targetPK = target.GetPK();
			var targetTable = target.GetClassInfo().TableName;
			var sourceTable = source.GetClassInfo().TableName;
			return query.Query(mode + " JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Join<Source, Target, T>(this QueryBuilder<T> query, JoinMode mode)
		{
			return Join(query, mode, typeof (Source), typeof (Target));
		}

		/// <summary>
		///     Inserts a TOP statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> Top<T>(this QueryBuilder<T> query, uint top)
		{
			switch (query.AccessLayer.Database.TargetDatabase)
			{
				case DbAccessType.MsSql:
				{
					var index = -1;
					var select = "SELECT";
					var part =
						query.Parts.FirstOrDefault(s => (index = s.Prefix.ToUpper().IndexOf(@select, StringComparison.Ordinal)) != -1);

					if (index == -1 || part == null)
						return query;

					part.Prefix = part.Prefix.Insert(index + @select.Length, " TOP " + top);
				}
					break;
				case DbAccessType.MySql:
					return query.Query("LIMIT BY {0}", top);
				default:
					throw new NotSupportedException("For the Selected DB type is no Top implementations Available. Use QueryD insted");
			}
			return query;
		}

		/// <summary>
		///     Adds Parameter to the Query object without adding a Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder<T> WithParamerters<T>(this QueryBuilder<T> query, dynamic paramerters)
		{
			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
			query.QueryQ(string.Empty, parameters.ToArray());
			return query;
		}
	}
}