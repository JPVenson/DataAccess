using System;
using System.Data;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper
{
    public interface IDatabaseStrategy : ICloneable
    {
        string ConnectionString { get; set; }
        string DatabaseFile { get; }
        string ServerName { get; }
        IDbConnection CreateConnection();

        IDbCommand CreateCommand(string strSql, IDbConnection conn);
        IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDataParameter[] fields);
        IDataParameter CreateParameter(string strName, object value);

        IDbDataAdapter CreateDataAdapter(IDbCommand cmd);

        DataTable CreateDataTable(string name, IDbCommand cmd);

        void Import(DataTable dt, IDbCommand cmd);

        string GetTimeStamp();
        string GetTimeStamp(DateTime dtValue);

        string[] GetTables(IDbConnection conn, String strFilter);
        string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude);
        int DropTable(IDbConnection conn, String strTableName);

        void CompactDatabase(string strSource, string strDest);
        void ShrinkDatabase(string strConnectionString);
        void PrepareQuery(IDbConnection conn, string strSql);

        IDbCommand GetlastInsertedID_Cmd(IDbConnection conn);
        string GetViewsSql(String strName);
        string GetStoredProcedureSql(String strName);

        bool SupportsView(IDbConnection conn, String strName);
        bool SupportsStoredProcedure(IDbConnection conn, String strName);

        IDataPager<T> CreatePager<T>();

        IUnGenericDataPager CreateUnmagedPager();
    }
}