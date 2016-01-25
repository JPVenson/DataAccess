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
	public interface IConstructorInfoCache<TAtt> : 
		IComparable<IConstructorInfoCache<TAtt>>, 
		IEquatable<IConstructorInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
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

		IConstructorInfoCache<TAtt> Init(ConstructorInfo ctorInfo);
	}
}