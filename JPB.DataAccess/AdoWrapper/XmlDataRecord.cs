using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.AdoWrapper
{
    internal class XmlDataRecord : IDataRecord
    {
        private readonly Type _target;
        private XElement baseElement;

        public XmlDataRecord(string xmlStream, Type target)
        {
            _target = target;
            if (string.IsNullOrEmpty(xmlStream))
            {
                baseElement = new XElement("faild");
                return;
            }

            baseElement = XDocument.Parse(xmlStream).Elements().ElementAt(0);
        }

        public string GetName(int i)
        {
            return baseElement.Elements().ElementAt(i).Name.LocalName;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            var name = GetName(i);

            var mapEntiysPropToSchema = this._target.ReMapSchemaToEntiysProp(name);

            var firstOrDefault = _target.GetProperties().FirstOrDefault(s => s.Name == mapEntiysPropToSchema);
            if (firstOrDefault == null)
                return null;

            var propertyType = firstOrDefault.PropertyType;
            var xElement = baseElement.Elements().ElementAt(i);

            if (xElement.HasElements)
                return xElement.ToString();

            var type = DataConverterExtensions.ChangeType(xElement.Value, propertyType);
            return type;
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
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

        public int FieldCount
        {
            get { return baseElement.Elements().Count(); }
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<XmlDataRecord> CreateListOfItems()
        {
            var xNodes = baseElement.Elements();
            return xNodes.Select(xNode => new XmlDataRecord(xNode.ToString(), this._target));
        }
    }
}
