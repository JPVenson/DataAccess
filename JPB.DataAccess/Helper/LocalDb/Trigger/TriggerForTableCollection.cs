namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerForTableCollection<TEntity> 
		: ITriggerForTableCollectionInternalUsage<TEntity> 
	{
		private readonly LocalDbRepository<TEntity> _table;

		internal TriggerForTableCollection(LocalDbRepository<TEntity> table)
		{
			_table = table;
			NotForReplication = new ReplicationNode<TEntity>(_table);
			WithReplication = new ReplicationNode<TEntity>(_table, NotForReplication);
		}

		/// <summary>
		/// Should the trigger also trigger when a XML set is loaded
		/// </summary>
		public IReplicationNode<TEntity> WithReplication { get; private set; }

		/// <summary>
		/// Should the trigger only trigger due to normal usage
		/// </summary>
		public IReplicationNode<TEntity> NotForReplication { get; private set; }

		public ISequentialTriggerCollection<TEntity> For
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.For;
				return NotForReplication.For;
			}
		}

		public virtual ISequentialTriggerCollection<TEntity> After
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.After;
				return NotForReplication.After;
			}
		}

		public virtual ITriggerInsteadtOfCollection<TEntity> InsteadOf
		{
			get
			{
				if (TriggerInsteadtOfCollection<TEntity>.AsInsteadtOf)
					return TriggerInsteadtOfCollection<TEntity>.Empty();
				
				if (_table.IsMigrating)
				{
					return WithReplication.InsteadOf;
				}
				return NotForReplication.InsteadOf;
			}
		}
	}
}