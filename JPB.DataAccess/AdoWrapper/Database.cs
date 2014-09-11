using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace JPB.DataAccess.AdoWrapper
{
    public sealed class Database : IDatabase
    {
        private IDbConnection _conn2;
        private volatile int _handlecounter;
        private IDatabaseStrategy _strategy;
        private IDbTransaction _trans;

        #region IDatabase Members

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

        public void Connect(bool bUseTransaction)
        {
            if (null == GetConnection())
                _conn2 = _strategy.CreateConnection();
            _handlecounter++;

            if (GetConnection().State != ConnectionState.Open)
                GetConnection().Open();

            if (_handlecounter == 0)
            {
                if (bUseTransaction)
                    _trans = GetConnection().BeginTransaction();
            }
        }

        public void TransactionCommit()
        {
            if (_trans != null)
            {
                _trans.Commit();
                _trans = null;
            }
        }

        public void TransactionRollback()
        {
            if (_trans != null)
                _trans.Rollback();
        }

        public void CloseConnection()
        {
            if (_handlecounter > 0)
                _handlecounter--;
            if (GetConnection() != null && _handlecounter == 0)
            {
                _trans = null;
                GetConnection().Close();
            }
        }

        public IDbCommand CreateCommand(string strSql, params IDbDataParameter[] fields)
        {
            IDbCommand cmd = _strategy.CreateCommand(GetConnection(), strSql, fields);
            if (_trans != null)
                cmd.Transaction = _trans;
            return cmd;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return _strategy.CreateParameter(strName, value);
        }

        //public IDbDataParameter CreateParameter_Bit(string strName, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_Bit(strName, nullable);
        //}

        //public IDbDataParameter CreateParameter_Int(string strName, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_Int(strName, nullable);
        //}

        //public IDbDataParameter CreateParameter_SmallInt(string strName)
        //{
        //    return _strategy.CreateParameter_SmallInt(strName);
        //}

        //public IDbDataParameter CreateParameter_BigInt(string strName)
        //{
        //    return _strategy.CreateParameter_BigInt(strName);
        //}

        //public IDbDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_VarChar(strName, iSize, nullable);
        //}

        //public IDbDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_NVarChar(strName, iSize, nullable);
        //}

        //public IDbDataParameter CreateParameter_NVarChar_MAX(string strName)
        //{
        //    return _strategy.CreateParameter_NVarChar_MAX(strName);
        //}

        //public IDbDataParameter CreateParameter_DateTime(string strName, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_DateTime(strName, nullable);
        //}

        //public IDbDataParameter CreateParameter_Time(string strName, bool nullable = false)
        //{
        //    return _strategy.CreateParameter_Time(strName, nullable);
        //}

        //public IDbDataParameter CreateParameter_SmallDateTime(string strName)
        //{
        //    return _strategy.CreateParameter_SmallDateTime(strName);
        //}

        public int ExecuteNonQuery(IDbCommand cmd)
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");

            if (_trans != null)
                cmd.Transaction = _trans;
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string strSql, params object[] obj)
        {
            return DoExecuteNonQuery(string.Format(strSql, obj));
        }

        public object GetlastInsertedID()
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");

            using (IDbCommand cmd = _strategy.GetlastInsertedID_Cmd(GetConnection()))
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
            return cmd.ExecuteScalar();
        }

        public object GetSkalar(string strSql, params object[] obj)
        {
            return DoGetSkalar(String.Format(strSql, obj));
        }

        public DateTime GetDateTimeSkalar(string strSql)
        {
            Connect(false);
            DateTime dt = Convert.ToDateTime(GetSkalar(strSql));
            CloseConnection();
            return dt;
        }

        public DataTable GetDataTable(string name, string strSql)
        {
            lock (this)
            {
                using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
                {
                    if (_trans != null)
                        cmd.Transaction = _trans;

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

                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        public string GetTimeStamp()
        {
            return _strategy.GetTimeStamp();
        }

        public string GetTimeStamp(DateTime dtValue)
        {
            return _strategy.GetTimeStamp(dtValue);
        }

        public void Import(IDatabase dbFrom, string strSqlFrom, string strSqlTo)
        {
            DataTable dtFrom = null;
            DataTable dtTo = null;
            IDbCommand cmdTo = null;
            try
            {
                if (GetConnection() == null) throw new Exception("~blur?~");

                dtFrom = dbFrom.GetDataTable("from", strSqlFrom);
                dtTo = dtFrom.Copy();

                cmdTo = _strategy.CreateCommand(strSqlTo, GetConnection());
                if (_trans != null)
                    cmdTo.Transaction = _trans;

                _strategy.Import(dtTo, cmdTo);
            }
            finally
            {
                if (dtFrom != null) dtFrom.Dispose();
                if (dtTo != null) dtTo.Dispose();
                if (cmdTo != null) cmdTo.Dispose();
            }
        }

        public void Import(DataTable dt, string strSql)
        {
            using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
                _strategy.Import(dt, cmd);
        }

        public string[] GetTables()
        {
            return _strategy.GetTables(GetConnection(), "%");
        }

        public string[] GetTables(String strFilter)
        {
            return _strategy.GetTables(GetConnection(), strFilter);
        }

        public string[] GetTableColumns(string strTableName, params object[] exclude)
        {
            return _strategy.GetTableColumns(GetConnection(), strTableName);
        }

        public int DropTable(string strTableName)
        {
            return _strategy.DropTable(GetConnection(), strTableName);
        }

        public void CompactDatabase()
        {
            string tmpFile = string.Format("{0}.tmp", Guid.NewGuid());
            string tmpFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                tmpFile);

            _strategy.CompactDatabase(DatabaseFile, tmpFilename);

            File.Delete(DatabaseFile);
            File.Move(tmpFilename, DatabaseFile);
        }

        public void ShrinkDatabase()
        {
            if (null == GetConnection()) throw new Exception("DB2.ExecuteNonQuery: void connection");

            _strategy.ShrinkDatabase(ConnectionString);
        }

        public void PrepareQuery(string strSql)
        {
            IDbConnection conn = GetConnection();
            conn.Open();
            _strategy.PrepareQuery(conn, strSql);
            conn.Close();
        }

        public void CreateTable(DataTable table, string strTableName, HashSet<string> hsColumns2Export)
        {
            string sql = string.Format("CREATE TABLE [{0}] ({1})",
                strTableName,
                string.Join(",", table.Columns
                    .Cast<DataColumn>()
                    .Where(
                        column => hsColumns2Export.Contains(column.ColumnName))
                    .Select(column => string.Format("[{0}] {1}",
                        column.ColumnName,
                        TypeToString(
                            column.DataType)))
                    .ToArray()));

            //using(TextWriter w = new StreamWriter(@"E:\__OUT\xxxx.txt", false))
            //{
            //    w.WriteLine(sql);
            //}

            ExecuteNonQuery(sql);
        }

        public void InsertTable(DataTable table, string strTableName, HashSet<string> hsColumns2Export,
            string strFilterExpression)
        {
            DataRow[] rows = table.Select(strFilterExpression);
            if (0 == rows.Length) return;

            Dictionary<string, string> htPars = table.Columns
                .Cast<DataColumn>()
                .Where(column => hsColumns2Export.Contains(column.ColumnName))
                .ToDictionary(
                    column => column.Caption,
                    column =>
                        string.Format("@{0}", column.Caption.Replace(' ', '_')));

            //-------------------------------------------------------------------------------------------------

            string strVars = string.Join(",", htPars.Keys.Select(key => string.Format("[{0}]", key)).ToArray());
            string strPars = string.Join(",", htPars.Values.ToArray());
            string strSql = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", strTableName, strVars, strPars);

            IDbCommand cmd = CreateCommand(strSql);
            foreach (DataColumn column in table.Columns)
            {
                if (hsColumns2Export.Contains(column.ColumnName))
                {
                    cmd.Parameters.Add(
                        CreateParameter(
                            htPars[column.Caption],
                            column.MaxLength));
                }
            }

            for (int i = 0; i < rows.Length; i++)
            {
                //if (0 == i && bUpdateFirst) continue;
                foreach (DataColumn column in table.Columns)
                {
                    if (hsColumns2Export.Contains(column.ColumnName))
                    {
                        var param = (IDbDataParameter) cmd.Parameters[htPars[column.Caption]];
                        param.Value = rows[i][column];
                    }
                }
                ExecuteNonQuery(cmd);
            }
        }

        public bool SupportsView(String strName)
        {
            return _strategy.SupportsView(GetConnection(), strName);
        }

        public IEnumerable<string> GetViews(String strName)
        {
            var lst = new List<string>();
            using (IDataReader dr = GetDataReader(_strategy.GetViewsSql(strName)))
            {
                while (dr.Read())
                    lst.Add((string) dr[0]);
                dr.Close();
            }
            return lst;
        }

        public bool SupportsStoredProcedure(String strName)
        {
            return _strategy.SupportsStoredProcedure(GetConnection(), strName);
        }

        public IEnumerable<string> GetStoredProcedure(String strName)
        {
            var lst = new List<string>();
            using (IDataReader dr = GetDataReader(_strategy.GetStoredProcedureSql(strName)))
            {
                while (dr.Read())
                    lst.Add((string) dr[0]);
                dr.Close();
            }
            return lst;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDatabase Clone()
        {
            var db = new Database();
            db.Attach((IDatabaseStrategy) _strategy.Clone());
            return db;
        }

        #endregion

        #region Query Helper

        public void ProcessEntitiesList(string strQuery, Action<IDataRecord> action, bool bHandleConnection)
        {
            if (bHandleConnection)
                Connect(false);

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
                Connect(false);

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
                Connect(false);

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
                Connect(false);

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
                Connect(false);

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
                Connect(false);

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

        public IDictionary<long, V> GetPagedEntitiesDictionary<V>(string strQuery, Func<IDataRecord, V> func,
            long iPageSize, bool bHandleConnection,
            string strExceptionMessage = null)
        {
            var htRes = new Dictionary<long, V>();

            if (bHandleConnection)
                Connect(false);

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
                            htRes.Add(index, func(dr));
                    }
                    catch (Exception)
                    {
                        if (strExceptionMessage != null)
                            throw new ApplicationException(strExceptionMessage + ", " + index);
                        else throw;
                    }
                }

                return htRes;
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
                Connect(false);

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
                Connect(false);

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
                Connect(true);

                action(this);

                TransactionCommit();
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
                Connect(true);

                T res = func(this);

                TransactionCommit();

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

        public void RunInTransaction(Action<IDatabase> action, IsolationLevel readUncommitted)
        {
        }

        #endregion Query Helper

        ~Database()
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

        public static Database Create(IDatabaseStrategy strategy)
        {
            if (null == strategy)
                return null;

            var db = new Database();
            db.Attach(strategy);
            return db;
        }

        [Obsolete]
        public static Database CreateMySQL(string strServer, string strDatabase)
        {
            return Create(new DsMSSQL(strServer, strDatabase));
        }

        [Obsolete]
        public static Database CreateMSSQL(string strServer, string strDatabase)
        {
            return Create(new DsMSSQL(strServer, strDatabase));
        }

        [Obsolete]
        public static Database CreateMSSQL(string strServer, string strDatabase, string strLogin, string strPassword)
        {
            return Create(new DsMSSQL(strServer, strDatabase, strLogin, strPassword));
        }

        [Obsolete]
        public static Database CreateMSSQL(string strConnStr)
        {
            return Create(new DsMSSQL(strConnStr));
        }

        //public static Database CreateExcel(string strFilename, bool bAssumeHeader, bool bUseImex, bool bNewExcel)
        //{
        //    return Create(bNewExcel
        //                      ? new DsExcel_XLSX(strFilename, bAssumeHeader, bUseImex)
        //                      : (AbstractDsExcel)new DsExcel_XLS(strFilename, bAssumeHeader, bUseImex));
        //}

        private int DoExecuteNonQuery(string strSql)
        {
            if (null == GetConnection())
                throw new Exception("DB2.ExecuteNonQuery: void connection");

            using (IDbCommand cmd = _strategy.CreateCommand(strSql, GetConnection()))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;

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
                return cmd.ExecuteScalar();
            }
        }

        public void OpenAndCloseDatabase()
        {
            Connect(false);
            CloseConnection();
        }

        private static string TypeToString(Type type)
        {
            if (type == typeof (string))
                return "TEXT";
            if (type == typeof (long) || type == typeof (int))
                return "NUMBER";
            if (type == typeof (double) || type == typeof (float))
                return "NUMBER";
            if (type == typeof (DateTime))
                return "DATETIME";
            return "TEXT";
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