using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess.SqlLite
{
    public class SqLite : IDatabaseStrategy
    {
        public SqLite(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public object Clone()
        {
            return new SqLite(ConnectionString);
        }

        public string ConnectionString { get; set; }
        public string DatabaseFile { get; private set; }
        public string ServerName { get; private set; }

        public IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            return new SQLiteCommand(strSql, (SQLiteConnection) conn);
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDbDataParameter[] fields)
        {
            var sqLiteCommand = new SQLiteCommand(strSql, (SQLiteConnection) conn);
            foreach (IDbDataParameter dbDataParameter in fields)
            {
                sqLiteCommand.Parameters.AddWithValue(dbDataParameter.ParameterName, dbDataParameter.Value);
            }
            return sqLiteCommand;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return new SQLiteParameter(strName, value);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            return new SQLiteDataAdapter((SQLiteCommand) cmd);
        }

        public DataTable CreateDataTable(string name, IDbCommand cmd)
        {
            var adapter = (SQLiteDataAdapter) CreateDataAdapter(cmd);
            var table = new DataTable(name);
            adapter.Fill(table);

            cmd.Dispose();
            adapter.Dispose();
            return table;
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

        public string[] GetTables(IDbConnection conn, string strFilter)
        {
            throw new NotImplementedException();
        }

        public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
        {
            string sql = string.Format("SHOW COLUMNS FROM {0}", strTableName);
            using (IDbCommand cmd = CreateCommand(sql, conn))
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

        public void PrepareQuery(IDbConnection conn, string strSql)
        {
            throw new NotImplementedException();
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            return CreateCommand("SELECT LAST_INSERT_ID()", conn);
        }

        public string GetViewsSql(string strName)
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
            using (IDbCommand cmd = CreateCommand(sql, conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }

        public bool SupportsStoredProcedure(IDbConnection conn, String strName)
        {
            string sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
            using (IDbCommand cmd = CreateCommand(sql, conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }
    }
}