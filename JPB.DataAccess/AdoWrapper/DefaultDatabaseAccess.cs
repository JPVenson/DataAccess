#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.IDatabase" />
	[DebuggerDisplay("OpenRuns {TransactionController.InstanceCounter}, IsConnectionOpen {_conn2 != null ? _conn2.State.ToString() : \"Closed\"}, IsTransactionOpen {TransactionController.Transaction != null}")]
	public sealed partial class DefaultDatabaseAccess : IDatabase
	{
		/// <summary>
		/// ctor
		/// </summary>
		public DefaultDatabaseAccess(IConnectionController connectionController)
		{
			ConnectionController = connectionController;
		}

		private IDatabaseStrategy _strategy;

		/// <summary>
		/// Controlls the current Transaction Behavior
		/// </summary>
		public IConnectionController ConnectionController { get; private set; }


		/// <summary>
		///     If enabled each query will be decompiled and the LastInsertedQuery property will be set
		/// </summary>
		public bool Debugger { get; set; }

		/// <summary>
		///     Finalizes an instance of the <see cref="DefaultDatabaseAccess" /> class.
		/// </summary>
		~DefaultDatabaseAccess()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ConnectionController.Connection != null)
				{
					while (ConnectionController.Transaction != null)
					{
						ConnectionController.Transaction.Rollback();
						ConnectionController.Transaction = null;
					}
					ConnectionController.Connection.Close();
					ConnectionController.Connection.Dispose();
					ConnectionController.Connection = null;
				}
			}
		}

		/// <summary>
		///     Creates the specified strategy.
		/// </summary>
		/// <param name="strategy">The strategy.</param>
		/// <returns></returns>
		public static DefaultDatabaseAccess Create(IDatabaseStrategy strategy)
		{
			if (null == strategy)
			{
				return null;
			}

			var db = new DefaultDatabaseAccess(new InstanceConnectionController());
			db.Attach(strategy);
			return db;
		}

		private int DoExecuteNonQuery(string strSql, params object[] param)
		{
			return Run(d =>
			{
				var counter = 0;
				using (
				var cmd = _strategy.CreateCommand(strSql, ConnectionController.Connection,
				param.Select(s => CreateParameter(counter++.ToString(), s)).ToArray()))
				{
					if (ConnectionController.Transaction != null)
					{
						cmd.Transaction = ConnectionController.Transaction;
					}
					AttachQueryDebugger(cmd);

					return cmd.ExecuteNonQuery();
				}
			});
		}

		private IDataReader DoGetDataReader(string strSql)
		{
			return Run(d =>
			{
				using (var cmd = _strategy.CreateCommand(strSql, ConnectionController.Connection))
				{
					if (ConnectionController.Transaction != null)
					{
						cmd.Transaction = ConnectionController.Transaction;
					}
					AttachQueryDebugger(cmd);
					return cmd.ExecuteReader();
				}
			});

		}

		private object DoGetSkalar(string strSql)
		{
			return Run(d =>
			{
				using (var cmd = _strategy.CreateCommand(strSql, ConnectionController.Connection))
				{
					if (ConnectionController.Transaction != null)
					{
						cmd.Transaction = ConnectionController.Transaction;
					}
					AttachQueryDebugger(cmd);
					return cmd.ExecuteScalar();
				}
			});

		}

		#region IDatabase Members

		/// <summary>
		///     Get database specific converter Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		public IWrapperDataPager<T, TE> CreatePager<T, TE>()
		{
			return _strategy.CreateConverterPager<T, TE>();
		}

		//private QueryDebugger CreateQueryDebuggerAuto(IDbCommand cmd)
		//{
		//    if (Debugger)
		//    {
		//        return new QueryDebugger(cmd, this);
		//    }
		//    return null;
		//}

		/// <summary>
		///     The Hock point for Adding the QueryDebugger to the Database
		/// </summary>
		/// <param name="cmd"></param>
		public void AttachQueryDebugger(IDbCommand cmd)
		{
			if (Debugger)
			{
				LastExecutedQuery = new QueryDebugger(cmd, this);
			}
		}

		/// <summary>
		///     Get the last Executed QueryCommand wrapped by a Debugger
		/// </summary>
		public QueryDebugger LastExecutedQuery { get; private set; }

		/// <summary>
		///     Get Database specific Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDataPager<T> CreatePager<T>()
		{
			return _strategy.CreatePager<T>();
		}

		/// <summary>
		///     Required
		///     Is used to attach a Strategy that handles certain kinds of Databases
		/// </summary>
		public void Attach(IDatabaseStrategy strategy)
		{
			_strategy = strategy;
			CloseConnection();
		}

		/// <summary>
		///     Defines the Target database we are conneting to
		/// </summary>
		public DbAccessType TargetDatabase
		{
			get
			{
				if (_strategy == null)
				{
					return DbAccessType.Unknown;
				}
				return _strategy.SourceDatabase;
			}
		}

		/// <summary>
		///     Is this Instance ready to be used
		/// </summary>
		public bool IsAttached
		{
			get { return _strategy != null; }
		}

		/// <summary>
		///     Get the Current Connection string
		/// </summary>
		public string ConnectionString
		{
			get { return null == _strategy ? null : _strategy.ConnectionString; }
		}

		/// <summary>
		///     If local instance get the file
		/// </summary>
		public string DatabaseFile
		{
			get { return null == _strategy ? null : _strategy.DatabaseFile; }
		}

		/// <summary>
		///     Get the Database name that we are connected to
		/// </summary>
		public string DatabaseName
		{
			get { return Run(e => ConnectionController.Connection.Database); }
		}

		/// <summary>
		///     Get the Server we are Connected to
		/// </summary>
		public string ServerName
		{
			get { return null == _strategy ? string.Empty : _strategy.ServerName; }
		}

		/// <summary>
		///     Required
		///     Is used to create an new Connection based on the Strategy and
		///     keep it
		/// </summary>
		/// <returns></returns>
		public IDbConnection GetConnection()
		{
			return ConnectionController.Connection ?? (ConnectionController.Connection = _strategy.CreateConnection());
		}

		/// <summary>
		///     Gets the default transaction level.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public IsolationLevel GetDefaultTransactionLevel()
		{
			switch (TargetDatabase)
			{
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
					return IsolationLevel.Unspecified;
				case DbAccessType.MsSql:
				case DbAccessType.MySql:
					return IsolationLevel.ReadUncommitted;
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
					return IsolationLevel.ReadCommitted;
				case DbAccessType.SqLite:
					return IsolationLevel.Serializable;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		///     Required
		///     When a new Connection is requested this function is used
		/// </summary>
		/// <param name="levl"></param>
		public void Connect(IsolationLevel? levl = null)
		{
			ConnectionController.Connection = GetConnection();
			//Connection exists check for open
			lock (ConnectionController.LockRoot)
			{
				if (ConnectionController.Connection.State != ConnectionState.Open)
				{
					ConnectionController.Connection.Open();
				}

				//This is the First call of connect so we Could
				//define it as Transaction
				if (levl != null)
				{
					if (ConnectionController.Transaction == null || ConnectionController.Transaction != null && AllowNestedTransactions)
					{
						ConnectionController.Transaction = ConnectionController.Connection.BeginTransaction(levl.GetValueOrDefault());
					}
				}

				//We created a Connection and proceed now with the DB access
				ConnectionController.InstanceCounter++;
			}

			//Interlocked.Increment(ref _handlecounter);
		}

		/// <summary>
		/// </summary>
		public void TransactionRollback()
		{
			//Error inside the call
			//Rollback the transaction and close the Connection
			if (ConnectionController.Transaction != null)
			{
				//Force all open connections to close
				ConnectionController.Transaction.Rollback();
				ConnectionController.Transaction = null;
				CloseConnection();
			}
		}

		/// <summary>
		/// </summary>
		public void TransactionCommit()
		{
			if (ConnectionController.Transaction != null)
			{
				CloseConnection();
				if (ConnectionController.InstanceCounter != 0)
				{
					ConnectionController.Transaction.Commit();
					ConnectionController.Transaction = null;
				}
			}
		}

		/// <inheritdoc />
		public void CloseConnection(bool forceExisting = false)
		{
			if (ConnectionController.Connection == null && forceExisting)
			{
				throw new InvalidOperationException("To call close connection you must first open one with Connect()");
			}

			if (ConnectionController.InstanceCounter == 0 && forceExisting)
			{
				throw new InvalidOperationException("Invalid State detected. A connection is still open but no handle was found to it");
			}

			//This is not the last call of Close so decrease the counter
			lock (ConnectionController.LockRoot)
			{
				if (ConnectionController.InstanceCounter > 0)
				{
					ConnectionController.InstanceCounter--;
				}

				if (ConnectionController.Connection == null || ConnectionController.InstanceCounter != 0)
				{
					return;
				}
				using (ConnectionController.Connection)
				{
					if (ConnectionController.Transaction != null)
					{
						using (ConnectionController.Transaction)
						{
							ConnectionController.Transaction.Commit();
						}
						ConnectionController.Transaction = null;
					}
					ConnectionController.Connection.Close();
				}
				ConnectionController.Connection = null;
			}
			//GC.Collect();
		}

		/// <summary>
		///     Required
		///     Closing all Connections that maybe open
		/// </summary>
		public void CloseAllConnection()
		{
			//This is not the last call of Close so decrease the counter
			lock (ConnectionController.LockRoot)
			{
				ConnectionController.InstanceCounter = 0;
				if (ConnectionController.Connection != null)
				{
					using (ConnectionController.Connection)
					{
						if (ConnectionController.Transaction != null)
						{
							using (ConnectionController.Transaction)
							{
								ConnectionController.Transaction.Commit();
							}
						}
						ConnectionController.Transaction = null;
						ConnectionController.Connection.Close();
					}
					ConnectionController.Connection = null;
				}

				_strategy.CloseAllConnections();
			}
		}

		/// <summary>
		///     Required
		///     Creates a Command based on the Strategy
		/// </summary>
		/// <param name="strSql"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, params IDataParameter[] fields)
		{
			var cmd = _strategy.CreateCommand(strSql, ConnectionController.Connection, fields);
			if (ConnectionController.Transaction != null)
			{
				cmd.Transaction = ConnectionController.Transaction;
			}
			return cmd;
		}

		/// <summary>
		///     Required
		///     Creates a Parameter based on the Strategy
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public IDataParameter CreateParameter(string strName, object value)
		{
			return _strategy.CreateParameter(strName, value);
		}

		/// <summary>
		///     Executes a query against the database
		/// </summary>
		/// <returns></returns>
		public int ExecuteNonQuery(IDbCommand cmd)
		{
			return Run(d =>
			{
				cmd.Connection = GetConnection();
				if (ConnectionController.Transaction != null)
				{
					cmd.Transaction = ConnectionController.Transaction;
				}
				AttachQueryDebugger(cmd);
				return cmd.ExecuteNonQuery();
			});
		}

		/// <summary>
		///     executes the query against the database and wrapps all params by using a counter. First param @0,@1,@n
		/// </summary>
		/// <returns></returns>
		public int ExecuteNonQuery(string strSql, params object[] obj)
		{
			return DoExecuteNonQuery(strSql, obj);
		}

		/// <summary>
		///     Getlasts the inserted identifier command.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception">DB2.ExecuteNonQuery: void connection</exception>
		public IDbCommand GetlastInsertedIdCommand()
		{
			return _strategy.GetlastInsertedID_Cmd(ConnectionController.Connection);
		}

		/// <summary>
		///     Formarts a Command to a executable QueryCommand
		/// </summary>
		/// <param name="comm"></param>
		/// <returns></returns>
		public string FormartCommandToQuery(IDbCommand comm)
		{
			return _strategy.FormartCommandToQuery(comm);
		}

		/// <summary>
		///     Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string ConvertParameter(DbType type)
		{
			return _strategy.ConvertParameter(type);
		}

		/// <summary>
		///     Gets the data reader for the given Sql Statement.
		/// </summary>
		/// <param name="strSql">The SQL.</param>
		/// <param name="obj">Arguments.</param>
		/// <returns></returns>
		public IDataReader GetDataReader(string strSql, params object[] obj)
		{
			return DoGetDataReader(string.Format(strSql, obj));
		}

		/// <summary>
		///     Gets a single Value from the Query
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		public object GetSkalar(IDbCommand cmd)
		{
			return Run(d =>
			{
				if (ConnectionController.Transaction != null)
				{
					cmd.Transaction = ConnectionController.Transaction;
				}
				AttachQueryDebugger(cmd);
				return cmd.ExecuteScalar();
			});
		}

		/// <summary>
		///     Gets a single Value from the Query
		/// </summary>
		/// <param name="strSql"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		[Obsolete("This method should not be used anymore as it does not ensure SqlInjection prevention")]
		public object GetSkalar(string strSql, params object[] obj)
		{
			return DoGetSkalar(string.Format(strSql, obj));
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <returns></returns>
		public IDatabase Clone()
		{
			var db = new DefaultDatabaseAccess(ConnectionController);
			db.Attach((IDatabaseStrategy)_strategy.Clone());
			return db;
		}

		#endregion

		#region QueryCommand Helper

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="action"></param>
		public void Run(Action<IDatabase> action)
		{
			Run((dd) =>
			{
				action(dd);
				return (object) null;
			});
		}

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public T Run<T>(Func<IDatabase, T> func)
		{
			try
			{
				Connect();

				return func(this);
			}
			finally
			{
				CloseConnection();
			}
		}

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="func"></param>
		/// <returns></returns>
		public async Task RunAsync(Func<IDatabase, Task> func)
		{
			await RunAsync(async (dd) =>
			{
				await func(dd);
				return (object)null;
			});
		}

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="func"></param>
		/// <returns></returns>
		public async Task<T> RunAsync<T>(Func<IDatabase, Task<T>> func)
		{
			try
			{
				Connect();
				return await func(this);
			}
			finally
			{
				CloseConnection();
			}
		}

		/// <summary>
		///     Creates a new Transaction and executes the Action inside it. Then closes the Transaction
		/// </summary>
		/// <param name="action"></param>
		public void RunInTransaction(Action<IDatabase> action)
		{
			RunInTransaction(action, GetDefaultTransactionLevel());
		}

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="action"></param>
		/// <param name="transaction"></param>
		public void RunInTransaction(Action<IDatabase> action, IsolationLevel transaction)
		{
			RunInTransaction((dd) =>
			{
				action(dd);
				return (object)null;
			}, transaction);
		}


		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public T RunInTransaction<T>(Func<IDatabase, T> func)
		{
			return RunInTransaction(func, GetDefaultTransactionLevel());
		}

		/// <summary>
		///     Runs the in transaction.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <param name="transaction">The transaction.</param>
		/// <returns></returns>
		public T RunInTransaction<T>(Func<IDatabase, T> func, IsolationLevel transaction)
		{
			return RunInTransactionAsync(async (dd) => await Task.FromResult(func(dd)), transaction).Result;
		}

		/// <summary>
		///     Runs the in transaction.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <returns></returns>
		public async Task<T> RunInTransactionAsync<T>(Func<IDatabase, Task<T>> func)
		{
			return await RunInTransactionAsync(func, GetDefaultTransactionLevel());
		}

		/// <summary>
		///     Runs the in transaction.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <returns></returns>
		public async Task RunInTransactionAsync(Func<IDatabase, Task> func)
		{
			await RunInTransactionAsync(async (d) =>
			{
				await func(d);
				return "";
			}, GetDefaultTransactionLevel());
		}


		/// <summary>
		///     Runs the in transaction.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <param name="transaction">The transaction.</param>
		/// <returns></returns>
		public async Task<T> RunInTransactionAsync<T>(Func<IDatabase, Task<T>> func, IsolationLevel transaction)
		{
			var preState = ConnectionController.InstanceCounter;
			try
			{
				Connect(transaction);

				return await func(this);
			}
			catch
			{
				TransactionRollback();
				throw;
			}
			finally
			{
				if (preState < ConnectionController.InstanceCounter)
				{
					CloseConnection();
				}
			}
		}

		/// <inheritdoc />
		public void PrepaireRemoteExecution(IDbCommand cmd)
		{
			AttachQueryDebugger(cmd);
		}

		/// <summary>
		/// IF set to true the underlaying provider supports Nested transactions
		/// </summary>
		public bool AllowNestedTransactions { get; set; }

		#endregion QueryCommand Helper
	}

	public sealed partial class DefaultDatabaseAccess
	{
	}
}