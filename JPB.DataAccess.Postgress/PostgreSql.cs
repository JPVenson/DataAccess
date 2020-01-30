using System;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using Npgsql;
using NpgsqlTypes;

namespace JPB.DataAccess.PostgreSQL
{
	public class PostgreSql : IDatabaseStrategy
	{
		public PostgreSql(string connectionString)
		{
			ConnectionString = connectionString;
		}
		
		public object Clone()
		{
			return new PostgreSql(ConnectionString);
		}

		public DbAccessType SourceDatabase
		{
			get { return DbAccessType.Unknown; }
		}

		public string ConnectionString { get; set; }
		public string DatabaseFile { get; }
		public string ServerName { get; }

		public IDbConnection CreateConnection()
		{
			return new NpgsqlConnection();
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn)
		{
			return new NpgsqlCommand(strSql, conn as NpgsqlConnection);
		}

		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var command = CreateCommand(strSql, conn);
			foreach (var dataParameter in fields)
			{
				command.Parameters.Add(dataParameter);
			}

			return command;
		}

		public IDataParameter CreateParameter(string strName, object value)
		{
			return new NpgsqlParameter(strName, value);
		}

		public IDbCommand GetLastInsertedID_Cmd(IDbConnection conn)
		{
			return CreateCommand("SELECT LASTVAL();", conn);
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
			if (!(command is NpgsqlCommand))
			{
				return command.CommandText;
			}

			var sql = new StringBuilder();

			switch (command.CommandType)
			{
				case CommandType.Text:
				case CommandType.TableDirect:
					foreach (var sp in command.Parameters.Cast<NpgsqlParameter>())
					{
						var paramTypeCompiler = sp.DbType.ToString().ToUpper();
						if (sp.Size > 0)
						{
							paramTypeCompiler += "(" + sp.Size + ")";
						}

						sql.AppendLine(sp.ParameterName + " "
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
		public static string ParameterValue(NpgsqlParameter sp)
		{
			var retval = "";

			switch (sp.NpgsqlDbType)
			{
				case NpgsqlDbType.Integer:
				case NpgsqlDbType.Numeric:
				case NpgsqlDbType.Real:
				case NpgsqlDbType.Smallint:
				case NpgsqlDbType.Double:
					retval = sp.Value.ToString().Replace("'", "''");
					break;

				case NpgsqlDbType.Bit:
					retval = (sp.Value is bool && (bool)sp.Value) ? "1" : "0";
					break;

				default:
					retval = "'" + sp.Value?.ToString().Replace("'", "''") + "'";
					break;
			}

			return retval;
		}

		public string ConvertParameter(DbType type)
		{
			return new NpgsqlParameter() { DbType = type }.NpgsqlDbType.ToString();
		}

		public void CloseAllConnections()
		{
			NpgsqlConnection.ClearAllPools();
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
