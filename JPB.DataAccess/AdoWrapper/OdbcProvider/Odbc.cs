/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.OdbcProvider
{
	/// <summary>
	/// Default UNTESTED impl
	/// </summary>
#pragma warning disable 1591
	public class Obdc : IDatabaseStrategy
	{
		private const string TEMPLATE_MSSQL_UNTRUSTED =
			"server={0};database={1};user id={2};password={3};Connect Timeout=100;Min Pool Size=5;trusted_connection=false";

		private const string TEMPLATE_MSSQL_TRUSTED =
			"server={0};database={1};Connect Timeout=100;Min Pool Size=5;trusted_connection=true";


		public Obdc(string strServer, string strDatabase)
		{
			ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
		}

		public Obdc(string strServer, string strDatabase, string strLogin, string strPassword)
		{
			if (0 == strLogin.Trim().Length && 0 == strPassword.Trim().Length)
				ConnectionString = string.Format(TEMPLATE_MSSQL_TRUSTED, strServer.Trim(), strDatabase.Trim());
			else
			{
				ConnectionString = string.Format(TEMPLATE_MSSQL_UNTRUSTED, strServer.Trim(), strDatabase.Trim(),
					strLogin.Trim(), strPassword.Trim());
			}
		}

		public Obdc(string strConnStr)
		{
			ConnectionString = strConnStr;
		}

		public object Clone()
		{
			return new Obdc(ConnectionString);
		}

		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.Obdc; }
		}

		public string ConnectionString { get; set; }
		public string DatabaseFile { get; private set; }

		public string ServerName
		{
			get
			{
				var cn = (OdbcConnection) CreateConnection();
				return cn.DataSource;
			}
		}

		public IDbConnection CreateConnection()
		{
			return new OdbcConnection(ConnectionString);
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			return new OdbcCommand(strSql, (OdbcConnection) (conn is OdbcConnection ? conn : CreateConnection()));
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var oleDbCommand = new OdbcCommand(strSql,
				(OdbcConnection) (conn is OdbcConnection ? conn : CreateConnection()));

			foreach (IDataParameter dbDataParameter in fields)
			{
				oleDbCommand.Parameters.AddWithValue(dbDataParameter.ParameterName, dbDataParameter.Value);
			}
			return oleDbCommand;
		}

		public IDataParameter CreateParameter(string strName, object value)
		{
			return new OdbcParameter(strName, value);
		}

		public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
		{
			return new OdbcDataAdapter(cmd as OdbcCommand);
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

		public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
		{
			return CreateCommand("SELECT LAST_INSERT_ID( );", conn);
		}

		public IDataPager<T> CreatePager<T>()
		{
			throw new NotImplementedException();
		}

		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			throw new NotImplementedException();
		}

		public string FormartCommandToQuery(IDbCommand command)
		{
			return command.ToString();
		}

		public string ConvertParameter(DbType type)
		{
			return type.ToString();
		}

		public void CloseAllConnections()
		{
			throw new NotImplementedException();
		}
	}
}
#pragma warning restore 1591