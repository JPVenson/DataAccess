using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Pager.Contracts;
using MySql.Data.MySqlClient;

namespace JPB.DataAccess.MySql
{
    public class MySql : IDatabaseStrategy
    {
        private const string TEMPLATE_MSSQL_UNTRUSTED =
            "server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5";

        private const string TEMPLATE_MSSQL_TRUSTED =
            "server={0};database={1};Connect Timeout=100;Min Pool Size=5";

        public MySql(string strServer, string strDatabase)
        {
            ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
        }

        public MySql(string strServer, string strDatabase, string strLogin, string strPassword)
        {
            if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
                ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
            else
            {
                ConnectionString = string.Format(TEMPLATE_MSSQL_UNTRUSTED, strServer.Trim(), strDatabase.Trim(),
                    strLogin.Trim(), strPassword.Trim());
            }
        }

        public MySql(string strConnStr)
        {
            ConnectionString = strConnStr;
        }

        public string ConnectionString { get; set; }

        public string DatabaseFile { get; private set; }
        public string ServerName { get; private set; }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            return new MySqlCommand(strSql, conn as MySqlConnection);
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDataParameter[] fields)
        {
            var mySqlCommand = new MySqlCommand(strSql, conn as MySqlConnection);
            foreach (var dbDataParameter in fields)
            {
                mySqlCommand.Parameters.Add(dbDataParameter);
            }

            return mySqlCommand;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return new MySqlParameter(strName, value);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            return new MySqlDataAdapter(cmd as MySqlCommand);
        }

        public DataTable CreateDataTable(string name, IDbCommand cmd)
        {
            var adapter = CreateDataAdapter(cmd) as MySqlDataAdapter;
            var table = new DataTable(name);
            adapter.Fill(table);

            cmd.Dispose();
            adapter.Dispose();

            return table;
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            return CreateCommand("SELECT * FROM LAST_INSERT_ID();", conn);
        }

        public IDataPager<T> CreatePager<T>()
        {
            return new MySqlDataPager<T>();
        }

        public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
        {
            return new MySqlDataConverterPager<T, TE>();
        }

        public string FormartCommandToQuery(IDbCommand command)
        {
            return command.ToString();
        }

        public object Clone()
        {
            return new MySql(ConnectionString);
        }
    }
}