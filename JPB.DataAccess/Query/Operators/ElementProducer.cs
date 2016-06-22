using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class ElementProducer<TPoco> : QueryBuilderX, IElementProducer<TPoco>
	{
		public ElementProducer(DbAccessLayer database, Type type) : base(database, type)
		{
			SetCache();
		}

		public ElementProducer(IQueryContainer database) : base(database)
		{
			SetCache();
		}

		public ElementProducer(IQueryBuilder database) : base(database)
		{
			SetCache();
		}

		public ElementProducer(IQueryBuilder database, Type type) : base(database, type)
		{
			SetCache();
		}

		public ElementProducer(DbAccessLayer database) : base(database)
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
		///     Adds a SQL WHERE statement
		///		does not emit any conditional statement
		///		should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("WHERE"));
		}
	}
}
