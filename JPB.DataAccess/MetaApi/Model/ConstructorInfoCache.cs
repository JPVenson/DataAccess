using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.MetaApi.Contract;
using JPB.DataAccess.MetaApi.Model.Equatable;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	[Serializable]
	public class ConstructorInfoCache<TAtt> : IConstructorInfoCache<TAtt> where TAtt : class, IAttributeInfoCache, new()
	{
		/// <summary>
		/// For internal use only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ConstructorInfoCache()
		{
		}

		internal ConstructorInfoCache(ConstructorInfo ctorInfo)
		{
			Init(ctorInfo);
		}

		public IConstructorInfoCache<TAtt> Init(ConstructorInfo ctorInfo)
		{
			if (!string.IsNullOrEmpty(MethodName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			MethodInfo = ctorInfo;
			MethodName = ctorInfo.Name;
			AttributeInfoCaches = new HashSet<TAtt>(ctorInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			return this;
		}

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public ConstructorInfo MethodInfo { get; private set; }

		/// <summary>
		///     The name of the constructor
		/// </summary>
		public string MethodName { get; private set; }

		/// <summary>
		///     All Attributes
		/// </summary>
		public HashSet<TAtt> AttributeInfoCaches { get; private set; }

		public int CompareTo(IConstructorInfoCache<TAtt> other)
		{
			return new ConstructorInfoCacheEquatableComparer<TAtt>().Compare(this, other);
		}

		public bool Equals(IConstructorInfoCache<TAtt> other)
		{
			return new ConstructorInfoCacheEquatableComparer<TAtt>().Equals(this, other);
		}
	}
}