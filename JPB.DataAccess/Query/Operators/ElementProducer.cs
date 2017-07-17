﻿#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Orders;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{TPoco}" />
	/// <seealso cref="System.Collections.Generic.IEnumerable{TPoco}" />
	public class ElementProducer<TPoco> : QueryBuilderX, IElementProducer<TPoco>, IEnumerable<TPoco>
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
		public ElementProducer(IQueryBuilder database, string currentIdentifier) : base(database)
		{
			CurrentIdentifier = currentIdentifier;
			SetCache();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ElementProducer{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public ElementProducer(IQueryBuilder database) : base(database)
		{
			SetCache();
			CurrentIdentifier = string.Format("{0}_{1}", Cache.TableName, ContainerObject.GetNextParameterId());
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

		/// <summary>
		///     Adds an SQL ORDER BY Statement
		/// </summary>
		/// <returns></returns>
		public OrderStatementQuery<TPoco> Order
		{
			get { return new OrderStatementQuery<TPoco>(this.QueryText("ORDER BY")); }
		}

		/// <summary>
		///     Gets the current identifier in the query.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

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
		///     Sets the cache.
		/// </summary>
		private void SetCache()
		{
			Cache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
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
	}
}