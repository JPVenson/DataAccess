﻿using System;
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
using JPB.DataAccess;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.AdoWrapper
{
    internal class XmlDataRecord : IDataRecord
    {
        /// <summary>
        /// This is our standart solution for Seriliation
        /// takes care of the loader strategy
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static XmlDataRecord TryParse(string xmlStream, Type target)
        {
            if (string.IsNullOrEmpty(xmlStream) || string.IsNullOrWhiteSpace(xmlStream))
                return null;
            try
            {
                var xDocument = XDocument.Parse(xmlStream, LoadOptions.None);
                return new XmlDataRecord(xDocument, target);
            }
            catch (Exception ex)
            {
                DbAccessLayer.RaiseException(null, ex);
                return null;
            }
        }

        private readonly Type _target;
        private readonly XElement baseElement;

        internal XmlDataRecord(string xmlStream, Type target)
        {
            _target = target;
            if (string.IsNullOrEmpty(xmlStream))
            {
                baseElement = new XElement("faild");
                return;
            }

            baseElement = XDocument.Parse(xmlStream).Elements().ElementAt(0);
        }

        internal XmlDataRecord(XDocument baseElement, Type target)
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
            var name = GetName(i);

            var mapEntiysPropToSchema = ConfigHelper.GetLocalToDbSchemaMapping(this._target, name);

            var firstOrDefault = ConfigHelper.GetPropertiesEx(_target).FirstOrDefault(s => s.Name == mapEntiysPropToSchema);
            if (firstOrDefault == null)
                return null;

            var propertyType = firstOrDefault.PropertyType;
            var xElement = baseElement.Elements().ElementAt(i);

            if (xElement.HasElements)
                return xElement.ToString();

            var type = DataConverterExtensions.ChangeType(xElement.Value, propertyType);
            return type;
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
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
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

        #endregion

        public int FieldCount
        {
            get { return baseElement.Elements().Count(); }
        }

        public IEnumerable<XmlDataRecord> CreateListOfItems()
        {
            var xNodes = baseElement.Elements();
            return xNodes.Select(xNode => new XmlDataRecord(xNode.ToString(), this._target));
        }

    }
}
