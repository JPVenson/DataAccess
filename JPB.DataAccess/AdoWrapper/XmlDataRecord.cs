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
	public class XmlDataRecord : IDataRecord
	{
		private readonly DbClassInfoCache _target;
		private readonly XElement baseElement;

		internal XmlDataRecord(string xmlStream, Type target)
			: this(xmlStream, target.GetClassInfo())
		{

		}

		internal XmlDataRecord(XDocument baseElement, Type target)
			: this(baseElement, target.GetClassInfo())
		{

		}

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

		internal XmlDataRecord(XDocument baseElement, DbClassInfoCache target)
		{
			_target = target;
			this.baseElement = baseElement.Elements().ElementAt(0);
		}

		public string GetName(int i)
		{
			return baseElement.Elements().ElementAt(i).Name.LocalName;
		}

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

		public int FieldCount
		{
			get { return baseElement.Elements().Count(); }
		}

		#region Unsupported

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		object IDataRecord.this[int i]
		{
			get { return GetValue(i); }
		}

		object IDataRecord.this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

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

		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		public byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			throw new NotImplementedException();
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		///     This is our standart solution for Seriliation
		///     takes care of the loader strategy
		/// </summary>
		/// <returns></returns>
		public static XmlDataRecord TryParse(string xmlStream, Type target, bool single)
		{
			if (string.IsNullOrEmpty(xmlStream) || string.IsNullOrWhiteSpace(xmlStream))
				return null;
			try
			{
				var xDocument = XDocument.Parse(xmlStream, LoadOptions.None);
				var record = new XmlDataRecord(xDocument, target);

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

		public IEnumerable<XmlDataRecord> CreateListOfItems()
		{
			var xNodes = baseElement.Elements();
			return xNodes.Select(xNode => new XmlDataRecord(xNode.ToString(), _target));
		}
	}
}