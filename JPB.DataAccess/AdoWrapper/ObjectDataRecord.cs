using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// Wraps access to an object by supporting the IDataReader interface
	/// When use functions that accepts a name allways use the Database names
	/// </summary>
	public class ObjectDataRecord : IDataReader
	{
		private readonly object _poco;
		private readonly DbConfig _config;
		private DbClassInfoCache _classTypeCache;
		private IDictionary<string, DbPropertyInfoCache> _inveredCache;

		public ObjectDataRecord(object poco, DbConfig config, int depth)
		{
			_poco = poco;
			_config = config;
			Depth = depth;
			_classTypeCache = config.GetOrCreateClassInfoCache(poco.GetType());
			RecordsAffected = -1;
			FieldCount = _classTypeCache.Propertys.Count;
		}

		private object GetConvertedValue(object val)
		{
			return val ?? DBNull.Value;
		}

		public object GetValue(string name)
		{
			return GetConvertedValue(_classTypeCache.Propertys[_classTypeCache.SchemaMappingDatabaseToLocal(name)].Getter.Invoke(_poco));
		}

		public string GetName(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Key;
		}

		public string GetDataTypeName(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Value.PropertyType.Name;
		}

		public Type GetFieldType(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Value.PropertyType;
		}

		public object GetValue(int i)
		{
			return GetConvertedValue(_classTypeCache.Propertys.ElementAt(i).Value.Getter.Invoke(_poco));
		}

		public int GetValues(object[] values)
		{
			var i = 0;
			foreach (var dbPropertyInfoCach in _classTypeCache.Propertys)
			{
				values[i++] = GetConvertedValue(dbPropertyInfoCach.Value.Getter.Invoke(_poco));
			}
			return _classTypeCache.Propertys.Count;
		}

		public int GetOrdinal(string name)
		{
			int index = 0;
			var localName = _classTypeCache.SchemaMappingDatabaseToLocal(name);
			foreach (var key in _classTypeCache.Propertys.Keys)
			{
				if (key == localName)
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public bool GetBoolean(int i)
		{
			return (bool)GetValue(i);
		}

		public byte GetByte(int i)
		{
			return (byte)GetValue(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			var bytes = (byte[])GetValue(i);
			var affected = 0;

			for (long j = fieldOffset; j < bytes.Length; j++)
			{
				affected++;
				buffer[bufferoffset + j] = bytes[j];
				if (length <= affected)
					return affected;
			}
			return affected;
		}

		public char GetChar(int i)
		{
			return (char)GetValue(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			var bytes = (char[])GetValue(i);
			var affected = 0;

			for (long j = bufferoffset; j < bytes.Length; j++)
			{
				affected++;
				buffer[bufferoffset + j] = bytes[j];
				if (length <= affected)
					return affected;
			}
			return affected;
		}

		public Guid GetGuid(int i)
		{
			return (Guid)GetValue(i);
		}

		public short GetInt16(int i)
		{
			return (short)GetValue(i);
		}

		public int GetInt32(int i)
		{
			return (int)GetValue(i);
		}

		public long GetInt64(int i)
		{
			return (long)GetValue(i);
		}

		public float GetFloat(int i)
		{
			return (float)GetValue(i);
		}

		public double GetDouble(int i)
		{
			return (double)GetValue(i);
		}

		public string GetString(int i)
		{
			return (string)GetValue(i);
		}

		public decimal GetDecimal(int i)
		{
			return (decimal)GetValue(i);
		}

		public DateTime GetDateTime(int i)
		{
			return (DateTime)GetValue(i);
		}

		public IDataReader GetData(int i)
		{
			return new ObjectDataRecord(this.GetValue(i), _config, this.Depth + 1);
		}

		public bool IsDBNull(int i)
		{
			return GetValue(i) == null;
		}

		public int FieldCount { get; private set; }

		object IDataRecord.this[int i]
		{
			get
			{
				return GetValue(i);
			}
		}

		object IDataRecord.this[string name]
		{
			get { return GetValue(name); }
		}

		public void Dispose()
		{
			this._classTypeCache = null;
		}

		public void Close()
		{
			IsClosed = true;
			this.Dispose();
		}

		/// <summary>
		/// Returns a table that 
		/// </summary>
		/// <returns></returns>
		public DataTable GetSchemaTable()
		{
			var dt = new DataTable();
			foreach (var item in _classTypeCache.Propertys)
			{
				var row = dt.NewRow();
				row.BeginEdit();
				row["ColumnName"] = item.Key;
				row["IsUnique"] = item.Value.PrimaryKeyAttribute != null;
				row["IsKey"] = item.Value.PrimaryKeyAttribute != null;
				row["DataType"] = item.Value.PropertyType;
				row["AllowDBNull"] = item.Value.PropertyType == typeof(string) || Nullable.GetUnderlyingType(item.Value.PropertyType) != null;
				row["ProviderType"] = DbAccessLayer.DbTypeMap[item.Value.PropertyType];
				row["IsIdentity"] = item.Value.PrimaryKeyAttribute != null;
				row["IsAutoIncrement"] = item.Value.PrimaryKeyAttribute != null;
				row["IsRowVersion"] = item.Value.RowVersionAttribute != null;
				row["DataTypeName"] = row["ProviderType"].ToString();
				row.EndEdit();
				dt.Rows.Add(row);
			}

			return dt;
		}

		public bool NextResult()
		{
			return false;
		}

		public bool Read()
		{
			return false;
		}

		public int Depth { get; private set; }
		public bool IsClosed { get; private set; }
		public int RecordsAffected { get; private set; }
	}
}
