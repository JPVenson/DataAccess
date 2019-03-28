#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Anonymous;
using JPB.DataAccess.DbInfoConfig;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///     Provides an IDataRecord Access that enumerates the Source record. Not ThreadSave
	/// </summary>
	/// <seealso cref="System.Data.IDataRecord" />
	/// <seealso cref="System.IDisposable" />
	public class EagarDataRecord : IDataRecord, IDisposable
	{
		/// <summary>
		///		If set to true <value>DBNull</value> values are converted to regular .net null values
		/// </summary>
		public bool WrapNulls { get; set; }

		internal void Add(string name, object value)
		{
			MetaHeader = MetaHeader.Concat(new[] {name}).ToArray();
			Objects.Add(value);
		}

		///  <summary>
		/// 		Creates a new Eager Data Record that contains all fields from the SourceRecord but not therese defined in fieldsExcluded
		///  </summary>
		///  <param name="sourceRecord"></param>
		///  <param name="fieldsExcluded"></param>
		///  <returns></returns>
		public static EagarDataRecord WithExcludedFields(IDataRecord sourceRecord, params string[] fieldsExcluded)
		{
			var buildList = new ArrayList();
			var metaBuildList = new List<string>();
			for (var i = 0; i < sourceRecord.FieldCount; i++)
			{
				var name = sourceRecord.GetName(i);
				if (fieldsExcluded.Contains(name))
				{
					continue;
				}

				var obj = sourceRecord.GetValue(i);
				buildList.Add(obj);
				metaBuildList.Add(name);
			}
			return new EagarDataRecord(metaBuildList.ToArray(), buildList);
		}

		///  <summary>
		/// 		Creates a new Eager Data Record that contains all fields from the SourceRecord but only therese defined in fieldsIncluded
		///  </summary>
		///  <param name="sourceRecord"></param>
		///  <param name="fieldsIncluded"></param>
		///  <returns></returns>
		public static EagarDataRecord WithIncludedFields(IDataRecord sourceRecord, params string[] fieldsIncluded)
		{
			var buildList = new ArrayList();
			var metaBuildList = new List<string>();
			for (var i = 0; i < sourceRecord.FieldCount; i++)
			{
				var name = sourceRecord.GetName(i);
				if (!fieldsIncluded.Contains(name))
				{
					continue;
				}

				var obj = sourceRecord.GetValue(i);
				buildList.Add(obj);
				metaBuildList.Add(name);
			}
			return new EagarDataRecord(metaBuildList.ToArray(), buildList);
		}

		/// <summary>
		///     Enumerates all items in the source record
		/// </summary>
		internal EagarDataRecord(string[] fields, ArrayList values)
		{
			Objects = values;
			MetaHeader = fields;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EagarDataRecord" /> class.
		/// </summary>
		protected internal EagarDataRecord()
		{
		}

		/// <summary>
		///     Gets or sets the objects.
		/// </summary>
		/// <value>
		///     The objects.
		/// </value>
		internal ArrayList Objects { get; set; }

		/// <summary>
		/// The Headers sorted by the occurence in the Class
		/// (Sorted for Performance reasons)
		/// </summary>
		protected string[] MetaHeader { get; set; }

		/// <summary>
		///     Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return MetaHeader[i];
		}

		/// <summary>
		///     Gets the data type information for the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The data type information for the specified field.
		/// </returns>
		public string GetDataTypeName(int i)
		{
			return GetValue(i).GetType().FullName;
		}

		/// <summary>
		///     Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that
		///     would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would
		///     be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </returns>
		public Type GetFieldType(int i)
		{
			return GetValue(i).GetType();
		}

		/// <summary>
		///     Return the value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Object" /> which will contain the field value upon return.
		/// </returns>
		public object GetValue(int i)
		{
			return GetValueInternal(i);
		}

		/// <summary>
		///		If overwritten provides the object on index <c>i</c>
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected internal virtual object GetValueInternal(int i)
		{
			var val = Objects[i];
			return val == DBNull.Value && WrapNulls ? null : val;
		}

		/// <summary>
		///     Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
		/// <returns>
		///     The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		public int GetValues(object[] values)
		{
			for (var i = 0; i < Objects.Count; i++)
			{
				if (values.Length > i)
				{
					break;
				}
				values.SetValue(GetValueInternal(i), i);
			}
			return values.Length;
		}

		/// <summary>
		///     Return the index of the named field.
		/// </summary>
		/// <param name="name">The name of the field to find.</param>
		/// <returns>
		///     The index of the named field.
		/// </returns>
		public int GetOrdinal(string name)
		{
			return Array.IndexOf(MetaHeader, name);
		}

		/// <summary>
		///     Gets the boolean.
		/// </summary>
		/// <param name="i">The i.</param>
		/// <returns>
		///     The value of the column.
		/// </returns>
		public bool GetBoolean(int i)
		{
			return (bool) GetValue(i);
		}

		/// <summary>
		///     Gets the 8-bit unsigned integer value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The 8-bit unsigned integer value of the specified column.
		/// </returns>
		public byte GetByte(int i)
		{
			return (byte) GetValue(i);
		}

		/// <summary>
		///     Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer
		///     offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		///     The actual number of bytes read.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     fieldOffset
		///     or
		///     bufferoffset
		///     or
		///     bufferoffset
		/// </exception>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			var value = (byte[]) GetValue(i);
			if (fieldOffset > value.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(fieldOffset));
			}

			if (bufferoffset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferoffset));
			}

			if (length > value.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferoffset));
			}

			long j;
			for (j = fieldOffset; j < value.Length || j < length; j++)
			{
				buffer[j + bufferoffset] = value[j];
			}
			return j;
		}

		/// <summary>
		///     Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The character value of the specified column.
		/// </returns>
		public char GetChar(int i)
		{
			return (char) GetValue(i);
		}

		/// <summary>
		///     Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given
		///     buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		///     The actual number of characters read.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     fieldoffset
		///     or
		///     bufferoffset
		///     or
		///     bufferoffset
		/// </exception>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			var value = (char[]) GetValue(i);
			if (fieldoffset > value.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(fieldoffset));
			}

			if (bufferoffset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferoffset));
			}

			if (length > value.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferoffset));
			}

			long j;
			for (j = fieldoffset; j < value.Length || j < length; j++)
			{
				buffer[j + bufferoffset] = value[j];
			}
			return j;
		}

		/// <summary>
		///     Returns the GUID value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The GUID value of the specified field.
		/// </returns>
		public Guid GetGuid(int i)
		{
			return (Guid) GetValue(i);
		}

		/// <summary>
		///     Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 16-bit signed integer value of the specified field.
		/// </returns>
		public short GetInt16(int i)
		{
			return (short) GetValue(i);
		}

		/// <summary>
		///     Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 32-bit signed integer value of the specified field.
		/// </returns>
		public int GetInt32(int i)
		{
			return (int) GetValue(i);
		}

		/// <summary>
		///     Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 64-bit signed integer value of the specified field.
		/// </returns>
		public long GetInt64(int i)
		{
			return (long) GetValue(i);
		}

		/// <summary>
		///     Gets the single-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The single-precision floating point number of the specified field.
		/// </returns>
		public float GetFloat(int i)
		{
			return (float) GetValue(i);
		}

		/// <summary>
		///     Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The double-precision floating point number of the specified field.
		/// </returns>
		public double GetDouble(int i)
		{
			return (double) GetValue(i);
		}

		/// <summary>
		///     Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The string value of the specified field.
		/// </returns>
		public string GetString(int i)
		{
			return (string) GetValue(i);
		}

		/// <summary>
		///     Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The fixed-position numeric value of the specified field.
		/// </returns>
		public decimal GetDecimal(int i)
		{
			return (decimal) GetValue(i);
		}

		/// <summary>
		///     Gets the date and time data value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The date and time data value of the specified field.
		/// </returns>
		public DateTime GetDateTime(int i)
		{
			return (DateTime) GetValue(i);
		}

		/// <summary>
		///     Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </returns>
		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
			//var val = GetValue(i);
			//return new EagarDataReader(val);
		}

		/// <summary>
		///     Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     true if the specified field is set to null; otherwise, false.
		/// </returns>
		public bool IsDBNull(int i)
		{
			return GetValue(i) is DBNull;
		}

		/// <summary>
		///     Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			get { return Objects.Count; }
		}

		/// <summary>
		///     Gets the <see cref="System.Object" /> with the specified i.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="i">The i.</param>
		/// <returns></returns>
		public object this[int i]
		{
			get
			{
				var value = GetValue(i);
				if (value is DBNull)
				{
					return null;
				}
				return value;
			}
		}

		/// <summary>
		///     Gets the <see cref="System.Object" /> with the specified name.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException">Name is unkown</exception>
		public object this[string name]
		{
			get
			{
				return GetValueInternal(GetOrdinal(name));
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Objects = null;
		}

		/// <summary>
		///     Gets the order of the Properties in the object
		/// </summary>
		/// <returns></returns>
		public IDictionary<int, string> GetDataOrder()
		{
			return MetaHeader.Select((val, index) => new {index, val}).ToDictionary(f => f.index, f => f.val);
		}

		//	};
		//		Objects = values
		//	{
		//	return new EgarDataRecord
		//{
		//public static EgarDataRecord FromDictionary(Dictionary<string, object> values)
		///// <returns></returns>
		///// <param name="values">The values.</param>
		///// </summary>
		/////     Creates a new Eagar recrod based on an Dictionary

		///// <summary>
	}
}