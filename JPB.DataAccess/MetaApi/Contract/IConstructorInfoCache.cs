using System;
using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	/// <summary>
	/// Holts all infos about an Constructor
	/// </summary>
	/// <typeparam name="TAtt"></typeparam>
	public interface IConstructorInfoCache<TAtt, TArg> :
		IComparable<IConstructorInfoCache<TAtt, TArg>>,
		IEquatable<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		/// <summary>
		///     Direct Reflection
		/// </summary>
		ConstructorInfo MethodInfo { get; }

		/// <summary>
		///     The name of the constructor
		/// </summary>
		string MethodName { get; }

		/// <summary>
		///     All Attributes
		/// </summary>
		HashSet<TAtt> AttributeInfoCaches { get; }

		/// <summary>
		/// Arguments defined for this Constructor
		/// </summary>
		HashSet<TArg> Arguments { get; }

		IConstructorInfoCache<TAtt, TArg> Init(ConstructorInfo ctorInfo);
	}
}