using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JPB.DataAccess.MetaApi.Contract;
using JPB.DataAccess.MetaApi.Model.Equatable;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	/// </summary>
	[DebuggerDisplay("{Attribute.ToString()}")]
	[Serializable]
	public class AttributeInfoCache : IAttributeInfoCache
	{
		protected Attribute _attribute;
		protected object _attributeName;
		/// <summary>
		/// For internal use Only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
		/// <summary>
		/// For Interal use Only
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IAttributeInfoCache Init(Attribute attribute)
		{
			Attribute = attribute;
			AttributeName = attribute.TypeId;
			return this;
		}

		public Attribute Attribute
		{
			get { return _attribute; }
			protected internal set { _attribute = value; }
		}

		public object AttributeName
		{
			get { return _attributeName; }
			protected internal set { _attributeName = value; }
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		public int CompareTo(AttributeInfoCache other)
		{
			return new AttributeEquatableComparer().Compare(this, other);
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		public bool Equals(IAttributeInfoCache other)
		{
			return new AttributeEquatableComparer().Equals(this, other);
		}
	}

	/// <summary>
	/// Easy access to the underlying Attribute by using generics
	/// </summary>
	/// <typeparam name="TAttr"></typeparam>
	public class AttributeInfoCache<TAttr> : AttributeInfoCache
		where TAttr : Attribute
	{
		public new TAttr Attribute { get; set; }
	}
}