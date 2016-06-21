namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	//public class ReplicationNode
	//{
	//	private readonly LocalDbReposetoryBase _table;
	//	private readonly ReplicationNode _duplication;

	//	internal ReplicationNode(LocalDbReposetoryBase table, ReplicationNode duplication = null)
	//	{
	//		_table = table;
	//		_duplication = duplication;

	//		if (duplication != null)
	//		{
	//			For = new TriggerForCollection(_table, duplication.For);
	//			After = new TriggerAfterCollection(_table, duplication.After);
	//		}
	//		else
	//		{
	//			For = new TriggerForCollection(_table);
	//			After = new TriggerAfterCollection(_table);
	//		}
	//		InsteadOf = new TriggerInsteadtOfCollection(_table);
	//	}

	//	/// <summary>
	//	/// Will be invoked bevor each operation
	//	/// </summary>
	//	public virtual ITriggerForCollection<ForActionCancelToken, object> For { get; private set; }

	//	/// <summary>
	//	/// Will be invoked after the operation and all consistency checks when no exception was thrown
	//	/// </summary>
	//	public virtual ITriggerAfterCollection<ForActionCancelToken, object> After { get; private set; }

	//	/// <summary>
	//	/// Will be invoked after <c>For</c> and bevor <c>After</c> and replaces the internal Add/Remove/Update call. 
	//	/// If you still want to Add/Remove/Update the Entity you have to call Add/Remove/Update again
	//	/// </summary>
	//	public virtual ITriggerInsteadtOfCollection<InsteadtOfActionToken, object> InsteadOf { get; private set; }

	//	internal static ReplicationNode Empty(LocalDbReposetoryBase table)
	//	{
	//		return new ReplicationNode(table);
	//	}
	//}

	public class ReplicationNode<TEntity> : IReplicationNode<TEntity>
	{
		private readonly LocalDbRepository<TEntity> _table;
		private readonly IReplicationNode<TEntity> _duplication;

		internal ReplicationNode(LocalDbRepository<TEntity> table, IReplicationNode<TEntity> duplication = null)
		{
			_table = table;
			_duplication = duplication;

			if (duplication != null)
			{
				For = new SequentialSequentialTriggerCollection<TEntity>(_table, _duplication.For);
				After = new SequentialSequentialTriggerCollection<TEntity>(_table, _duplication.After);
				InsteadOf = new TriggerInsteadtOfCollection<TEntity>(_table, _duplication.InsteadOf);
			}
			else
			{
				For = new SequentialSequentialTriggerCollection<TEntity>(_table);
				After = new SequentialSequentialTriggerCollection<TEntity>(_table);
				InsteadOf = new TriggerInsteadtOfCollection<TEntity>(_table);
			}
		}

		/// <summary>
		/// Will be invoked bevor each operation
		/// </summary>
		public virtual ISequentialTriggerCollection<TEntity> For { get; private set; }

		/// <summary>
		/// Will be invoked after the operation and all consistency checks when no exception was thrown
		/// </summary>
		public virtual ISequentialTriggerCollection<TEntity> After { get; private set; }

		/// <summary>
		/// Will be invoked after <c>For</c> and bevor <c>After</c> and replaces the internal Add/Remove/Update call. 
		/// If you still want to Add/Remove/Update the Entity you have to call Add/Remove/Update again
		/// </summary>
		public virtual ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; private set; }
	}
}