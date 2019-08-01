#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.Operators.Orders;
using JPB.DataAccess.Framework.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Framework.Query.Operators
{
    /// <summary>
    ///		Defines a Query that can return any count of <typeparamref name="TPoco"/>
    /// </summary>
    /// <typeparam name="TPoco"></typeparam>
    public class ElementResultQuery<TPoco> : IdentifyableQuery<TPoco>, 
        IElementProducer<TPoco>,
        IEnumerableQuery<TPoco>
    {
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
                    ContainerObject.SearchLast<ISelectableQueryPart>().Limit = limit;
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
        ///		Sets the ContainerObject`s ExecuteAsync
        /// </summary>
        /// <param name="async"></param>
        /// <returns></returns>
        public ElementResultQuery<TPoco> ExecutionMode(bool async)
        {
            ContainerObject.ExecuteAsync = async;
            return this;
        }


        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TPoco> GetEnumerator()
        {
            return new QueryEagerEnumerator<TPoco>(ContainerObject, true);
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
    }
}