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
		IMethodInfoCache<TAtt, TArg>,
		IComparable<IConstructorInfoCache<TAtt, TArg>>,
		IEquatable<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{

	}
}