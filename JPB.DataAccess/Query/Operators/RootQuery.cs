#region

using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Selection;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///     Defines the root for every Query
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		/// Changes the ResultType property in a Fluid syntax
		/// </summary>
		/// <param name="resultType"></param>
		/// <returns></returns>
		public RootQuery ConfigType(Type resultType)
		{
			if (resultType == null)
			{
				throw new ArgumentNullException("resultType");
			}
			ContainerObject.ForType = resultType;
			return this;
		}

		/// <summary>
		/// Changes how the result is enumerated in a Fluid syntax
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public RootQuery ConfigEnumerationMode(EnumerationMode mode)
		{
			ContainerObject.EnumerationMode = mode;
			return this;
		}

		/// <summary>
		/// Changes the AllowParamterRenaming flag in a Fluid syntax
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public RootQuery ConfigAllowParamterRenaming(bool mode)
		{
			ContainerObject.AllowParamterRenaming = mode;
			return this;
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public DatabaseObjectSelector Select
		{
			get { return new DatabaseObjectSelector(this); }
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public CountElementsObjectSelector Count
		{
			get { return new CountElementsObjectSelector(this); }
		}

		public PrepaireUpdateQuery Update
		{
			get { return new PrepaireUpdateQuery(this); }
		}

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
					ContainerObject.AccessLayer.GetClassInfo(typeof(T)), argumentsForFactory);
			return new SelectQuery<T>(this.QueryCommand(cmd));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> SelectFactory<T>(params object[] argumentsForFactory)
		{
			return new DatabaseObjectSelector(this).Table<T>(argumentsForFactory);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Distinct<T>()
		{
			var cmd = DbAccessLayer.CreateSelect(ContainerObject.AccessLayer.GetClassInfo(typeof(T)), "DISTINCT");
			return new SelectQuery<T>(this.QueryText(cmd));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create an update statement that will check for the id of the obj
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public ConditionalEvalQuery<T> UpdateEntity<T>(T obj)
		{
			return new ConditionalEvalQuery<T>(new UpdateQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateUpdate(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj))));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create an update statement for the whole table based on the obj
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public UpdateQuery<T> UpdateStatement<T>(T obj)
		{
			return new UpdateQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateUpdateSimple(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		/// <summary>
		///     Adds a Delete - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public DeleteQuery<T> Delete<T>(T obj)
		{
			return new DeleteQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateDelete(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public DeleteQuery<T> Delete<T>()
		{
			return new DeleteQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateDelete(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)))));
		}
	}
}