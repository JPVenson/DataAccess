using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JPB.DataAccess.AdoWrapper
{
    public sealed class EgarDataRecord : IDataRecord, IDisposable
    {
        public EgarDataRecord(IDataRecord sourceRecord)
        {
            Objects = new Dictionary<string, object>();
            for (int i = 0; i < sourceRecord.FieldCount; i++)
            {
                object obj = sourceRecord[i];
                var name = sourceRecord.GetName(i);
                Objects.Add(name, obj);
            }
        }

        internal Dictionary<string, object> Objects { get; set; }

        public string GetName(int i)
        {
            return Objects.ElementAt(i).Key;
        }

        public string GetDataTypeName(int i)
        {
            return Objects.ElementAt(i).Value.GetType().FullName;
        }

        public Type GetFieldType(int i)
        {
            return Objects.ElementAt(i).Value.GetType();
        }

        public object GetValue(int i)
        {
            return Objects.ElementAt(i).Value;
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < Objects.Count(); i++)
            {
                if (values.Length > i)
                    break;
                values.SetValue(Objects.ElementAt(i), i);
            }
            return values.Length;
        }

        public int GetOrdinal(string name)
        {
            return (int)Objects.FirstOrDefault(s => s.Key == name).Value;
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

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

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
            throw new NotImplementedException();
            //return new XmlDataRecord(GetString(i), typeof(object));
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) is DBNull;
        }

        public int FieldCount
        {
            get { return Objects.Count; }
        }

        object IDataRecord.this[int i]
        {
            get
            {
                object value = GetValue(i);
                if (value is DBNull)
                    return null;
                return value;
            }
        }

        object IDataRecord.this[string name]
        {
            get
            {
                var firstOrDefault = Objects.Select(s => (KeyValuePair<string, object>?)s).FirstOrDefault(s =>
                               s.HasValue && s.Value.Key == name);
                if (firstOrDefault != null)
                {
                    object value = firstOrDefault.Value.Value;
                    return value;
                }
                return null;
            }
        }

        public void Dispose()
        {
            Objects.Clear();
            Objects = null;
        }
    }
}