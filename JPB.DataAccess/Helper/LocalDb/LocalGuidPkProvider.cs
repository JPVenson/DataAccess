using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// </summary>
	public class LocalGuidPkProvider : ILocalPrimaryKeyValueProvider
	{
		public Type GeneratingType
		{
			get { return typeof (Guid); }
		}

		public object GetNextValue()
		{
			return Guid.NewGuid();
		}

		public object GetUninitilized()
		{
			return null;
		}

		public ILocalPrimaryKeyValueProvider Clone()
		{
			return new LocalGuidPkProvider();
		}

		public bool Equals(object x, object y)
		{
			return (Guid) x == (Guid) y;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}