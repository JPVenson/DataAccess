namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Contains all events for the External Remote strategy
	/// </summary>
	public class RemotingStrategyEvents
	{
		public RemotingStrategyEvents()
		{
			
		}
		
		/// <summary>
		///		Will be raised when a new Parameter is Created
		/// </summary>
		public event ParameterEvent ParameterCreated;


		/// <summary>
		///		Will be raised when a command is cancled
		/// </summary>
		public event CommandEvent CommandCancel;
		/// <summary>
		///		Will be raised when a command should be prepared
		/// </summary>
		public event CommandEvent CommandPrepare;


		/// <summary>
		///		Will be raised when a Transaction is created
		/// </summary>
		public event TransactionEvent TransactionCreated;
		/// <summary>
		///		Will be raised when a Transaction should be commited
		/// </summary>
		public event TransactionEvent TransactionCommit;
		/// <summary>
		///		Will be raised when a Transaction should be rolled back
		/// </summary>
		public event TransactionEvent TransactionRollback;


		/// <summary>
		///		Will be raised when a Connection is created
		/// </summary>
		public event ConnectionEvent ConnectionCreated;
		/// <summary>
		///		Will be raised when a Connection should be opened
		/// </summary>
		public event ConnectionEvent ConnectionOpened;
		/// <summary>
		///		Will be raised when a Connection should be closed
		/// </summary>
		public event ConnectionEvent ConnectionClosed;

		internal virtual void OnConnectionOpened(RemotingDbConnection connection)
		{
			ConnectionOpened?.Invoke(connection);
		}

		internal virtual void OnConnectionClosed(RemotingDbConnection connection)
		{
			ConnectionClosed?.Invoke(connection);
		}

		internal virtual void OnConnectionCreated(RemotingDbConnection connection)
		{
			ConnectionCreated?.Invoke(connection);
		}

		internal virtual void OnTransactionRollback(RemoteDbTransaction connection)
		{
			TransactionRollback?.Invoke(connection);
		}

		internal virtual void OnTransactionCommit(RemoteDbTransaction connection)
		{
			TransactionCommit?.Invoke(connection);
		}

		internal virtual void OnTransactionCreated(RemoteDbTransaction connection)
		{
			TransactionCreated?.Invoke(connection);
		}

		internal virtual void OnCommandPrepare(RemotingDbCommand command)
		{
			CommandPrepare?.Invoke(command);
		}

		internal virtual void OnCommandCancel(RemotingDbCommand command)
		{
			CommandCancel?.Invoke(command);
		}

		internal virtual void OnParameterCreated(RemotingDbParameter connection)
		{
			ParameterCreated?.Invoke(connection);
		}
	}
}