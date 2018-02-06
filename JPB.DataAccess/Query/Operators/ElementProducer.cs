#region

using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Orders;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	public class IdentifyableQuery<TPoco> : QueryBuilderX
	{
		/// <summary>
		///     Easy access to the Cache for TPoco
		/// </summary>
		protected internal DbClassInfoCache Cache;

		/// <summary>
		///     Initializes a new instance of the <see cref="ElementProducer{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="currentIdentifier">The current identifier.</param>
		public IdentifyableQuery(IQueryBuilder database, string currentIdentifier) : base(database)
		{
			CurrentIdentifier = currentIdentifier;
			SetCache();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ElementProducer{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public IdentifyableQuery(IQueryBuilder database) : base(database)
		{
			SetCache();
		}

		/// <summary>
		///     Gets the current identifier in the query.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

		protected void CreateNewIdentifier()
		{
			CurrentIdentifier = string.Format("{0}_{1}", Cache.TableName, ContainerObject.GetNextParameterId());
		}

		/// <summary>
		///     Sets the cache.
		/// </summary>
		private void SetCache()
		{
			Cache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
		}
	}

	public class ElementResultQuery<TPoco> : IdentifyableQuery<TPoco>, IElementProducer<TPoco>, IEnumerable<TPoco>
	{
		/// <summary>
		///     Adds an SQL ORDER BY Statement
		/// </summary>
		/// <returns></returns>
		public OrderStatementQuery<TPoco> Order
		{
			get { return new OrderStatementQuery<TPoco>(this.QueryText("ORDER BY")); }
		}

		/// <summary>
		///     Creates an Statement based on this query to select a Subset of rows by Limit
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		public ElementProducer<TPoco> LimitBy(int limit)
		{
			QueryBuilderX wrapper;
			switch (ContainerObject.AccessLayer.DbAccessType)
			{
				case DbAccessType.MsSql:
					CreateNewIdentifier();
					wrapper = new ElementProducer<TPoco>(this, CurrentIdentifier)
							.QueryD(string.Format("SELECT TOP {0} * FROM (", limit)).Append(this).QueryD(") AS " + CurrentIdentifier);
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
		public IEnumerable<TPoco> ForResult()
		{
			return new QueryEnumeratorEx<TPoco>(this);
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

		public ElementResultQuery(IQueryBuilder database, string currentIdentifier) : base(database, currentIdentifier)
		{
		}

		public ElementResultQuery(IQueryBuilder database) : base(database)
		{
		}
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{TPoco}" />
	/// <seealso cref="System.Collections.Generic.IEnumerable{TPoco}" />
	public class ElementProducer<TPoco> : ElementResultQuery<TPoco>
	{
		public ElementProducer(IQueryBuilder database, string currentIdentifier) : base(database, currentIdentifier)
		{
		}

		public ElementProducer(IQueryBuilder database) : base(database)
		{
		}


		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(CurrentIdentifier)); }
		}
	}
}