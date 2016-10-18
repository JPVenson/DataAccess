/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// Provides access to the Given object
	/// </summary>
	/// <seealso cref="JPB.DataAccess.AdoWrapper.EgarDataRecord" />
	/// <seealso cref="System.Data.IDataReader" />
	public sealed class EagarDataReader : EgarDataRecord, IDataReader
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EagarDataReader"/> class.
		/// </summary>
		/// <param name="sourceObject">The source object.</param>
		/// <param name="accessLayer">The access layer.</param>
		internal EagarDataReader(object sourceObject, DbAccessLayer accessLayer)
		{
			foreach (var item in accessLayer.Config.GetOrCreateClassInfoCache(sourceObject.GetType()).Propertys)
			{
				Objects.Add(item.Key, item.Value.Getter.Invoke(sourceObject));
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="EagarDataReader" /> class.
		/// </summary>
		/// <param name="sourceRecord">The source record.</param>
		/// <param name="accessLayer">The access layer.</param>
		public EagarDataReader(IDataRecord sourceRecord, DbAccessLayer accessLayer)
			: base(sourceRecord, accessLayer)
		{
		}
		/// <summary>
		/// Closes the <see cref="T:System.Data.IDataReader" /> Object.
		/// </summary>
		public void Close()
		{

		}
		/// <summary>
		/// Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.IDataReader" />.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Advances the data reader to the next result, when reading the results of batch SQL statements.
		/// </summary>
		/// <returns>
		/// true if there are more rows; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool NextResult()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
		/// </summary>
		/// <returns>
		/// true if there are more rows; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Read()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Gets a value indicating the depth of nesting for the current row.
		/// </summary>
		public int Depth { get; private set; }
		/// <summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public bool IsClosed { get; private set; }
		/// <summary>
		/// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		public int RecordsAffected { get; private set; }
	}

	/// <summary>
	/// Provides an IDataRecord Access that enumerates the Source record
	/// </summary>
	/// <seealso cref="System.Data.IDataRecord" />
	/// <seealso cref="System.IDisposable" />
	public class EgarDataRecord : IDataRecord, IDisposable
	{
		/// <summary>
		/// The access layer
		/// </summary>
		private readonly DbAccessLayer _accessLayer;

		/// <summary>
		/// Enumerates all items in the source record
		/// </summary>
		/// <param name="sourceRecord">The source record.</param>
		/// <param name="accessLayer">The access layer.</param>
		public EgarDataRecord(IDataRecord sourceRecord, DbAccessLayer accessLayer)
			: this()
		{
			_accessLayer = accessLayer;
			for (var i = 0; i < sourceRecord.FieldCount; i++)
			{
				var obj = sourceRecord.GetValue(i);
				var name = sourceRecord.GetName(i);
				Objects.Add(name, obj);
			}
		}

		/// <summary>
		/// Creates a new Eagar recrod based on an Dictionary
		/// </summary>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public static EgarDataRecord FromDictionary(Dictionary<string, object> values)
		{
			return new EgarDataRecord()
			{
				Objects = values
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EgarDataRecord" /> class.
		/// </summary>
		protected internal EgarDataRecord()
		{
			Objects = new Dictionary<string, object>();
		}

		/// <summary>
		/// Gets or sets the objects.
		/// </summary>
		/// <value>
		/// The objects.
		/// </value>
		internal Dictionary<string, object> Objects { get; set; }

		/// <summary>
		/// Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return Objects.ElementAt(i).Key;
		}

		/// <summary>
		/// Gets the data type information for the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The data type information for the specified field.
		/// </returns>
		public string GetDataTypeName(int i)
		{
			return Objects.ElementAt(i).Value.GetType().FullName;
		}

		/// <summary>
		/// Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </returns>
		public Type GetFieldType(int i)
		{
			return Objects.ElementAt(i).Value.GetType();
		}

		/// <summary>
		/// Return the value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Object" /> which will contain the field value upon return.
		/// </returns>
		public object GetValue(int i)
		{
			return Objects.ElementAt(i).Value;
		}

		/// <summary>
		/// Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
		/// <returns>
		/// The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		public int GetValues(object[] values)
		{
			for (var i = 0; i < Objects.Count(); i++)
			{
				if (values.Length > i)
					break;
				values.SetValue(Objects.ElementAt(i), i);
			}
			return values.Length;
		}

		/// <summary>
		/// Return the index of the named field.
		/// </summary>
		/// <param name="name">The name of the field to find.</param>
		/// <returns>
		/// The index of the named field.
		/// </returns>
		public int GetOrdinal(string name)
		{
			int counter = 0;
			foreach (var obj in Objects)
			{
				if (obj.Key == name)
					return counter;
				counter++;
			}
			return -1;
		}

		/// <summary>
		/// Gets the boolean.
		/// </summary>
		/// <param name="i">The i.</param>
		/// <returns>
		/// The value of the column.
		/// </returns>
		public bool GetBoolean(int i)
		{
			return (bool)GetValue(i);
		}

		/// <summary>
		/// Gets the 8-bit unsigned integer value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		/// The 8-bit unsigned integer value of the specified column.
		/// </returns>
		public byte GetByte(int i)
		{
			return (byte)GetValue(i);
		}

		/// <summary>
		/// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		/// The actual number of bytes read.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">fieldOffset
		/// or
		/// bufferoffset
		/// or
		/// bufferoffset</exception>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			var value = (byte[])GetValue(i);
			if (fieldOffset > value.Length)
				throw new ArgumentOutOfRangeException("fieldOffset");

			if (bufferoffset > buffer.Length)
				throw new ArgumentOutOfRangeException("bufferoffset");

			if (length > value.Length)
				throw new ArgumentOutOfRangeException("bufferoffset");

			long j;
			for (j = fieldOffset; j < value.Length || j < length; j++)
			{
				buffer[j + bufferoffset] = value[j];
			}
			return j;
		}

		/// <summary>
		/// Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		/// The character value of the specified column.
		/// </returns>
		public char GetChar(int i)
		{
			return (char)GetValue(i);
		}

		/// <summary>
		/// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		/// The actual number of characters read.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// fieldoffset
		/// or
		/// bufferoffset
		/// or
		/// bufferoffset
		/// </exception>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			var value = (char[])GetValue(i);
			if (fieldoffset > value.Length)
				throw new ArgumentOutOfRangeException("fieldoffset");

			if (bufferoffset > buffer.Length)
				throw new ArgumentOutOfRangeException("bufferoffset");

			if (length > value.Length)
				throw new ArgumentOutOfRangeException("bufferoffset");

			long j;
			for (j = fieldoffset; j < value.Length || j < length; j++)
			{
				buffer[j + bufferoffset] = value[j];
			}
			return j;
		}

		/// <summary>
		/// Returns the GUID value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The GUID value of the specified field.
		/// </returns>
		public Guid GetGuid(int i)
		{
			return (Guid)GetValue(i);
		}

		/// <summary>
		/// Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 16-bit signed integer value of the specified field.
		/// </returns>
		public short GetInt16(int i)
		{
			return (short)GetValue(i);
		}

		/// <summary>
		/// Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 32-bit signed integer value of the specified field.
		/// </returns>
		public int GetInt32(int i)
		{
			return (int)GetValue(i);
		}

		/// <summary>
		/// Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 64-bit signed integer value of the specified field.
		/// </returns>
		public long GetInt64(int i)
		{
			return (long)GetValue(i);
		}

		/// <summary>
		/// Gets the single-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The single-precision floating point number of the specified field.
		/// </returns>
		public float GetFloat(int i)
		{
			return (float)GetValue(i);
		}

		/// <summary>
		/// Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The double-precision floating point number of the specified field.
		/// </returns>
		public double GetDouble(int i)
		{
			return (double)GetValue(i);
		}

		/// <summary>
		/// Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The string value of the specified field.
		/// </returns>
		public string GetString(int i)
		{
			return (string)GetValue(i);
		}

		/// <summary>
		/// Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The fixed-position numeric value of the specified field.
		/// </returns>
		public decimal GetDecimal(int i)
		{
			return (decimal)GetValue(i);
		}

		/// <summary>
		/// Gets the date and time data value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The date and time data value of the specified field.
		/// </returns>
		public DateTime GetDateTime(int i)
		{
			return (DateTime)GetValue(i);
		}

		/// <summary>
		/// Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </returns>
		public IDataReader GetData(int i)
		{
			var val = GetValue(i);
			return new EagarDataReader(val, _accessLayer);
		}

		/// <summary>
		/// Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// true if the specified field is set to null; otherwise, false.
		/// </returns>
		public bool IsDBNull(int i)
		{
			return GetValue(i) is DBNull;
		}

		/// <summary>
		/// Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			get { return Objects.Count; }
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified i.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="i">The i.</param>
		/// <returns></returns>
		object IDataRecord.this[int i]
		{
			get
			{
				var value = GetValue(i);
				if (value is DBNull)
					return null;
				return value;
			}
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified name.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException">Name is unkown</exception>
		object IDataRecord.this[string name]
		{
			get
			{
				object val = null;
				if (Objects.TryGetValue(name, out  val))
				{
					return val;
				}
				throw new IndexOutOfRangeException("Name is unkown");
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Objects.Clear();
			Objects = null;
		}
	}
}