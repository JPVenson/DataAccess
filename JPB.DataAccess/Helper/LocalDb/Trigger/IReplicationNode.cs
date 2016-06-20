namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface IReplicationNode<TEntity>
	{
		ISequentialTriggerCollection<TEntity> After { get; }
		ISequentialTriggerCollection<TEntity> For { get; }
		ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; }
	}
}