#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	[Serializable]
	public abstract class ConstructorInfoCache<TAtt, TArg> : MethodInfoCache<TAtt, TArg>,
		IConstructorInfoCache<TAtt, TArg>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		/// <summary>
		///     For internal use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
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
		///     Invoke implimentation for Constructors. Calls the underlying Methodinfo without specifying an Caller
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public object Invoke(params object[] param)
		{
			return Invoke(null, param);
		}

		/// <summary>
		///     For internal use Only
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override IMethodInfoCache<TAtt, TArg> Init(MethodBase info)
		{
			if (info is ConstructorInfo)
				return Init(info as ConstructorInfo);

			throw new NotImplementedException();
		}

		/// <summary>
		///     The method info casted as ConstructorInfo
		/// </summary>
		public new ConstructorInfo MethodInfo
		{
			get { return base.MethodInfo as ConstructorInfo; }
			protected internal set { base.MethodInfo = value; }
		}

		MethodBase IMethodInfoCache<TAtt, TArg>.MethodInfo
		{
			get { return MethodInfo; }
		}

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		public new virtual object Invoke(object target, params object[] param)
		{
			return MethodInfo.Invoke(param);
		}

		/// <summary>
		///     For Interal use Only
		/// </summary>
		/// <param name="ctorInfo"></param>
		/// <returns></returns>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IConstructorInfoCache<TAtt, TArg> Init(ConstructorInfo ctorInfo)
		{
			base.Init(ctorInfo);

			Arguments = new HashSet<TArg>(ctorInfo.GetParameters().Select(f => new TArg().Init(f) as TArg));
			return this;
		}
#pragma warning disable CS1591
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