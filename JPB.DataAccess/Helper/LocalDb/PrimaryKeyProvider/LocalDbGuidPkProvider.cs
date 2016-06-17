using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	/// 
	/// </summary>
	public class LocalDbGuidPkProvider : ILocalDbPrimaryKeyConstraint
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
			return new Guid();
		}

		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbGuidPkProvider();
		}

		public new bool Equals(object x, object y)
		{
			return (Guid)x == (Guid)y;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}