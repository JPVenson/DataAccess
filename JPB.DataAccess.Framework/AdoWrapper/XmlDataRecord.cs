#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///		Wrapper for the XmlDataRecord to comply with the WrapDbNullAttribute
	/// </summary>
	public class NullWrapperXmlDataRecord : XmlDataRecord
	{
		/// <inheritdoc />
		internal NullWrapperXmlDataRecord(string xmlStream, Type target, DbConfig config) : base(xmlStream, target, config)
		{
		}

		/// <inheritdoc />
		internal NullWrapperXmlDataRecord(XDocument baseElement, Type target, DbConfig config = null) : base(baseElement, target, config)
		{
		}

		/// <inheritdoc />
		internal NullWrapperXmlDataRecord(string xmlStream, DbClassInfoCache target) : base(xmlStream, target)
		{
		}

		/// <inheritdoc />
		internal NullWrapperXmlDataRecord(XDocument baseElement, DbClassInfoCache target) : base(baseElement, target)
		{
		}

		/// <inheritdoc />
		public override object GetValue(int i)
		{
			var value = base.GetValue(i);

			return value == DBNull.Value ? null : value;
		}
	}

	/// <summary>
	///     This is an Helper for reading Xml Based columns in a way as a Ado.net Constructor is written
	/// </summary>
	/// <seealso cref="System.Data.IDataRecord" />
	public class XmlDataRecord : IDataRecord
	{
		/// <summary>
		///     The target
		/// </summary>
		private readonly DbClassInfoCache _target;

		/// <summary>
		///     The base element
		/// </summary>
		private readonly XElement _baseElement;

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlDataRecord" /> class.
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		/// <param name="config">The configuration.</param>
		internal XmlDataRecord(string xmlStream, Type target, DbConfig config)
			: this(xmlStream, config.GetOrCreateClassInfoCache(target))
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlDataRecord" /> class.
		/// </summary>
		/// <param name="baseElement">The base element.</param>
		/// <param name="target">The target.</param>
		/// <param name="config">The configuration.</param>
		internal XmlDataRecord(XDocument baseElement, Type target, DbConfig config = null)
			: this(baseElement,
				config != null
					? config.GetOrCreateClassInfoCache(target)
					: target.GetClassInfo())
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlDataRecord" /> class.
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		internal XmlDataRecord(string xmlStream, DbClassInfoCache target)
		{
			_target = target;
			if (string.IsNullOrEmpty(xmlStream))
			{
				_baseElement = new XElement("failed to load XML Stream");
				return;
			}

			_baseElement = XDocument.Parse(xmlStream).Elements().ElementAt(0);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlDataRecord" /> class.
		/// </summary>
		/// <param name="baseElement">The base element.</param>
		/// <param name="target">The target.</param>
		internal XmlDataRecord(XDocument baseElement, DbClassInfoCache target)
		{
			_target = target;
			this._baseElement = baseElement.Elements().ElementAt(0);
		}

		/// <summary>
		///     Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return _baseElement.Elements().ElementAt(i).Name.LocalName;
		}

		/// <summary>
		///     Return the value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Object" /> which will contain the field value upon return.
		/// </returns>
		public virtual object GetValue(int i)
		{
			if (i == -1)
			{
				return DBNull.Value;
			}

			if (i >= _baseElement.Elements().Count())
			{
				return DBNull.Value;
			}

			var name = GetName(i);

			var mapEntities = _target.GetDbToLocalSchemaMapping(name);

			var firstOrDefault = _target.GetPropertiesEx().FirstOrDefault(s => s.Name == mapEntities);
			if (firstOrDefault == null)
			{
				return DBNull.Value;
			}

			var propertyType = firstOrDefault.PropertyType;
			var xElement = _baseElement.Elements().ElementAt(i);

			if (xElement.HasElements)
			{
				return xElement.ToString();
			}

			object value = xElement.Value;
			var type = DataConverterExtensions.ChangeType(ref value, propertyType);
			return value;
		}

		/// <summary>
		///     Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			get { return _baseElement.Elements().Count(); }
		}

		/// <summary>
		///     This is our standart solution for Seriliation
		///     takes care of the loader strategy
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		/// <param name="single">if set to <c>true</c> [single].</param>
		/// <param name="config">The access layer.</param>
		/// <returns></returns>
		public static XmlDataRecord TryParse(string xmlStream, Type target, bool single, DbConfig config = null)
		{
			if (string.IsNullOrEmpty(xmlStream) || string.IsNullOrWhiteSpace(xmlStream))
			{
				return null;
			}
			try
			{
				var xDocument = XDocument.Parse(xmlStream, LoadOptions.None);
				var record = new XmlDataRecord(xDocument, target, config);

				if (single)
				{
					return record.CreateListOfItems().FirstOrDefault();
				}

				return record;
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		///     Creates the list of items.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<XmlDataRecord> CreateListOfItems()
		{
			var xNodes = _baseElement.Elements();
			return xNodes.Select(xNode => new NullWrapperXmlDataRecord(xNode.ToString(), _target));
		}

		#region Unsupported

		/// <summary>
		///     Gets the data type information for the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The data type information for the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
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
		/// <exception cref="NotImplementedException"></exception>
		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the <see cref="System.Object" /> with the specified i.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="i">The i.</param>
		/// <returns></returns>
		object IDataRecord.this[int i]
		{
			get { return GetValue(i); }
		}

		/// <summary>
		///     Gets the <see cref="System.Object" /> with the specified name.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		object IDataRecord.this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		/// <summary>
		///     Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
		/// <returns>
		///     The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
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
			var elements = _baseElement.Elements().ToArray();
			for (var i = 0; i < elements.Count(); i++)
			{
				var item = elements[i];
				if (item.Name == name)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		///     Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The value of the column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the 8-bit unsigned integer value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The 8-bit unsigned integer value of the specified column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public byte GetByte(int i)
		{
			throw new NotImplementedException();
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
		/// <exception cref="NotImplementedException"></exception>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The character value of the specified column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public char GetChar(int i)
		{
			throw new NotImplementedException();
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
		/// <exception cref="NotImplementedException"></exception>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns the GUID value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The GUID value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 16-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 32-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 64-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the single-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The single-precision floating point number of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The double-precision floating point number of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The string value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The fixed-position numeric value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets the date and time data value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The date and time data value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     true if the specified field is set to null; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}