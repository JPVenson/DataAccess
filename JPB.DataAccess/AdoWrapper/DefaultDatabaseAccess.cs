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
	public sealed class DefaultDatabaseAccess : IDatabase
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

		private int DoExecuteNonQuery(string strSql)
		{
			if (null == GetConnection())
				throw new Exception("DB2.ExecuteNonQuery: void connection");

			using (var cmd = _strategy.CreateCommand(strSql, GetConnection()))
			{
				if (_trans != null)
					cmd.Transaction = _trans;
				LastExecutedQuery = CreateQueryDebuggerAuto(cmd);

				return cmd.ExecuteNonQuery();
			}
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

		/// <summary>
		/// Opens the and close database.
		/// </summary>
		public void OpenAndCloseDatabase()
		{
			Connect();
			CloseConnection();
		}

		/// <summary>
		///     Will change the object to DBNull if it is C# null
		/// </summary>
		/// <returns></returns>
		public static object Dbcast(IDataRecord dr, string strFieldName, object objFallThru)
		{
			var obj = dr[strFieldName];
			return null == obj || obj is DBNull ? objFallThru : obj;
		}

		/// <summary>
		///     Will change the object to DBNull if it is C# null
		/// </summary>
		/// <returns></returns>
		public static object Dbcast(object obj, object objFallThru)
		{
			return null == obj || obj is DBNull ? objFallThru : obj;
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
		///     Detaches this instance.
		/// </summary>
		public void Detach()
		{
			_strategy = null;
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
		/// Required
		/// Is used to create an new Transaction based on the Strategy
		/// </summary>
		/// <returns></returns>
		public IDbTransaction GetTransaction()
		{
			return _trans;
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
				if (_trans != null)
				{
					_trans.Commit();
				}
				_trans = null;
				_conn2.Close();
				_conn2.Dispose();
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

			if (_conn2 != null && _handlecounter == 0)
			{
				if (_trans != null)
				{
					_trans.Commit();
				}
				_trans = null;
				_conn2.Close();
				_conn2.Dispose();
				_conn2 = null;
			}
			GC.Collect();

			//HACK
			SqlConnection.ClearAllPools();
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
		/// Required
		/// Return the last inserted id based on the Strategy
		/// </summary>
		/// <returns></returns>
		public object GetlastInsertedId()
		{
			using (var cmd = GetlastInsertedIdCommand())
				return GetSkalar(cmd);
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
		/// Gets the data table.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <returns></returns>
		public DataTable GetDataTable(string name, string strSql)
		{
			lock (this)
			{
				using (var cmd = _strategy.CreateCommand(strSql, GetConnection()))
				{
					if (_trans != null)
						cmd.Transaction = _trans;
					LastExecutedQuery = CreateQueryDebuggerAuto(cmd);

					return _strategy.CreateDataTable(name, cmd);
				}
			}
		}
		/// <summary>
		/// Gets the data set.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <returns></returns>
		public DataSet GetDataSet(string strSql)
		{
			lock (this)
			{
				using (var cmd = _strategy.CreateCommand(strSql, GetConnection()))
				{
					IDataAdapter da = _strategy.CreateDataAdapter(cmd); //todo//
					LastExecutedQuery = CreateQueryDebuggerAuto(cmd);

					var ds = new DataSet();
					da.Fill(ds);
					return ds;
				}
			}
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
		/// Execute a QueryCommand and map the result that is created with the func
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strQuery"></param>
		/// <param name="func"></param>
		/// <param name="bHandleConnection"></param>
		/// <returns></returns>
		public IEnumerable<T> GetEntitiesList<T>(string strQuery, Func<IDataRecord, T> func, bool bHandleConnection)
		{
			if (bHandleConnection)
				Connect();

			try
			{
				using (var dr = GetDataReader(strQuery))
				{
					while (dr.Read())
						yield return func(dr);
					dr.Close();
				}
			}
			finally
			{
				if (bHandleConnection)
					CloseConnection();
			}
		}
		/// <summary>
		/// Required
		/// Execute a QueryCommand and map the result that is created with the func
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmd"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public IEnumerable<T> GetEntitiesList<T>(IDbCommand cmd, Func<IDataRecord, T> func)
		{
			LastExecutedQuery = CreateQueryDebuggerAuto(cmd);
			using (var dr = cmd.ExecuteReader())
			{
				while (dr.Read())
					yield return func(dr);
				dr.Close();
			}
		}
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
		/// <summary>
		/// Processes the entities list.
		/// </summary>
		/// <param name="strQuery">The string query.</param>
		/// <param name="action">The action.</param>
		/// <param name="bHandleConnection">if set to <c>true</c> [b handle connection].</param>
		public void ProcessEntitiesList(string strQuery, Action<IDataRecord> action, bool bHandleConnection)
		{
			if (bHandleConnection)
				Connect();

			try
			{
				using (var dr = GetDataReader(strQuery))
				{
					while (dr.Read())
						action(dr);
					dr.Close();
				}
			}
			finally
			{
				if (bHandleConnection)
					CloseConnection();
			}
		}
		/// <summary>
		/// Tries the on entities list.
		/// </summary>
		/// <param name="strQuery">The string query.</param>
		/// <param name="action">The action.</param>
		/// <param name="strMessageOnEmpty">The string message on empty.</param>
		/// <param name="bHandleConnection">if set to <c>true</c> [b handle connection].</param>
		/// <returns></returns>
		public Exception TryOnEntitiesList(string strQuery, Action<IDataRecord> action, string strMessageOnEmpty,
			bool bHandleConnection)
		{
			if (bHandleConnection)
				Connect();

			try
			{
				using (var dr = GetDataReader(strQuery))
				{
					if (false == dr.Read()) return new Exception(strMessageOnEmpty);

					action(dr);

					dr.Close();

					return null;
				}
			}
			catch (Exception ex)
			{
				return ex;
			}
			finally
			{
				if (bHandleConnection)
					CloseConnection();
			}
		}
		/// <summary>
		/// Gets the index of the entities list with.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strQuery">The string query.</param>
		/// <param name="func">The function.</param>
		/// <param name="bHandleConnection">if set to <c>true</c> [b handle connection].</param>
		/// <returns></returns>
		public IEnumerable<T> GetEntitiesListWithIndex<T>(string strQuery, Func<long, IDataRecord, T> func,
			bool bHandleConnection)
		{
			if (bHandleConnection)
				Connect();

			try
			{
				using (var dr = GetDataReader(strQuery))
				{
					long index = -1;
					while (dr.Read())
					{
						index++;
						yield return func(index, dr);
					}
					dr.Close();
				}
			}
			finally
			{
				if (bHandleConnection)
					CloseConnection();
			}
		}
		/// <summary>
		/// Gets the entities dictionary.
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="strQuery">The string query.</param>
		/// <param name="func">The function.</param>
		/// <param name="bHandleConnection">if set to <c>true</c> [b handle connection].</param>
		/// <param name="strExceptionMessage">The string exception message.</param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public IDictionary<K, V> GetEntitiesDictionary<K, V>(string strQuery, Func<IDataRecord, KeyValuePair<K, V>> func,
			bool bHandleConnection, string strExceptionMessage = null)
		{
			var htRes = new Dictionary<K, V>();

			if (bHandleConnection)
				Connect();

			try
			{
				using (var dr = GetDataReader(strQuery))
				{
					while (dr.Read())
					{
						var kvp = func(dr);
						try
						{
							htRes.Add(kvp.Key, kvp.Value);
						}
						catch (Exception)
						{
							if (strExceptionMessage != null)
								throw new ApplicationException(strExceptionMessage + ", " + kvp.Key);
							else throw;
						}
					}
					dr.Close();
				}

				return htRes;
			}
			finally
			{
				if (bHandleConnection)
					CloseConnection();
			}
		}
		/// <summary>
		/// Gets the entities dictionary.
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="cmd">The command.</param>
		/// <param name="func">The function.</param>
		/// <returns></returns>
		public IDictionary<K, V> GetEntitiesDictionary<K, V>(IDbCommand cmd, Func<IDataRecord, KeyValuePair<K, V>> func)
		{
			var htRes = new Dictionary<K, V>();
			LastExecutedQuery = CreateQueryDebuggerAuto(cmd);

			using (var dr = cmd.ExecuteReader())
			{
				while (dr.Read())
				{
					var kvp = func(dr);

					htRes.Add(kvp.Key, kvp.Value);
				}
				dr.Close();
			}

			return htRes;
		}
		/// <summary>
		/// Gets the next paging step.
		/// </summary>
		/// <typeparam name="V"></typeparam>
		/// <param name="strQuery">The string query.</param>
		/// <param name="func">The function.</param>
		/// <param name="iPageSize">Size of the i page.</param>
		/// <param name="default">The default.</param>
		/// <param name="bHandleConnection">if set to <c>true</c> [b handle connection].</param>
		/// <param name="strExceptionMessage">The string exception message.</param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public V GetNextPagingStep<V>(string strQuery, Func<IDataRecord, V> func, long iPageSize, V @default,
			bool bHandleConnection, string strExceptionMessage = null)
		{
			if (bHandleConnection)
				Connect();

			IDataReader dr = null;
			try
			{
				long index = -1;
				long rotate = -1;
				dr = GetDataReader(strQuery);
				while (dr.Read())
				{
					index += 1;
					rotate += 1;
					if (rotate == iPageSize - 1)
						rotate = -1;

					try
					{
						if (0 == rotate)
							return func(dr);
					}
					catch (Exception)
					{
						if (strExceptionMessage != null)
							throw new ApplicationException(strExceptionMessage + ", " + index);
						else throw;
					}
				}

				return @default;
			}
			finally
			{
				if (dr != null)
				{
					dr.Close();
					dr.Dispose();
				}

				if (bHandleConnection)
					CloseConnection();
			}
		}

		#endregion QueryCommand Helper
	}
}