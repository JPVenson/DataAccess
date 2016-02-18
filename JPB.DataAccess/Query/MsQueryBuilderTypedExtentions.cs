/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

namespace JPB.DataAccess.Query
{
	//public static partial class MsQueryBuilderExtentions
	//{
	//	/// <summary>
	//	///     Adds a QueryCommand part to <paramref name="builder" />
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> QueryQ<T>(this IQueryContainer<T> builder, string query,
	//		params IQueryParameter[] parameters)
	//	{
	//		return builder.Add(new GenericQueryPart(query, parameters));
	//	}

	//	/// <summary>
	//	///     Adds a QueryCommand part to <paramref name="builder" />
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> QueryD<T>(this IQueryContainer<T> builder, string query, dynamic paramerters)
	//	{
	//		if (paramerters != null)
	//		{
	//			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
	//			return builder.Add(new GenericQueryPart(query, parameters));
	//		}

	//		return builder.Add(new GenericQueryPart(query));
	//	}

	//	/// <summary>
	//	///     Adds a QueryCommand part to <paramref name="builder" />
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> QueryD<T>(this IQueryContainer<T> builder, string query)
	//	{
	//		return QueryD(builder, query, null);
	//	}

	//	/// <summary>
	//	///     Adds a QueryCommand part to <paramref name="builder" />
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> QueryCommand<T>(this IQueryContainer<T> builder, IDbCommand command)
	//	{
	//		return builder.Add(GenericQueryPart.FromCommand(command));
	//	}

	//	/// <summary>
	//	///     Adds a QueryCommand part to <paramref name="builder" />
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> QueryCommand<T>(this IQueryContainer<T> builder, string query, params object[] args)
	//	{
	//		return builder.QueryQ(string.Format(query, args));
	//	}

	//	/// <summary>
	//	///     Adds a Select - Statement
	//	///     Uses reflection or a Factory mehtod to create
	//	/// </summary>
	//	/// <typeparam name="T"></typeparam>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Select<T>(this IQueryContainer<T> query)
	//	{
	//		return query.Select(typeof (T));
	//	}

	//	/// <summary>
	//	///     Adds a Update - Statement
	//	///     Uses reflection or a Factory mehtod to create
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Select<T>(this IQueryContainer<T> query, Type type)
	//	{
	//		return query.QueryCommand(query.AccessLayer.CreateSelectQueryFactory(type.GetClassInfo(), query.AccessLayer.Database));
	//	}

	//	/// <summary>
	//	///     Adds a Update - Statement
	//	///     Uses reflection or a Factory mehtod to create
	//	/// </summary>
	//	/// <typeparam name="T"></typeparam>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Update<T>(this IQueryContainer<T> query, T obj)
	//	{
	//		return query.Update(typeof (T), obj);
	//	}

	//	/// <summary>
	//	///     Adds a Select - Statement
	//	///     Uses reflection or a Factory mehtod to create
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Update<T>(this IQueryContainer<T> query, Type type, object obj)
	//	{
	//		return query.QueryCommand(query.AccessLayer._CreateUpdate(type.GetClassInfo(), obj));
	//	}

	//	/// <summary>
	//	///     Creates a Common Table Expression that selects a Specific type
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> WithCteForType<T>(this IQueryContainer<T> query, Type target, string cteName,
	//		bool useStarOperator = false)
	//	{
	//		var cteBuilder = new StringBuilder();
	//		cteBuilder.Append("WITH ");
	//		cteBuilder.Append(cteName);
	//		cteBuilder.Append(" (");
	//		cteBuilder.Append(!useStarOperator ? target.GetClassInfo().CreatePropertyCsv() : "*");
	//		cteBuilder.Append(") AS (");
	//		cteBuilder.Append(DbAccessLayer.CreateSelect(target));
	//		cteBuilder.Append(")");
	//		query.QueryCommand(cteBuilder.ToString());
	//		return query;
	//	}

	//	/// <summary>
	//	///     Creates a Common Table Expression that selects a Specific type
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> InBracket<T>(this IQueryContainer<T> query, Action<QueryBuilder<T>> header)
	//	{
	//		query.QueryCommand("(");
	//		header(query);
	//		query.QueryCommand(")");
	//		return query;
	//	}

	//	/// <summary>
	//	///     Creates a Subselect
	//	/// </summary>
	//	/// <returns></returns>
	//	[Obsolete("Use the InBracket Mehtod", true)]
	//	public static IQueryContainer<T> SubSelect<T>(this IQueryContainer<T> query, Action<QueryBuilder<T>> subSelect)
	//	{
	//		query.QueryCommand("(");
	//		subSelect(query);
	//		query.QueryCommand(")");
	//		return query;
	//	}

	//	/// <summary>
	//	///     Wraps the Existing command into a DataPager for the underlying Database
	//	///     Accepts only Where statements
	//	///     <example>
	//	///     </example>
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IDataPager AsPager<T>(this IQueryContainer<T> query, int pageSize)
	//	{
	//		var targetQuery = query.Compile();
	//		var dbAccess = query.AccessLayer.Database.CreatePager<T>();
	//		dbAccess.AppendedComands.Add(targetQuery);
	//		dbAccess.PageSize = pageSize;
	//		return dbAccess;
	//	}

	//	/// <summary>
	//	///     Creates a Common Table Expression that selects a Specific type
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> SubSelect<T>(this IQueryContainer<T> query, Type type)
	//	{
	//		query.QueryCommand("(");
	//		query.Select(type);
	//		query.QueryCommand(")");
	//		return query;
	//	}

	//	/// <summary>
	//	///     Append an AS part
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> As<T>(this IQueryContainer<T> query, string alias)
	//	{
	//		return query.QueryCommand("AS " + alias);
	//	}

	//	/// <summary>
	//	///     Append an Contains part
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Contains<T>(this IQueryContainer<T> query, string alias)
	//	{
	//		return query.QueryCommand("CONTAINS ({0})", alias);
	//	}

	//	/// <summary>
	//	///     Append an Contains part
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Contains<T>(this IQueryContainer<T> query, object alias)
	//	{
	//		var paramaterAutoId = query.GetParamaterAutoId();
	//		return query.QueryQ(string.Format("CONTAINS (@{0})", paramaterAutoId),
	//			new QueryParameter(paramaterAutoId.ToString(CultureInfo.InvariantCulture), alias));
	//	}

	//	/// <summary>
	//	///     Append an RowNumberOrder part
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> RowNumberOrder<T>(this IQueryContainer<T> query, string over, bool Desc = false)
	//	{
	//		return query.QueryCommand("ROW_NUMBER() OVER (ORDER BY {0} {1})", over, Desc ? "DESC" : "ASC");
	//	}

	//	/// <summary>
	//	///     Append an RowNumberOrder part
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> RowNumberOrder<T, TProp>(this IQueryContainer<T> query, Expression<Func<T, TProp>> exp,
	//		bool Desc = false)
	//	{
	//		return query.QueryCommand("ROW_NUMBER() OVER (ORDER BY {0} {1})", MetaInfoStoreExtentions.GetPropertyInfoFromLabda(exp),
	//			Desc ? "DESC" : "ASC");
	//	}

	//	/// <summary>
	//	///     Adds a SQL WHERE Condition
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Where<T>(this IQueryContainer<T> query, string condition, dynamic paramerters = null)
	//	{
	//		if (paramerters != null)
	//		{
	//			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
	//			return query.QueryQ(string.Format("WHERE {0}", condition), parameters.ToArray());
	//		}
	//		return query.QueryCommand("WHERE {0}", condition);
	//	}

	//	/// <summary>
	//	///     Adds And Condition
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> And<T>(this IQueryContainer<T> query, string condition, dynamic paramerters = null)
	//	{
	//		if (paramerters != null)
	//		{
	//			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
	//			return query.QueryQ(string.Format("AND {0}", condition), parameters.ToArray());
	//		}
	//		return query.QueryCommand("AND {0}", condition);
	//	}

	//	/// <summary>
	//	///     Adds Or Condition
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Or<T>(this IQueryContainer<T> query, string condition, dynamic paramerters = null)
	//	{
	//		if (paramerters != null)
	//		{
	//			IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
	//			return query.QueryQ("OR " + condition, parameters.ToArray());
	//		}
	//		return query.QueryCommand("OR {0}", condition);
	//	}

	//	/// <summary>
	//	///     Adds a LEFT JOIN to the Statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Join<T>(this IQueryContainer<T> query, Type source, Type target)
	//	{
	//		var sourcePK = source.GetFK(target);
	//		var targetPK = target.GetPK();
	//		var targetTable = target.GetClassInfo().TableName;
	//		var sourceTable = source.GetClassInfo().TableName;
	//		return query.QueryCommand("JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
	//	}

	//	/// <summary>
	//	///     Adds a JOIN to the Statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Join<T>(this IQueryContainer<T> query, JoinMode mode, Type source, Type target)
	//	{
	//		return Join(query, mode.JoinType, source, target);
	//	}

	//	/// <summary>
	//	///     Adds a JOIN to the Statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Join<T>(this IQueryContainer<T> query, string mode, Type source, Type target)
	//	{
	//		if (query == null) throw new ArgumentNullException("query");
	//		if (mode == null) throw new ArgumentNullException("mode");
	//		if (source == null) throw new ArgumentNullException("source");
	//		if (target == null) throw new ArgumentNullException("target");

	//		var sourcePK = source.GetFK(target);
	//		var targetPK = target.GetPK();
	//		var targetTable = target.GetClassInfo().TableName;
	//		var sourceTable = source.GetClassInfo().TableName;
	//		return query.QueryCommand(mode + " JOIN {0} ON {0}.{1} = {3}.{2}", targetTable, targetPK, sourcePK, sourceTable);
	//	}

	//	/// <summary>
	//	///     Adds a JOIN to the Statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Join<Source, Target, T>(this IQueryContainer<T> query, JoinMode mode)
	//	{
	//		return Join(query, mode, typeof (Source), typeof (Target));
	//	}

	//	/// <summary>
	//	///     Inserts a TOP statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> Top<T>(this IQueryContainer<T> query, uint top)
	//	{
	//		switch (query.AccessLayer.Database.TargetDatabase)
	//		{
	//			case DbAccessType.MsSql:
	//			{
	//				var index = -1;
	//				var select = "SELECT";
	//				var part =
	//					query.Parts.FirstOrDefault(s => (index = s.Prefix.ToUpper().IndexOf(@select, StringComparison.Ordinal)) != -1);

	//				if (index == -1 || part == null)
	//					return query;

	//				part.Prefix = part.Prefix.Insert(index + @select.Length, " TOP " + top);
	//			}
	//				break;
	//			case DbAccessType.MySql:
	//				return query.QueryCommand("LIMIT BY {0}", top);
	//			default:
	//				throw new NotSupportedException("For the Selected DB type is no Top implementations Available. Use QueryD insted");
	//		}
	//		return query;
	//	}

	//	/// <summary>
	//	///     Adds Parameter to the QueryCommand object without adding a Statement
	//	/// </summary>
	//	/// <returns></returns>
	//	public static IQueryContainer<T> WithParamerters<T>(this IQueryContainer<T> query, dynamic paramerters)
	//	{
	//		IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
	//		query.QueryQ(string.Empty, parameters.ToArray());
	//		return query;
	//	}
	//}
}