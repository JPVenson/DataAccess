#region

using JPB.DataAccess.Framework.Contacts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IConstraintCollection<TEntity> : IConstraintCollectionInternal
	{
		/// <summary>
		///     Gets the check constraints.
		/// </summary>
		/// <value>
		///     The check.
		/// </value>
		ICheckConstraints<TEntity> Check { get; }

		/// <summary>
		///     Gets the default constraints.
		/// </summary>
		/// <value>
		///     The default.
		/// </value>
		IDefaultConstraints<TEntity> Default { get; }

		/// <summary>
		///     Gets the primary key constraints.
		/// </summary>
		/// <value>
		///     The primary key.
		/// </value>
		ILocalDbPrimaryKeyConstraint PrimaryKey { get; }

		/// <summary>
		///     Gets the unique constraints.
		/// </summary>
		/// <value>
		///     The unique.
		/// </value>
		IUniqueConstrains<TEntity> Unique { get; }
	}
}