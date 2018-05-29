/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
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
			ServerName = strServer;
			DatabaseFile = strDatabase;
			if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
			{
				ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
			}
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

		public DbAccessType SourceDatabase
		{
			get
			{
				return DbAccessType.MySql;
			}
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

		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
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

		public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
		{
			return CreateCommand("SELECT LAST_INSERT_ID();", conn);
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
			if (!(command is MySqlCommand))
			{
				return command.CommandText;
			}

			var sql = new StringBuilder();
			var firstParam = true;

			if (!string.IsNullOrEmpty(command.Connection.Database))
			{
				sql.AppendLine("USE  [" + command.Connection.Database + "];");
			}

			switch (command.CommandType)
			{
				case CommandType.StoredProcedure:
					sql.AppendLine("DECLARE @return_value int;");

					foreach (MySqlParameter sp in command.Parameters.Cast<MySqlParameter>())
					{
						if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
						{
							sql.Append("DECLARE " + sp.ParameterName + "\t" + sp.DbType + "\t= ");

							sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "NULL" : QueryDebugger.ParameterValue(sp)) + ";");
						}
					}

					sql.AppendLine("EXEC [" + command.CommandText + "]");

					foreach (IDataParameter sp in command.Parameters.Cast<IDataParameter>())
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

					foreach (IDataParameter sp in command.Parameters.Cast<IDataParameter>())
					{
						if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
						{
							sql.AppendLine("SELECT '" + sp.ParameterName + "' = CONVERT(NVARCHAR, " + sp.ParameterName + ");");
						}
					}
					break;
				case CommandType.Text:
				case CommandType.TableDirect:
					foreach (MySqlParameter sp in command.Parameters.Cast<MySqlParameter>())
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

					sql.AppendLine(command.CommandText);
					break;
			}

			return sql.ToString();
		}

		/// <summary>
		/// Parameters the value.
		/// </summary>
		/// <param name="sp">The sp.</param>
		/// <returns></returns>
		public static string ParameterValue(MySqlParameter sp)
		{
			var retval = "";

			switch (sp.MySqlDbType)
			{
				case MySqlDbType.Decimal:
				case MySqlDbType.Float:
				case MySqlDbType.Int16:
				case MySqlDbType.Int24:
				case MySqlDbType.Int32:
				case MySqlDbType.Int64:
				case MySqlDbType.Byte:
				case MySqlDbType.Double:
				case MySqlDbType.NewDecimal:
					retval = sp.Value.ToString().Replace("'", "''");
					break;

				case MySqlDbType.Bit:
					retval = (sp.Value is bool && (bool)sp.Value) ? "1" : "0";
					break;

				default:
					retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
					break;
			}

			return retval;
		}

		public string ConvertParameter(DbType type)
		{
			return new MySqlParameter { DbType = type }.MySqlDbType.ToString();
		}

		public object Clone()
		{
			return new MySql(ConnectionString);
		}

		public void CloseAllConnections()
		{
			MySqlConnection.ClearAllPools();
		}

		public IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand("", conn);
		}

		public IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand("", conn);
		}
	}
}