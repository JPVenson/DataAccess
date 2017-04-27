#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class ClassInfoEquatableComparer
		: IEqualityComparer<IClassInfoCache>,
			IEqualityComparer<Type>,
			IComparer<IClassInfoCache>
	{
		public int Compare(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
		}

		public bool Equals(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.Name != y.Name)
				return false;
			if (x.Type == y.Type)
				return true;
			return true;
		}

		public int GetHashCode(IClassInfoCache obj)
		{
			return GetHashCode(obj.Type);
		}

		public bool Equals(Type x, Type y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.FullName != y.FullName)
				return false;
			if (x == y)
				return true;
			if (x.IsEquivalentTo(y))
				return true;
			return false;
		}

		public int GetHashCode(Type obj)
		{
			return obj.GetHashCode();
		}
	}
}