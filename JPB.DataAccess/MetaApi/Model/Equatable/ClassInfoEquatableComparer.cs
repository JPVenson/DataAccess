#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	public class ClassInfoEquatableComparer
		: IEqualityComparer<IClassInfoCache>,
			IEqualityComparer<Type>,
			IComparer<IClassInfoCache>
	{
		/// <inheritdoc />
		public int Compare(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}
			return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
		}

		/// <inheritdoc />
		public bool Equals(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			if (x.Name != y.Name)
			{
				return false;
			}
			if (x.Type == y.Type)
			{
				return true;
			}
			return true;
		}

		/// <inheritdoc />
		public int GetHashCode(IClassInfoCache obj)
		{
			return GetHashCode(obj.Type);
		}

		/// <inheritdoc />
		public bool Equals(Type x, Type y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			if (x.FullName != y.FullName)
			{
				return false;
			}
			if (x == y)
			{
				return true;
			}
			if (x.IsEquivalentTo(y))
			{
				return true;
			}
			return false;
		}

		/// <inheritdoc />
		public int GetHashCode(Type obj)
		{
			return obj.GetHashCode();
		}
	}
}