namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface ITriggerForTableCollection<TEntity>
	{
		/// <summary>
		/// Gets trigger Collections for the not for replication mode.
		/// </summary>
		/// <value>
		/// The not for replication.
		/// </value>
		IReplicationNode<TEntity> NotForReplication { get; }
		/// <summary>
		/// Gets trigger Collections for replication mode
		/// </summary>
		/// <value>
		/// The with replication.
		/// </value>
		IReplicationNode<TEntity> WithReplication { get; }
	}

	internal interface ITriggerForTableCollectionInternalUsage<TEntity>
			: ITriggerForTableCollection<TEntity>
	{
		ISequentialTriggerCollection<TEntity> For { get; }

		ISequentialTriggerCollection<TEntity> After { get; }

		ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; }
	}
}