namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerForTableCollection
	{
		private readonly LocalDbReposetoryBase _table;

		internal TriggerForTableCollection(LocalDbReposetoryBase table)
		{
			_table = table;
			NotForReplication = new ReplicationNode(_table);
			WithReplication = new ReplicationNode(_table, NotForReplication);
		}

		/// <summary>
		/// Should the trigger also trigger when a XML set is loaded
		/// </summary>
		public ReplicationNode WithReplication { get; private set; }

		/// <summary>
		/// Should the trigger only trigger due to normal usage
		/// </summary>
		public ReplicationNode NotForReplication { get; private set; }

		internal TriggerForCollection For
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.For;
				return NotForReplication.For;
			}
		}

		internal TriggerAfterCollection After
		{
			get
			{
				if (_table.IsMigrating)
					return WithReplication.After;
				return NotForReplication.After;
			}
		}

		internal TriggerInsteadtOfCollection InsteadOf
		{
			get
			{
				if (TriggerInsteadtOfCollection.AsInsteadtOf)
					return TriggerInsteadtOfCollection.Empty();

				TriggerInsteadtOfCollection target;
				if (_table.IsMigrating)
				{
					target = WithReplication.InsteadOf;
				}
				else
				{
					target = NotForReplication.InsteadOf;
				}
				return target;
			}
		}
	}
}