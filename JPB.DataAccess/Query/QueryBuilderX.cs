using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query
{
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