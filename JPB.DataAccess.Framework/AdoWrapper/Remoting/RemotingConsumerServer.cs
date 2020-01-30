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
	public class RemotingConsumerServer
	{
		private readonly Func<DbAccessLayer> _accessLayerFactory;

		public RemotingConsumerServer(Func<DbAccessLayer> accessLayerFactory)
		{
			_accessLayerFactory = accessLayerFactory;
			ConnectionTuples = new ConcurrentDictionary<string, ConnectionTuple>();
			LockRoot = new object();
		}

		ConcurrentDictionary<string, ConnectionTuple> ConnectionTuples { get; set; }

		class ConnectionTuple
		{
			public string ConnectionId { get; set; }
			public string TransactionId { get; set; }
			public DbAccessLayer DbAccessLayer { get; set; }
		}

		public object LockRoot { get; }

		public string RegisterConnection()
		{
			var connectionId = GenerateUniqeId();
			var connectionTuple = new ConnectionTuple()
			{
				DbAccessLayer = _accessLayerFactory(),
				ConnectionId = connectionId
			};
			ConnectionTuples.TryAdd(connectionId, connectionTuple);
			connectionTuple.DbAccessLayer.Database.Connect();
			return connectionId;
		}

		public void CloseConnection(string connectionId)
		{
			if (ConnectionTuples.TryGetValue(connectionId, out var connection))
			{
				connection.DbAccessLayer.Database.CloseConnection();
				ConnectionTuples.TryRemove(connectionId, out _);
			}
		}

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

			connection.TransactionId = GenerateUniqeId();
			connection.DbAccessLayer.Database.ConnectionController.Transaction = connection.DbAccessLayer.Database
				.ConnectionController.Connection.BeginTransaction();
			return connection.TransactionId;
		}

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
			connection.DbAccessLayer.Database.TransactionRollback();
			return true;
		}

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

			connection.DbAccessLayer.Database.TransactionCommit();
			return true;
		}

		protected virtual RemotingStrategyExternal.RemotingDbCommandData Deserialize(string commandGraph)
		{
			using (var ms = new MemoryStream(Encoding.Default.GetBytes(commandGraph)))
			{
				return RemotingStrategyExternal.RemotingCommandSerializer.ReadObject(ms) as RemotingStrategyExternal.RemotingDbCommandData;
			}
		}

		private IDbCommand ToCommand(RemotingStrategyExternal.RemotingDbCommandData command,
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

			var command = Deserialize(commandGraph);

			var executeCommand = connection.DbAccessLayer.CommandProcessor.ExecuteCommand(connection.DbAccessLayer,
				ToCommand(command, connection));
			return executeCommand;
		}

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

			var command = Deserialize(commandGraph);
			return connection.DbAccessLayer.CommandProcessor.GetSkalar(connection.DbAccessLayer,
				ToCommand(command, connection), null);
		}

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

			var command = Deserialize(commandGraph);
			return connection.DbAccessLayer.CommandProcessor.ExecuteMARSCommand(connection.DbAccessLayer,
				ToCommand(command, connection), out recordsAffected);
		}

		public string GenerateUniqeId()
		{
			lock (ConnectionTuples)
			{
				return Guid.NewGuid().ToString();
			}
		}

		public bool HasOpenConnection()
		{
			return ConnectionTuples.Any();
		}
	}
}
