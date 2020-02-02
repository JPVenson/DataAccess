using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Allows the external execution of DbCommands
	/// </summary>
	public abstract class RemotingStrategy : IDatabaseStrategy
	{
		/// <summary>
		/// 
		/// </summary>
		public DbConfig Config { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="emulateDbType"></param>
		/// <param name="config"></param>
		public RemotingStrategy(DbAccessType emulateDbType, DbConfig config)
		{
			Config = config;
			SourceDatabase = DbAccessType.Remoting | emulateDbType;
			Events = new RemotingStrategyEvents();
		}
		
		/// <inheritdoc />
		public abstract object Clone();

		/// <summary>
		///		Should execute the Query and return an value int
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public abstract int ExecuteQuery(RemotingDbCommand command);

		/// <summary>
		///		Should execute the query and return a single result
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public abstract object ExecuteScalar(RemotingDbCommand command);

		/// <summary>
		///		You can overwrite this method to return multiple sets of IDataRecords.
		///		If you only got objects you can use the ObjectDataRecord to wrap them into IDataRecords
		/// </summary>
		/// <param name="command"></param>
		/// <param name="behavior"></param>
		/// <param name="recordsAffected"></param>
		/// <returns></returns>
		public virtual IEnumerable<IEnumerable<IDataRecord>> EnumerateCommand(RemotingDbCommand command, CommandBehavior behavior, out int recordsAffected)
		{
			throw new NotImplementedException("Please ether overwrite the ExecuteReader function or the EnumerateCommand function.");
		}

		/// <summary>
		///		Should execute the query an return a IDataReader that contains the result.
		///		You can overwrite the EnumerateDataRecord method to use an inbuild IDataReader
		/// </summary>
		/// <param name="command"></param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		public virtual IDataReader ExecuteReader(RemotingDbCommand command, CommandBehavior behavior)
		{
			return new VirtualDataReader(EnumerateCommand(command, behavior, out var records), Config, records);
		}

		private class VirtualDataReader : IDataReader
		{
			private readonly DbConfig _config;
			private readonly IEnumerator<IEnumerable<IDataRecord>> _dataSetsEnumerator;
			private IEnumerator<IDataRecord> _setEnumerator;

			public IDataRecord CurrentRecord { get; set; }

			public VirtualDataReader(IEnumerable<IEnumerable<IDataRecord>> enumerateCommand, DbConfig config, int records)
			{
				RecordsAffected = records;
				_config = config;
				_dataSetsEnumerator = enumerateCommand.GetEnumerator();
				if (_dataSetsEnumerator.MoveNext())
				{
					_setEnumerator = _dataSetsEnumerator.Current.GetEnumerator();
				}
			}

			#region Record Delegate

			public bool GetBoolean(int i)
			{
				return CurrentRecord.GetBoolean(i);
			}

			public byte GetByte(int i)
			{
				return CurrentRecord.GetByte(i);
			}

			public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
			{
				return CurrentRecord.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			}

			public char GetChar(int i)
			{
				return CurrentRecord.GetChar(i);
			}

			public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
			{
				return CurrentRecord.GetChars(i, fieldoffset, buffer, bufferoffset, length);
			}

			public IDataReader GetData(int i)
			{
				return CurrentRecord.GetData(i);
			}

			public string GetDataTypeName(int i)
			{
				return CurrentRecord.GetDataTypeName(i);
			}

			public DateTime GetDateTime(int i)
			{
				return CurrentRecord.GetDateTime(i);
			}

			public decimal GetDecimal(int i)
			{
				return CurrentRecord.GetDecimal(i);
			}

			public double GetDouble(int i)
			{
				return CurrentRecord.GetDouble(i);
			}

			public Type GetFieldType(int i)
			{
				return CurrentRecord.GetFieldType(i);
			}

			public float GetFloat(int i)
			{
				return CurrentRecord.GetFloat(i);
			}

			public Guid GetGuid(int i)
			{
				return CurrentRecord.GetGuid(i);
			}

			public short GetInt16(int i)
			{
				return CurrentRecord.GetInt16(i);
			}

			public int GetInt32(int i)
			{
				return CurrentRecord.GetInt32(i);
			}

			public long GetInt64(int i)
			{
				return CurrentRecord.GetInt64(i);
			}

			public string GetName(int i)
			{
				return CurrentRecord.GetName(i);
			}

			public int GetOrdinal(string name)
			{
				return CurrentRecord.GetOrdinal(name);
			}

			public string GetString(int i)
			{
				return CurrentRecord.GetString(i);
			}

			public object GetValue(int i)
			{
				return CurrentRecord.GetValue(i);
			}

			public int GetValues(object[] values)
			{
				return CurrentRecord.GetValues(values);
			}

			public bool IsDBNull(int i)
			{
				return CurrentRecord.IsDBNull(i);
			}

			public int FieldCount
			{
				get { return CurrentRecord?.FieldCount ?? 0; }
			}

			public object this[int i]
			{
				get { return CurrentRecord[i]; }
			}

			public object this[string name]
			{
				get { return CurrentRecord[name]; }
			}

			#endregion

			public void Dispose()
			{
				Close();
				if (_setEnumerator is IDisposable disposable)
				{
					disposable.Dispose();
				}

				if (CurrentRecord is IDisposable disposableRecord)
				{
					disposableRecord.Dispose();
				}
			}

			public void Close()
			{
				IsClosed = true;
			}

			public DataTable GetSchemaTable()
			{
				throw new NotImplementedException();
			}

			public bool NextResult()
			{
				var canRead = !IsClosed && _dataSetsEnumerator.MoveNext();
				if (canRead)
				{
					_setEnumerator = _dataSetsEnumerator.Current.GetEnumerator();
				}
				return canRead;
			}

			public bool Read()
			{
				var canRead = !IsClosed && _setEnumerator.MoveNext();
				if (canRead)
				{
					CurrentRecord = _setEnumerator.Current;
				}
				return canRead;
			}

			public int Depth { get; } = 1;
			public bool IsClosed { get; private set; }
			public int RecordsAffected { get; private set; }
		}

		/// <summary>
		///		Defines all events you can attach to
		/// </summary>
		public RemotingStrategyEvents Events { get; set; }
		
		/// <inheritdoc />
		public DbAccessType SourceDatabase { get; }
		
		/// <inheritdoc />
		public string ConnectionString { get; set; }
		/// <inheritdoc />
		public string DatabaseFile { get; }
		/// <inheritdoc />
		public string ServerName { get; }
		
		/// <inheritdoc />
		public IDbConnection CreateConnection()
		{
			return new RemotingDbConnection(this);
		}
		
		/// <inheritdoc />
		public IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields)
		{
			var remotingDbCommand = new RemotingDbCommand(this)
			{
				CommandText = strSql,
				Connection = conn
			};
			foreach (var dataParameter in fields)
			{
				remotingDbCommand.Parameters.Add(dataParameter);
			}
			return remotingDbCommand;
		}
		
		/// <inheritdoc />
		public IDataParameter CreateParameter(string strName, object value)
		{
			return new RemotingDbParameter(this)
			{
				ParameterName = strName,
				Value = value
			};
		}
		
		/// <inheritdoc />
		public virtual IDbCommand GetLastInsertedID_Cmd(IDbConnection conn)
		{
			if (SourceDatabase.HasFlagFast(DbAccessType.MsSql))
			{
				return CreateCommand("SELECT SCOPE_IDENTITY() as Value", conn);
			}

			if (SourceDatabase.HasFlagFast(DbAccessType.SqLite))
			{
				return CreateCommand("SELECT last_insert_rowid() as Value", conn);
			}

			if (SourceDatabase.HasFlagFast(DbAccessType.MySql))
			{
				return CreateCommand("SELECT LAST_INSERT_ID();", conn);
			}
			throw new NotImplementedException($"The GetLastInsertedId ist not implemented for the mixed type of '{SourceDatabase}' please overwrite this function to provide an query.");
		}
		
		/// <inheritdoc />
		public abstract IDataPager<T> CreatePager<T>();
		
		/// <inheritdoc />
		public IWrapperDataPager<T, TE> CreateConverterPager<T, TE>()
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public virtual string FormartCommandToQuery(IDbCommand command)
		{
			return command.CommandText;
		}
		
		/// <inheritdoc />
		public virtual string ConvertParameter(DbType type)
		{
			return type.ToString();
		}
		
		/// <inheritdoc />
		public void CloseAllConnections()
		{
		}
		
		/// <inheritdoc />
		public virtual IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public virtual IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
		{
			throw new NotImplementedException();
		}
	}
}