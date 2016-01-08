using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Config;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.QueryBuilder
{
	/// <summary>
	///     Provieds A set of extentions for Microsoft SQL Serve
	/// </summary>
	public static partial class MsQueryBuilderExtentions
	{
		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder QueryQ(this QueryBuilder builder, string query, params IQueryParameter[] parameters)
		{
			return builder.Add(new GenericQueryPart(query, parameters));
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder QueryD(this QueryBuilder builder, string query, dynamic paramerters)
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
		public static QueryBuilder QueryD(this QueryBuilder builder, string query)
		{
			return QueryD(builder, query, null);
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Query(this QueryBuilder builder, IDbCommand command)
		{
			return builder.Add(GenericQueryPart.FromCommand(command));
		}

		/// <summary>
		///     Adds a Query part to <paramref name="builder" />
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Query(this QueryBuilder builder, string query, params object[] args)
		{
			return builder.QueryQ(string.Format(query, args));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static QueryBuilder Select<T>(this QueryBuilder query)
		{
			return query.Select(typeof (T));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Select(this QueryBuilder query, Type type)
		{
			return query.Query(DbAccessLayer.CreateSelectQueryFactory(type.GetClassInfo(), query.Database));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static QueryBuilder Update<T>(this QueryBuilder query, T obj)
		{
			return query.Update(typeof (T), obj);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Update(this QueryBuilder query, Type type, object obj)
		{
			return query.Query(DbAccessLayer.createUpdate(obj, query.Database));
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder WithCteForType(this QueryBuilder query, Type target, string cteName,
			bool useStarOperator = false)
		{
			var cteBuilder = new StringBuilder();
			cteBuilder.Append("WITH ");
			cteBuilder.Append(cteName);
			cteBuilder.Append(" (");
			cteBuilder.Append(!useStarOperator ? target.CreatePropertyCsv() : "*");
			if (query.AutoLinebreak)
				cteBuilder.AppendLine();
			cteBuilder.Append(") AS (");
			cteBuilder.Append(DbAccessLayer.CreateSelect(target));
			cteBuilder.Append(")");
			query.Add(new CteQueryPart(cteBuilder.ToString()));
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		public static QueryBuilder WithCte(this QueryBuilder query, string cteName, Action<QueryBuilder> cteAction, bool subCte = false)
		{
			GenericQueryPart lod = query.Parts.LastOrDefault();
			string prefix = string.Empty;

			if (lod is CteQueryPart || subCte)
			{
				prefix = string.Format(", {0} AS", cteName);
			}
			else
			{
				prefix = string.Format("WITH {0} AS ", cteName);
			}

			query.AutoLinebreakAction();
			query.Add(new GenericQueryPart(prefix));
			query.InBracket(cteAction);
			query.Add(new GenericQueryPart(""));
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder LineBreak(this QueryBuilder query)
		{
			query.Query(Environment.NewLine);
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder InBracket(this QueryBuilder query, Action<QueryBuilder> header)
		{
			query.AutoLinebreakAction();
			query.Query("(");
			header(query);
			query.Query(")");
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder SubSelect(this QueryBuilder query, Action<QueryBuilder> subSelect)
		{
			query.AutoLinebreakAction();
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
		public static IDataPager AsPager<T>(this QueryBuilder query, int pageSize)
		{
			IDbCommand targetQuery = query.Compile();
			IDataPager<T> dbAccess = query.Database.CreatePager<T>();
			dbAccess.AppendedComands.Add(targetQuery);
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		/// Creates a Query that uses the * Operator to select all date from the inner query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static QueryBuilder SelectStarFrom(this QueryBuilder query, Action<QueryBuilder> from)
		{
			query.Query("SELECT * FROM");
			query.InBracket(from);
			query.AutoLinebreakAction();
			//from(query);
			return query;
		}

		/// <summary>
		/// Adds a select * from without a table name, to the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static QueryBuilder SelectStar(this QueryBuilder query)
		{
			query.Query("SELECT * FROM");
			return query;
		}
		
		/// <summary>
		/// Adds a Select * from followed by the table name of the entity that is used in the <paramref name="type"/>
		/// </summary>
		/// <param name="query"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static QueryBuilder SelectStar(this QueryBuilder query, Type type)
		{
			query.Query("SELECT * FROM {0}", type.GetTableName());
			query.AutoLinebreakAction();
			return query;
		}

		/// <summary>
		/// Adds a Between statement followed by anything added from the action
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static QueryBuilder Between(this QueryBuilder query, Action<QueryBuilder> from)
		{
			query.Query("BETWEEN");
			from(query);
			return query;
		}

	

		/// <summary>
		/// Adds a Between statement to the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static QueryBuilder Between(this QueryBuilder query)
		{
			query.Query("BETWEEN");
			return query;
		}



		/// <summary>
		/// Adds a between statement followed by a query defined in <paramref name="valueA"/> folowed by an and statement and an secound query defined in the <paramref name="valueB"/>
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static QueryBuilder Between(this QueryBuilder query, Action<QueryBuilder> valueA, Action<QueryBuilder> valueB)
		{
			query.Between();
			query.InBracket(valueA);
			query.And();
			query.InBracket(valueB);
			query.AutoLinebreakAction();
			return query;
		}

		/// <summary>
		/// Adds a static beween statement for the given 2 values
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static QueryBuilder Between(this QueryBuilder query, Double valueA, Double valueB)
		{
			string paramaterAAutoId = query.GetParamaterAutoId().ToString();
			string paramaterBAutoId = query.GetParamaterAutoId().ToString();

			query.Query("BETWEEN @{0} AND @{1}", paramaterAAutoId, paramaterBAutoId);
			query.QueryQ("",
				new QueryParameter(paramaterAAutoId, valueA),
				new QueryParameter(paramaterBAutoId, valueB));
			query.AutoLinebreakAction();
			return query;
		}

		/// <summary>
		///     Wraps the Existing command into a DataPager for the underlying Database
		///     Accepts only Where statements
		///     <example>
		///     </example>
		/// </summary>
		/// <returns></returns>
		public static IWrapperDataPager<T, TE> AsPagerViewModel<T, TE>(this QueryBuilder query, int pageSize)
		{
			IDbCommand targetQuery = query.Compile();
			IWrapperDataPager<T, TE> dbAccess = query.Database.CreatePager<T, TE>();
			dbAccess.BaseQuery = targetQuery;
			dbAccess.PageSize = pageSize;
			return dbAccess;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder SubSelect(this QueryBuilder query, Type type)
		{
			query.Query("(");
			query.Select(type);
			query.Query(")");
			query.AutoLinebreakAction();
			return query;
		}

		///// <summary>
		///// Creates a Common Table Expression that selects a Specific type
		///// </summary>
		//
		//
		//
		//
		///// <returns></returns>
		//public static QueryBuilder WithCteForQuery(this QueryBuilder query, Action<QueryBuilder> subSelect, string cteName)
		//{
		//    var queryBuilder = new QueryBuilder(query.Database);
		//    subSelect(queryBuilder);
		//    var compileFlat = queryBuilder.CompileFlat();
		//    var cteBuilder = new StringBuilder();
		//    cteBuilder.Append("WITH ");
		//    cteBuilder.Append(cteName);
		//    cteBuilder.Append(" AS ( ");
		//    cteBuilder.Append(compileFlat.Item1);
		//    query.Parts.Append(new QueryPart(cteBuilder.ToString(), compileFlat.Item2));
		//    return query;
		//}

		/// <summary>
		///     Append an AS part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder As(this QueryBuilder query, string alias)
		{
			return query.Query("AS " + alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Contains(this QueryBuilder query, string alias)
		{
			return query.Query("CONTAINS ({0})", alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Contains(this QueryBuilder query, object alias)
		{
			int paramaterAutoId = query.GetParamaterAutoId();
			return query.QueryQ(string.Format("CONTAINS (@{0})", paramaterAutoId),
				new QueryParameter(paramaterAutoId.ToString(CultureInfo.InvariantCulture), alias));
		}

		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder RowNumberOrder(this QueryBuilder query, string over, bool Desc = false)
		{
			return query.Query("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, Desc ? "DESC" : "ASC");
		}

		/// <summary>
		///     Adds a SQL WHERE Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Where(this QueryBuilder query, string condition, dynamic paramerters = null)
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
		public static QueryBuilder And(this QueryBuilder query, string condition, dynamic paramerters = null)
		{
			if (paramerters != null)
			{
				IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
				return query.QueryQ(string.Format("AND {0}", condition), parameters.ToArray());
			}
			return query.Query("AND {0}", condition);
		}

		/// <summary>
		///     Adds And Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder And(this QueryBuilder query)
		{
			return query.Query("AND");
		}

		/// <summary>
		///     Adds Or Condition
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Or(this QueryBuilder query, string condition, dynamic paramerters = null)
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
		public static QueryBuilder Join(this QueryBuilder query, Type source, Type target)
		{
			string sourcePK = source.GetFK(target);
			string targetPK = target.GetPK();
			string targetTable = target.GetTableName();
			string sourceTable = source.GetTableName();
			return query.Query("JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Join(this QueryBuilder query, JoinMode mode, Type source, Type target)
		{
			return Join(query, mode.JoinType, source, target);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Join(this QueryBuilder query, string mode, Type source, Type target)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (mode == null) throw new ArgumentNullException("mode");
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");

			string sourcePK = source.GetFK(target);
			string targetPK = target.GetPK();
			string targetTable = target.GetTableName();
			string sourceTable = source.GetTableName();
			return query.Query(mode + " JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Join<Source, Target>(this QueryBuilder query, JoinMode mode)
		{
			return Join(query, mode, typeof (Source), typeof (Target));
		}

		/// <summary>
		///     Inserts a TOP statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder Top(this QueryBuilder query, uint top)
		{
			switch (query.Database.TargetDatabase)
			{
				case DbAccessType.MsSql:
				{
					int index = -1;
					string select = "SELECT";
					GenericQueryPart part =
						query.Parts.LastOrDefault(s => (index = s.Prefix.ToUpper().IndexOf(@select, StringComparison.Ordinal)) != -1);

					if (index == -1 || part == null)
						throw new NotSupportedException("Please create a Select Statement befor calling this");

					part.Prefix = part.Prefix.Insert(index + @select.Length, " TOP " + top);
				}
					break;
				case DbAccessType.MySql:
					return query.Query("LIMIT BY {0}", top);
				default:
					throw new NotSupportedException("For the Selected DB type is no Top implementations Available");
			}
			return query;
		}

		/// <summary>
		///     Adds Parameter to the Query object without adding a Statement
		/// </summary>
		/// <returns></returns>
		public static QueryBuilder WithParamerters(this QueryBuilder query, dynamic paramerters)
		{
			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
			query.QueryQ(string.Empty, parameters.ToArray());
			return query;
		}

		/// <summary>
		///     This will execute the query Async
		/// </summary>
		/// <returns></returns>
		public static async Task<IEnumerable<T>> ConfigurateAwaiter<T>(this QueryBuilder<T> query)
		{
			var task = new Task<IEnumerable<T>>(query.ToArray);

			task.Start();
			await task;

			return task.Result;
		}

		/// <summary>
		/// </summary>
		public abstract class JoinMode
		{
			internal JoinMode(string joinType)
			{
				JoinType = joinType;
			}

			/// <summary>
			/// Query string
			/// </summary>
			public string JoinType { get; private set; }
		}
	}
}