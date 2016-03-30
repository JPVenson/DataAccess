/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class AttributeEquatableComparer : IEqualityComparer<IAttributeInfoCache>, IComparer<IAttributeInfoCache>
	{
		public int Compare(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return x.Attribute.TypeId.GetHashCode() - y.Attribute.TypeId.GetHashCode();
		}

		public bool Equals(IAttributeInfoCache x, IAttributeInfoCache y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			return x.Attribute.TypeId.Equals(y.Attribute.TypeId);
		}

		public int GetHashCode(IAttributeInfoCache obj)
		{
			return obj.Attribute.GetHashCode();
		}
	}
}