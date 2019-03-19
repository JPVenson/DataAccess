#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Orders;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Operators
{
    /// <summary>
    ///		Defines a Query that can return any count of <typeparamref name="TPoco"/>
    /// </summary>
    /// <typeparam name="TPoco"></typeparam>
    public class ElementResultQuery<TPoco> : IdentifyableQuery<TPoco>, IElementProducer<TPoco>, IEnumerable<TPoco>
    {
        /// <inheritdoc />
        public ElementResultQuery(IQueryBuilder database, string currentIdentifier) : base(database, currentIdentifier)
        {
        }

        /// <inheritdoc />
        public ElementResultQuery(IQueryBuilder database) : base(database)
        {
        }

        /// <summary>
        ///     Adds an SQL ORDER BY Statement
        /// </summary>
        /// <returns></returns>
        public virtual OrderStatementQuery<TPoco> Order
        {
            get { return new OrderStatementQuery<TPoco>(Add(new OrderByColumnQueryPart())); }
        }

        /// <summary>
        ///     Creates an Statement based on this query to select a Subset of rows by Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual ElementProducer<TPoco> LimitBy(int limit)
        {
            switch (ContainerObject.AccessLayer.DbAccessType)
            {
                case DbAccessType.MsSql:
                    ContainerObject.Search<ISelectableQueryPart>().Limit = limit;
                    break;

                case DbAccessType.SqLite:
                case DbAccessType.MySql:
                    var elementProducer = new RootQuery(ContainerObject.AccessLayer)
                        .WithCte(this, out var cteId)
                        .Select.Identifier<TPoco>(cteId)
                        .Add(new LimitByQueryPart(limit));

                    return new ElementProducer<TPoco>(elementProducer);
            }
            return new ElementProducer<TPoco>(this);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TPoco> GetEnumerator()
        {
            return base.GetEnumerator<TPoco>();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Executes the Current QueryBuilder by setting the type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TPoco> ForResult(bool async = true)
        {
            return new QueryEnumeratorEx<TPoco>(this, async);
        }

        /// <summary>
        ///     Executes the Current QueryBuilder by setting the type
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public IDataPager<TPoco> ForPagedResult(int page, int pageSize)
        {
            var pager = ContainerObject.AccessLayer.Database.CreatePager<TPoco>();
            pager.CommandQuery = this;
            pager.PageSize = pageSize;
            pager.CurrentPage = page;
            pager.LoadPage(ContainerObject.AccessLayer);
            return pager;
        }

        private class QueryBuilderContainerDebugView : ElementResultQuery<TPoco>
        {
            public QueryBuilderContainerDebugView(ElementResultQuery<TPoco> container) : base(container)
            {
            }

            public new IEnumerator<TPoco> GetEnumerator()
            {
                return base.GetEnumerator<TPoco>(false);
            }
        }
    }
}