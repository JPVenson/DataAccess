#region

using System;
using JPB.DataAccess.Framework.Contacts.MetaApi;
using JPB.DataAccess.Framework.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.Framework.MetaApi.Model
{
	/// <summary>
	///     Fake Constructor for Structs
	/// </summary>
	public class ConstructorStructFakeInfoCache :
		MethodInfoCache<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>>,
		IConstructorInfoCache<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>>
	{
		/// <summary>
		///     Creates a new Constructor for an Struct
		/// </summary>
		/// <param name="del"></param>
		/// <param name="name"></param>
		public ConstructorStructFakeInfoCache(Func<object> del, string name)
			: base((e, f) => { return del(); }, typeof(ConstructorStructFakeInfoCache), ".ctor")
		{
		}

#pragma warning disable CS1591
		public int CompareTo(IConstructorInfoCache<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>> other)
		{
			return
				new ConstructorInfoCacheEquatableComparer<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>>().Compare(
					this, other);
		}

		public bool Equals(IConstructorInfoCache<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>> other)
		{
			return
				new ConstructorInfoCacheEquatableComparer<AttributeInfoCache, MethodArgsInfoCache<AttributeInfoCache>>().Equals(
					this, other);
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}