/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.SqLite
{
	/// <summary>
	/// Wrapps MsSQL spezifc data
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.IDatabaseStrategy" />
	public class SqLite : IDatabaseStrategy
	{
		static SqLite()
		{
			ConnectionCounter = new ConcurrentDictionary<string, SqLiteConnectionCounter>();
		}

		internal static ConcurrentDictionary<string, SqLiteConnectionCounter> ConnectionCounter { get; }

		/// <summary>
		/// The connection string
		/// </summary>
		private string _connStr;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqLite"/> class.
		/// </summary>
		/// <param name="strConnStr">The string connection string.</param>
		public SqLite(string strConnStr)
		{
			_connStr = strConnStr;
		}

		#region IDatabaseStrategy Members

		/// <summary>
		/// Defines the database type this Strategy is used for
		/// </summary>
		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.SqLite; }
		}

		/// <summary>
		/// An Valid Connection string for the given Strategy
		/// </summary>
		public string ConnectionString
		{
			get { return _connStr; }
			set { _connStr = value; }
		}

		private static Regex _fileNameRegex = new Regex("[.]*Data Source=(.*);");

		/// <summary>
		/// Optional used when connecting to a Local file
		/// </summary>
		public string DatabaseFile
		{
			get { return _fileNameRegex.Match(ConnectionString)?.Groups[1]?.Value; }
		}

		/// <summary>
		/// Should return the current database if availibe
		/// </summary>
		public string ServerName
		{
			get
			{
				using (var cn = (SQLiteConnection)CreateConnection())
				{
					return cn.DataSource;
				}
			}
		}

		/// <summary>
		/// Creates a new Provider specific Connection that will held open until all actors want to close it
		/// </summary>
		/// <returns></returns>
		public IDbConnection CreateConnection()
		{
			var sqLiteConnection = new SQLiteConnection(_connStr);
			var counter = ConnectionCounter.GetOrAdd(DatabaseFile, s => new SqLiteConnectionCounter(s));
			counter.AddConnection(sqLiteConnection);
			return sqLiteConnection;
		}

		/// <summary>
		/// Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			var cmd = new SQLiteCommand(strSql);
			cmd.Connection = (SQLiteConnection)conn;
			return cmd;
		}

		/// <summary>
		/// Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var cmd = (SQLiteCommand)CreateCommand(strSql, conn);

			foreach (IDataParameter dataParameter in fields)
			{
				cmd.Parameters.AddWithValue(dataParameter.ParameterName, dataParameter.Value);
			}

			return cmd;
		}

		/// <summary>
		/// Creates a query parameter.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public IDataParameter CreateParameter(string strName, object value)
		{
			return new SQLiteParameter(strName, value);
		}

		/// <summary>
		/// Creates the data adapter.
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
		{
			var adapter = new SQLiteDataAdapter();
			adapter.SelectCommand = (SQLiteCommand)cmd;
			return adapter;
		}

		/// <summary>
		/// Creates a data table.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Getlasts a inserted identifier command.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
		{
			//return this.CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
			return CreateCommand("SELECT last_insert_rowid() as Value", conn);
		}

		/// <summary>
		/// Creates a data pager.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDataPager<T> CreatePager<T>()
		{
			return new SqLiteDataPager<T>();
		}

		/// <summary>
		/// Creates the a pager that can convert each item.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <returns></returns>
		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			return new SqLiteDataConverterPager<T, TE>();
		}

		/// <summary>
		/// Formarts a Command into a QueryCommand after the Strategy rules
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public string FormartCommandToQuery(IDbCommand command)
		{
			return CommandAsMsSql(command);
		}

		/// <summary>
		/// Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string ConvertParameter(DbType type)
		{
			return new SQLiteParameter { DbType = type }.DbType.ToString();
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return new SqLite(_connStr);
		}

		/// <summary>
		/// Compacts the database.
		/// </summary>
		/// <param name="strSource">The string source.</param>
		/// <param name="strDest">The string dest.</param>
		/// <exception cref="NotImplementedException">The method or operation is not implemented.</exception>
		public void CompactDatabase(string strSource, string strDest)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// Shrinks the database.
		/// </summary>
		/// <param name="strConnectionString">The string connection string.</param>
		/// <exception cref="NotImplementedException">The method or operation is not implemented.</exception>
		public void ShrinkDatabase(string strConnectionString)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// Prepares the query.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <exception cref="NotImplementedException">The method or operation is not implemented.</exception>
		public void PrepareQuery(IDbConnection conn, string strSql)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// Imports the specified dt.
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="cmd">The command.</param>
		public void Import(DataTable dt, IDbCommand cmd)
		{
			using (var adapter = new SQLiteDataAdapter())
			{
				adapter.SelectCommand = (SQLiteCommand)cmd;

				foreach (DataRow row in dt.Rows)
				{
					row.SetAdded();
				}

				adapter.Update(dt);
			}
		}

		/// <summary>
		/// Gets the time stamp.
		/// </summary>
		/// <returns></returns>
		public string GetTimeStamp()
		{
			return GetTimeStamp(DateTime.Now);
		}

		/// <summary>
		/// Gets the time stamp.
		/// </summary>
		/// <param name="dtValue">The dt value.</param>
		/// <returns></returns>
		public string GetTimeStamp(DateTime dtValue)
		{
			var dt = dtValue;

			return string.Format(
				"CONVERT(datetime,'{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}',120)",
				dt.Year, dt.Month, dt.Day,
				dt.Hour, dt.Minute, dt.Second);
		}

		/// <summary>
		/// Gets the tables.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strFilter">The string filter.</param>
		/// <returns></returns>
		public string[] GetTables(IDbConnection conn, String strFilter)
		{
			const string sql = "select NAME from SYSOBJECTS where TYPE = 'U' AND NAME <> 'dtproperties' order by NAME";
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			using (IDataReader dr = cmd.ExecuteReader())
			{
				var list = new List<string>();
				while (dr.Read())
				{
					list.Add((string)dr[0]);
				}
				return list.ToArray();
			}
		}

		/// <summary>
		/// Gets the table columns.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strTableName">Name of the string table.</param>
		/// <param name="exclude">The exclude.</param>
		/// <returns></returns>
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
				{
					list.Add((string)dr[0]);
				}
				return list.ToArray();
			}
		}

		/// <summary>
		/// Drops the table.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strTableName">Name of the string table.</param>
		/// <returns></returns>
		public int DropTable(IDbConnection conn, String strTableName)
		{
			var sql = String.Format("DROP TABLE {0}", strTableName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Gets the views SQL.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <returns></returns>
		public string GetViewsSql(String strName)
		{
			return string.Format("SELECT name FROM sysobjects WHERE type in (N'V') AND name LIKE '{0}'", strName);
		}

		/// <summary>
		/// Gets the stored procedure SQL.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <returns></returns>
		public string GetStoredProcedureSql(String strName)
		{
			return string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
		}

		/// <summary>
		/// Supportses the view.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strName">Name of the string.</param>
		/// <returns></returns>
		public bool SupportsView(IDbConnection conn, String strName)
		{
			var sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'V') AND name LIKE '{0}'", strName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			using (IDataReader dr = cmd.ExecuteReader())
			{
				return (dr.Read());
			}
		}

		/// <summary>
		/// Supportses the stored procedure.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <param name="strName">Name of the string.</param>
		/// <returns></returns>
		public bool SupportsStoredProcedure(IDbConnection conn, String strName)
		{
			var sql = string.Format("SELECT name FROM sysobjects WHERE type in (N'P') AND name LIKE '{0}'", strName);
			using (var cmd = new SQLiteCommand(sql, (SQLiteConnection)conn))
			using (IDataReader dr = cmd.ExecuteReader())
			{
				return (dr.Read());
			}
		}

		/// <summary>
		/// Commands as ms SQL.
		/// </summary>
		/// <param name="sc">The sc.</param>
		/// <returns></returns>
		public static String CommandAsMsSql(IDbCommand sc)
		{
			if (!(sc is SQLiteCommand))
			{
				return sc.CommandText;
			}

			var sql = new StringBuilder();
			var firstParam = true;

			if (!string.IsNullOrEmpty(sc.Connection.Database))
			{
				sql.AppendLine("USE  [" + sc.Connection.Database + "];");
			}

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

							if (firstParam)
							{
								firstParam = false;
							}

							if (sp.Direction == ParameterDirection.Input)
							{
								sql.AppendLine(sp.ParameterName + " = " + QueryDebugger.ParameterValue(sp));
							}
							else

							{
								sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " OUTPUT");
							}
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

		/// <summary>
		/// Parameters the value.
		/// </summary>
		/// <param name="sp">The sp.</param>
		/// <returns></returns>
		public static String ParameterValue(SQLiteParameter sp)
		{
			return sp.DbType.ToString().ToUpper();
		}

		public void CloseAllConnections()
		{
			foreach (var sqLiteConnectionCounter in ConnectionCounter.Where(f => f.Key.Equals(DatabaseFile)))
			{
				sqLiteConnectionCounter.Value.Dispose();
			}
			ConnectionCounter.Clear();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		#endregion
	}
}