using System;
using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.Config.Contract
{
	public interface IMethodInfoCache
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
		HashSet<AttributeInfoCache> AttributeInfoCaches { get; }

		IMethodInfoCache Init(MethodInfo info);

		int CompareTo(MethodInfoCache other);

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		object Invoke(object target, params object[] param);
	}
}