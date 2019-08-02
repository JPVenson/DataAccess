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

	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="IReplicationNode{TEntity}" />
	public class ReplicationNode<TEntity> : IReplicationNode<TEntity>
	{
		internal ReplicationNode(LocalDbRepository<TEntity> table, IReplicationNode<TEntity> duplication = null)
		{
			var table1 = table;
			var duplication1 = duplication;

			if (duplication != null)
			{
				For = new SequentialSequentialTriggerCollection<TEntity>(table1, duplication1.For);
				After = new SequentialSequentialTriggerCollection<TEntity>(table1, duplication1.After);
				InsteadOf = new TriggerInsteadtOfCollection<TEntity>(table1, duplication1.InsteadOf);
			}
			else
			{
				For = new SequentialSequentialTriggerCollection<TEntity>(table1);
				After = new SequentialSequentialTriggerCollection<TEntity>(table1);
				InsteadOf = new TriggerInsteadtOfCollection<TEntity>(table1);
			}
		}

		/// <summary>
		///     Will be invoked bevor each operation
		/// </summary>
		public virtual ISequentialTriggerCollection<TEntity> For { get; private set; }

		/// <summary>
		///     Will be invoked after the operation and all consistency checks when no exception was thrown
		/// </summary>
		public virtual ISequentialTriggerCollection<TEntity> After { get; private set; }

		/// <summary>
		///     Will be invoked after <c>For</c> and bevor <c>After</c> and replaces the internal Add/Remove/Update call.
		///     If you still want to Add/Remove/Update the Entity you have to call Add/Remove/Update again
		/// </summary>
		public virtual ITriggerInsteadtOfCollection<TEntity> InsteadOf { get; private set; }
	}
}