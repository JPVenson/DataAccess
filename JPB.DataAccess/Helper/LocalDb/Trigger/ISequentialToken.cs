namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface ISequentialToken<out TEntity>
	{
		/// <summary>
		///     Gets a value indicating whether this <see cref="ISequentialToken{TEntity}" /> is canceled.
		/// </summary>
		/// <value>
		///     <c>true</c> if canceled; otherwise, <c>false</c>.
		/// </value>
		bool Canceled { get; }

		/// <summary>
		///     Gets the item.
		/// </summary>
		/// <value>
		///     The item.
		/// </value>
		TEntity Item { get; }

		/// <summary>
		///     Gets the reason.
		/// </summary>
		/// <value>
		///     The reason.
		/// </value>
		string Reason { get; }

		/// <summary>
		///     Cancels the specified reason.
		/// </summary>
		/// <param name="reason">The reason.</param>
		void Cancel(string reason);
	}
}