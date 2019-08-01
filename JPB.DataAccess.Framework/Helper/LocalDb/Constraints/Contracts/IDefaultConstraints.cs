#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JPB.DataAccess.Framework.DbInfoConfig;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbDefaultConstraint{TEntity}" />
	public interface IDefaultConstraints<TEntity> : ICollection<ILocalDbDefaultConstraint<TEntity>>
	{
		/// <summary>
		///     Adds the specified configuration.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="config">The configuration.</param>
		/// <param name="name">The name.</param>
		/// <param name="generateValue">The generate value.</param>
		/// <param name="column">The column.</param>
		void Add<TValue>(DbConfig config, string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column);

		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="generateValue">The generate value.</param>
		/// <param name="column">The column.</param>
		void Add<TValue>(string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column);

		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="setter">The setter.</param>
		void Add<TValue>(string name, TValue value, Action<TEntity, TValue> setter);

		/// <summary>
		///     Enforces all Contrains on the element
		/// </summary>
		/// <param name="elementToAdd">The element to add.</param>
		void Enforce(TEntity elementToAdd);
	}
}