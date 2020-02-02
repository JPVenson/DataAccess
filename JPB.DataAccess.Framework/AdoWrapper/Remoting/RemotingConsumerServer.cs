using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Can be used as an Endpoint on another system for the RemotingStrategyExternal
	/// </summary>
	public class RemotingConsumerServer
	{
		private readonly Func<DbAccessLayer> _accessLayerFactory;

		/// <summary>
		///		Should create a new DbAccessLayer for each connection that is requested
		/// </summary>
		/// <param name="accessLayerFactory"></param>
		public RemotingConsumerServer(Func<DbAccessLayer> accessLayerFactory)
		{
			_accessLayerFactory = accessLayerFactory;
			ConnectionTuples = new ConcurrentDictionary<string, ConnectionTuple>();
			LockRoot = new object();
		}

		ConcurrentDictionary<string, ConnectionTuple> ConnectionTuples { get; set; }

		/// <summary>
		///		Cancels all connections that did not receive instructions for the set amount of time
		/// </summary>
		/// <param name="age"></param>
		public void CancelAllOpenConnections(TimeSpan age)
		{
			var absolutAge = DateTime.Now - age;
			foreach (var keyValuePair in ConnectionTuples.ToArray())
			{
				if (keyValuePair.Value.LastOperationReceived < absolutAge)
				{
					CloseConnection(keyValuePair.Key);
				}
			}
		}

		class ConnectionTuple
		{
			public string ConnectionId { get; set; }
			public string TransactionId { get; set; }
			public DbAccessLayer DbAccessLayer { get; set; }
			public DateTime LastOperationReceived { get; set; }
		}

		private object LockRoot { get; }

		/// <summary>
		///		Creates a new Connection
		/// </summary>
		/// <returns></returns>
		public string RegisterConnection()
		{
			var connectionId = GenerateUniqueId();
			var connectionTuple = new ConnectionTuple()
			{
				DbAccessLayer = _accessLayerFactory(),
				ConnectionId = connectionId,
				LastOperationReceived = DateTime.Now
			};
			ConnectionTuples.TryAdd(connectionId, connectionTuple);
			connectionTuple.DbAccessLayer.Database.Connect();
			return connectionId;
		}

		/// <summary>
		///		Closes a connection
		/// </summary>
		/// <param name="connectionId"></param>
		public void CloseConnection(string connectionId)
		{
			if (ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				var connectionState = connection.DbAccessLayer.Database.ConnectionController.Connection?.State;
				if (connectionState != ConnectionState.Broken || connectionState == ConnectionState.Closed)
				{
					connection.DbAccessLayer.Database.ConnectionController.InstanceCounter = 0;
					connection.DbAccessLayer.Database.CloseConnection();
				}

				ConnectionTuples.TryRemove(connectionId, out _);
			}
		}

		/// <summary>
		///		Registers a new Transaction for a Connection or returns the current transaction
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public string RegisterTransaction(string connectionId)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to register a transaction for a connection that does not exist");
			}
			
			if (connection.TransactionId != null)
			{
				return connection.TransactionId;
			}

			connection.LastOperationReceived = DateTime.Now;
			connection.TransactionId = GenerateUniqueId();
			connection.DbAccessLayer.Database.ConnectionController.Transaction = connection.DbAccessLayer.Database
				.ConnectionController.Connection.BeginTransaction();
			return connection.TransactionId;
		}

		/// <summary>
		///		Rollbacks a Transaction
		/// </summary>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public bool RollbackTransaction(string connectionId, string transactionId)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does not exist");
			}

			if (connection.TransactionId == null)
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does not exist");
			}

			if (connection.DbAccessLayer.Database.ConnectionController.Transaction == null)
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does " +
				                                    "not exist or was resolved earlier");
			}
			connection.LastOperationReceived = DateTime.Now;
			connection.DbAccessLayer.Database.TransactionRollback();
			return true;
		}

		/// <summary>
		///		Commits a Transaction
		/// </summary>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public bool CommitTransaction(string connectionId, string transactionId)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does not exist");
			}

			if (connection.TransactionId == null)
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does not exist");
			}

			if (connection.DbAccessLayer.Database.ConnectionController.Transaction == null)
			{
				throw new InvalidOperationException("Tried to rollback a transaction for a connection that does " +
				                                    "not exist or was resolved earlier");
			}
			
			connection.LastOperationReceived = DateTime.Now;
			connection.DbAccessLayer.Database.TransactionCommit();
			return true;
		}

		/// <summary>
		///		Deserialises the Remoting Data
		/// </summary>
		/// <param name="commandGraph"></param>
		/// <returns></returns>
		protected virtual RemotingStrategyExternal.RemotingDbCommandData Deserialize(string commandGraph)
		{
			using (var ms = new MemoryStream(Encoding.Default.GetBytes(commandGraph)))
			{
				return RemotingStrategyExternal.RemotingCommandSerializer.ReadObject(ms) as RemotingStrategyExternal.RemotingDbCommandData;
			}
		}

		private static IDbCommand ToCommand(RemotingStrategyExternal.RemotingDbCommandData command,
			ConnectionTuple conData)
		{
			var dbCommand = conData.DbAccessLayer.Database.CreateCommand(command.CommandText);
			dbCommand.CommandType = command.CommandType;
			dbCommand.CommandTimeout = command.CommandTimeout == 0 ? 30 : command.CommandTimeout;
			foreach (var remotingDbParameterData in command.Parameters)
			{
				var dbDataParameter = dbCommand.CreateParameter();
				dbCommand.Parameters.Add(dbDataParameter);
				dbDataParameter.Precision = remotingDbParameterData.Precision;
				dbDataParameter.Scale = remotingDbParameterData.Scale;
				dbDataParameter.DbType = remotingDbParameterData.DbType;
				dbDataParameter.Direction = remotingDbParameterData.Direction;
				dbDataParameter.ParameterName = remotingDbParameterData.ParameterName;
				dbDataParameter.SourceVersion = remotingDbParameterData.SourceVersion;
				dbDataParameter.Value = remotingDbParameterData.Value;
				dbDataParameter.Size = remotingDbParameterData.Size;
			}

			return dbCommand;
		}

		/// <summary>
		///		Execute the Command on the given connection
		/// </summary>
		/// <param name="commandGraph"></param>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public int ExecuteQuery(string commandGraph, string connectionId, string transactionId)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to execute on a connection that does not exist");
			}

			if (connection.DbAccessLayer.Database.ConnectionController.Connection?.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Tried to execute on a connection that is not ready to accept commands");
			}
			connection.LastOperationReceived = DateTime.Now;

			var command = Deserialize(commandGraph);

			var executeCommand = connection.DbAccessLayer.CommandProcessor.ExecuteCommand(connection.DbAccessLayer,
				ToCommand(command, connection));
			return executeCommand;
		}
		
		/// <summary>
		///		Execute the Command on the given connection
		/// </summary>
		/// <param name="commandGraph"></param>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public object ExecuteScalar(string commandGraph, string connectionId, string transactionId)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to execute on a connection that does not exist");
			}

			if (connection.DbAccessLayer.Database.ConnectionController.Connection?.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Tried to execute on a connection that is not ready to accept commands");
			}
			connection.LastOperationReceived = DateTime.Now;

			var command = Deserialize(commandGraph);
			return connection.DbAccessLayer.CommandProcessor.GetSkalar(connection.DbAccessLayer,
				ToCommand(command, connection), null);
		}
		
		/// <summary>
		///		Execute the Command on the given connection
		/// </summary>
		/// <param name="commandGraph"></param>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public IEnumerable<IEnumerable<IDataRecord>> EnumerateCommand(string commandGraph, string connectionId, string transactionId, out int recordsAffected)
		{
			if (!ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				throw new InvalidOperationException("Tried to execute on a connection that does not exist");
			}

			if (connection.DbAccessLayer.Database.ConnectionController.Connection?.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Tried to execute on a connection that is not ready to accept commands");
			}
			connection.LastOperationReceived = DateTime.Now;

			var command = Deserialize(commandGraph);
			return connection.DbAccessLayer.CommandProcessor.ExecuteMARSCommand(connection.DbAccessLayer,
				ToCommand(command, connection), out recordsAffected);
		}

		/// <summary>
		///		Creates a new ID
		/// </summary>
		/// <returns></returns>
		public string GenerateUniqueId()
		{
			lock (ConnectionTuples)
			{
				return Guid.NewGuid().ToString();
			}
		}

		/// <summary>
		///		Returns true if there are any open connections
		/// </summary>
		/// <returns></returns>
		public bool HasOpenConnection()
		{
			return ConnectionTuples.Any();
		}
	}
}
