using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// 
	/// </summary>
	public class LocalIntPkProvider : ILocalPrimaryKeyValueProvider
	{
		private volatile int _counter = 1;

		public Type GeneratingType
		{
			get { return typeof (int); }
		}

		public object GetNextValue()
		{
			return _counter++;
		}

		public object Clone()
		{
			return new LocalIntPkProvider();
		}

		public bool Equals(object x, object y)
		{
			return (int)x == (int)y;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}