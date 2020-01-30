#region

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	///     Wraps MsSQL data Access
	/// </summary>
	/// <seealso cref="IDatabaseStrategy" />
	public class MsSql : IDatabaseStrategy
	{
		/// <summary>
		///     The template MSSQL untrusted
		/// </summary>
		private const string TEMPLATE_MSSQL_UNTRUSTED =
			"server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

		/// <summary>
		///     The template MSSQL trusted
		/// </summary>
		private const string TEMPLATE_MSSQL_TRUSTED =
			"server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";

		/// <summary>
		///     The connection string
		/// </summary>
		private string _connStr = string.Empty;

		/// <summary>
		///     Initializes a new instance of the <see cref="MsSql" /> class.
		/// </summary>
		/// <param name="strServer">The string server.</param>
		/// <param name="strDatabase">The string database.</param>
		public MsSql(string strServer, string strDatabase)
		{
			_connStr = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MsSql" /> class.
		/// </summary>
		/// <param name="strServer">The string server.</param>
		/// <param name="strDatabase">The string database.</param>
		/// <param name="strLogin">The string login.</param>
		/// <param name="strPassword">The string password.</param>
		public MsSql(string strServer, string strDatabase, string strLogin, string strPassword)
		{
			if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
			{
				_connStr = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
			}
			else
			{
				_connStr = string.Format(TEMPLATE_MSSQL_UNTRUSTED, strServer.Trim(), strDatabase.Trim(),
				strLogin.Trim(), strPassword.Trim());
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MsSql" /> class.
		/// </summary>
		/// <param name="strConnStr">The string connection string.</param>
		public MsSql(string strConnStr)
		{
			_connStr = strConnStr;
		}

		#region IDatabaseStrategy Members

		/// <summary>
		///     Defines the database type this Strategy is used for
		/// </summary>
		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.MsSql; }
		}

		/// <summary>
		///     An Valid Connection string for the given Strategy
		/// </summary>
		public string ConnectionString
		{
			get { return _connStr; }
			set { _connStr = value; }
		}

		/// <summary>
		///     Optional used when connecting to a Local file
		/// </summary>
		/// <exception cref="Exception">The method or operation is not implemented.</exception>
		public string DatabaseFile
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		/// <summary>
		///     Should return the current database if availibe
		/// </summary>
		public string ServerName
		{
			get
			{
				var cn = (SqlConnection) CreateConnection();
				return cn.DataSource;
			}
		}

		/// <summary>
		///     Creates a new Provider specific Connection that will held open until all actors want to close it
		/// </summary>
		/// <returns></returns>
		public IDbConnection CreateConnection()
		{
			var sqlConnection = new SqlConnection(_connStr);
			return sqlConnection;
		}

		/// <summary>
		///     Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			var cmd = new SqlCommand(strSql);
			cmd.Connection = (SqlConnection) conn;
			return cmd;
		}

		/// <summary>
		///     Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var cmd = (SqlCommand) CreateCommand(strSql, conn);

			foreach (var dataParameter in fields)
			{
				cmd.Parameters.AddWithValue(dataParameter.ParameterName, dataParameter.Value);
			}

			return cmd;
		}

		/// <summary>
		///     Creates a query parameter.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public IDataParameter CreateParameter(string strName, object value)
		{
			return new SqlParameter(strName, value);
		}

		/// <summary>
		///     Getlasts a inserted identifier command.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		public IDbCommand GetLastInsertedID_Cmd(IDbConnection conn)
		{
			//return this.CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
			return CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
		}

		/// <summary>
		///     Creates a data pager.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDataPager<T> CreatePager<T>()
		{
			return new MsSqlDataPager<T>();
		}

		/// <summary>
		///     Creates the a pager that can convert each item.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <returns></returns>
		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			return new MsSqlDataConverterPager<T, TE>();
		}

		/// <summary>
		///     Formarts a Command into a QueryCommand after the Strategy rules
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public string FormartCommandToQuery(IDbCommand command)
		{
			return CommandAsMsSql(command);
		}

		/// <summary>
		///     Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string ConvertParameter(DbType type)
		{
			return new SqlParameter {DbType = type}.SqlDbType.ToString();
		}

		/// <summary>
		///     Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		///     A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return new MsSql(_connStr);
		}
		
		/// <summary>
		///     Commands as ms SQL.
		/// </summary>
		/// <param name="sc">The sc.</param>
		/// <returns></returns>
		public static string CommandAsMsSql(IDbCommand sc)
		{
			var sql = new StringBuilder();
			var firstParam = true;

			if (sc.Connection != null && !string.IsNullOrEmpty(sc.Connection.Database))
			{
				sql.AppendLine("USE  [" + sc.Connection.Database + "];");
			}

			switch (sc.CommandType)
			{
				case CommandType.StoredProcedure:
					sql.AppendLine("DECLARE @return_value int;");

					foreach (var sp in sc.Parameters.Cast<SqlParameter>())
					{
						if (sp.Direction == ParameterDirection.InputOutput || sp.Direction == ParameterDirection.Output)
						{
							sql.Append("DECLARE " + sp.ParameterName + "\t" + sp.SqlDbType + "\t= ");

							sql.AppendLine((sp.Direction == ParameterDirection.Output ? "NULL" : QueryDebugger.ParameterValue(sp)) + ";");
						}
					}

					sql.AppendLine("EXEC [" + sc.CommandText + "]");

					foreach (var sp in sc.Parameters.Cast<IDataParameter>())
					{
						if (sp.Direction != ParameterDirection.ReturnValue)
						{
							sql.Append(firstParam ? "\t" : "\t, ");

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

					foreach (var sp in sc.Parameters.Cast<IDataParameter>())
					{
						if (sp.Direction == ParameterDirection.InputOutput || sp.Direction == ParameterDirection.Output)
						{
							sql.AppendLine("SELECT '" + sp.ParameterName + "' = CONVERT(NVARCHAR, " + sp.ParameterName + ");");
						}
					}
					break;
				case CommandType.Text:
				case CommandType.TableDirect:
					foreach (var sp in sc.Parameters.Cast<SqlParameter>())
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

		/// <summary>
		///     Parameters the value.
		/// </summary>
		/// <param name="sp">The sp.</param>
		/// <returns></returns>
		public static string ParameterValue(SqlParameter sp)
		{
			var retval = "";

			if (sp.IsNullable && sp.Value == null)
			{
				return "{NULL}";
			}

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
					retval = sp.Value is bool && (bool) sp.Value ? "1" : "0";
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
		
		/// <summary>
		///     Calls the API to close all open Connections and free the Database
		/// </summary>
		public void CloseAllConnections()
		{
			//HACK
			SqlConnection.ClearAllPools();
		}

		/// <inheritdoc />
		public IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] ON", classInfoTableName), conn);
		}

		/// <inheritdoc />
		public IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] OFF", classInfoTableName), conn);
		}

		#endregion
	}
}