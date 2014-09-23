using System;
using System.Data;
using System.Data.OleDb;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.OleDB
{
    public class OleDb : IDatabaseStrategy
    {
        private const string TEMPLATE_MSSQL_UNTRUSTED =
            "server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

        private const string TEMPLATE_MSSQL_TRUSTED =
            "server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";


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

        public string ConnectionString { get; set; }
        public string DatabaseFile { get; private set; }

        public string ServerName
        {
            get
            {
                var cn = (OleDbConnection) CreateConnection();
                return cn.DataSource;
            }
        }

        public IDbConnection CreateConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            return new OleDbCommand(strSql, (OleDbConnection) (conn is OleDbConnection ? conn : CreateConnection()));
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDataParameter[] fields)
        {
            var oleDbCommand = new OleDbCommand(strSql,
                (OleDbConnection) (conn is OleDbConnection ? conn : CreateConnection()));

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
                adapter.SelectCommand = (OleDbCommand) cmd;

                var table = new DataTable(name);
                adapter.Fill(table);

                cmd.Dispose();
                adapter.Dispose();

                return table;
            }
        }

        public void Import(DataTable dt, IDbCommand cmd)
        {
            using (var adapter = new OleDbDataAdapter())
            {
                adapter.SelectCommand = (OleDbCommand) cmd;

                foreach (DataRow row in dt.Rows)
                    row.SetAdded();

                adapter.Update(dt);
            }
        }

        public string GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        public string GetTimeStamp(DateTime dtValue)
        {
            DateTime dt = dtValue;

            return string.Format(
                "CONVERT(datetime,'{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}',120)",
                dt.Year, dt.Month, dt.Day,
                dt.Hour, dt.Minute, dt.Second);
        }

        public string[] GetTables(IDbConnection conn, string strFilter)
        {
            throw new NotImplementedException();
        }

        public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string GetViewsSql(string strName)
        {
            throw new NotImplementedException();
        }

        public string GetStoredProcedureSql(string strName)
        {
            throw new NotImplementedException();
        }

        public bool SupportsView(IDbConnection conn, string strName)
        {
            throw new NotImplementedException();
        }

        public bool SupportsStoredProcedure(IDbConnection conn, string strName)
        {
            throw new NotImplementedException();
        }

        public IDataPager<T> CreatePager<T>()
        {
            throw new NotImplementedException();
        }

        public IUnGenericDataPager CreateUnmagedPager()
        {
            throw new NotImplementedException();
        }
    }
}