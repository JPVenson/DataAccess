using System;
using System.Data;
using System.Data.OleDb;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper.OleDB
{
    public class OleDb : IDatabaseStrategy
    {
        private const string TEMPLATE_MSSQL_UNTRUSTED =
            "server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

        private const string TEMPLATE_MSSQL_TRUSTED =
            "server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";

        public string FormartCommandToQuery(IDbCommand command)
        {
            return command.ToString();
        }

        public OleDb(string strServer, string strDatabase)
        {
            ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
        }

        public OleDb(string strServer, string strDatabase, string strLogin, string strPassword)
        {
            if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
                ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
            else
            {
                ConnectionString = string.Format(TEMPLATE_MSSQL_UNTRUSTED, strServer.Trim(), strDatabase.Trim(),
                    strLogin.Trim(), strPassword.Trim());
            }
        }

        public OleDb(string strConnStr)
        {
            ConnectionString = strConnStr;
        }

        public object Clone()
        {
            return new OleDb(ConnectionString);
        }

        public DbAccessType SourceDatabase
        {
            get
            {
                return DbAccessType.OleDb;
            }
        }

        public string ConnectionString { get; set; }
        public string DatabaseFile { get; private set; }

        public string ServerName
        {
            get
            {
                var cn = (OleDbConnection)CreateConnection();
                return cn.DataSource;
            }
        }

        public IDbConnection CreateConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            return new OleDbCommand(strSql, (OleDbConnection)(conn is OleDbConnection ? conn : CreateConnection()));
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDataParameter[] fields)
        {
            var oleDbCommand = new OleDbCommand(strSql,
                (OleDbConnection)(conn is OleDbConnection ? conn : CreateConnection()));

            foreach (var dbDataParameter in fields)
            {
                oleDbCommand.Parameters.AddWithValue(dbDataParameter.ParameterName, dbDataParameter.Value);
            }
            return oleDbCommand;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return new OleDbParameter(strName, value);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            return new OleDbDataAdapter(cmd as OleDbCommand);
        }

        public DataTable CreateDataTable(string name, IDbCommand cmd)
        {
            using (var adapter = new OleDbDataAdapter())
            {
                adapter.SelectCommand = (OleDbCommand)cmd;

                var table = new DataTable(name);
                adapter.Fill(table);

                cmd.Dispose();
                adapter.Dispose();

                return table;
            }
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        public IDataPager<T> CreatePager<T>()
        {
            throw new NotImplementedException();
        }

        public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
        {
            throw new NotImplementedException();
        }
    }
}