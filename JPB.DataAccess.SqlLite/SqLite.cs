/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.SqLite
{
	/// <summary>
	///     Wrapps MsSQL spezifc data
	/// </summary>
	public class SqLite : IDatabaseStrategy
	{
		private string _connStr = string.Empty;
		
		public SqLite(string strConnStr)
		{
			_connStr = strConnStr;
		}

		#region IDatabaseStrategy Members

		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.SqLite; }
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
				var cn = (SQLiteConnection)CreateConnection();
				return cn.DataSource;
			}
		}

		public IDbConnection CreateConnection()
		{
			var SQLiteConnection = new SQLiteConnection(_connStr);
			return SQLiteConnection;
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			var cmd = new SQLiteCommand(strSql);
			cmd.Connection = (SQLiteConnection)conn;
			return cmd;
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var cmd = (SQLiteCommand)CreateCommand(strSql, conn);

			foreach (IDataParameter dataParameter in fields)
			{
				cmd.Parameters.AddWithValue(dataParameter.ParameterName, dataParameter.Value);
			}

			return cmd;
		}

		public IDataParameter CreateParameter(string strName, object value)
		{
			return new SQLiteParameter(strName, value);
		}

		public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
		{
			var adapter = new SQLiteDataAdapter();
			adapter.SelectCommand = (SQLiteCommand)cmd;
			return adapter;
		}

		public DataTable CreateDataTable(string name, IDbCommand cmd)
		{
			using (var adapter = new SQLiteDataAdapter())
			{
				adapter.SelectCommand = (SQLiteCommand)cmd;

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
			return CreateCommand("SELECT last_insert_rowid() as Value", conn);
		}

		public IDataPager<T> CreatePager<T>()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public string FormartCommandToQuery(IDbCommand command)
		{
			return CommandAsMsSql(command);
		}

		public string ConvertParameter(DbType type)
		{
			return new SQLiteParameter { DbType = type }.DbType.ToString();
		}

		public object Clone()
		{
			return new SqLite(_connStr);
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
			using (var adapter = new SQLiteDataAdapter())
			{
				adapter.SelectCommand = (SQLiteCommand)cmd;

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
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
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
			var sql = string.Format(
				"select NAME from SYSCOLUMNS where ID=(select ID from SYSOBJECTS where TYPE = 'U' AND NAME = '{0}')",
				strTableName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
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
			var sql = String.Format("DROP TABLE {0}", strTableName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
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
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			using (IDataReader dr = cmd.ExecuteReader())
				return (dr.Read());
		}

		public bool SupportsStoredProcedure(IDbConnection conn, String strName)
		{
			var sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			using (IDataReader dr = cmd.ExecuteReader())
				return (dr.Read());
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public static String CommandAsMsSql(IDbCommand sc)
		{
			if (!(sc is SQLiteCommand))
				return sc.CommandText;

			var sql = new StringBuilder();
			var firstParam = true;

			if (!string.IsNullOrEmpty(sc.Connection.Database))
				sql.AppendLine("USE  [" + sc.Connection.Database + "];");

			switch (sc.CommandType)
			{
				case CommandType.StoredProcedure:
					sql.AppendLine("DECLARE @return_value int;");

					foreach (SQLiteParameter sp in sc.Parameters.Cast<SQLiteParameter>())
					{
						if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
						{
							sql.Append("DECLARE " + sp.ParameterName + "\t" + sp.DbType + "\t= ");

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
					foreach (SQLiteParameter sp in sc.Parameters.Cast<SQLiteParameter>())
					{
						var paramTypeCompiler = sp.DbType.ToString().ToUpper();
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

		public static String ParameterValue(SQLiteParameter sp)
		{
			return sp.DbType.ToString().ToUpper();
		}

		#endregion
	}
}