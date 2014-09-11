using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.AdoWrapper
{
    public class EgarDataRecord : IDataRecord
    {
        public Dictionary<string, object> Objects { get; set; }

        public EgarDataRecord(IDataRecord sourceRecord)
        {
            Objects = new Dictionary<string, object>();
            for (int i = 0; i < sourceRecord.FieldCount; i++)
            {
                var obj = sourceRecord[i];
                Objects.Add(sourceRecord.GetName(i), obj);
            }
        }

        public string GetName(int i)
        {
            return Objects.ElementAt(i).Key;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
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
                values.SetValue(Objects.ElementAt(i),i);
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
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
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
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) is DBNull;
        }

        public int FieldCount { get { return this.Objects.Count; }}

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

        object IDataRecord.this[string name]
        {
            get
            {
                var value = Objects.First(s => s.Key == name).Value;
                if (value is DBNull)
                    return null;
                return value;
            }
        }
    }
}
