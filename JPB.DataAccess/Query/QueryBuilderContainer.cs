#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	/// Base class for each QueryPart
	/// </summary>
	public abstract class QueryBuilderContainer : IQueryBuilder
	{
		internal QueryBuilderContainer(DbAccessLayer database, Type type) : this(new InternalContainerContainer(database, type))
		{
			if (database == null)
			{
				throw new ArgumentNullException("database", "Please use a valid Database");
			}
			if (database == null)
			{
				throw new ArgumentNullException("type", "Please use a valid Type");
			}
		}

		internal QueryBuilderContainer(IQueryContainer database)
		{
			if (database == null)
			{
				throw new ArgumentNullException("database", "Please use a valid Database");
			}

			ContainerObject = database.Clone();
		}

		internal QueryBuilderContainer(IQueryBuilder database) : this(database.ContainerObject.Clone())
		{
			if (database == null)
			{
				throw new ArgumentNullException("database", "Please use a valid Database");
			}
		}

		internal QueryBuilderContainer(IQueryBuilder database, Type type) : this(database.ContainerObject.Clone())
		{
			if (database == null)
			{
				throw new ArgumentNullException("database", "Please use a valid Database");
			}
			if (database == null)
			{
				throw new ArgumentNullException("type", "Please use a valid Type");
			}

			ContainerObject.ForType = type;
		}

		/// <summary>
		///     Creates a new Query
		/// </summary>
		/// <param name="database"></param>
		protected QueryBuilderContainer(DbAccessLayer database) : this(new InternalContainerContainer(database))
		{
			if (database == null)
			{
				throw new ArgumentNullException("database", "Please use a valid DbAccess Layer");
			}
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
		public virtual IEnumerable<E> ForResult<E>(bool async = true)
		{
			return new QueryEnumeratorEx<E>(this, async);
		}

		/// <summary>
		/// Runs the Query that does not expect to have an result
		/// </summary>
		public virtual void ExecuteNonQuery()
		{
			var dbCommand = ContainerObject.Compile();
			ContainerObject.AccessLayer.ExecuteGenericCommand(dbCommand);
		}

		/// <inheritdoc />
		public IEnumerator<TPoco> GetEnumerator<TPoco>()
		{
			return GetEnumerator<TPoco>(true);
		}

		/// <inheritdoc />
		public virtual IEnumerator<TPoco> GetEnumerator<TPoco>(bool async)
		{
			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
			{
				return new QueryEagerEnumerator<TPoco>(ContainerObject, async);
			}
			return new QueryLazyEnumerator<TPoco>(ContainerObject, async);
		}

		/// <inheritdoc />
		public abstract IQueryBuilder CloneWith<T>(T instance) where T : IQueryBuilder;

		/// <inheritdoc />
		public override string ToString()
		{
			return ContainerObject.CompileFlat().Item1;
		}
	}
}