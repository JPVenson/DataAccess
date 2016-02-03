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
	public class ConstructorInfoCache<TAtt, TArg> : IConstructorInfoCache<TAtt, TArg> 
		where TAtt : class, IAttributeInfoCache, new() 
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
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

		/// <summary>
		/// For Interal use Only
		/// </summary>
		/// <param name="ctorInfo"></param>
		/// <returns></returns>
		public virtual IConstructorInfoCache<TAtt, TArg> Init(ConstructorInfo ctorInfo)
		{
			if (!string.IsNullOrEmpty(MethodName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			MethodInfo = ctorInfo;
			MethodName = ctorInfo.Name;
			AttributeInfoCaches = new HashSet<TAtt>(ctorInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			Arguments = new HashSet<TArg>(ctorInfo.GetParameters().Select(f => new TArg().Init(f) as TArg));
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

		/// <summary>
		///		All Arguments on this Constructor
		/// </summary>
		public HashSet<TArg> Arguments { get; private set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public int CompareTo(IConstructorInfoCache<TAtt, TArg> other)
		{
			return new ConstructorInfoCacheEquatableComparer<TAtt, TArg>().Compare(this, other);
		}

		public bool Equals(IConstructorInfoCache<TAtt, TArg> other)
		{
			return new ConstructorInfoCacheEquatableComparer<TAtt, TArg>().Equals(this, other);
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}