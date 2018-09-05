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

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Provieds A set of extentions for Microsoft SQL Serve
	/// </summary>
	public static class MsQueryBuilderExtentions
	{
		/// <summary>
		///     Sets an Variable to the given value
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static RootQuery SetVariable(this RootQuery query, string name, object value)
		{
			var transpiledValue = MsSql.ParameterValue(new SqlParameter(name, value));
			var sqlName = name;
			if (!sqlName.StartsWith("@"))
			{
				sqlName = "@" + sqlName;
			}

			query.QueryText("SET {0} = {1}", sqlName, transpiledValue);
			return query;
		}

		/// <summary>
		///     Declares a new Variable of the Given SQL Type by using its length
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="length">The length.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static RootQuery DeclareVariable(this RootQuery query, string name, SqlDbType type, int length = int.MaxValue,
			object value = null)
		{
			var sqlName = name;
			if (!sqlName.StartsWith("@"))
			{
				sqlName = "@" + sqlName;
			}
			var typeName = type.ToString();
			if (new SqlParameter("xxx", type).Size > 0)
			{
				typeName = "(MAX)";
			}

			query = query.QueryText("DECLARE {0} {1};", sqlName, typeName);
			if (value != null)
			{
				query = query.SetVariable(sqlName, value);
			}
			return query;
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="target">The target.</param>
		/// <param name="cteName">Name of the cte.</param>
		/// <param name="useStarOperator">if set to <c>true</c> [use star operator].</param>
		/// <returns></returns>
		[Obsolete("Will be Removed in Future")]
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
			return query.Add(new CteQueryPart(cteBuilder.ToString()));
		}

		/// <summary>
		///     Creates a FOR XML statement that uses the name of the given type to allow the .net XML Serilizer to read the output
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public static ElementProducer<string> ForXml<TPoco>(this ElementProducer<TPoco> query, Type target)
		{
			return new ElementProducer<string>(query.QueryText("FOR XML PATH('{0}'),ROOT('ArrayOf{0}'), TYPE", target.Name),
				query.CurrentIdentifier);
		}

		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="cteName">Name of the cte.</param>
		/// <param name="cteAction">The cte action.</param>
		/// <param name="subCte">if set to <c>true</c> [sub cte].</param>
		/// <returns></returns>
		public static RootQuery WithCte<T>(this RootQuery query,
			string cteName,
			Func<RootQuery, ISelectQuery<T>> cteAction,
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

			return new RootQuery(query.Add(new GenericQueryPart(prefix)).InBracket(cteAction).Add(new CteQueryPart("")));
		}


		/// <summary>
		///     Creates a Common Table Expression that selects a Specific type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TN">The type of the n.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="cteName">Name of the cte.</param>
		/// <param name="subCte">if set to <c>true</c> [sub cte].</param>
		/// <returns></returns>
		public static ElementProducer<TN> AsCte<T, TN>(this ElementProducer<T> query,
			string cteName,
			bool subCte = false)
		{
			var cp = new RootQuery(query.ContainerObject.AccessLayer);
			var prefix = string.Format("WITH {0} AS (", cteName);
			cp = cp.QueryText(prefix);
			foreach (var genericQueryPart in query.ContainerObject.Parts)
			{
				cp = cp.Add(genericQueryPart);
			}

			return new ElementProducer<TN>(cp.Add(new CteQueryPart(")")).QueryText(string.Format("SELECT {1} FROM {0}", cteName,
				query.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TN))
					.GetSchemaMapping()
					.Aggregate((e, f) => e + ", " + f))), query.CurrentIdentifier);
		}


		/// <summary>
		///     Creates an closed sub select
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="subSelect">The sub select.</param>
		/// <returns></returns>
		public static SelectQuery<T> SubSelect<T>(this RootQuery query,
			Func<SelectQuery<T>, SelectQuery<T>> subSelect)
		{
			var q = subSelect(query
					.QueryText("(")
					.Select.Table<T>());
			return q.QueryText(")");
		}


		/// <summary>
		///     Creates an closed sub select
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="subSelect">The sub select.</param>
		/// <param name="identifyer">The Identifyer for this SubQuery</param>
		/// <returns></returns>
		public static SelectQuery<T> SubSelect<T>(this RootQuery query,
			Func<SelectQuery<T>> subSelect, string identifyer)
		{
			return new SelectQuery<T>(new SelectQuery<T>(query.QueryD("SELECT * FROM ").InBracket(f => f.Append(subSelect()))).As(identifyer));
		}


		/// <summary>
		///     Creates a QueryCommand that uses the * Operator to select all date from the inner query
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static SelectQuery<T> SelectStar<T>(this RootQuery query)
		{
			return
				new SelectQuery<T>(
					query.QueryText("SELECT * FROM " + query.ContainerObject.AccessLayer.GetClassInfo(typeof(T)).TableName));
		}

		/// <summary>
		///     Adds a between statement followed by a query defined in <paramref name="valA" /> folowed by an and statement and
		///     an secound query defined in the <paramref name="valB" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="valA">The value a.</param>
		/// <param name="valB">The value b.</param>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Between<T>(
			this ConditionalOperatorQuery<T> query,
			Func<RootQuery, RootQuery> valA,
			Func<RootQuery, RootQuery> valB)
		{
			var condtion = new RootQuery(query.QueryText("BETWEEN"));
			condtion = valA(new RootQuery(condtion));
			condtion = condtion.QueryText("AND");
			condtion = valA(new RootQuery(condtion));

			return new ConditionalEvalQuery<T>(condtion, query.State);
		}

		/// <summary>
		///     Creates an TSQL Apply statement
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="innerText">The inner text.</param>
		/// <returns></returns>
		public static RootQuery Apply(this RootQuery query,
			ApplyMode mode,
			Func<RootQuery, IQueryBuilder> innerText)
		{
			return new RootQuery(query.QueryText(mode.ApplyType).InBracket(innerText));
		}

		/// <summary>
		///     Creates an TSQL Apply statement
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="innerText">The inner text.</param>
		/// <returns></returns>
		public static ElementProducer<T> Apply<T>(this ElementProducer<T> query,
			ApplyMode mode,
			Func<RootQuery, IQueryBuilder> innerText)
		{
			return new ElementProducer<T>(new RootQuery(query.QueryText(mode.ApplyType)).InBracket(innerText), query.CurrentIdentifier);
		}

		/// <summary>
		///     Append an AS part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		public static ElementProducer<T> As<T>(this ElementProducer<T> query, string alias)
		{
			return query.QueryText("AS " + alias);
		}

		/// <summary>
		///     Append an Contains part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		public static ConditionalEvalQuery<T> Contains<T>(this ConditionalColumnQuery<T> query, object alias)
		{
			return
				new ConditionalEvalQuery<T>(
					query.QueryQ("CONTAINS (@Cont_A)", new QueryParameter("@Cont_A", alias, alias.GetType())), query.State);
		}

		/// <summary>
		///     Append an RowNumberOrder part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="over">The over.</param>
		/// <param name="desc">if set to <c>true</c> [desc].</param>
		/// <returns></returns>
		public static ElementProducer<T> RowNumberOrder<T>(this ElementProducer<T> query, string over, bool desc = false)
		{
			return query.QueryText("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, desc ? "DESC" : "ASC");
		}

		/// <summary>
		///     Adds a LEFT JOIN to the Statement
		/// </summary>
		/// <typeparam name="TLeft">The type of the left.</typeparam>
		/// <typeparam name="TRight">The type of the right.</typeparam>
		/// <typeparam name="TAggregation">The type of the aggregation.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		public static ElementProducer<TAggregation> Join<TLeft, TRight, TAggregation>(this ElementProducer<TLeft> query,
			string mode = null)
		{
			if (mode == null)
			{
				mode = "";
			}

			var accessLayer = query.ContainerObject.AccessLayer;
			var sourcePK = typeof(TLeft).GetFK(typeof(TRight), query.ContainerObject.AccessLayer.Config);
			if (sourcePK == null)
			{
				throw new ArgumentNullException("No Fk for this Constalation found");
			}
			var targetPK = typeof(TRight).GetPK(query.ContainerObject.AccessLayer.Config);
			if (targetPK == null)
			{
				throw new ArgumentNullException("No pk for this Constalation found");
			}
			var targetTable = accessLayer.GetClassInfo(typeof(TRight)).TableName;
			var sourceTable = accessLayer.GetClassInfo(typeof(TLeft)).TableName;
			return
				new ElementProducer<TAggregation>(
					query.QueryText("{4} JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable, mode),
					query.CurrentIdentifier);
		}

		/// <summary>
		///     Adds a JOIN to the Statement
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <typeparam name="TAggregation">The type of the aggregation.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		public static ElementProducer<TAggregation> Join<T, TE, TAggregation>(this ElementProducer<T> query, JoinMode mode)
		{
			return Join<T, TE, TAggregation>(query, mode.JoinType);
		}

		/// <summary>
		///     Inserts a TOP statement
		/// </summary>
		/// <typeparam name="TPoco"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="top">The top.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">For the Selected DB type is no Top implementations Available</exception>
		public static ElementProducer<TPoco> Top<TPoco>(this ElementProducer<TPoco> query, uint top)
		{
			if (query.ContainerObject.AccessLayer.Database.TargetDatabase != DbAccessType.MsSql)
			{
				throw new InvalidOperationException(string.Format("Invalid Target Database {0} by using the MSSQL extentions", query.ContainerObject.AccessLayer.Database.TargetDatabase));
			}

			QueryBuilderX wrapper = new QueryBuilderX(query.ContainerObject.AccessLayer).QueryD(string.Format("SELECT TOP {0} * FROM (", top)).Append(query).QueryD(")");
			return new ElementProducer<TPoco>(wrapper);
		}

		/// <summary>
		///     Creates an TSQL Count(1) statement
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static ElementProducer<long> Count<TPoco>(this RootQuery query)
		{
			return new ElementProducer<long>(query.QueryText("SELECT COUNT(1) FROM {0}",
				query.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName), null);
		}

		/// <summary>
		///     Creates an TSQL Count(1) statement
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static ElementProducer<long> CountLong<TPoco>(this IElementProducer<TPoco> query)
		{
			return query.Count<TPoco, long>();
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
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static ElementProducer<short> CountShort<TPoco>(this IElementProducer<TPoco> query)
		{
			return query.Count<TPoco, short>();
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
			var cteName = "countCte" + query.ContainerObject.GetNextParameterId();
			return new ElementProducer<TOut>(new RootQuery(query.ContainerObject.AccessLayer)
				.WithCte(cteName, f =>
				{
					var order = false;
					foreach (var genericQueryPart in query.ContainerObject.Parts)
					{
						var partType = genericQueryPart.Builder != null ? genericQueryPart.Builder.GetType() : null;
						if (partType != null && order)
						{
							if (partType != typeof(OrderByColumn<TPoco>) && partType != typeof(OrderStatementQuery<TPoco>))
							{
								order = false;
							}
						}

						if (genericQueryPart.Prefix == "ORDER BY")
						{
							order = true;
						}
						if (!order)
						{
							f = f.Add(genericQueryPart);
						}
					}

					return new SelectQuery<TPoco>(f);
				}).QueryText("SELECT COUNT(1) FROM " + cteName), cteName);
		}

		/// <summary>
		///     Creates an TSQL OrderBy statment
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="over">The over.</param>
		/// <returns></returns>
		public static ConditionalQuery<TPoco> OrderBy<TPoco>(this ElementProducer<TPoco> query, string over)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} ASC", over),
				new CondtionBuilderState(query.CurrentIdentifier));
		}

		/// <summary>
		///     Creates an TSQL OrderBy statment
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="desc">if set to <c>true</c> [desc].</param>
		/// <returns></returns>
		public static ConditionalQuery<TPoco> OrderBy<TPoco, TE>(this ElementProducer<TPoco> query,
			Expression<Func<TPoco, TE>> columnName, bool desc = false)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} ASC", columnName.GetPropertyInfoFromLamdba()),
				new CondtionBuilderState(query.CurrentIdentifier));
		}

		/// <summary>
		///     Creates an TSQL OrderBy statment
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="over">The over.</param>
		/// <returns></returns>
		public static ConditionalQuery<TPoco> OrderByDesc<TPoco>(this ElementProducer<TPoco> query, string over)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} DESC", over),
				new CondtionBuilderState(query.CurrentIdentifier));
		}

		/// <summary>
		///     Creates an TSQL OrderBy statment
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <param name="query">The query.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="desc">if set to <c>true</c> [desc].</param>
		/// <returns></returns>
		public static ConditionalQuery<TPoco> OrderByDesc<TPoco, TE>(this ElementProducer<TPoco> query,
			Expression<Func<TPoco, TE>> columnName, bool desc = false)
		{
			return new ConditionalQuery<TPoco>(query.QueryText("ORDER BY {0} DESC", columnName.GetPropertyInfoFromLamdba()),
				new CondtionBuilderState(query.CurrentIdentifier));
		}

		/// <summary>
		/// </summary>
		public abstract class JoinMode
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="JoinMode" /> class.
			/// </summary>
			/// <param name="joinType">Type of the join.</param>
			internal JoinMode(string joinType)
			{
				JoinType = joinType;
			}

			/// <summary>
			///     QueryCommand string
			/// </summary>
			/// <value>
			///     The type of the join.
			/// </value>
			public string JoinType { get; private set; }
		}

		/// <summary>
		/// </summary>
		public abstract class ApplyMode
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="ApplyMode" /> class.
			/// </summary>
			/// <param name="applyType">Type of the apply.</param>
			internal ApplyMode(string applyType)
			{
				ApplyType = applyType;
			}

			/// <summary>
			///     QueryCommand string
			/// </summary>
			/// <value>
			///     The type of the apply.
			/// </value>
			public string ApplyType { get; private set; }
		}
	}
}