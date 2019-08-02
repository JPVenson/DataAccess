#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	/// </summary>
	[DebuggerDisplay("{Attribute.ToString()}")]
	[Serializable]
	public class AttributeInfoCache : IAttributeInfoCache
	{
		private Attribute _attribute;
		private object _attributeName;

		/// <summary>
		///     For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
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
		///     For Interal use Only
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IAttributeInfoCache Init(Attribute attribute)
		{
			Attribute = attribute;
			AttributeName = attribute.TypeId;
			return this;
		}

		/// <summary>
		///     The Instance of the current Attribute
		/// </summary>
		public Attribute Attribute
		{
			get { return _attribute; }
			protected internal set { _attribute = value; }
		}

		/// <summary>
		///     ToBeSupported
		/// </summary>
		public object AttributeName
		{
			get { return _attributeName; }
			protected internal set { _attributeName = value; }
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public int CompareTo(AttributeInfoCache other)
		{
			return new AttributeEquatableComparer().Compare(this, other);
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		public bool Equals(IAttributeInfoCache other)
		{
			return new AttributeEquatableComparer().Equals(this, other);
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	///     Easy access to the underlying Attribute by using generics
	/// </summary>
	/// <typeparam name="TAttr"></typeparam>
	public class AttributeInfoCache<TAttr> : AttributeInfoCache
		where TAttr : Attribute
	{
		/// <summary>
		///     The Instance of the current Attribute
		/// </summary>
		public new TAttr Attribute { get; set; }
	}
}