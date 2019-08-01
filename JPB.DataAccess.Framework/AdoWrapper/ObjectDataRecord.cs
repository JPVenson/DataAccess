#region

using System;
using System.Data;
using System.Linq;
using JPB.DataAccess.Framework.DbInfoConfig;
using JPB.DataAccess.Framework.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework.Manager;

#endregion

namespace JPB.DataAccess.Framework.AdoWrapper
{
	/// <summary>
	///     Wraps access to an object by supporting the IDataReader interface
	///     When use functions that accepts a name allways use the Database names
	/// </summary>
	/// <seealso cref="System.Data.IDataReader" />
	public class ObjectDataRecord : IDataReader
	{
		private readonly DbConfig _config;
		private readonly object _poco;
		private DbClassInfoCache _classTypeCache;

		/// <summary>
		///     Creates a new Object Data Recored that proviedes Access to an single Poco by using the IDataReader interface
		/// </summary>
		/// <param name="poco"></param>
		/// <param name="config"></param>
		/// <param name="depth"></param>
		public ObjectDataRecord(object poco, DbConfig config, int depth)
		{
			_poco = poco;
			_config = config;
			Depth = depth;
			_classTypeCache = config.GetOrCreateClassInfoCache(poco.GetType());
			RecordsAffected = -1;
			FieldCount = _classTypeCache.Propertys.Count;
		}

		/// <summary>Gets the name for the field to find.</summary>
		/// <returns>The name of the field or the empty string (""), if there is no value to return.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public string GetName(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Key;
		}

		/// <summary>Gets the data type information for the specified field.</summary>
		/// <returns>The data type information for the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public string GetDataTypeName(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Value.PropertyType.Name;
		}

		/// <summary>
		///     Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" />
		///     that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </summary>
		/// <returns>
		///     The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that
		///     would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public Type GetFieldType(int i)
		{
			return _classTypeCache.Propertys.ElementAt(i).Value.PropertyType;
		}

		/// <summary>Return the value of the specified field.</summary>
		/// <returns>The <see cref="T:System.Object" /> which will contain the field value upon return.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public object GetValue(int i)
		{
			return GetConvertedValue(_classTypeCache.Propertys.ElementAt(i).Value.Getter.Invoke(_poco));
		}

		/// <summary>Populates an array of objects with the column values of the current record.</summary>
		/// <returns>The number of instances of <see cref="T:System.Object" /> in the array.</returns>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into. </param>
		public int GetValues(object[] values)
		{
			var i = 0;
			foreach (var dbPropertyInfoCach in _classTypeCache.Propertys)
			{
				values[i++] = GetConvertedValue(dbPropertyInfoCach.Value.Getter.Invoke(_poco));
			}
			return _classTypeCache.Propertys.Count;
		}

		/// <summary>Return the index of the named field.</summary>
		/// <returns>The index of the named field.</returns>
		/// <param name="name">The name of the field to find. </param>
		public int GetOrdinal(string name)
		{
			var index = 0;
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

		/// <summary>Gets the value of the specified column as a Boolean.</summary>
		/// <returns>The value of the column.</returns>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public bool GetBoolean(int i)
		{
			return (bool) GetValue(i);
		}

		/// <summary>Gets the 8-bit unsigned integer value of the specified column.</summary>
		/// <returns>The 8-bit unsigned integer value of the specified column.</returns>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public byte GetByte(int i)
		{
			return (byte) GetValue(i);
		}

		/// <summary>
		///     Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given
		///     buffer offset.
		/// </summary>
		/// <returns>The actual number of bytes read.</returns>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <param name="fieldOffset">The index within the field from which to start the read operation. </param>
		/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation. </param>
		/// <param name="length">The number of bytes to read. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			var bytes = (byte[]) GetValue(i);
			var affected = 0;

			for (var j = fieldOffset; j < bytes.Length; j++)
			{
				affected++;
				buffer[bufferoffset + j] = bytes[j];
				if (length <= affected)
				{
					return affected;
				}
			}
			return affected;
		}

		/// <summary>Gets the character value of the specified column.</summary>
		/// <returns>The character value of the specified column.</returns>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public char GetChar(int i)
		{
			return (char) GetValue(i);
		}

		/// <summary>
		///     Reads a stream of characters from the specified column offset into the buffer as an array, starting at the
		///     given buffer offset.
		/// </summary>
		/// <returns>The actual number of characters read.</returns>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <param name="fieldoffset">The index within the row from which to start the read operation. </param>
		/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation. </param>
		/// <param name="length">The number of bytes to read. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			var bytes = (char[]) GetValue(i);
			var affected = 0;

			for (long j = bufferoffset; j < bytes.Length; j++)
			{
				affected++;
				buffer[bufferoffset + j] = bytes[j];
				if (length <= affected)
				{
					return affected;
				}
			}
			return affected;
		}

		/// <summary>Returns the GUID value of the specified field.</summary>
		/// <returns>The GUID value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public Guid GetGuid(int i)
		{
			return (Guid) GetValue(i);
		}

		/// <summary>Gets the 16-bit signed integer value of the specified field.</summary>
		/// <returns>The 16-bit signed integer value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public short GetInt16(int i)
		{
			return (short) GetValue(i);
		}

		/// <summary>Gets the 32-bit signed integer value of the specified field.</summary>
		/// <returns>The 32-bit signed integer value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public int GetInt32(int i)
		{
			return (int) GetValue(i);
		}

		/// <summary>Gets the 64-bit signed integer value of the specified field.</summary>
		/// <returns>The 64-bit signed integer value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public long GetInt64(int i)
		{
			return (long) GetValue(i);
		}

		/// <summary>Gets the single-precision floating point number of the specified field.</summary>
		/// <returns>The single-precision floating point number of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public float GetFloat(int i)
		{
			return (float) GetValue(i);
		}

		/// <summary>Gets the double-precision floating point number of the specified field.</summary>
		/// <returns>The double-precision floating point number of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public double GetDouble(int i)
		{
			return (double) GetValue(i);
		}

		/// <summary>Gets the string value of the specified field.</summary>
		/// <returns>The string value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public string GetString(int i)
		{
			return (string) GetValue(i);
		}

		/// <summary>Gets the fixed-position numeric value of the specified field.</summary>
		/// <returns>The fixed-position numeric value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public decimal GetDecimal(int i)
		{
			return (decimal) GetValue(i);
		}

		/// <summary>Gets the date and time data value of the specified field.</summary>
		/// <returns>The date and time data value of the specified field.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public DateTime GetDateTime(int i)
		{
			return (DateTime) GetValue(i);
		}

		/// <summary>Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.</summary>
		/// <returns>The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public IDataReader GetData(int i)
		{
			return new ObjectDataRecord(GetValue(i), _config, Depth + 1);
		}

		/// <summary>Return whether the specified field is set to null.</summary>
		/// <returns>true if the specified field is set to null; otherwise, false.</returns>
		/// <param name="i">The index of the field to find. </param>
		/// <exception cref="T:System.IndexOutOfRangeException">
		///     The index passed was outside the range of 0 through
		///     <see cref="P:System.Data.IDataRecord.FieldCount" />.
		/// </exception>
		public bool IsDBNull(int i)
		{
			return GetValue(i) == null;
		}

		/// <summary>Gets the number of columns in the current row.</summary>
		/// <returns>
		///     When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The
		///     default is -1.
		/// </returns>
		public int FieldCount { get; private set; }

		object IDataRecord.this[int i]
		{
			get { return GetValue(i); }
		}

		object IDataRecord.this[string name]
		{
			get { return GetValue(name); }
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			_classTypeCache = null;
		}

		/// <summary>Closes the <see cref="T:System.Data.IDataReader" /> Object.</summary>
		public void Close()
		{
			IsClosed = true;
			Dispose();
		}

		/// <summary>
		///     Returns a table that
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
				row["AllowDBNull"] = item.Value.PropertyType == typeof(string) ||
				                     Nullable.GetUnderlyingType(item.Value.PropertyType) != null;
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

		/// <summary>Advances the data reader to the next result, when reading the results of batch SQL statements.</summary>
		/// <returns>true if there are more rows; otherwise, false.</returns>
		public bool NextResult()
		{
			return false;
		}

		/// <summary>Advances the <see cref="T:System.Data.IDataReader" /> to the next record.</summary>
		/// <returns>Allways false</returns>
		public bool Read()
		{
			return false;
		}

		/// <summary>Gets a value indicating the depth of nesting for the current row.</summary>
		/// <returns>The level of nesting.</returns>
		public int Depth { get; private set; }

		/// <summary>Gets a value indicating whether the data reader is closed.</summary>
		/// <returns>true if the data reader is closed; otherwise, false.</returns>
		public bool IsClosed { get; private set; }

		/// <summary>Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.</summary>
		/// <returns>
		///     The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1
		///     for SELECT statements.
		/// </returns>
		public int RecordsAffected { get; private set; }

		private object GetConvertedValue(object val)
		{
			return val ?? DBNull.Value;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public object GetValue(string name)
		{
			return
				GetConvertedValue(_classTypeCache.Propertys[_classTypeCache.SchemaMappingDatabaseToLocal(name)].Getter.Invoke(_poco));
		}
	}
}