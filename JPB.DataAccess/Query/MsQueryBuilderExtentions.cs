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
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Provieds A set of extentions for Microsoft SQL Serve
	/// </summary>
	public static class MsQueryBuilderExtentions
	{
		

		/// <summary>
		///		Sets an Variable to the given value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static RootQuery SetVariable(this RootQuery query, string name, object value)
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
		public static RootQuery DeclareVariable(this RootQuery query, string name, SqlDbType type, int length = int.MaxValue, object value = null)
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
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <returns></returns>
		public static RootQuery WithCteForType(this RootQuery query, Type target, string cteName,
			bool useStarOperator = false)
		{
			var cteBuilder = new StringBuilder();
			cteBuilder.Append("WITH ");
			cteBuilder.Append(cteName);
			cteBuilder.Append(" (");
			cteBuilder.Append(!useStarOperator ? query.ContainerObject.AccessLayer.GetClassInfo(target).CreatePropertyCsv() : "*");
			cteBuilder.Append(") AS (");
			cteBuilder.Append(query.ContainerObject.AccessLayer.CreateSelect(target));
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
		public static ElementProducer<string> ForXml<TPoco>(this ElementProducer<TPoco> query, Type target)
		{
			return new ElementProducer<string>(query.QueryText("FOR XML PATH('{0}'),ROOT('ArrayOf{0}'), TYPE", target.Name));
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		public static RootQuery WithCte(this RootQuery query, 
			string cteName, 
			Action<RootQuery> cteAction,
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
		///    Creates an closed sub select
		/// </summary>
		/// <returns></returns>
		public static SelectQuery<T> SubSelect<T>(this RootQuery query,
			Action<SelectQuery<T>> subSelect)
		{
			var q = query
				.QueryText("(")
				.Select<T>();
			subSelect(q);
			return q.QueryText(")");
		}

	
		
		/// <summary>
		///     Creates a QueryCommand that uses the * Operator to select all date from the inner query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static SelectQuery<T> SelectStar<T>(this RootQuery query) 
		{
			return new SelectQuery<T>(query.QueryText("SELECT * FROM " + query.ContainerObject.AccessLayer.GetClassInfo(typeof(T)).TableName));
		}
		
		/// <summary>
		///     Adds a Between statement followed by anything added from the action
		/// </summary>
		/// <param name="query"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Between<T>(this ConditionalColumnQuery<T> query)
		{
			return new ConditionalEvalQuery<T>(query.QueryText("BETWEEN"));
		}

		/// <summary>
		///     Adds a between statement followed by a query defined in <paramref name="valueA" /> folowed by an and statement and
		///     an secound query defined in the <paramref name="valueB" />
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Between<T>(
			this ConditionalColumnQuery<T> query,
			object valA,
			object valB)
		{
			return new ConditionalEvalQuery<T>(query.Between()
				.QueryQ("@bet_valA", new QueryParameter("@bet_valA", valA, valA.GetType()))
				.And()
				.QueryQ("@bet_valB", new QueryParameter("@bet_valB", valA, valA.GetType())));
		}

		/// <summary>
		///     Adds a between statement followed by a query defined in <paramref name="valueA" /> folowed by an and statement and
		///     an secound query defined in the <paramref name="valueB" />
		/// </summary>
		/// <param name="query"></param>
		/// <param name="valueA"></param>
		/// <param name="valueB"></param>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Between<T>(
			this ConditionalColumnQuery<T> query,
			Action<RootQuery> valA,
			Action<RootQuery> valB)
		{
			var condtion = query.Between();
			valA(new RootQuery(condtion));
			condtion.And();
			valA(new RootQuery(condtion));

			return new ConditionalEvalQuery<T>(condtion);
		}

		public static RootQuery Apply(this RootQuery query, 
			ApplyMode mode, 
			Action<IQueryBuilder> innerText)
		{
			query
				.QueryText(mode.ApplyType);
			query.InBracket(innerText);
			return query;
		}

		/// <summary>
		///     Append an AS part
		/// </summary>
		/// <returns></returns>
		public static ElementProducer<T> As<T>(this ElementProducer<T> query, string alias)
		{
			return query.QueryText("AS " + alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Contains<T>(this ConditionalColumnQuery<T> query, object alias)
		{
			return new ConditionalEvalQuery<T>(query.QueryQ("CONTAINS (@Cont_A{0})", new QueryParameter("@Cont_A", alias, alias.GetType())));
		}
		
		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <returns></returns>
		public static ElementProducer<T> RowNumberOrder<T>(this ElementProducer<T> query, string over, bool desc = false)
		{
			return query.QueryText("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, desc ? "DESC" : "ASC");
		}

		/// <summary>
		///     Adds a LEFT JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static RootQuery Join<T,TE>(this ElementProducer<T> query, string mode = null)
		{
			var accessLayer = query.ContainerObject.AccessLayer;
			var sourcePK = typeof(T).GetFK(typeof(TE), query.ContainerObject.AccessLayer.Config);
			var targetPK = accessLayer.GetClassInfo(typeof(TE)).GetPK(query.ContainerObject.AccessLayer.Config);
			var targetTable = accessLayer.GetClassInfo(typeof(TE)).TableName;
			var sourceTable = accessLayer.GetClassInfo(typeof(T)).TableName;
			return new RootQuery(query.QueryText("JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable));
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <returns></returns>
		public static RootQuery Join<T, TE>(this ElementProducer<T> query, JoinMode mode)
		{
			return Join<T,TE>(query, mode.JoinType);
		}

		/// <summary>
		///     Inserts a TOP statement
		/// </summary>
		/// <returns></returns>
		public static ElementProducer<T> Top<T>(this ElementProducer<T> query, uint top)
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

		//public static IQueryBuilder<T> Count<T>(this IQueryBuilder<T> query, string what)
		//	where T : IElementProducer
		//{
		//	return query.QueryText("COUNT(" + what + ")");
		//}

		public static ConditionalQuery<TPoco> OrderBy<TPoco>(this ElementProducer<TPoco> query, string over)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} ASC", over));
		}

		public static ConditionalQuery<TPoco> OrderBy<TPoco, TE>(this ElementProducer<TPoco> query, Expression<Func<TPoco, TE>> columnName, bool desc = false)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} ASC", columnName.GetPropertyInfoFromLabda()));
		}

		public static ConditionalQuery<TPoco> OrderByDesc<TPoco>(this ElementProducer<TPoco> query, string over)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} DESC", over));
		}

		public static ConditionalQuery<TPoco> OrderByDesc<TPoco, TE>(this ElementProducer<TPoco> query, Expression<Func<TPoco, TE>> columnName, bool desc = false)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} DESC", columnName.GetPropertyInfoFromLabda()));
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