using System;
using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	public interface IMethodInfoCache<TAtt> :
		IComparable<IMethodInfoCache<TAtt>>,
		IEquatable<IMethodInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		Delegate Delegate { get; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		MethodInfo MethodInfo { get; }

		/// <summary>
		///     The name of the method
		/// </summary>
		string MethodName { get; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		HashSet<TAtt> AttributeInfoCaches { get; }

		IMethodInfoCache<TAtt> Init(MethodInfo info);
		
		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		object Invoke(object target, params object[] param);
	}
}