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
            get { return new OrderStatementQuery<TPoco>(this.QueryText("ORDER BY")); }
        }

        /// <summary>
        ///     Creates an Statement based on this query to select a Subset of rows by Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual ElementProducer<TPoco> LimitBy(int limit)
        {
            QueryBuilderX wrapper;
            switch (ContainerObject.AccessLayer.DbAccessType)
            {
                case DbAccessType.MsSql:
                    CreateNewIdentifier();
                    wrapper = new QueryBuilderX(ContainerObject.AccessLayer)
                        .QueryD(string.Format("SELECT TOP {0} * FROM ", limit))
                        .InBracket(e => e.Append(this))
                        .QueryD("AS " + CurrentIdentifier);
                    break;
                case DbAccessType.SqLite:
                case DbAccessType.MySql:
                    wrapper = new QueryBuilderX(ContainerObject.AccessLayer)
                              .QueryD("SELECT * FROM (").Append(this).QueryD(string.Format(") LIMIT {0}", limit));
                    break;
                default:
                    throw new NotImplementedException(string.Format("Invalid Target Database {0}",
                    ContainerObject.AccessLayer.Database.TargetDatabase));
            }

            return new ElementProducer<TPoco>(wrapper);
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
            var command = ContainerObject.Compile();
            var pager = ContainerObject.AccessLayer.Database.CreatePager<TPoco>();
            if (pager is MsSqlUntypedDataPager<TPoco>)
            {
                var msPager = pager as MsSqlUntypedDataPager<TPoco>;
                msPager.CommandQuery = this;
            }
            else
            {
                pager.BaseQuery = command;
            }

            pager.PageSize = pageSize;
            pager.CurrentPage = page;
            pager.LoadPage(ContainerObject.AccessLayer);
            return pager;
        }

        /// <summary>
        ///     Adds a SQL WHERE statement
        ///     does not emit any conditional statement
        ///     should be followed by Column()
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public ElementProducer<TPoco> Alias(string alias)
        {
            return new ElementProducer<TPoco>(this, alias);
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