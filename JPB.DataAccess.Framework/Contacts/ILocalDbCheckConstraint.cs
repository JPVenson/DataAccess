namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Creates a new Strong Typed Constraint
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbConstraint" />
	public interface ILocalDbCheckConstraint<in TEntity> : ILocalDbConstraint
	{
		/// <summary>
		///     The function that checks if the certain constraint is fulfilled
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if success false if failed</returns>
		bool CheckConstraint(TEntity item);
	}
}