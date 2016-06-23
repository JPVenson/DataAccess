using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Orders;

namespace JPB.DataAccess.Query.Operators
{
	public class ElementProducer<TPoco> : QueryBuilderX, IElementProducer<TPoco>, IEnumerable<TPoco>
	{
		public ElementProducer(IQueryBuilder database) : base(database)
		{
			SetCache();
		}

		private void SetCache()
		{
			Cache = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
		}

		/// <summary>
		/// Easy access to the Cache for TPoco
		/// </summary>
		protected internal DbClassInfoCache Cache;

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IEnumerable<TPoco> ForResult()
		{
			return new QueryEnumeratorEx<TPoco>(this);
		}

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IDataPager<TPoco> ForPagedResult()
		{
			var command = this.ContainerObject.Compile();
			var pager = base.ContainerObject.AccessLayer.Database.CreatePager<TPoco>();
			pager.BaseQuery = command;
			return pager;
		}


		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IDataPager<TPoco> ForPagedResult(int page, int pageSize)
		{
			var command = this.ContainerObject.Compile();
			var pager = base.ContainerObject.AccessLayer.Database.CreatePager<TPoco>();
			pager.BaseQuery = command;
			pager.PageSize = pageSize;
			pager.CurrentPage = page;
			pager.LoadPage(this.ContainerObject.AccessLayer);
			return pager;
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///		does not emit any conditional statement
		///		should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState());
		}

		/// <summary>
		///    Adds an SQL ORDER BY Statement
		/// </summary>
		/// <returns></returns>
		public OrderStatementQuery<TPoco> Order()
		{
			return new OrderStatementQuery<TPoco>(this.QueryText("ORDER BY"));
		}

		public IEnumerator<TPoco> GetEnumerator()
		{
			return base.GetEnumerator<TPoco>();
		}
	}
}
