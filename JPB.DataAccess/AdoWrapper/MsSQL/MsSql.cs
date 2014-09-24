using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper.MsSql
{
    public class MsSql : IDatabaseStrategy
    {
        private const string TEMPLATE_MSSQL_UNTRUSTED =
            "server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

        private const string TEMPLATE_MSSQL_TRUSTED =
            "server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";

        private string _connStr = string.Empty;

        public MsSql(string strServer, string strDatabase)
        {
            _connStr = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
        }

        public MsSql(string strServer, string strDatabase, string strLogin, string strPassword)
        {
            if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
                _connStr = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
            else
            {
                _connStr = string.Format(TEMPLATE_MSSQL_UNTRUSTED, strServer.Trim(), strDatabase.Trim(),
                    strLogin.Trim(), strPassword.Trim());
            }
        }

        public MsSql(string strConnStr)
        {
            _connStr = strConnStr;
        }

        #region IDatabaseStrategy Members

        public string ConnectionString
        {
            get { return _connStr; }
            set { _connStr = value; }
        }

        public string DatabaseFile
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string ServerName
        {
            get
            {
                var cn = (SqlConnection)CreateConnection();
                return cn.DataSource;
            }
        }

        public void CompactDatabase(string strSource, string strDest)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void ShrinkDatabase(string strConnectionString)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void PrepareQuery(IDbConnection conn, string strSql)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connStr);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            var cmd = new SqlCommand(strSql);
            cmd.Connection = (SqlConnection)conn;
            return cmd;
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDataParameter[] fields)
        {
            var cmd = (SqlCommand) CreateCommand(strSql, conn);

            foreach (var dataParameter in fields)
            {
                cmd.Parameters.AddWithValue(dataParameter.ParameterName, dataParameter.Value);
            }

            return cmd;
        }

        public IDataParameter CreateParameter(string strName, object value)
        {
            return new SqlParameter(strName, value);
        }

        //public IDbDataParameter CreateParameter_Bit(string strName, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.Bit) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.VarChar, iSize) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.NVarChar, iSize) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_NVarChar_MAX(string strName)
        //{
        //    return new SqlParameter(strName, SqlDbType.NVarChar);
        //}

        //public IDbDataParameter CreateParameter_Int(string strName, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.Int) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_SmallInt(string strName)
        //{
        //    return new SqlParameter(strName, SqlDbType.SmallInt);
        //}

        //public IDbDataParameter CreateParameter_BigInt(string strName)
        //{
        //    return new SqlParameter(strName, SqlDbType.BigInt);
        //}

        //public IDbDataParameter CreateParameter_DateTime(string strName, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.DateTime) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_Time(string strName, bool nullable = false)
        //{
        //    return new SqlParameter(strName, SqlDbType.Time) { IsNullable = nullable };
        //}

        //public IDbDataParameter CreateParameter_SmallDateTime(string strName)
        //{
        //    return new SqlParameter(strName, SqlDbType.SmallDateTime);
        //}

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            var adapter = new SqlDataAdapter();
            adapter.SelectCommand = (SqlCommand)cmd;
            return adapter;
        }

        public DataTable CreateDataTable(string name, IDbCommand cmd)
        {
            using (var adapter = new SqlDataAdapter())
            {
                adapter.SelectCommand = (SqlCommand)cmd;

                var table = new DataTable(name);
                adapter.Fill(table);

                cmd.Dispose();
                adapter.Dispose();

                return table;
            }
        }

        public void Import(DataTable dt, IDbCommand cmd)
        {
            using (var adapter = new SqlDataAdapter())
            {
                adapter.SelectCommand = (SqlCommand)cmd;

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

        public string[] GetTables(IDbConnection conn, String strFilter)
        {
            const string sql = "select NAME from SYSOBJECTS where TYPE = 'U' AND NAME <> 'dtproperties' order by NAME";
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn))
            using (IDataReader dr = cmd.ExecuteReader())
            {
                var list = new List<string>();
                while (dr.Read())
                    list.Add((string)dr[0]);
                return list.ToArray();
            }
        }

        public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
        {
            string sql = string.Format(
                "select NAME from SYSCOLUMNS where ID=(select ID from SYSOBJECTS where TYPE = 'U' AND NAME = '{0}')",
                strTableName);
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn))
            using (IDataReader dr = cmd.ExecuteReader())
            {
                var list = new List<string>();
                while (dr.Read())
                    list.Add((string)dr[0]);
                return list.ToArray();
            }
        }

        public int DropTable(IDbConnection conn, String strTableName)
        {
            string sql = String.Format("DROP TABLE {0}", strTableName);
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn))
                return cmd.ExecuteNonQuery();
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            //return this.CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
            return CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
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
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }

        public bool SupportsStoredProcedure(IDbConnection conn, String strName)
        {
            string sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn))
            using (IDataReader dr = cmd.ExecuteReader())
                return (dr.Read());
        }

        public IDataPager<T> CreatePager<T>()
        {
            return new MsSqlDataPager<T>();
        }

        public IUnGenericDataPager CreateUnmagedPager()
        {
            return new MsSqlUntypedDataPager();
        }

        public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
        {
            return new MsSqlDataConverterPager<T, TE>();
        }

        public object Clone()
        {
            return new MsSql(_connStr);
        }

        #endregion
    }
}