using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// 
	/// </summary>
	public class LocalLongPkProvider : ILocalPrimaryKeyValueProvider
	{
		private long _counter = 1;

		public Type GeneratingType
		{
			get { return typeof(long); }
		}

		public object GetNextValue()
		{
			return _counter++;
		}

		public object GetUninitilized()
		{
			return 0L;
		}

		public object Clone()
		{
			return new LocalLongPkProvider();
		}

		public bool Equals(object x, object y)
		{
			return (long)x == (long)y;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}