namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IInsteadtOfActionToken<TEntity>
	{
		/// <summary>
		///     Gets the item.
		/// </summary>
		/// <value>
		///     The item.
		/// </value>
		TEntity Item { get; }

		/// <summary>
		///     Gets the table.
		/// </summary>
		/// <value>
		///     The table.
		/// </value>
		LocalDbRepository<TEntity> Table { get; }
	}
}