/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Data;
using System.Data.OleDb;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.OleDBProvider
{
	/// <summary>
	/// Default UNTESTED impl
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.IDatabaseStrategy" />
	public class OleDb : IDatabaseStrategy
	{
		/// <summary>
		/// The template MSSQL untrusted
		/// </summary>
		private const string TEMPLATE_MSSQL_UNTRUSTED =
			"server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

		/// <summary>
		/// The template MSSQL trusted
		/// </summary>
		private const string TEMPLATE_MSSQL_TRUSTED =
			"server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDb"/> class.
		/// </summary>
		/// <param name="strServer">The string server.</param>
		/// <param name="strDatabase">The string database.</param>
		public OleDb(string strServer, string strDatabase)
		{
			ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDb"/> class.
		/// </summary>
		/// <param name="strServer">The string server.</param>
		/// <param name="strDatabase">The string database.</param>
		/// <param name="strLogin">The string login.</param>
		/// <param name="strPassword">The string password.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDb"/> class.
		/// </summary>
		/// <param name="strConnStr">The string connection string.</param>
		public OleDb(string strConnStr)
		{
			ConnectionString = strConnStr;
		}

		/// <summary>
		/// Formarts a Command into a QueryCommand after the Strategy rules
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public string FormartCommandToQuery(IDbCommand command)
		{
			return command.ToString();
		}

		/// <summary>
		/// Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string ConvertParameter(DbType type)
		{
			return type.ToString();
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return new OleDb(ConnectionString);
		}

		/// <summary>
		/// Defines the database type this Strategy is used for
		/// </summary>
		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.OleDb; }
		}

		/// <summary>
		/// An Valid Connection string for the given Strategy
		/// </summary>
		public string ConnectionString { get; set; }
		/// <summary>
		/// Optional used when connecting to a Local file
		/// </summary>
		public string DatabaseFile { get; private set; }

		/// <summary>
		/// Should return the current database if availibe
		/// </summary>
		public string ServerName
		{
			get
			{
				var cn = (OleDbConnection) CreateConnection();
				return cn.DataSource;
			}
		}

		/// <summary>
		/// Creates a new Provider specific Connection that will held open until all actors want to close it
		/// </summary>
		/// <returns></returns>
		public IDbConnection CreateConnection()
		{
			return new OleDbConnection(ConnectionString);
		}

		/// <summary>
		/// Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			return new OleDbCommand(strSql, (OleDbConnection) (conn is OleDbConnection ? conn : CreateConnection()));
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
			var oleDbCommand = new OleDbCommand(strSql,
				(OleDbConnection) (conn is OleDbConnection ? conn : CreateConnection()));

			foreach (IDataParameter dbDataParameter in fields)
			{
				oleDbCommand.Parameters.AddWithValue(dbDataParameter.ParameterName, dbDataParameter.Value);
			}
			return oleDbCommand;
		}

		/// <summary>
		/// Creates a query parameter.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public IDataParameter CreateParameter(string strName, object value)
		{
			return new OleDbParameter(strName, value);
		}

		/// <summary>
		/// Creates the data adapter.
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
		{
			return new OleDbDataAdapter(cmd as OleDbCommand);
		}

		/// <summary>
		/// Creates a data table.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Getlasts a inserted identifier command.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a data pager.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public IDataPager<T> CreatePager<T>()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates the a pager that can convert each item.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			throw new NotImplementedException();
		}
	}
}