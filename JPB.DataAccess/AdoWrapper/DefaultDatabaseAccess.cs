using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.IDatabase" />
	[DebuggerDisplay("OpenRuns {_handlecounter}, IsConnectionOpen {_conn2 != null}, IsTransactionOpen {_trans != null}")]
	public sealed partial class DefaultDatabaseAccess : IDatabase
	{
		private IDbConnection _conn2;
		private volatile int _handlecounter;
		private IDatabaseStrategy _strategy;
		private IDbTransaction _trans;

		/// <summary>
		///     If enabled each query will be decompiled and the LastInsertedQuery property will be set
		/// </summary>
		public bool Debugger { get; set; }

		/// <summary>
		/// Finalizes an instance of the <see cref="DefaultDatabaseAccess"/> class.
		/// </summary>
		~DefaultDatabaseAccess()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (GetConnection() != null)
				{
					_conn2.Dispose();
					_conn2 = null;
				}
			}
		}

		/// <summary>
		/// Creates the specified strategy.
		/// </summary>
		/// <param name="strategy">The strategy.</param>
		/// <returns></returns>
		public static DefaultDatabaseAccess Create(IDatabaseStrategy strategy)
		{
			if (null == strategy)
				return null;

			var db = new DefaultDatabaseAccess();
			db.Attach(strategy);
			return db;
		}

		private int DoExecuteNonQuery(string strSql, params object[] param)
		{
			if (null == GetConnection())
				throw new Exception("DB2.ExecuteNonQuery: void connection");
			var counter = 0;
			using (
				var cmd = _strategy.CreateCommand(strSql, GetConnection(),
					param.Select(s => CreateParameter(counter++.ToString(), s)).ToArray()))
			{
				if (_trans != null)
					cmd.Transaction = _trans;
				LastExecutedQuery = CreateQueryDebuggerAuto(cmd);

				return cmd.ExecuteNonQuery();
			}
		}

		private IDataReader DoGetDataReader(string strSql)
		{
			if (null == GetConnection()) throw new Exception("DB2.GetDataReader: void connection");

			using (var cmd = _strategy.CreateCommand(strSql, GetConnection()))
			{
				if (_trans != null)
					cmd.Transaction = _trans;
				LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
				return cmd.ExecuteReader();
			}
		}

		private object DoGetSkalar(string strSql)
		{
			if (null == GetConnection()) throw new Exception("DB2.GetSkalar: void connection");

			using (var cmd = _strategy.CreateCommand(strSql, GetConnection()))
			{
				if (_trans != null)
					cmd.Transaction = _trans;
				LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
				return cmd.ExecuteScalar();
			}
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

		private QueryDebugger CreateQueryDebuggerAuto(IDbCommand cmd)
		{
			if (Debugger)
			{
				return new QueryDebugger(cmd, this);
			}
			return null;
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
					return DbAccessType.Unknown;
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
		/// Get the Current Connection string
		/// </summary>
		public string ConnectionString
		{
			get { return null == _strategy ? null : _strategy.ConnectionString; }
		}

		/// <summary>
		/// If local instance get the file
		/// </summary>
		public string DatabaseFile
		{
			get { return null == _strategy ? null : _strategy.DatabaseFile; }
		}

		/// <summary>
		/// Get the Database name that we are connected to
		/// </summary>
		public string DatabaseName
		{
			get { return GetConnection().Database; }
		}

		/// <summary>
		/// Get the Server we are Connected to
		/// </summary>
		public string ServerName
		{
			get { return null == _strategy ? String.Empty : _strategy.ServerName; }
		}

		/// <summary>
		/// Required
		/// Is used to create an new Connection based on the Strategy and
		/// keep it
		/// </summary>
		/// <returns></returns>
		public IDbConnection GetConnection()
		{
			return _conn2 ?? (_conn2 = _strategy.CreateConnection());
		}

		/// <summary>
		/// Gets the default transaction level.
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
		/// Required
		/// When a new Connection is requested this function is used
		/// </summary>
		/// <param name="levl"></param>
		public void Connect(IsolationLevel? levl = null)
		{
			//check for an Active connection
			if (null == _conn2)
				//No Connection open one
				_conn2 = GetConnection();
			//Connection exists check for open
			lock (_conn2)
			{
				if (_conn2.State != ConnectionState.Open)
					_conn2.Open();
			}

			//This is the First call of connect so we Could
			//define it as Transaction
			if (_handlecounter == 0 && levl != null)
				_trans = _conn2.BeginTransaction(levl.GetValueOrDefault());

			//We created a Connection and proceed now with the DB access
			Interlocked.Increment(ref _handlecounter);
		}

		/// <summary>
		/// </summary>
		public void TransactionRollback()
		{
			//Error inside the call
			//Rollback the transaction and close the Connection
			if (_trans != null)
			{
				//Force all open connections to close
				Interlocked.Exchange(ref _handlecounter, 0);
				_trans.Rollback();
				_trans = null;
				CloseConnection();
			}
		}
		/// <summary>
		/// Required
		/// Closing a open Connection
		/// </summary>
		public void CloseConnection()
		{
			Debug.Assert(_handlecounter >= 0);

			//This is not the last call of Close so decrease the counter
			lock (this)
			{
				if (_handlecounter > 0)
					_handlecounter--;
			}

			if (_conn2 != null && _handlecounter == 0)
			{
				using (_conn2)
				{
					if (_trans != null)
					{
						using (_trans)
						{
							_trans.Commit();
						}
					}
					_trans = null;
					_conn2.Close();
				}
				_conn2 = null;
			}
			//GC.Collect();
		}
		/// <summary>
		/// Required
		/// Closing all Connections that maybe open
		/// </summary>
		public void CloseAllConnection()
		{
			Debug.Assert(_handlecounter >= 0);

			//This is not the last call of Close so decrease the counter
			lock (this)
			{
				_handlecounter = 0;
			}

			if (_conn2 != null)
			{
				using (_conn2)
				{
					if (_trans != null)
					{
						using (_trans)
						{
							_trans.Commit();
						}
					}
					_trans = null;
					_conn2.Close();
				}
				_conn2 = null;
			}

			_strategy.CloseAllConnections();
		}
		/// <summary>
		/// Required
		/// Creates a Command based on the Strategy
		/// </summary>
		/// <param name="strSql"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, params IDataParameter[] fields)
		{
			var cmd = _strategy.CreateCommand(strSql, GetConnection(), fields);
			LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
			if (_trans != null)
				cmd.Transaction = _trans;
			return cmd;
		}
		/// <summary>
		/// Required
		/// Creates a Parameter based on the Strategy
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
			if (null == GetConnection())
				throw new Exception("DB2.ExecuteNonQuery: void connection");

			if (_trans != null)
				cmd.Transaction = _trans;
			LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
			return cmd.ExecuteNonQuery();
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
		/// Getlasts the inserted identifier command.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception">DB2.ExecuteNonQuery: void connection</exception>
		public IDbCommand GetlastInsertedIdCommand()
		{
			if (null == GetConnection())
				throw new Exception("DB2.ExecuteNonQuery: void connection");

			return _strategy.GetlastInsertedID_Cmd(GetConnection());
		}
		/// <summary>
		/// Formarts a Command to a executable QueryCommand
		/// </summary>
		/// <param name="comm"></param>
		/// <returns></returns>
		public string FormartCommandToQuery(IDbCommand comm)
		{
			return _strategy.FormartCommandToQuery(comm);
		}
		/// <summary>
		/// Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string ConvertParameter(DbType type)
		{
			return _strategy.ConvertParameter(type);
		}

		/// <summary>
		/// Gets the data reader for the given Sql Statement.
		/// </summary>
		/// <param name="strSql">The SQL.</param>
		/// <param name="obj">Arguments.</param>
		/// <returns></returns>
		public IDataReader GetDataReader(string strSql, params object[] obj)
		{
			return DoGetDataReader(String.Format(strSql, obj));
		}
		/// <summary>
		/// Gets a single Value from the Query
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		public object GetSkalar(IDbCommand cmd)
		{
			if (_trans != null)
				cmd.Transaction = _trans;
			LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
			return cmd.ExecuteScalar();
		}
		/// <summary>
		/// Gets a single Value from the Query
		/// </summary>
		/// <param name="strSql"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public object GetSkalar(string strSql, params object[] obj)
		{
			return DoGetSkalar(String.Format(strSql, obj));
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns></returns>
		public IDatabase Clone()
		{
			var db = new DefaultDatabaseAccess();
			db.Attach((IDatabaseStrategy) _strategy.Clone());
			return db;
		}

		#endregion

		#region QueryCommand Helper

		/// <summary>
		/// Required
		/// Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="action"></param>
		public void Run(Action<IDatabase> action)
		{
			try
			{
				Connect();

				action(this);
			}
			finally
			{
				CloseConnection();
			}
		}
		/// <summary>
		/// Required
		/// Opens a Connection or reuse an existing one and then execute the action
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
		///     Creates a new Transaction and executes the Action inside it. Then closes the Transaction
		/// </summary>
		/// <param name="action"></param>
		public void RunInTransaction(Action<IDatabase> action)
		{
			RunInTransaction(action, GetDefaultTransactionLevel());
		}
		/// <summary>
		/// Required
		/// Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <param name="action"></param>
		/// <param name="transaction"></param>
		public void RunInTransaction(Action<IDatabase> action, IsolationLevel transaction)
		{
			try
			{
				Connect(transaction);

				action(this);
			}
			catch
			{
				TransactionRollback();
				throw;
			}
			finally
			{
				CloseConnection();
			}
		}


		/// <summary>
		/// Required
		/// Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public T RunInTransaction<T>(Func<IDatabase, T> func)
		{
			return RunInTransaction(func, GetDefaultTransactionLevel());
		}
		/// <summary>
		/// Runs the in transaction.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <param name="transaction">The transaction.</param>
		/// <returns></returns>
		public T RunInTransaction<T>(Func<IDatabase, T> func, IsolationLevel transaction)
		{
			try
			{
				//defaulting it
				//Connect(IsolationLevel.ReadUncommitted);
				Connect(transaction);

				var res = func(this);

				return res;
			}
			catch
			{
				TransactionRollback();
				throw;
			}
			finally
			{
				CloseConnection();
			}
		}

		#endregion QueryCommand Helper
	}

	public sealed partial class DefaultDatabaseAccess
	{

	}
}