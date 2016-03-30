/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	///     Wrapps MsSQL spezifc data
	/// </summary>
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

		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.MsSql; }
		}

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
				var cn = (SqlConnection) CreateConnection();
				return cn.DataSource;
			}
		}

		public IDbConnection CreateConnection()
		{
			var sqlConnection = new SqlConnection(_connStr);
			return sqlConnection;
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			var cmd = new SqlCommand(strSql);
			cmd.Connection = (SqlConnection) conn;
			return cmd;
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var cmd = (SqlCommand) CreateCommand(strSql, conn);

			foreach (IDataParameter dataParameter in fields)
			{
				cmd.Parameters.AddWithValue(dataParameter.ParameterName, dataParameter.Value);
			}

			return cmd;
		}

		public IDataParameter CreateParameter(string strName, object value)
		{
			return new SqlParameter(strName, value);
		}

		public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
		{
			var adapter = new SqlDataAdapter();
			adapter.SelectCommand = (SqlCommand) cmd;
			return adapter;
		}

		public DataTable CreateDataTable(string name, IDbCommand cmd)
		{
			using (var adapter = new SqlDataAdapter())
			{
				adapter.SelectCommand = (SqlCommand) cmd;

				var table = new DataTable(name);
				adapter.Fill(table);

				cmd.Dispose();
				adapter.Dispose();

				return table;
			}
		}

		public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
		{
			//return this.CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
			return CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
		}

		public IDataPager<T> CreatePager<T>()
		{
			return new MsSqlDataPager<T>();
		}

		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			return new MsSqlDataConverterPager<T, TE>();
		}

		public string FormartCommandToQuery(IDbCommand command)
		{
			return CommandAsMsSql(command);
		}

		public string ConvertParameter(DbType type)
		{
			return new SqlParameter {DbType = type}.SqlDbType.ToString();
		}

		public object Clone()
		{
			return new MsSql(_connStr);
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

		public void Import(DataTable dt, IDbCommand cmd)
		{
			using (var adapter = new SqlDataAdapter())
			{
				adapter.SelectCommand = (SqlCommand) cmd;

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
			var dt = dtValue;

			return string.Format(
				"CONVERT(datetime,'{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}',120)",
				dt.Year, dt.Month, dt.Day,
				dt.Hour, dt.Minute, dt.Second);
		}

		public string[] GetTables(IDbConnection conn, String strFilter)
		{
			const string sql = "select NAME from SYSOBJECTS where TYPE = 'U' AND NAME <> 'dtproperties' order by NAME";
			using (var cmd = new SqlCommand(sql, (SqlConnection) conn))
			using (IDataReader dr = cmd.ExecuteReader())
			{
				var list = new List<string>();
				while (dr.Read())
					list.Add((string) dr[0]);
				return list.ToArray();
			}
		}

		public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
		{
			var sql = string.Format(
				"select NAME from SYSCOLUMNS where ID=(select ID from SYSOBJECTS where TYPE = 'U' AND NAME = '{0}')",
				strTableName);
			using (var cmd = new SqlCommand(sql, (SqlConnection) conn))
			using (IDataReader dr = cmd.ExecuteReader())
			{
				var list = new List<string>();
				while (dr.Read())
					list.Add((string) dr[0]);
				return list.ToArray();
			}
		}

		public int DropTable(IDbConnection conn, String strTableName)
		{
			var sql = String.Format("DROP TABLE {0}", strTableName);
			using (var cmd = new SqlCommand(sql, (SqlConnection) conn))
				return cmd.ExecuteNonQuery();
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
			var sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'V') AND name LIKE '{0}'", strName);
			using (var cmd = new SqlCommand(sql, (SqlConnection) conn))
			using (IDataReader dr = cmd.ExecuteReader())
				return (dr.Read());
		}

		public bool SupportsStoredProcedure(IDbConnection conn, String strName)
		{
			var sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
			using (var cmd = new SqlCommand(sql, (SqlConnection) conn))
			using (IDataReader dr = cmd.ExecuteReader())
				return (dr.Read());
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public static String CommandAsMsSql(IDbCommand sc)
		{
			if (!(sc is SqlCommand))
				return sc.CommandText;

			var sql = new StringBuilder();
			var firstParam = true;

			if (!string.IsNullOrEmpty(sc.Connection.Database))
				sql.AppendLine("USE  [" + sc.Connection.Database + "];");

			switch (sc.CommandType)
			{
				case CommandType.StoredProcedure:
					sql.AppendLine("DECLARE @return_value int;");

					foreach (SqlParameter sp in sc.Parameters.Cast<SqlParameter>())
					{
						if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
						{
							sql.Append("DECLARE " + sp.ParameterName + "\t" + sp.SqlDbType + "\t= ");

							sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "NULL" : QueryDebugger.ParameterValue(sp)) + ";");
						}
					}

					sql.AppendLine("EXEC [" + sc.CommandText + "]");

					foreach (IDataParameter sp in sc.Parameters.Cast<IDataParameter>())
					{
						if (sp.Direction != ParameterDirection.ReturnValue)
						{
							sql.Append((firstParam) ? "\t" : "\t, ");

							if (firstParam) firstParam = false;

							if (sp.Direction == ParameterDirection.Input)
								sql.AppendLine(sp.ParameterName + " = " + QueryDebugger.ParameterValue(sp));
							else

								sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " OUTPUT");
						}
					}
					sql.AppendLine(";");

					sql.AppendLine("SELECT 'Return Value' = CONVERT(NVARCHAR, @return_value);");

					foreach (IDataParameter sp in sc.Parameters.Cast<IDataParameter>())
					{
						if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
						{
							sql.AppendLine("SELECT '" + sp.ParameterName + "' = CONVERT(NVARCHAR, " + sp.ParameterName + ");");
						}
					}
					break;
				case CommandType.Text:
				case CommandType.TableDirect:
					foreach (SqlParameter sp in sc.Parameters.Cast<SqlParameter>())
					{
						var paramTypeCompiler = sp.SqlDbType.ToString().ToUpper();
						if (sp.Size > 0)
						{
							paramTypeCompiler += "(" + sp.Size + ")";
						}

						sql.AppendLine("DECLARE " + " "
						               + sp.ParameterName + " "
						               + paramTypeCompiler + " = "
						               + ParameterValue(sp) + ";");
					}

					sql.AppendLine(sc.CommandText);
					break;
			}

			return sql.ToString();
		}

		public static String ParameterValue(SqlParameter sp)
		{
			var retval = "";

			switch (sp.SqlDbType)
			{
				case SqlDbType.BigInt:
				case SqlDbType.Decimal:
				case SqlDbType.Float:
				case SqlDbType.Int:
				case SqlDbType.Real:
				case SqlDbType.TinyInt:
				case SqlDbType.Money:
				case SqlDbType.SmallInt:
				case SqlDbType.SmallMoney:
				case SqlDbType.Udt:
					retval = sp.Value.ToString().Replace("'", "''");
					break;

				case SqlDbType.Bit:
					retval = (sp.Value is bool && (bool) sp.Value) ? "1" : "0";
					break;

				case SqlDbType.Binary:
				case SqlDbType.Char:
				case SqlDbType.DateTime:
				case SqlDbType.Image:
				case SqlDbType.NChar:
				case SqlDbType.NText:
				case SqlDbType.NVarChar:
				case SqlDbType.UniqueIdentifier:
				case SqlDbType.SmallDateTime:
				case SqlDbType.Text:
				case SqlDbType.Timestamp:
				case SqlDbType.VarBinary:
				case SqlDbType.VarChar:
				case SqlDbType.Variant:
				case SqlDbType.Xml:
				case SqlDbType.Structured:
				case SqlDbType.Date:
				case SqlDbType.Time:
				case SqlDbType.DateTime2:
				case SqlDbType.DateTimeOffset:
					retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
					break;
				default:
					retval = sp.Value.ToString().Replace("'", "''");
					break;
			}

			return retval;
		}

		#endregion
	}
}