using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using JetBrains.Annotations;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public abstract class RemotingStrategyExternal : RemotingStrategy
	{
		static RemotingStrategyExternal()
		{
			RemotingCommandSerializer = new DataContractSerializer(typeof(RemotingDbCommandData), new[]
			{
				typeof(RemotingDbParameterData),
				typeof(DBNull)
			});
		}

		/// <inheritdoc />
		public RemotingStrategyExternal(DbAccessType emulateDbType, DbConfig config) : base(emulateDbType, config)
		{
			ConnectionIds = new ConcurrentDictionary<RemotingDbConnection, string>();
			TransactionIds = new ConcurrentDictionary<RemoteDbTransaction, string>();

			Events.ConnectionOpened += Events_ConnectionOpened;
			Events.ConnectionClosed += Events_ConnectionClosed;

			Events.TransactionCreated += Events_TransactionCreated;
			Events.TransactionCommit += Events_TransactionCommit;
			Events.TransactionRollback += Events_TransactionRollback;
		}

		private string GetOrThrowConnectionId(RemotingDbConnection connection)
		{
			if (ConnectionIds.TryGetValue(connection, out var id))
			{
				return id;
			}

			throw new InvalidOperationException("Missing Connection Id");
		}

		private string TryGetTransactionId(RemoteDbTransaction connection)
		{
			if (connection == null)
			{
				return null;
			}

			if (TransactionIds.TryGetValue(connection, out var id))
			{
				return id;
			}

			return null;
		}

		private void Events_TransactionRollback(RemoteDbTransaction connection)
		{
			var id = GetOrThrowConnectionId(connection.Connection as RemotingDbConnection);
			var transaction = TransactionIds.GetOrAdd(connection, f => null);
			RollbackTransaction(id, transaction);
		}

		private void Events_TransactionCommit(RemoteDbTransaction connection)
		{
			var id = GetOrThrowConnectionId(connection.Connection as RemotingDbConnection);
			var transaction = TransactionIds.GetOrAdd(connection, f => null);
			CommitTransaction(id, transaction);
		}

		private void Events_TransactionCreated(RemoteDbTransaction connection)
		{
			TransactionIds.TryAdd(connection, RegisterTransaction(GetOrThrowConnectionId(connection.Connection as RemotingDbConnection)));
		}

		private void Events_ConnectionClosed(RemotingDbConnection connection)
		{
			var id = GetOrThrowConnectionId(connection);
			ConnectionIds.TryRemove(connection, out _);
			CloseConnection(id);
		}
		/// <summary>
		///		The list of all Connections and its ids
		/// </summary>
		public ConcurrentDictionary<RemotingDbConnection, string> ConnectionIds { get; set; }

		/// <summary>
		///		The list of all Transactions and its ids
		/// </summary>
		public ConcurrentDictionary<RemoteDbTransaction, string> TransactionIds { get; set; }

		private void Events_ConnectionOpened(RemotingDbConnection connection)
		{
			ConnectionIds.TryAdd(connection, RegisterConnection());
		}

		/// <summary>
		///		Should call an external provider and provide a new connection id
		/// </summary>
		/// <returns></returns>
		public abstract string RegisterConnection();

		/// <summary>
		///		Should close the connection
		/// </summary>
		/// <param name="connectionId"></param>
		public abstract void CloseConnection(string connectionId);

		/// <summary>
		///		Should register a new connection
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public abstract string RegisterTransaction(string connectionId);

		/// <summary>
		///		Should rollback a transaction
		/// </summary>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public abstract bool RollbackTransaction(string connectionId, string transactionId);

		/// <summary>
		///		Should commit a transaction
		/// </summary>
		/// <param name="connectionId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public abstract bool CommitTransaction(string connectionId, string transactionId);

		public abstract int ExecuteQuery(string commandGraph, string connectionId, [CanBeNull] string transactionId);
		public abstract object ExecuteScalar(string commandGraph, string connectionId, [CanBeNull] string transactionId);

		public abstract IEnumerable<IEnumerable<IDataRecord>> ExecuteCommand(string commandGraph, string connectionId,
			[CanBeNull] string transactionId, out int recordsAffected);

		internal static DataContractSerializer RemotingCommandSerializer;

		public class RemotingDbCommandData
		{
			public string CommandText { get; set; }
			public int CommandTimeout { get; set; }
			public CommandType CommandType { get; set; }
			public RemotingDbParameterData[] Parameters { get; set; }
		}

		public class RemotingDbParameterData
		{
			public DbType DbType { get; set; }
			public ParameterDirection Direction { get; set; }
			public bool IsNullable { get; set; }
			public string ParameterName { get; set; }
			public string SourceColumn { get; set; }
			public DataRowVersion SourceVersion { get; set; }
			public object Value { get; set; }
			public byte Precision { get; set; }
			public byte Scale { get; set; }
			public int Size { get; set; }
		}

		public string SerializeCommand(RemotingDbCommand command)
		{
			var data = new RemotingDbCommandData()
			{
				CommandText = command.CommandText,
				Parameters = command.Parameters.OfType<RemotingDbParameter>().Select(f => new RemotingDbParameterData()
				{
					ParameterName = f.ParameterName,
					Value = f.Value,
					DbType = f.DbType,
					Direction = f.Direction == 0 ? ParameterDirection.Input : f.Direction,
					IsNullable = f.IsNullable,
					Precision = f.Precision,
					Scale = f.Scale,
					Size = f.Size,
					SourceColumn = f.SourceColumn,
					SourceVersion = f.SourceVersion == 0 ? DataRowVersion.Default : f.SourceVersion
				}).ToArray(),
				CommandTimeout = command.CommandTimeout,
				CommandType = command.CommandType == 0 ? CommandType.Text : command.CommandType,
			};

			using (var textWriter = new MemoryStream())
			{
				RemotingCommandSerializer.WriteObject(textWriter, data);
				return Encoding.Default.GetString(textWriter.ToArray());
			}
		}

		public override IEnumerable<IEnumerable<IDataRecord>> EnumerateCommand(RemotingDbCommand command, CommandBehavior behavior, out int recordsAffected)
		{
			return ExecuteCommand(SerializeCommand(command),
				GetOrThrowConnectionId(command.Connection as RemotingDbConnection),
				TryGetTransactionId(command.Transaction as RemoteDbTransaction), out recordsAffected);
		}

		public override int ExecuteQuery(RemotingDbCommand command)
		{
			return ExecuteQuery(SerializeCommand(command),
				GetOrThrowConnectionId(command.Connection as RemotingDbConnection),
				TryGetTransactionId(command.Transaction as RemoteDbTransaction));
		}

		public override object ExecuteScalar(RemotingDbCommand command)
		{
			return ExecuteScalar(SerializeCommand(command),
				GetOrThrowConnectionId(command.Connection as RemotingDbConnection),
				TryGetTransactionId(command.Transaction as RemoteDbTransaction));
		}
	}
}