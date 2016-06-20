namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface ITriggerForTableCollection<TEntity> 
	{
		IReplicationNode<TEntity> NotForReplication { get; }
		IReplicationNode<TEntity> WithReplication { get; }
	}

	public interface ITriggerForTableCollectionInternalUsage<TEntity>
			: ITriggerForTableCollection<TEntity>
	{
		ISequentialTriggerCollection<TEntity> For { get; }

		ISequentialTriggerCollection<TEntity> After { get; }

		ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; }
	}
}