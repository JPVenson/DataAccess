#region

using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class AttributeEquatableComparer : IEqualityComparer<IAttributeInfoCache>, IComparer<IAttributeInfoCache>
	{
		public int Compare(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}
			return x.Attribute.TypeId.GetHashCode() - y.Attribute.TypeId.GetHashCode();
		}

		public bool Equals(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return x.Attribute.TypeId.Equals(y.Attribute.TypeId);
		}

		public int GetHashCode(IAttributeInfoCache obj)
		{
			return obj.Attribute.GetHashCode();
		}
	}
}