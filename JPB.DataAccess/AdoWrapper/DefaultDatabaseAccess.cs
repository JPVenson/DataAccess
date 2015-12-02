using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper
{

    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("OpenRuns {_handlecounter}, IsConnectionOpen {_conn2 != null}, IsTransactionOpen {_trans != null}")]
    public sealed class DefaultDatabaseAccess : IDatabase
    {
        private IDbConnection _conn2;
        private volatile int _handlecounter;
        private IDatabaseStrategy _strategy;
        private IDbTransaction _trans;

        #region IDatabase Members

        public IWrapperDataPager<T, TE> CreatePager<T, TE>()
        {
            return _strategy.CreateConverterPager<T, TE>();
        }

        /// <summary>
        /// Get the last Executed Query wrapped by a Debugger
        /// </summary>
        public QueryDebugger LastExecutedQuery { get; private set; }

        public IDataPager<T> CreatePager<T>()
        {
            return _strategy.CreatePager<T>();
        }

        public void Attach(IDatabaseStrategy strategy)
        {
            _strategy = strategy;
            CloseConnection();
        }

        public void Detach()
        {
            _strategy = null;
            CloseConnection();
        }

        public DbAccessType TargetDatabase
        {
            get
            {
                if(_strategy == null)
                    return DbAccessType.Unknown;
                return _strategy.SourceDatabase;
            }
        }

        public bool IsAttached
        {
            get { return (_strategy != null); }
        }

        public string ConnectionString
        {
            get { return (null == _strategy) ? null : _strategy.ConnectionString; }
        }

        public string DatabaseFile
        {
            get { return (null == _strategy) ? null : _strategy.DatabaseFile; }
        }

        public string DatabaseName
        {
            get { return GetConnection().Database; }
        }

        public string ServerName
        {
            get { return (null == _strategy) ? String.Empty : _strategy.ServerName; }
        }

        public IDbConnection GetConnection()
        {
            return _conn2 ?? (_conn2 = _strategy.CreateConnection());
        }

        public IDbTransaction GetTransaction()
        {
            return _trans;
        }

        public void Connect(IsolationLevel? levl = null)
        {
            //check for an Active connection
            if (null == _conn2)
                //No Connection open one
                _conn2 = GetConnection();
            //Connection exists check for open
            if (_conn2.State != ConnectionState.Open)
                lock (typeof(DefaultDatabaseAccess))
                {
                    _conn2.Open();
                }

            //This is the First call of connect so we Could
            //define it as Transaction
            if (_handlecounter == 0 && levl != null)
                _trans = _conn2.BeginTransaction(levl.GetValueOrDefault());

            //We created a Connection and proceed now with the DB access
            _handlecounter++;
        }

        /// <summary>
        /// 
        /// </summary>
        public void TransactionRollback()
        {
            //Error inside the call
            //Rollback the transaction and close the Connection
            if (_trans != null)
            {
                //Force all open connections to close
                _handlecounter = 0;
                _trans.Rollback();
                _trans = null;
                CloseConnection();
            }
        }

        public void CloseConnection()
        {
            Debug.Assert(_handlecounter >= 0);

            //This is not the last call of Close so decrease the counter
            if (_handlecounter > 0)
                _handlecounter--;

            if (_conn2 != null && _handlecounter == 0)
            {
                if (_trans != null)
                {
                    _trans.Commit();
                }
                _trans = null;
                _conn2.Close();
            }
        }

        public IDbCommand CreateCommand(string strSql, params IDataParameter[] fields)
        {
            IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection(), fields);
            LastExecutedQuery = cmd.CreateQueryDebugger(this);
            if (_trans != null)
                cmd.Transaction = _trans;
            return cmd;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return _strategy.CreateParameter(strName, value);
        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IDbCommand cmd)
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");

            if (_trans != null)
                cmd.Transaction = _trans;
            LastExecutedQuery = cmd.CreateQueryDebugger(this);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// executes the query against the database and wrapps all params by using a counter. First param @0,@1,@n
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string strSql, params object[] obj)
        {
            return DoExecuteNonQuery(strSql, obj);
        }

        public IDbCommand GetlastInsertedIdCommand()
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");

            return _strategy.GetlastInsertedID_Cmd(GetConnection());
        }

        public string FormartCommandToQuery(IDbCommand comm)
        {
            return this._strategy.FormartCommandToQuery(comm);
        }

        public string ConvertParameter(DbType type)
        {
            return this._strategy.ConvertParameter(type);
        }

        public object GetlastInsertedID()
        {
            using (var cmd = GetlastInsertedIdCommand())
                return GetSkalar(cmd);
        }

        public IDataReader GetDataReader(string strSql, params object[] obj)
        {
            return DoGetDataReader(String.Format(strSql, obj));
        }

        public object GetSkalar(IDbCommand cmd)
        {
            if (_trans != null)
                cmd.Transaction = _trans;
            LastExecutedQuery = cmd.CreateQueryDebugger(this);
            return cmd.ExecuteScalar();
        }

        public object GetSkalar(string strSql, params object[] obj)
        {
            return DoGetSkalar(String.Format(strSql, obj));
        }

        public DataTable GetDataTable(string name, string strSql)
        {
            lock (this)
            {
                using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
                {
                    if (_trans != null)
                        cmd.Transaction = _trans;
                    LastExecutedQuery = cmd.CreateQueryDebugger(this);

                    return _strategy.CreateDataTable(name, cmd);
                }
            }
        }

        public DataSet GetDataSet(string strSql)
        {
            lock (this)
            {
                using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
                {
                    IDataAdapter da = _strategy.CreateDataAdapter(cmd); //todo//
                    LastExecutedQuery = cmd.CreateQueryDebugger(this);

                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDatabase Clone()
        {
            var db = new DefaultDatabaseAccess();
            db.Attach((IDatabaseStrategy)_strategy.Clone());
            return db;
        }

        #endregion

        #region Query Helper

        public void ProcessEntitiesList(string strQuery, Action<IDataRecord> action, bool bHandleConnection)
        {
            if (bHandleConnection)
                Connect();

            try
            {
                using (IDataReader dr = GetDataReader(strQuery))
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

        public Exception TryOnEntitiesList(string strQuery, Action<IDataRecord> action, string strMessageOnEmpty,
            bool bHandleConnection)
        {
            if (bHandleConnection)
                Connect();

            try
            {
                using (IDataReader dr = GetDataReader(strQuery))
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

        public IEnumerable<T> GetEntitiesList<T>(string strQuery, Func<IDataRecord, T> func, bool bHandleConnection)
        {
            if (bHandleConnection)
                Connect();

            try
            {
                using (IDataReader dr = GetDataReader(strQuery))
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

        public IEnumerable<T> GetEntitiesList<T>(IDbCommand cmd, Func<IDataRecord, T> func)
        {
            LastExecutedQuery = cmd.CreateQueryDebugger(this);
            using (IDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    yield return func(dr);
                dr.Close();
            }
        }

        public IEnumerable<T> GetEntitiesListWithIndex<T>(string strQuery, Func<long, IDataRecord, T> func,
            bool bHandleConnection)
        {
            if (bHandleConnection)
                Connect();

            try
            {
                using (IDataReader dr = GetDataReader(strQuery))
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

        public IDictionary<K, V> GetEntitiesDictionary<K, V>(string strQuery, Func<IDataRecord, KeyValuePair<K, V>> func,
            bool bHandleConnection, string strExceptionMessage = null)
        {
            var htRes = new Dictionary<K, V>();

            if (bHandleConnection)
                Connect();

            try
            {
                using (IDataReader dr = GetDataReader(strQuery))
                {
                    while (dr.Read())
                    {
                        KeyValuePair<K, V> kvp = func(dr);
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

        public IDictionary<K, V> GetEntitiesDictionary<K, V>(IDbCommand cmd, Func<IDataRecord, KeyValuePair<K, V>> func)
        {
            var htRes = new Dictionary<K, V>();
            LastExecutedQuery = cmd.CreateQueryDebugger(this);

            using (IDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    KeyValuePair<K, V> kvp = func(dr);

                    htRes.Add(kvp.Key, kvp.Value);
                }
                dr.Close();
            }

            return htRes;
        }

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
                    if (rotate == (iPageSize - 1))
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


        public void RunInTransaction(Action<IDatabase> action)
        {
            try
            {
                Connect(IsolationLevel.ReadUncommitted);

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

        public T RunInTransaction<T>(Func<IDatabase, T> func)
        {
            try
            {
                Connect(IsolationLevel.ReadUncommitted);

                T res = func(this);

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

        #endregion Query Helper

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

            using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;
                LastExecutedQuery = cmd.CreateQueryDebugger(this);

                return cmd.ExecuteNonQuery();
            }
        }

        private int DoExecuteNonQuery(string strSql, params object[] param)
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");
            int counter = 0;
            using (IDbCommand cmd = _strategy.CreateCommand(strSql,
                GetConnection(), 
                param.Select(s => this.CreateParameter(counter++.ToString(), s)).ToArray()))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;
                LastExecutedQuery = cmd.CreateQueryDebugger(this);

                return cmd.ExecuteNonQuery();
            }
        }

        private IDataReader DoGetDataReader(string strSql)
        {
            if (null == GetConnection()) throw new Exception("DB2.GetDataReader: void connection");

            using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;
                LastExecutedQuery = cmd.CreateQueryDebugger(this);
                return cmd.ExecuteReader();
            }
        }

        private object DoGetSkalar(string strSql)
        {
            if (null == GetConnection()) throw new Exception("DB2.GetSkalar: void connection");

            using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;
                LastExecutedQuery = cmd.CreateQueryDebugger(this);
                return cmd.ExecuteScalar();
            }
        }

        public void OpenAndCloseDatabase()
        {
            Connect();
            CloseConnection();
        }

        public static object DBCAST(IDataRecord dr, string strFieldName, object objFallThru)
        {
            object obj = dr[strFieldName];
            return (null == obj || obj is DBNull) ? objFallThru : obj;
        }

        public static object DBCAST(object obj, object objFallThru)
        {
            return (null == obj || obj is DBNull) ? objFallThru : obj;
        }
    }
}