using System;
using System.Data;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper
{
    public interface IDatabaseStrategy : ICloneable
    {
        /// <summary>
        /// Defines the database type this Strategy is used for
        /// </summary>
        DbAccessType SourceDatabase { get; }

        /// <summary>
        /// An Valid Connection string for the given Strategy
        /// </summary>
        string ConnectionString { get; set; }
        /// <summary>
        /// Optional used when connecting to a Local file
        /// </summary>
        string DatabaseFile { get; }
        /// <summary>
        /// Should return the current database if availibe
        /// </summary>
        string ServerName { get; }


        IDbConnection CreateConnection();
        IDbCommand CreateCommand(string strSql, IDbConnection conn);
        IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields);
        IDataParameter CreateParameter(string strName, object value);

        IDbDataAdapter CreateDataAdapter(IDbCommand cmd);

        DataTable CreateDataTable(string name, IDbCommand cmd);
        
        IDbCommand GetlastInsertedID_Cmd(IDbConnection conn);

        IDataPager<T> CreatePager<T>();

        IWrapperDataPager<T,TE> CreateConverterPager<T, TE>();

        /// <summary>
        /// Formarts a Command into a Query after the Strategy rules
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        string FormartCommandToQuery(IDbCommand command);


        /// <summary>
        /// Converts the Generic DbType to the Specific represntation
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string ConvertParameter(DbType type);
    }
}