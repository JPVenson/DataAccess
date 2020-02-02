using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Helper.LocalDb;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Wraps an External Connection
	/// </summary>
	public class RemotingDbConnection : IDbConnection
	{
		/// <summary>
		///		The Associated Strategy
		/// </summary>
		public RemotingStrategy Strategy { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strategy"></param>
		public RemotingDbConnection(RemotingStrategy strategy)
		{
			Strategy = strategy;
			ConnectionTimeout = TimeSpan.FromSeconds(100).Milliseconds;
			Strategy.Events.OnConnectionCreated(this);
		}
		
		/// <inheritdoc />
		public void Dispose()
		{
			if (State != ConnectionState.Closed)
			{
				Close();
			}
		}
		
		/// <inheritdoc />
		public IDbTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}
		
		/// <inheritdoc />
		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return new RemoteDbTransaction(this, il, Strategy);
		}
		
		/// <inheritdoc />
		public void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public void Close()
		{
			State = ConnectionState.Closed;
			Strategy.Events.OnConnectionClosed(this);
		}
		
		/// <inheritdoc />
		public IDbCommand CreateCommand()
		{
			return new RemotingDbCommand(Strategy)
			{
				Connection = this
			};
		}
		
		/// <inheritdoc />
		public void Open()
		{
			Strategy.Events.OnConnectionOpened(this);
			State = ConnectionState.Open;
		}
		
		/// <inheritdoc />
		public string ConnectionString { get; set; }
		/// <inheritdoc />
		public int ConnectionTimeout { get; }
		/// <inheritdoc />
		public string Database { get; }
		/// <inheritdoc />
		public ConnectionState State { get; set; }
	}
}