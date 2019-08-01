#region

using JPB.DataAccess.Framework.Contacts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbCheckConstraint{TEntity}" />
	public interface ILocalDbUniqueConstraint<in TEntity> : ILocalDbCheckConstraint<TEntity>
	{
		/// <summary>
		///     Adds the specified item to the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		void Add(TEntity item);

		/// <summary>
		///     Deletes the specified item from the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		void Delete(TEntity item);

		/// <summary>
		///     Updates the specified item in the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		void Update(TEntity item);
	}
}