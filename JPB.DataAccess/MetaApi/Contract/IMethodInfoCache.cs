using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	public interface IMethodInfoCache<TAtt, TArg> : 
		IComparable<IMethodInfoCache<TAtt, TArg>>,
		IEquatable<IMethodInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		Delegate Delegate { get; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		MethodBase MethodInfo { get; }

		/// <summary>
		///     The name of the method
		/// </summary>
		string MethodName { get; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		HashSet<TAtt> AttributeInfoCaches { get; }

		/// <summary>
		/// Arguments for this Method
		/// </summary>
		HashSet<TArg> Arguments { get; }

		IMethodInfoCache<TAtt, TArg> Init(MethodBase info);

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		object Invoke(object target, params object[] param);
	}
}