using System;
using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbUniqueConstraint{TEntity}" />
	public interface IUniqueConstrains<TEntity> : ICollection<ILocalDbUniqueConstraint<TEntity>>
	{
		/// <summary>
		/// Adds the specified Unique key generator.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="item">The item.</param>
		void Add<TValue>(string name, Func<TEntity, TValue> item);
		/// <summary>
		/// Enforces the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		void Enforce(TEntity item);
		/// <summary>
		/// Add this item to the Unique constraint.
		/// </summary>
		/// <param name="item">The item.</param>
		void ItemAdded(TEntity item);
		/// <summary>
		/// Remove this item from the Unique constraint
		/// </summary>
		/// <param name="item">The item.</param>
		void ItemRemoved(TEntity item);
		/// <summary>
		/// Update this item in the Unique constraint
		/// </summary>
		/// <param name="item">The item.</param>
		void ItemUpdated(TEntity item);

		/// <summary>
		/// Gets the initializer for this Constraint.
		/// It can be used to store the internal state
		/// </summary>
		/// <returns></returns>
		object GetInitializer();

		/// <summary>
		/// Gets the initializer for this Constraint.
		/// It can be used to restore the internal state
		/// </summary>
		/// <returns></returns>
		void SetInitializer(object initializerValue);
	}
}