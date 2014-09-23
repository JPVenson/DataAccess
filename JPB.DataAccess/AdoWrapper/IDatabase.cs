using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper
{
    public interface IDatabase : IDisposable
    {
        bool IsAttached { get; }

        string ConnectionString { get; }
        string DatabaseFile { get; }
        string DatabaseName { get; }
        string ServerName { get; }

        IDataPager<T> CreatePager<T>();
        IUnGenericDataPager CreateUntypedPager(); 

        /// <summary>
        ///     Required
        ///     Is used to attach a Strategy that handles certain kinds of Databases
        /// </summary>
        /// <param name="strategy"></param>
        void Attach(IDatabaseStrategy strategy);

        void Detach();

        /// <summary>
        ///     Required
        ///     Is used to create an new Connection based on the Strategy and
        ///     keep it
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();

        /// <summary>
        ///     Required
        ///     Is used to create an new Transaction based on the Strategy
        /// </summary>
        /// <returns></returns>
        IDbTransaction GetTransaction();

        /// <summary>
        ///     Required
        ///     When a new Connection is requested this function is used
        /// </summary>
        /// <param name="bUseTransaction"></param>
        void Connect(bool bUseTransaction);

        void TransactionCommit();
        void TransactionRollback();

        /// <summary>
        ///     Required
        ///     Closing a open Connection
        /// </summary>
        void CloseConnection();

        int ExecuteNonQuery(string strSql, params object[] obj);
        int ExecuteNonQuery(IDbCommand cmd);

        /// <summary>
        ///     Required
        ///     Return the last inserted id based on the Strategy
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Required
        ///     Creates a Command based on the Strategy
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        IDbCommand CreateCommand(string strSql, params IDataParameter[] fields);

        /// <summary>
        ///     Required
        ///     Creates a Parameter based on the Strategy
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataParameter CreateParameter(string strName, object value);

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

        /// <summary>
        ///     Required
        ///     Execute a Query and map the result that is created with the func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strQuery"></param>
        /// <param name="func"></param>
        /// <param name="bHandleConnection"></param>
        /// <returns></returns>
        IEnumerable<T> GetEntitiesList<T>(string strQuery, Func<IDataRecord, T> func, bool bHandleConnection);

        /// <summary>
        ///     Required
        ///     Execute a Query and map the result that is created with the func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="func"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Required
        ///     Opens a Connection or reuse an existing one and then execute the action
        /// </summary>
        /// <param name="action"></param>
        void Run(Action<IDatabase> action);

        /// <summary>
        ///     Required
        ///     Opens a Connection or reuse an existing one and then execute the action
        /// </summary>
        /// <param name="action"></param>
        T Run<T>(Func<IDatabase, T> func);

        /// <summary>
        ///     Required
        ///     Opens a Connection or reuse an existing one and then execute the action
        /// </summary>
        /// <param name="action"></param>
        void RunInTransaction(Action<IDatabase> action);

        /// <summary>
        ///     Required
        ///     Opens a Connection or reuse an existing one and then execute the action
        /// </summary>
        /// <param name="action"></param>
        T RunInTransaction<T>(Func<IDatabase, T> func);

        IDatabase Clone();
    }
}