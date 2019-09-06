using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using Oracle.ManagedDataAccess.Client;

namespace JPB.DataAccess.Oracle
{
    public class Oracle : IDatabaseStrategy
    {
        public Oracle(string strConnStr)
        {
            ConnectionString = strConnStr;
        }

        public string ConnectionString { get; set; }

        public string DatabaseFile { get; private set; }
        public string ServerName { get; private set; }

        public DbAccessType SourceDatabase
        {
            get
            {
                return DbAccessType.MsSql; //.Oracle;
            }
        }

        public object Clone()
        {
            return new Oracle(ConnectionString);
        }

        public void CloseAllConnections()
        {
            OracleConnection.ClearAllPools();
        }

        public string ConvertParameter(DbType type)
        {
            return new OracleParameter { DbType = type }.OracleDbType.ToString();
        }
        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            return new OracleCommand(strSql, conn as OracleConnection);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
        {
            var oracleCommand = new OracleCommand(strSql, conn as OracleConnection);
            foreach (var dbDataParameter in fields)
            {
                oracleCommand.Parameters.Add(dbDataParameter);
            }

            return oracleCommand;
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(ConnectionString);
        }

        public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
        {
            return new OracleDataConverterPager<T, TE>();
        }

        public IDataPager<T> CreatePager<T>()
        {
            return new OracleDataPager<T>();
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return new OracleParameter(strName, value);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            return new OracleDataAdapter(cmd as OracleCommand);
        }

        public IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
        {
            return null;
        }

        public IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
        {
            return null;
        }

        public string FormartCommandToQuery(IDbCommand command)
        {
            return "";
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            throw new NotImplementedException("This Functionalty is not supported by the Oracle database.");
        }
    }
}
