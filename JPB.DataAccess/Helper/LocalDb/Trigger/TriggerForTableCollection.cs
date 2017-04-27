namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.ITriggerForTableCollectionInternalUsage{TEntity}" />
	public class TriggerForTableCollection<TEntity>
		: ITriggerForTableCollectionInternalUsage<TEntity>
	{
		/// <summary>
		///     The table
		/// </summary>
		private readonly LocalDbRepository<TEntity> _table;

		/// <summary>
		///     Initializes a new instance of the <see cref="TriggerForTableCollection{TEntity}" /> class.
		/// </summary>
		/// <param name="table">The table.</param>
		internal TriggerForTableCollection(LocalDbRepository<TEntity> table)
		{
			_table = table;
			NotForReplication = new ReplicationNode<TEntity>(_table);
			WithReplication = new ReplicationNode<TEntity>(_table, NotForReplication);
		}

		/// <summary>
		///     Should the trigger also trigger when a XML set is loaded
		/// </summary>
		/// <value>
		///     The with replication.
		/// </value>
		public IReplicationNode<TEntity> WithReplication { get; private set; }

		/// <summary>
		///     Should the trigger only trigger due to normal usage
		/// </summary>
		/// <value>
		///     The not for replication.
		/// </value>
		public IReplicationNode<TEntity> NotForReplication { get; private set; }

		/// <summary>
		///     Gets for.
		/// </summary>
		/// <value>
		///     For.
		/// </value>
		public ISequentialTriggerCollection<TEntity> For
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.For;
				return NotForReplication.For;
			}
		}

		/// <summary>
		///     Gets the after.
		/// </summary>
		/// <value>
		///     The after.
		/// </value>
		public virtual ISequentialTriggerCollection<TEntity> After
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.After;
				return NotForReplication.After;
			}
		}

		/// <summary>
		///     Gets the instead of collection.
		/// </summary>
		/// <value>
		///     The instead of.
		/// </value>
		public virtual ITriggerInsteadtOfCollection<TEntity> InsteadOf
		{
			get
			{
				if (TriggerInsteadtOfCollection<TEntity>.AsInsteadtOf)
					return TriggerInsteadtOfCollection<TEntity>.Empty();

				if (_table.IsMigrating)
					return WithReplication.InsteadOf;
				return NotForReplication.InsteadOf;
			}
		}
	}
}