using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Helper.LocalDb;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public class RemotingDbConnection : IDbConnection
	{
		public RemotingStrategy Strategy { get; }

		public RemotingDbConnection(RemotingStrategy strategy)
		{
			Strategy = strategy;
			ConnectionTimeout = TimeSpan.FromSeconds(100).Milliseconds;
			Strategy.Events.OnConnectionCreated(this);
		}

		public void Dispose()
		{
			if (State != ConnectionState.Closed)
			{
				Close();
			}
		}

		public IDbTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return new RemoteDbTransaction(this, il, Strategy);
		}

		public void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public void Close()
		{
			State = ConnectionState.Closed;
			Strategy.Events.OnConnectionClosed(this);
		}

		public IDbCommand CreateCommand()
		{
			return new RemotingDbCommand(Strategy)
			{
				Connection = this
			};
		}

		public void Open()
		{
			Strategy.Events.OnConnectionOpened(this);
			State = ConnectionState.Open;
		}

		public string ConnectionString { get; set; }
		public int ConnectionTimeout { get; }
		public string Database { get; }
		public ConnectionState State { get; set; }
	}
}