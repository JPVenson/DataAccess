using System;

namespace JPB.DataAccess.Config.Model
{
	public class AttributeInfoCache : IComparable<AttributeInfoCache>
	{
		public AttributeInfoCache(Attribute attribute)
		{
			Attribute = attribute;
			AttributeName = attribute.TypeId;
		}

		public Attribute Attribute { get; private set; }
		public object AttributeName { get; private set; }

		public int CompareTo(AttributeInfoCache other)
		{
			return Attribute.GetHashCode() - other.Attribute.GetHashCode();
		}
	}
}