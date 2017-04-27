#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbCheckConstraint{TEntity}" />
	public interface ICheckConstraints<TEntity> : ICollection<ILocalDbCheckConstraint<TEntity>>
	{
		/// <summary>
		///     Adds the specified Check Constraint.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="item">The item.</param>
		void Add(string name, Func<TEntity, bool> item);

		/// <summary>
		///     Enforces all Constraints on the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		void Enforce(TEntity item);
	}
}