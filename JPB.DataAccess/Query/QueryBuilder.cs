#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
		public IEnumerable<E> ForResult<E>(bool async = true)
		{
			return new QueryEnumeratorEx<E>(this, async);
		}

		/// <summary>
		/// Runs the Query that does not expect to have an result
		/// </summary>
		public void ExecuteNonQuery()
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
		public IEnumerator<TPoco> GetEnumerator<TPoco>(bool async)
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
		[MustUseReturnValue]
		public QueryBuilderX Append(QueryBuilderX right)
		{
			if (right.ContainerObject == ContainerObject)
			{
				return this;
			}

			var builder = this;
			foreach (var part in right.ContainerObject.Parts)
			{
				builder = builder.Add(part);
			}
			return builder;
		}

		/// <inheritdoc />
		//public override IQueryBuilder Clone()
		//{
		//	return new QueryBuilderX(ContainerObject.Clone());
		//}

		[MustUseReturnValue]
		public override IQueryBuilder CloneWith<T>(T instance)
		{
			var t = GetType();

			var ctors = t.GetConstructors()
						 .FirstOrDefault(f =>
						 {
							 var para = f.GetParameters();
							 if (para.Length > 1)
							 {
								 return false;
							 }
							 var firstOrDefault = para.FirstOrDefault();
							 return firstOrDefault != null && firstOrDefault.ParameterType.IsAssignableFrom(typeof(T));
						 });

			if (ctors == null)
			{
				throw new NotImplementedException(string.Format("Framework error. The ctor for this type is not Implemented! {0}", typeof(T)));
			}

			return ctors.Invoke(new object[] { instance }) as IQueryBuilder;

			return Activator.CreateInstance(t, BindingFlags.Default, null, instance as IQueryBuilder) as IQueryBuilder;
		}
	}
}