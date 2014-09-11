using System;
using System.Data;

namespace JPB.DataAccess.AdoWrapper
{
    public interface IDatabaseStrategy : ICloneable
    {
        string ConnectionString { get; }
        string DatabaseFile { get; }
        string ServerName { get; }
        IDbConnection CreateConnection();

        IDbCommand CreateCommand(string strSql, IDbConnection conn);
        IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDbDataParameter[] fields);

        //IDbDataParameter CreateParameter_Bit(string strName, bool nullable = false);
        //IDbDataParameter CreateParameter_Int(string strName, bool nullable = false);
        //IDbDataParameter CreateParameter_SmallInt(string strName);
        //IDbDataParameter CreateParameter_BigInt(string strName);
        //IDbDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false);
        //IDbDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false);
        //IDbDataParameter CreateParameter_NVarChar_MAX(string strName);
        //IDbDataParameter CreateParameter_DateTime(string strName, bool nullable = false);
        //IDbDataParameter CreateParameter_Time(string strName, bool nullable = false);
        //IDbDataParameter CreateParameter_SmallDateTime(string strName);

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
    }
}