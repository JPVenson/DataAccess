/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace JPB.DataAccess.Contacts.MetaApi
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
		Func<object, object[], object> Delegate { get; }

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
		HashSet<TAtt> Attributes { get; }

		/// <summary>
		/// Arguments for this Method
		/// </summary>
		HashSet<TArg> Arguments { get; }

		/// <summary>
		/// When set to true, an IL Wrapper is used inside the Invoke method
		/// </summary>
		bool UseILWrapper { get; set; }

		IMethodInfoCache<TAtt, TArg> Init(MethodBase info);
		IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo, Type sourceType);

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		object Invoke(object target, params object[] param);
	}
}