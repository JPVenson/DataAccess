#region

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace JPB.DataAccess.Contacts.MetaApi
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <seealso cref="IMethodArgsInfoCache{TArg}" />
	/// <seealso cref="IMethodArgsInfoCache{TArg}" />
	public interface IMethodArgsInfoCache<TArg>
		: IComparable<IMethodArgsInfoCache<TArg>>, IEquatable<IMethodArgsInfoCache<TArg>>
		where TArg : class, IAttributeInfoCache, new()
	{
		/// <summary>
		///     The name of this Param
		/// </summary>
		string ArgumentName { get; }

		/// <summary>
		///     The type of this Param
		/// </summary>
		Type Type { get; }

		/// <summary>
		///     All Attached Attributes
		/// </summary>
		HashSet<TArg> Attributes { get; }

		/// <summary>
		///     Direct reflection
		/// </summary>
		ParameterInfo ParameterInfo { get; }

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		IMethodArgsInfoCache<TArg> Init(ParameterInfo info);
	}
}