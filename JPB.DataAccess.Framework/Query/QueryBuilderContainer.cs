#region

using System;
using System.Collections.Generic;
using System.Linq;
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
		/// <summary>
		///		Copy Constructor
		/// </summary>
		/// <param name="database"></param>
		internal QueryBuilderContainer(IQueryContainer database)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}

			ContainerObject = database.Clone();
		}

		/// <summary>
		///		Copy Constructor
		/// </summary>
		/// <param name="database"></param>
		internal QueryBuilderContainer(IQueryBuilder database) : this(database.ContainerObject)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database), "Please use a valid Database");
			}
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
			var target = new QueryBuilderX(this);
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
		
		/// <summary>
		/// Runs the Query that does not expect to have an result
		/// </summary>
		public virtual void ExecuteNonQuery()
		{
			var query = ContainerObject.Compile(out var columns);
			var dbCommand 
				= ContainerObject.AccessLayer.Database.CreateCommandWithParameterValues(query.Query, query.Parameters.ToArray());
			
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
		public override string ToString()
		{
			return ContainerObject.Compile(out var columns).Query;
		}
	}
}