using System;
using System.Collections.Generic;
using System.Data;

namespace JPB.DataAccess.AdoWrapper
{
    public interface IDatabase : IDisposable
    {
        bool IsAttached { get; }

        string ConnectionString { get; }
        string DatabaseFile { get; }
        string DatabaseName { get; }
        string ServerName { get; }
        void Attach(IDatabaseStrategy strategy);
        void Detach();
        IDbConnection GetConnection();
        IDbTransaction GetTransaction();

        void Connect(bool bUseTransaction);
        void TransactionCommit();
        void TransactionRollback();
        void CloseConnection();

        int ExecuteNonQuery(string strSql, params object[] obj);
        int ExecuteNonQuery(IDbCommand cmd);
        object GetlastInsertedID();

        IDataReader GetDataReader(string strSql, params object[] obj);

        object GetSkalar(IDbCommand cmd);
        object GetSkalar(string strSql, params object[] obj);
        DateTime GetDateTimeSkalar(string strSql);

        DataTable GetDataTable(string name, string strSql);
        DataSet GetDataSet(string strSql);

        void Import(IDatabase dbFrom, string strSqlFrom, string strSqlTo);
        void Import(DataTable dt, string strSql);

        string GetTimeStamp();
        string GetTimeStamp(DateTime dtValue);

        IDbCommand CreateCommand(string strSql, params IDbDataParameter[] fields);

        IDataParameter CreateParameter(string strName, object value);

        //IDataParameter CreateParameter_Bit(string strName, bool nullable = false);
        //IDataParameter CreateParameter_Int(string strName, bool nullable = false);
        //IDataParameter CreateParameter_SmallInt(string strName);
        //IDataParameter CreateParameter_BigInt(string strName);
        //IDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false);
        //IDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false);
        //IDataParameter CreateParameter_NVarChar_MAX(string strName);
        //IDataParameter CreateParameter_DateTime(string strName, bool nullable = false);
        //IDataParameter CreateParameter_Time(string strName, bool nullable = false);
        //IDataParameter CreateParameter_SmallDateTime(string strName);

        string[] GetTables();
        string[] GetTables(String strFilter);
        string[] GetTableColumns(string strTableName, params object[] exclude);
        int DropTable(string strTableName);

        void CompactDatabase();
        void ShrinkDatabase();
        void PrepareQuery(string strSql);

        void CreateTable(DataTable table, string strTableName, HashSet<string> hsColumns2Export);

        void InsertTable(DataTable table, string strTableName, HashSet<string> hsColumns2Export,
            string strFilterExpression);

        bool SupportsView(String strName);
        IEnumerable<string> GetViews(String strName);
        bool SupportsStoredProcedure(String strName);
        IEnumerable<string> GetStoredProcedure(String strName);

        void ProcessEntitiesList(string strQuery, Action<IDataRecord> action, bool bHandleConnection);

        Exception TryOnEntitiesList(string strQuery, Action<IDataRecord> action, string strMessageOnEmpty,
            bool bHandleConnection);

        IEnumerable<T> GetEntitiesList<T>(string strQuery, Func<IDataRecord, T> func, bool bHandleConnection);
        IEnumerable<T> GetEntitiesList<T>(IDbCommand cmd, Func<IDataRecord, T> func);

        IEnumerable<T> GetEntitiesListWithIndex<T>(string strQuery, Func<long, IDataRecord, T> func,
            bool bHandleConnection);

        IDictionary<K, V> GetEntitiesDictionary<K, V>(string strQuery, Func<IDataRecord, KeyValuePair<K, V>> func,
            bool bHandleConnection, string strExceptionMessage = null);

        IDictionary<K, V> GetEntitiesDictionary<K, V>(IDbCommand cmd, Func<IDataRecord, KeyValuePair<K, V>> func);

        V GetNextPagingStep<V>(string strQuery, Func<IDataRecord, V> func, long iPageSize, V @default,
            bool bHandleConnection, string strExceptionMessage = null);

        IDictionary<long, V> GetPagedEntitiesDictionary<V>(string strQuery, Func<IDataRecord, V> func, long iPageSize,
            bool bHandleConnection, string strExceptionMessage = null);

        void Run(Action<IDatabase> action);
        T Run<T>(Func<IDatabase, T> func);
        void RunInTransaction(Action<IDatabase> action);
        T RunInTransaction<T>(Func<IDatabase, T> func);

        IDatabase Clone();
    }
}