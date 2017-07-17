#region

using System;

#endregion

namespace JPB.DataAccess.Contacts.MetaApi
{
	/// <summary>
	///     Holts all infos about an Constructor
	/// </summary>
	/// <typeparam name="TAtt">The type of the att.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.MetaApi.IMethodInfoCache{TAtt, TArg}" />
	/// <seealso cref="IConstructorInfoCache{TAtt,TArg}" />
	/// <seealso cref="IConstructorInfoCache{TAtt,TArg}" />
	public interface IConstructorInfoCache<TAtt, TArg> :
		IMethodInfoCache<TAtt, TArg>,
		IComparable<IConstructorInfoCache<TAtt, TArg>>,
		IEquatable<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
	}
}