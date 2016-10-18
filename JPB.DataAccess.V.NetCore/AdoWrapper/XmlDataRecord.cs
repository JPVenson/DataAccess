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
using System.Reflection;
using System.Xml.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// This is an Helper for reading Xml Based columns in a way as a Ado.net Constructor is written
	/// </summary>
	/// <seealso cref="System.Data.IDataRecord" />
	public class XmlDataRecord : IDataRecord
	{
		/// <summary>
		/// The target
		/// </summary>
		private readonly DbClassInfoCache _target;
		/// <summary>
		/// The base element
		/// </summary>
		private readonly XElement baseElement;

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDataRecord"/> class.
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		/// <param name="config">The configuration.</param>
		internal XmlDataRecord(string xmlStream, Type target, DbConfig config)
			: this(xmlStream, config.GetOrCreateClassInfoCache(target))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDataRecord"/> class.
		/// </summary>
		/// <param name="baseElement">The base element.</param>
		/// <param name="target">The target.</param>
		/// <param name="config">The configuration.</param>
		internal XmlDataRecord(XDocument baseElement, Type target, DbConfig config = null)
			: this(baseElement,
				  config != null
					? config.GetOrCreateClassInfoCache(target)
					: new DbConfig().GetOrCreateClassInfoCache(target))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDataRecord"/> class.
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		internal XmlDataRecord(string xmlStream, DbClassInfoCache target)
		{
			_target = target;
			if (string.IsNullOrEmpty(xmlStream))
			{
				baseElement = new XElement("faild");
				return;
			}

			baseElement = XDocument.Parse(xmlStream).Elements().ElementAt(0);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDataRecord"/> class.
		/// </summary>
		/// <param name="baseElement">The base element.</param>
		/// <param name="target">The target.</param>
		internal XmlDataRecord(XDocument baseElement, DbClassInfoCache target)
		{
			_target = target;
			this.baseElement = baseElement.Elements().ElementAt(0);
		}

		/// <summary>
		/// Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return baseElement.Elements().ElementAt(i).Name.LocalName;
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
			if (i == -1)
				return System.DBNull.Value;
			if (i >= baseElement.Elements().Count())
				return System.DBNull.Value;

			var name = GetName(i);

			var mapEntiysPropToSchema = _target.GetDbToLocalSchemaMapping(name);

			var firstOrDefault = _target.GetPropertiesEx().FirstOrDefault(s => s.Name == mapEntiysPropToSchema);
			if (firstOrDefault == null)
				return System.DBNull.Value;

			var propertyType = firstOrDefault.PropertyType;
			var xElement = baseElement.Elements().ElementAt(i);

			if (xElement.HasElements)
				return xElement.ToString();

			var type = DataConverterExtensions.ChangeType(xElement.Value, propertyType);
			return type;
		}

		/// <summary>
		/// Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			get { return baseElement.Elements().Count(); }
		}

		#region Unsupported

		/// <summary>
		/// Gets the data type information for the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The data type information for the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
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
			get { return GetValue(i); }
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified name.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		object IDataRecord.this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		/// <summary>
		/// Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into.</param>
		/// <returns>
		/// The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
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
			var elements = baseElement.Elements().ToArray();
			for (int i = 0; i < elements.Count(); i++)
			{
				var item = elements[i];
				if (item.Name == name)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		/// The value of the column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the 8-bit unsigned integer value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		/// The 8-bit unsigned integer value of the specified column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public byte GetByte(int i)
		{
			throw new NotImplementedException();
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
		/// <exception cref="NotImplementedException"></exception>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		/// The character value of the specified column.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public char GetChar(int i)
		{
			throw new NotImplementedException();
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
		/// <exception cref="NotImplementedException"></exception>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the GUID value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The GUID value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 16-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 32-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The 64-bit signed integer value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the single-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The single-precision floating point number of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The double-precision floating point number of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The string value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The fixed-position numeric value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the date and time data value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The date and time data value of the specified field.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// true if the specified field is set to null; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// This is our standart solution for Seriliation
		/// takes care of the loader strategy
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <param name="target">The target.</param>
		/// <param name="single">if set to <c>true</c> [single].</param>
		/// <param name="accessLayer">The access layer.</param>
		/// <returns></returns>
		public static XmlDataRecord TryParse(string xmlStream, Type target, bool single, DbConfig accessLayer = null)
		{
			if (string.IsNullOrEmpty(xmlStream) || string.IsNullOrWhiteSpace(xmlStream))
				return null;
			try
			{
				var xDocument = XDocument.Parse(xmlStream, LoadOptions.None);
				var record = new XmlDataRecord(xDocument, target, accessLayer);

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
		/// Creates the list of items.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<XmlDataRecord> CreateListOfItems()
		{
			var xNodes = baseElement.Elements();
			return xNodes.Select(xNode => new XmlDataRecord(xNode.ToString(), _target));
		}
	}
}