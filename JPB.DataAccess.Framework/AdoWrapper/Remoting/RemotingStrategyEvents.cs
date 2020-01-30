namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public class RemotingStrategyEvents
	{
		public RemotingStrategyEvents()
		{
			
		}
		
		public event ParameterEvent ParameterCreated;

		public event CommandEvent CommandCancel;
		public event CommandEvent CommandPrepare;

		public event TransactionEvent TransactionCreated;
		public event TransactionEvent TransactionCommit;
		public event TransactionEvent TransactionRollback;

		public event ConnectionEvent ConnectionCreated;
		public event ConnectionEvent ConnectionOpened;
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