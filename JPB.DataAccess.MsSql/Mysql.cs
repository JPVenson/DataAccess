using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Pager.Contracts;
using MySql.Data.MySqlClient;

namespace JPB.DataAccess.MySql
{
    internal class MySql : IDatabaseStrategy
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

        public string[] GetTables(IDbConnection conn, string strFilter)
        {
            throw new NotImplementedException();
        }

        public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
        {
            string sql = string.Format("SHOW COLUMNS FROM {0}", strTableName);
            using (var cmd = new MySqlCommand(sql, (MySqlConnection) conn))
            using (IDataReader dr = cmd.ExecuteReader())
            {
                var list = new List<string>();
                while (dr.Read())
                    list.Add((string) dr[0]);
                return list.ToArray();
            }
        }

        public int DropTable(IDbConnection conn, string strTableName)
        {
            throw new NotImplementedException();
        }

        public void CompactDatabase(string strSource, string strDest)
        {
            throw new NotImplementedException();
        }

        public void ShrinkDatabase(string strConnectionString)
        {
            throw new NotImplementedException();
        }

        public void Import(DataTable dt, IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public string GetTimeStamp()
        {
            throw new NotImplementedException();
        }

        public string GetTimeStamp(DateTime dtValue)
        {
            throw new NotImplementedException();
        }

        public void PrepareQuery(IDbConnection conn, string strSql)
        {
            throw new NotImplementedException();
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            return CreateCommand("SELECT LAST_INSERT_ID()", conn);
        }

        public string GetViewsSql(String strName)
        {
            return string.Format("SELECT name FROM sysobjects WHERE type in (N'V') AND name LIKE '{0}'", strName);
        }

        public string GetStoredProcedureSql(String strName)
        {
            return string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
        }

        public bool SupportsView(IDbConnection conn, String strName)
        {
            string sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'V') AND name LIKE '{0}'", strName);
            using (var cmd = new MySqlCommand(sql, (MySqlConnection) conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }

        public bool SupportsStoredProcedure(IDbConnection conn, String strName)
        {
            string sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
            using (var cmd = new MySqlCommand(sql, (MySqlConnection) conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }

        public IDataPager<T> CreatePager<T>()
        {
            throw new NotImplementedException();
        }

        public IUnGenericDataPager CreateUnmagedPager()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return new MySql(ConnectionString);
        }

        public IDbDataParameter CreateParameter_Bit(string strName, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.Bit);
        }

        public IDbDataParameter CreateParameter_Int(string strName, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.Int32);
        }

        public IDbDataParameter CreateParameter_SmallInt(string strName)
        {
            return new MySqlParameter(strName, MySqlDbType.Int24);
        }

        public IDbDataParameter CreateParameter_BigInt(string strName)
        {
            return new MySqlParameter(strName, MySqlDbType.Int64);
        }

        public IDbDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.VarChar);
        }

        public IDbDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.VarChar);
        }

        public IDbDataParameter CreateParameter_NVarChar_MAX(string strName)
        {
            return new MySqlParameter(strName, MySqlDbType.VarChar);
        }

        public IDbDataParameter CreateParameter_DateTime(string strName, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.DateTime);
        }

        public IDbDataParameter CreateParameter_Time(string strName, bool nullable = false)
        {
            return new MySqlParameter(strName, MySqlDbType.Time);
        }

        public IDbDataParameter CreateParameter_SmallDateTime(string strName)
        {
            return new MySqlParameter(strName, MySqlDbType.DateTime);
        }
    }
}