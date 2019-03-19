#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

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
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}
			if (database == null)
			{
				throw new ArgumentNullException(nameof(type), "Please use a valid Type");
			}
		}

		internal QueryBuilderContainer(IQueryContainer database)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}

			ContainerObject = database.Clone();
		}

		internal QueryBuilderContainer(IQueryBuilder database) : this(database.ContainerObject)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}
		}

		internal QueryBuilderContainer(IQueryBuilder database, Type type) : this(database.ContainerObject)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}
			if (database == null)
			{
				throw new ArgumentNullException(nameof(type), "Please use a valid Type");
			}

			ContainerObject.ForType = type;
		}

		/// <inheritdoc />
		protected QueryBuilderContainer(DbAccessLayer database) : this(new InternalContainerContainer(database))
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid DbAccess Layer");
			}
		}

		/// <inheritdoc />
		public IQueryBuilder Add(IQueryPart queryPart)
		{
			var target = CloneWith(this);
			target.ContainerObject.Add(queryPart);
			return target;
		}

		/// <summary>
		/// <see cref="IQueryContainer.Interceptors"/>
		/// </summary>
		public List<IQueryCommandInterceptor> Interceptors
		{
			get
			{
				return ContainerObject.Interceptors;
			}
		}

		/// <inheritdoc />
		public IQueryContainer ContainerObject { get; protected set; }

		/// <inheritdoc />
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
			foreach (var queryCommandInterceptor in Interceptors)
			{
				dbCommand = queryCommandInterceptor.NonQueryExecuting(dbCommand);

				if (dbCommand == null)
				{
					throw new InvalidOperationException($"The Command interceptor: '{queryCommandInterceptor}' has returned null");
				}
			}
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
			var enumerationMode = ContainerObject.PostProcessors.Any()
				? EnumerationMode.FullOnLoad
				: ContainerObject.EnumerationMode;

			if (enumerationMode == EnumerationMode.FullOnLoad)
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