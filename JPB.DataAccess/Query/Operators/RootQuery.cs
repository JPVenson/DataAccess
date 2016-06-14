using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Select<T>()
		{
			var cmd = ContainerObject
				.AccessLayer
				.CreateSelectQueryFactory(
					this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)));
			return new SelectQuery<T>(this.QueryCommand(cmd));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Select<T>(IQueryBuilder query)
		{
			return new SelectQuery<T>(query.QueryText("SELECT"));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public UpdateQuery<T> Update<T>(T obj)
		{
			return new UpdateQuery<T>(this
			.QueryCommand(
				this
				.ContainerObject
				.AccessLayer
				._CreateUpdate(this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		public RootQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public RootQuery(IQueryContainer database) : base(database)
		{
		}

		public RootQuery(IQueryBuilder database) : base(database)
		{
		}

		public RootQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public RootQuery(DbAccessLayer database) : base(database)
		{
		}
	}
}
