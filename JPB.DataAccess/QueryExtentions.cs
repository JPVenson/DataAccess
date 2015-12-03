using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess
{
    public static class QueryExtentions
    {
        public static string SelectExpression = "SELECT";
        public static string UpdateExpression = "UPDATE";
        public static string DeleteExpression = "DELETE";
        public static string InsertExpression = "INSERT";

        private static IQueryable<T> SqlQuery<T>(this IQueryable<T> query, Expression predicate, MethodInfo info)
        {
            var queryable = query as Query<T>;

            var expressions = new List<Expression>
            {
                // old expression
                queryable.Expression,
                predicate
            };

            MethodCallExpression methodCallExpression = Expression.Call(null, info, expressions);

            return
                query.Provider.CreateQuery<T>(methodCallExpression);
        }


        public static IQueryable<T> WhereSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }

        public static IQueryable<T> AndSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }

        public static IQueryable<T> OrSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }

        public static IQueryable<T> LessThenSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }

        public static IQueryable<T> IsSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }

        public static IQueryable<T> EqualsSql<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return SqlQuery(query, predicate, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)));
        }
    }
}