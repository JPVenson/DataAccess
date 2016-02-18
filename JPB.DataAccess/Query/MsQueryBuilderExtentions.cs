/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Provieds A set of extentions for Microsoft SQL Serve
	/// </summary>
	public static class MsQueryBuilderExtentions
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
					var newName = query.ContainerObject.GetParamaterAutoId().ToString().CheckParamter();
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
		public static IQueryBuilder<T> Add<T,E>(this IQueryBuilder<E> query, GenericQueryPart part) 
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
					var newName = query.ContainerObject.GetParamaterAutoId().ToString().CheckParamter();
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
		///		Declares a new Variable of the Given SQL Type by using its length 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IQueryBuilder<IRootQuery> SetVariable(this IQueryBuilder<IRootQuery> query, string name, object value)
		{
			var transpiledValue = MsSql.ParameterValue(new SqlParameter(name, value));
			var sqlName = name;
			if (!sqlName.StartsWith("@"))
				sqlName = "@" + sqlName;

			query.QueryText("SET {0} = {1}", sqlName, transpiledValue);
			return query;
		}

		/// <summary>
		///		Declares a new Variable of the Given SQL Type by using its length 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IQueryBuilder<IRootQuery> DeclareVariable(this IQueryBuilder<IRootQuery> query, string name, SqlDbType type, int length = int.MaxValue, object value = null)
		{
			var sqlName = name;
			if (!sqlName.StartsWith("@"))
				sqlName = "@" + sqlName;
			var typeName = type.ToString();
			if (new SqlParameter("xxx", type).Size > 0)
			{
				typeName = "(MAX)";
			}

			query.QueryText("DECLARE {0} {1};", sqlName, typeName);
			if (value != null)
				query.SetVariable(sqlName, value);
			return query;
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
		public static IQueryBuilder<IRootQuery> WithCteForType(this IQueryBuilder<IRootQuery> query, Type target, string cteName,
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
			query.Add(new CteQueryPart(cteBuilder.ToString()));
			return query;
		}

		/// <summary>
		/// Creates a FOR XML statement that uses the name of the given type to allow the .net XML Serilizer to read the output
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> ForXml(this IQueryBuilder<ISelectQuery> query, Type target)
		{
			return query.QueryText("FOR XML PATH('{0}'),ROOT('ArrayOf{0}'), TYPE", target.Name);
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		public static IQueryBuilder<IRootQuery> WithCte(this IQueryBuilder<IRootQuery> query, 
			string cteName, Action<IQueryBuilder<INestedRoot>> cteAction,
			bool subCte = false)
		{
			var lod = query.ContainerObject.Parts.LastOrDefault();
			var prefix = string.Empty;

			if (lod is CteQueryPart || subCte)
			{
				prefix = string.Format(", {0} AS", cteName);
			}
			else
			{
				prefix = string.Format("WITH {0} AS ", cteName);
			}

			query.Add(new GenericQueryPart(prefix));
			query.InBracket(cteAction);
			query.Add(new CteQueryPart(""));
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> LineBreak<T>(this IQueryBuilder<T> query) where T : IQueryElement
		{
			query.QueryText(Environment.NewLine);
			return query;
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
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> SubSelect<T>(this IQueryBuilder<IRootQuery> query,
			Action<IQueryBuilder<INestedRoot>> subSelect,
			Type type)
			where T: IQueryElement
		{
			query.QueryText("(");
			query.Select(type);
			var nestedQuery = query.ChangeType<INestedRoot>();
			subSelect(nestedQuery);
			query.QueryText(")");
			return query.ChangeType<T>();
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
		///     Creates a QueryCommand that uses the * Operator to select all date from the inner query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> SelectStarFrom<T>(this IQueryBuilder<T> query, Action<IQueryBuilder<INestedRoot>> from) 
			where T : IRootQuery
		{
			query.QueryText("SELECT * FROM");
			query
				.ChangeType<INestedRoot>()
				.InBracket(from);
			return query.ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a select * from without a table name, to the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> SelectStar(this IQueryBuilder<IRootQuery> query)
		{
			query.QueryText("SELECT * FROM");
			return query.ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a select * from without a table name, to the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> SelectStar(this IQueryBuilder<IRootQuery> query, string table)
		{
			query.QueryText("SELECT * FROM " + table);
			return query.ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a Select * from followed by the table name of the entity that is used in the <paramref name="type" />
		/// </summary>
		/// <param name="query"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> SelectStar(this IQueryBuilder<IRootQuery> query, Type type)
		{
			query.QueryText("SELECT * FROM {0}", type.GetClassInfo().TableName);
			return query.ChangeType<ISelectQuery>();
		}

		/// <summary>
		///     Adds a Between statement followed by anything added from the action
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Between(this IQueryBuilder<IConditionalQuery> query, Action<IQueryBuilder<IConditionalQuery>> from)
		{
			query.QueryText("BETWEEN");
			from(query);
			return query;
		}


		/// <summary>
		///     Adds a Between statement to the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Between(this IQueryBuilder<IConditionalQuery> query)
		{
			query.QueryText("BETWEEN");
			return query;
		}


		/// <summary>
		///     Adds a between statement followed by a query defined in <paramref name="valueA" /> folowed by an and statement and
		///     an secound query defined in the <paramref name="valueB" />
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Between(
			this IQueryBuilder<IConditionalQuery> query,
			Action<IQueryBuilder<INestedRoot>> valueA,
			Action<IQueryBuilder<INestedRoot>> valueB)
		{
			query.Between();
			query.InBracket(valueA);
			query.And();
			query.InBracket(valueB);
			return query;
		}

		/// <summary>
		///     Adds a static beween statement for the given 2 values
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Between(this IQueryBuilder<IConditionalQuery> query, Double valueA, Double valueB)
		{
			var paramaterAAutoId = query.ContainerObject.GetParamaterAutoId().ToString();
			var paramaterBAutoId = query.ContainerObject.GetParamaterAutoId().ToString();

			query.QueryText("BETWEEN @{0} AND @{1}", paramaterAAutoId, paramaterBAutoId);
			query.QueryQ("",
				new QueryParameter(paramaterAAutoId, valueA),
				new QueryParameter(paramaterBAutoId, valueB));
			return query;
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
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IRootQuery> SubSelect(this IQueryBuilder<IRootQuery> query, Type type)
		{
			query.QueryText("(");
			query.Select(type);
			query.QueryText(")");
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
		//public static IQueryContainer WithCteForQuery(this IQueryContainer query, Action<QueryBuilder> subSelect, string cteName)
		//{
		//    var IQueryContainer = new IQueryContainer(query.Database);
		//    subSelect(queryContainer);
		//    var compileFlat = IQueryContainer.CompileFlat();
		//    var cteBuilder = new StringBuilder();
		//    cteBuilder.Append("WITH ");
		//    cteBuilder.Append(cteName);
		//    cteBuilder.Append(" AS ( ");
		//    cteBuilder.Append(compileFlat.Item1);
		//    query.Parts.Append(new QueryPart(cteBuilder.ToString(), compileFlat.Item2));
		//    return query;
		//}

		public static IQueryBuilder<IRootQuery> Apply(this IQueryBuilder<IRootQuery> query, ApplyMode mode, Action<IQueryBuilder<INestedRoot>> innerText, string asId)
		{
			query.QueryText(mode.ApplyType);
			var quer = query.ChangeType<INestedRoot>();
			quer.InBracket(innerText);
			quer.ChangeType<IElementProducer>().As(asId);
			return query;
		}

		/// <summary>
		///     Append an AS part
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> As<T>(this IQueryBuilder<T> query, string alias)
			where T : IElementProducer
		{
			return query.QueryText("AS " + alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Contains(this IQueryBuilder<IConditionalQuery> query, string alias)
		{
			return query.QueryText("CONTAINS ({0})", alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IConditionalQuery> Contains(this IQueryBuilder<IConditionalQuery> query, object alias)
		{
			var paramaterAutoId = query.ContainerObject.GetParamaterAutoId();
			return query.QueryQ(string.Format("CONTAINS (@{0})", paramaterAutoId),
				new QueryParameter(paramaterAutoId.ToString(CultureInfo.InvariantCulture), alias));
		}

		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<ISelectQuery> RowNumberOrder(this IQueryBuilder<ISelectQuery> query, string over, bool Desc = false)
		{
			return query.QueryText("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, Desc ? "DESC" : "ASC");
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
		///     Adds a LEFT JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IElementProducer> Join(this IQueryBuilder<IElementProducer> query, Type source, Type target)
		{
			var sourcePK = source.GetFK(target);
			var targetPK = target.GetPK();
			var targetTable = target.GetClassInfo().TableName;
			var sourceTable = source.GetClassInfo().TableName;
			return query.QueryText("JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IElementProducer> Join(this IQueryBuilder<IElementProducer> query, JoinMode mode, Type source, Type target)
		{
			return Join(query, mode.JoinType, source, target);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IElementProducer> Join(this IQueryBuilder<IElementProducer> query, string mode, Type source, Type target)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (mode == null) throw new ArgumentNullException("mode");
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");

			var sourcePK = source.GetFK(target);
			var targetPK = target.GetPK();
			var targetTable = target.GetClassInfo().TableName;
			var sourceTable = source.GetClassInfo().TableName;
			return query.QueryText(mode + " JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<IElementProducer> Join<Source, Target>(this IQueryBuilder<IElementProducer> query, JoinMode mode)
		{
			return Join(query, mode, typeof(Source), typeof(Target));
		}

		/// <summary>
		///     Inserts a TOP statement
		/// </summary>
		/// <returns></returns>
		public static IQueryBuilder<T> Top<T>(this IQueryBuilder<T> query, uint top) 
			where T : IElementProducer
		{
			switch (query.ContainerObject.AccessLayer.Database.TargetDatabase)
			{
				case DbAccessType.MsSql:
					{
						var index = -1;
						var select = "SELECT";
						var part =
							query.ContainerObject.Parts.LastOrDefault(s => (index = s.Prefix.ToUpper().IndexOf(@select, StringComparison.Ordinal)) != -1);

						if (index == -1 || part == null)
							throw new NotSupportedException("Please create a Select Statement befor calling this");

						part.Prefix = part.Prefix.Insert(index + @select.Length, " TOP " + top);
					}
					break;
				case DbAccessType.MySql:
					return query.QueryText("LIMIT BY {0}", top);
				case DbAccessType.SqLite:
					return query.QueryText("LIMIT {0}", top);
				default:
					throw new NotSupportedException("For the Selected DB type is no Top implementations Available");
			}
			return query;
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

		public static IQueryBuilder<T> Count<T>(this IQueryBuilder<T> query, string what)
			where T : IElementProducer
		{
			return query.QueryText("COUNT(" + what + ")");
		}

		public static IQueryBuilder<T> OrderBy<T>(this IQueryBuilder<T> query, string over, bool desc = false)
			where T : IElementProducer
		{
			return query.QueryText("ORDER BY {0} {1}", over, desc ? "DESC" : "ASC");
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
			///     QueryCommand string
			/// </summary>
			public string JoinType { get; private set; }
		}

		public abstract class ApplyMode
		{
			internal ApplyMode(string applyType)
			{
				ApplyType = applyType;
			}

			/// <summary>
			///     QueryCommand string
			/// </summary>
			public string ApplyType { get; private set; }
		}
	}
}