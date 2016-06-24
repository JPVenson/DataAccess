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
using JPB.DataAccess.Query.Operators.Selection;

namespace JPB.DataAccess.Query.Operators
{
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Execute<T>(params object[] argumentsForFactory)
		{
			var cmd = ContainerObject
				.AccessLayer
				.CreateSelectQueryFactory(
					this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), argumentsForFactory);
			return new SelectQuery<T>(this.QueryCommand(cmd));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public DatabaseObjectSelector Select()
		{
			return new DatabaseObjectSelector(this);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Select<T>(params object[] argumentsForFactory)
		{
			return new DatabaseObjectSelector(this).Table<T>();
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Distinct<T>()
		{
			var cmd = DbAccessLayer.CreateSelect(this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), "DISTINCT");
			return new SelectQuery<T>(this.QueryText(cmd));
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
				DbAccessLayer
				.CreateUpdate(this
				.ContainerObject
				.AccessLayer.Database, this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
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
