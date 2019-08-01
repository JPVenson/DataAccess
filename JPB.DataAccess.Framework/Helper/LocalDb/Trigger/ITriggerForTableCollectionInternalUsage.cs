namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	internal interface ITriggerForTableCollectionInternalUsage<TEntity>
			: ITriggerForTableCollection<TEntity>
	{
		ISequentialTriggerCollection<TEntity> For { get; }

		ISequentialTriggerCollection<TEntity> After { get; }

		ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; }
	}
}