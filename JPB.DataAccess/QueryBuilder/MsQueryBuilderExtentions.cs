using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// Provieds A set of extentions for Microsoft SQL Serve
    /// </summary>
    public static class MsQueryBuilderExtentions
    {
        /// <summary>
        /// Adds a Query part to <paramref name="builder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static QueryBuilder QueryQ(this QueryBuilder builder, string query, params IQueryParameter[] parameters)
        {
            return builder.Add(new QueryPart(query, parameters));
        }

        /// <summary>
        /// Adds a Query part to <paramref name="builder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="query"></param>
        /// <param name="paramerters"></param>
        /// <returns></returns>
        public static QueryBuilder QueryD(this QueryBuilder builder, string query, dynamic paramerters)
        {
            if (paramerters != null)
            {
                IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
                return builder.Add(new QueryPart(query, parameters));
            }

            return builder.Add(new QueryPart(query));
        }


        /// <summary>
        /// Adds a Query part to <paramref name="builder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static QueryBuilder Query(this QueryBuilder builder, IDbCommand command)
        {
            return builder.Add(QueryPart.FromCommand(command));
        }

        /// <summary>
        /// Adds a Query part to <paramref name="builder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static QueryBuilder Query(this QueryBuilder builder, string query, params object[] args)
        {
            return builder.QueryQ(string.Format(query, args));
        }

        /// <summary>
        /// Adds a Select - Statement
        /// Uses reflection or a Factory mehtod to create
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QueryBuilder Select<T>(this QueryBuilder query)
        {
            return query.Select(typeof(T));
        }

        /// <summary>
        /// Adds a Update - Statement
        /// Uses reflection or a Factory mehtod to create
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static QueryBuilder Select(this QueryBuilder query, Type type)
        {
            return query.Query(DbAccessLayer.CreateSelectQueryFactory(type, query.Database));
        }

        /// <summary>
        /// Adds a Update - Statement
        /// Uses reflection or a Factory mehtod to create
        /// </summary>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QueryBuilder Update<T>(this QueryBuilder query, T obj)
        {
            return query.Update(typeof(T), obj);
        }

        /// <summary>
        /// Adds a Select - Statement
        /// Uses reflection or a Factory mehtod to create
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static QueryBuilder Update(this QueryBuilder query, Type type, object obj)
        {
            return query.Query(DbAccessLayer.createUpdate(obj, query.Database));
        }

        /// <summary>
        /// Add an AS part
        /// </summary>
        /// <param name="query"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static QueryBuilder As(this QueryBuilder query, string alias)
        {
            return query.Query("AS " + alias);
        }

        /// <summary>
        /// Adds a SQL WHERE Condition
        /// </summary>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="paramerters"></param>
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
        /// Adds And Condition
        /// </summary>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="paramerters"></param>
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
        /// Adds Or Condition
        /// </summary>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="paramerters"></param>
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
        /// Adds a LEFT JOIN to the Statement
        /// </summary>
        /// <param name="query"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static QueryBuilder Join(this QueryBuilder query, Type source, Type target)
        {
            var sourcePK = source.GetPK();
            var targetPK = target.GetPK();
            var targetTable = target.GetTableName();
            return query.Query("LEFT JOIN {0} ON {1} = {2}", targetTable, targetPK, sourcePK);
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract class JoinMode
        {
            internal JoinMode(string joinType)
            {
                JoinType = joinType;
            }

            public string JoinType { get; private set; }
        }

        /// <summary>
        /// Adds a JOIN to the Statement
        /// </summary>
        /// <param name="query"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static QueryBuilder Join(this QueryBuilder query, JoinMode mode, Type source, Type target)
        {
            return Join(query, mode.JoinType, source, target);
        }

        /// <summary>
        /// Adds a JOIN to the Statement
        /// </summary>
        /// <param name="query"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static QueryBuilder Join(this QueryBuilder query, string mode, Type source, Type target)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (mode == null) throw new ArgumentNullException("mode");
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            var sourcePK = source.GetFK(target);
            var targetPK = target.GetPK();
            var targetTable = target.GetTableName();
            return query.Query(mode + " JOIN {0} ON {1} = {2}", targetTable, targetPK, sourcePK);
        }

        /// <summary>
        /// Adds a JOIN to the Statement
        /// </summary>
        /// <param name="query"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static QueryBuilder Join<Source,Target>(this QueryBuilder query, JoinMode mode)
        {
            return Join(query, mode, typeof (Source), typeof (Target));
        }

        /// <summary>
        /// Inserts a TOP statement into an existing TSQL Select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="top">a Non negativ number</param>
        /// <returns></returns>
        public static QueryBuilder MsTop(this QueryBuilder query, uint top)
        {
            int index = -1;
            var select = "SELECT";
            var part = query.Parts.FirstOrDefault(s => (index = s.Prefix.ToUpper().IndexOf(select, System.StringComparison.Ordinal)) != -1);

            if (index == -1 || part == null)
                return query;

            part.Prefix = part.Prefix.Insert(index + @select.Length, " TOP " + top);

            return query;
        }

        /// <summary>
        /// Inserts a TOP statement into an existing MySQL Select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static QueryBuilder MyTop(this QueryBuilder query, uint top)
        {
            return query.Query("LIMIT BY {0}", top);
        }

        /// <summary>
        /// Adds Parameter to the Query object without adding a Statement
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramerters"></param>
        /// <returns></returns>
        public static QueryBuilder WithParamerters(this QueryBuilder query, dynamic paramerters)
        {
            IEnumerable<IQueryParameter> parameters = DbAccessLayerHelper.EnumarateFromDynamics(paramerters);
            query.QueryQ(string.Empty, parameters.ToArray());
            return query;
        }

        /// <summary>
        /// This will execute the query Async
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> ConfigurateAwaiter<T>(this QueryBuilder<T> query)
        {
            var task = new Task<IEnumerable<T>>(query.ToArray);

            task.Start();
            await task;

            return task.Result;
        }
    }
}