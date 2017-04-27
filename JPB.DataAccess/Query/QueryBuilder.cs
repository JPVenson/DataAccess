#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	public abstract class QueryBuilderContainer : IQueryBuilder
	{
		internal QueryBuilderContainer(DbAccessLayer database, Type type)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");
			if (database == null)
				throw new ArgumentNullException("type", "Please use a valid Type");

			ContainerObject = new InternalContainerContainer(database, type);
		}

		internal QueryBuilderContainer(IQueryContainer database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");

			ContainerObject = database;
		}

		internal QueryBuilderContainer(IQueryBuilder database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");

			ContainerObject = database.ContainerObject;
		}

		internal QueryBuilderContainer(IQueryBuilder database, Type type)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");
			if (database == null)
				throw new ArgumentNullException("type", "Please use a valid Type");

			ContainerObject = database.ContainerObject;
			ContainerObject.ForType = type;
		}

		/// <summary>
		///     Creates a new Query
		/// </summary>
		/// <param name="database"></param>
		protected QueryBuilderContainer(DbAccessLayer database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid DbAccess Layer");

			ContainerObject = new InternalContainerContainer(database);
		}

		/// <summary>
		///     The interal value holder
		/// </summary>
		public IQueryContainer ContainerObject { get; protected set; }

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IEnumerable<E> ForResult<E>()
		{
			return new QueryEnumeratorEx<E>(this);
		}

		/// <summary>
		/// Runs the Query that does not expect to have an result
		/// </summary>
		public void ExecuteNonQuery()
		{
			var dbCommand = ContainerObject.Compile();
			ContainerObject.AccessLayer.ExecuteGenericCommand(dbCommand);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public abstract IQueryBuilder Clone();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<TPoco> GetEnumerator<TPoco>()
		{
			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator<TPoco>(ContainerObject);
			return new QueryLazyEnumerator<TPoco>(ContainerObject);
		}

		public Task<IEnumerable<TE>> ForAsyncResult<TE>()
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryBuilder" />
	public class QueryBuilderX : QueryBuilderContainer
	{
		/// <inheritdoc />
		public QueryBuilderX(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public QueryBuilderX(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public QueryBuilderX(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public QueryBuilderX(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public QueryBuilderX(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///     Appends the specified query Builder.
		/// </summary>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public QueryBuilderX Append(QueryBuilderX right)
		{
			if (right.ContainerObject == ContainerObject)
				return this;

			foreach (var part in right.ContainerObject.Parts)
				this.Add(part);
			return this;
		}

		/// <inheritdoc />
		public override IQueryBuilder Clone()
		{
			return new QueryBuilderX(ContainerObject.Clone());
		}
	}
}