using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class AttributeEquatableComparer : IEqualityComparer<IAttributeInfoCache>, IComparer<IAttributeInfoCache>
	{
		public bool Equals(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			return x.Attribute.TypeId.Equals(y.Attribute.TypeId);
		}

		public int GetHashCode(IAttributeInfoCache obj)
		{
			return obj.GetHashCode();
		}

		public int Compare(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			return x.Attribute.TypeId.GetHashCode() - y.Attribute.TypeId.GetHashCode();
		}
	}
}
