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

namespace JPB.DataAccess.AdoWrapper
{
	public sealed class EagarDataReader : EgarDataRecord, IDataReader
	{
		internal EagarDataReader(object sourceObject)
		{
			var type = sourceObject.GetType().GetClassInfo();
			foreach (var item in type.Propertys)
			{
				Objects.Add(item.Key, item.Value.Getter.Invoke(sourceObject));
			}
		}

		public EagarDataReader(IDataRecord sourceRecord)
			: base(sourceRecord)
		{
		}

		public void Close()
		{

		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		public bool Read()
		{
			throw new NotImplementedException();
		}

		public int Depth { get; private set; }
		public bool IsClosed { get; private set; }
		public int RecordsAffected { get; private set; }
	}

	/// <summary>
	/// Provides an IDataRecord Access that enumerates the Source record
	/// </summary>
	public class EgarDataRecord : IDataRecord, IDisposable
	{
		/// <summary>
		/// Enumerates all items in the source record
		/// </summary>
		/// <param name="sourceRecord"></param>
		public EgarDataRecord(IDataRecord sourceRecord)
			: this()
		{
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
		/// <returns></returns>
		public static EgarDataRecord FromDictionary(Dictionary<string, object> values)
		{
			return new EgarDataRecord()
			{
				Objects = values
			};
		}

		protected internal EgarDataRecord()
		{
			Objects = new Dictionary<string, object>();
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
			for (var i = 0; i < Objects.Count(); i++)
			{
				if (values.Length > i)
					break;
				values.SetValue(Objects.ElementAt(i), i);
			}
			return values.Length;
		}

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
			return new EagarDataReader(GetValue(i));
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
				object val = null;
				if (Objects.TryGetValue(name, out  val))
				{
					return val;
				}
				throw new IndexOutOfRangeException("Name is unkown");
			}
		}

		public void Dispose()
		{
			Objects.Clear();
			Objects = null;
		}
	}
}