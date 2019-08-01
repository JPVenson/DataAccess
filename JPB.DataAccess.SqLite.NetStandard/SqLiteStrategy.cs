/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Contacts.Pager;
using JPB.DataAccess.Framework.DebuggerHelper;
using JPB.DataAccess.Framework.Manager;

namespace JPB.DataAccess.SqLite.NetStandard
{
	/// <summary>
	/// Wrapps MsSQL spezifc data
	/// </summary>
	/// <seealso cref="IDatabaseStrategy" />
	public class SqLiteStrategy : IDatabaseStrategy
	{
		static SqLiteStrategy()
		{
			ConnectionCounter = new ConcurrentDictionary<string, SqLiteConnectionCounter>();
		}

		public static ConcurrentDictionary<string, SqLiteConnectionCounter> ConnectionCounter { get; }

		/// <summary>
		/// The connection string
		/// </summary>
		private string _connStr;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqLiteStrategy"/> class.
		/// </summary>
		/// <param name="strConnStr">The string connection string.</param>
		public SqLiteStrategy(string strConnStr)
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
			get { return DatabaseFile; }
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
			return CommandAsSqLite(command);
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
			return new SqLiteStrategy(_connStr);
		}

		/// <summary>
		/// Commands as ms SQL.
		/// </summary>
		/// <param name="sc">The sc.</param>
		/// <returns></returns>
		public static String CommandAsSqLite(IDbCommand sc)
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

		public IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand("", conn);
		}

		public IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand("", conn);
		}

		#endregion
	}
}