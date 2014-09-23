using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JPB.DataAccess.QueryProvider
{
    public class Query<T> : IOrderedQueryable<T>
    {
        private readonly QueryProvider provider;

        public Query(QueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.provider = provider;
            Expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (expression != null)
            {
                if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type) && !(expression is ParameterExpression))
                {
                    throw new ArgumentOutOfRangeException("expression");
                }
            }
            this.provider = provider;
            Expression = expression;
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Gibt einen Enumerator zurück, der die Auflistung durchläuft.
        /// </summary>
        /// <returns>
        ///     Ein <see cref="T:System.Collections.Generic.IEnumerator`1" />, der zum Durchlaufen der Auflistung verwendet werden
        ///     kann.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> execute = ((IEnumerable) provider.Execute(Expression)).Cast<T>();
            return execute.GetEnumerator();
        }

        /// <summary>
        ///     Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
        /// </summary>
        /// <returns>
        ///     Ein <see cref="T:System.Collections.IEnumerator" />-Objekt, das zum Durchlaufen der Auflistung verwendet werden
        ///     kann.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IQueryable

        /// <summary>
        ///     Ruft die Ausdrucksbaumstruktur ab, die mit der Instanz von <see cref="T:System.Linq.IQueryable" /> verknüpft ist.
        /// </summary>
        /// <returns>
        ///     Der <see cref="T:System.Linq.Expressions.Expression" />, der mit dieser Instanz von
        ///     <see cref="T:System.Linq.IQueryable" /> verknüpft ist.
        /// </returns>
        public Expression Expression { get; private set; }

        /// <summary>
        ///     Ruft den Typ der Elemente ab, die zurückgegeben werden, wenn die Ausdrucksbaumstruktur ausgeführt wird, die mit
        ///     dieser Instanz von <see cref="T:System.Linq.IQueryable" /> verknüpft ist.
        /// </summary>
        /// <returns>
        ///     Ein <see cref="T:System.Type" />, der den Typ der Elemente darstellt, die zurückgegeben werden, wenn die
        ///     Ausdrucksbaumstruktur ausgeführt wird, die mit diesem Objekt verknüpft ist.
        /// </returns>
        public Type ElementType
        {
            get { return typeof (T); }
        }

        /// <summary>
        ///     Ruft den Abfrageanbieter ab, der dieser Datenquelle zugeordnet ist.
        /// </summary>
        /// <returns>
        ///     Der <see cref="T:System.Linq.IQueryProvider" />, der dieser Datenquelle zugeordnet ist.
        /// </returns>
        public IQueryProvider Provider
        {
            get { return provider; }
        }

        #endregion
    }
}