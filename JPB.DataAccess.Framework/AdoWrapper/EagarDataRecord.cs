﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///     Provides an IDataRecord Access that enumerates the Source record. Not ThreadSave
	/// </summary>
	/// <seealso cref="System.Data.IDataRecord" />
	/// <seealso cref="System.IDisposable" />
	public class EagarDataRecord : IDataRecord, IDisposable, IXmlSerializable
	{
		//internal EagarDataRecord(SerializationInfo info, StreamingContext context)
		//{
			
		//}

		//public void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
			
		//}

		/// <summary>
		///     Enumerates all items in the source record
		/// </summary>
		public EagarDataRecord(string[] fields, IList values) : this()
		{
			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				MetaHeader.Add(field, values[i]);
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EagarDataRecord" /> class.
		/// </summary>
		public EagarDataRecord()
		{
			MetaHeader = new MultiValueDictionary<string, object>();
		}

		private EagarDataRecord(MultiValueDictionary<string, object> subRecordMetaHeader)
		{
			MetaHeader = new MultiValueDictionary<string, object>(subRecordMetaHeader);
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			WrapNulls = reader.GetAttribute("wnull") != null;
			reader.ReadStartElement();//<d>
			while (reader.Name == "d")
			{
				var name = reader.GetAttribute("n");
				object val = DBNull.Value;
				if (!reader.IsEmptyElement)
				{
					reader.ReadStartElement();//<v>
					var type = reader.GetAttribute("t");
					var returnType = Type.GetType(type);
					if (returnType == typeof(Guid))
					{
						val = XmlConvert.ToGuid(reader.ReadElementContentAsString());
					}
					else
					{
						val = reader.ReadElementContentAs(returnType, null);
					}
					
					reader.ReadEndElement();//</d>
				}
				else
				{
					reader.ReadStartElement();//<d>		
				}
				MetaHeader.Add(name, val);
			}
		}

		private const string XmlNamespace = "https://github.com/JPVenson/DataAccess";

		public void WriteXml(XmlWriter writer)
		{
			if (WrapNulls)
			{
				writer.WriteAttributeString("wnull", "");
			}

			foreach (var header in MetaHeader.Collection)
			{
				writer.WriteStartElement("d");
				writer.WriteAttributeString("n", header.Key);
				if (header.Value != DBNull.Value)
				{
					writer.WriteStartElement("v");
					writer.WriteAttributeString("t", header.Value.GetType().FullName);
					if (header.Value is Guid guid)
					{
						writer.WriteValue(XmlConvert.ToString(guid));
					}
					else
					{
						writer.WriteValue(header.Value);	
					}
					
					writer.WriteEndElement();//</v>
				}
				writer.WriteEndElement();//</d>
			}
		}

		/// <summary>
		///     If set to true
		///     <value>DBNull</value>
		///     values are converted to regular .net null values
		/// </summary>
		public bool WrapNulls { get; set; }

		internal MultiValueDictionary<string, object> MetaHeader { get; set; }

		/// <summary>
		///     Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return MetaHeader.KeyAt(i);
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
		///     Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
		/// <returns>
		///     The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		public int GetValues(object[] values)
		{
			for (var i = 0; i < MetaHeader.Count; i++)
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
			return MetaHeader.IndexOf(name);
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
			return (bool)GetValue(i);
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
			return (byte)GetValue(i);
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
			var value = (byte[])GetValue(i);
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
			return (char)GetValue(i);
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
			var value = (char[])GetValue(i);
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
			return (Guid)GetValue(i);
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
			return (short)GetValue(i);
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
			return (int)GetValue(i);
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
			return (long)GetValue(i);
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
			return (float)GetValue(i);
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
			return (double)GetValue(i);
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
			return (string)GetValue(i);
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
			return (decimal)GetValue(i);
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
			return (DateTime)GetValue(i);
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
			get { return MetaHeader.Count; }
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
			get { return GetValue(name); }
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		internal void Add(string name, object value)
		{
			MetaHeader.Add(name, value);
		}

		internal void Remove(string name)
		{
			MetaHeader.Remove(name);
		}

		/// <summary>
		///     Creates a new Eager Data Record that contains all fields from the SourceRecord but not therese defined in
		///     fieldsExcluded
		/// </summary>
		/// <param name="sourceRecord"></param>
		/// <param name="fieldsExcluded"></param>
		/// <returns></returns>
		public static EagarDataRecord WithExcludedFields(IDataRecord sourceRecord, params string[] fieldsExcluded)
		{
			if (sourceRecord is EagarDataRecord subRecord)
			{
				return new EagarDataRecord(subRecord.MetaHeader);
			}

			var buildList = new ArrayList();
			var metaBuildList = new string[sourceRecord.FieldCount];
			for (var i = 0; i < sourceRecord.FieldCount; i++)
			{
				var name = sourceRecord.GetName(i);
				if (fieldsExcluded.Contains(name))
				{
					continue;
				}

				var obj = sourceRecord.GetValue(i);
				buildList.Add(obj);
				metaBuildList[i] = name;
			}

			return new EagarDataRecord(metaBuildList, buildList);
		}

		/// <summary>
		///     Return the value of the specified field.
		/// </summary>
		/// <param name="name">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Object" /> which will contain the field value upon return.
		/// </returns>
		public object GetValue(string name)
		{
			return GetValueInternal(name);
		}

		/// <summary>
		///     If overwritten provides the object on index <c>i</c>
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected internal virtual object GetValueInternal(int i)
		{
			var val = MetaHeader[i];
			return val == DBNull.Value && WrapNulls ? null : val;
		}

		/// <summary>
		///     If overwritten provides the object on index <c>i</c>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected internal virtual object GetValueInternal(string name)
		{
			var val = MetaHeader[name];
			return val == DBNull.Value && WrapNulls ? null : val;
		}
	}
}