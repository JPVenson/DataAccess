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
        
        IDbCommand GetlastInsertedID_Cmd(IDbConnection conn);

        IDataPager<T> CreatePager<T>();

        IWrapperDataPager<T,TE> CreateConverterPager<T, TE>();
    }
}