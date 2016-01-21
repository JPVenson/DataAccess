using System;
using System.Diagnostics;
using JPB.DataAccess.Config.Contract;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	/// </summary>
	[DebuggerDisplay("{Attribute.ToString()}")]
	[Serializable]
	public class AttributeInfoCache : IComparable<AttributeInfoCache>, IAttributeInfoCache
	{
		protected Attribute _attribute;
		protected object _attributeName;

		public AttributeInfoCache()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="attribute"></param>
		public AttributeInfoCache(Attribute attribute)
		{
			Init(attribute);
		}

		public IAttributeInfoCache Init(Attribute attribute)
		{
			Attribute = attribute;
			AttributeName = attribute.TypeId;
			return this;
		}

		/// <summary>
		///		Direct Reflection
		/// </summary>
		public Attribute Attribute
		{
			get { return _attribute; }
			protected internal set { _attribute = value; }
		}

		/// <summary>
		///		Uniqe ID for the Attribute [ToBeSupported]
		/// </summary>
		public object AttributeName
		{
			get { return _attributeName; }
			protected internal set { _attributeName = value; }
		}

		public int CompareTo(AttributeInfoCache other)
		{
			return Attribute.GetHashCode() - other.Attribute.GetHashCode();
		}
	}

	/// <summary>
	/// Easy access to the underlying Attribute by using generics
	/// </summary>
	/// <typeparam name="TAttr"></typeparam>
	public class AttributeInfoCache<TAttr> : AttributeInfoCache 
		where TAttr: Attribute
	{
		public new TAttr Attribute { get; set; }
	}
}