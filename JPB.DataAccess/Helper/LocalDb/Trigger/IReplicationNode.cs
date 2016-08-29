namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IReplicationNode<TEntity>
	{
		/// <summary>
		/// Gets the after trigger Collection.
		/// </summary>
		/// <value>
		/// The after.
		/// </value>
		ISequentialTriggerCollection<TEntity> After { get; }
		/// <summary>
		/// Gets for trigger Collection.
		/// </summary>
		/// <value>
		/// For.
		/// </value>
		ISequentialTriggerCollection<TEntity> For { get; }
		/// <summary>
		/// Gets the instead of trigger Collection.
		/// </summary>
		/// <value>
		/// The instead of.
		/// </value>
		ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; }
	}
}