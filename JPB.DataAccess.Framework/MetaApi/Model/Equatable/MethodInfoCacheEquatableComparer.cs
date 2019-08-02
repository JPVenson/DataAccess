#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TAtt">The type of the att.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.MetaApi.IMethodInfoCache{TAtt, TArg}" />
	/// <seealso cref="JPB.DataAccess.Contacts.MetaApi.IMethodInfoCache{TAtt, TArg}" />
	internal class MethodInfoCacheEquatableComparer<TAtt, TArg>
		: IComparer<IMethodInfoCache<TAtt, TArg>>,
			IEqualityComparer<IMethodInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new() where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		/// <summary>
		///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
		///     the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero
		///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
		///     <paramref name="y" />.
		/// </returns>
		public int Compare(IMethodInfoCache<TAtt, TArg> x, IMethodInfoCache<TAtt, TArg> y)
		{
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}
			return string.Compare(x.MethodName, y.MethodName, StringComparison.Ordinal);
		}
#pragma warning disable CS1711 // XML comment has a typeparam tag, but there is no type parameter by that name
		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type
		///     <typeparam name="TAtt" />
		///     to compare.
		/// </param>
		/// <param name="y">The second object of type
		///     <typeparam name="TAtt" />
		///     to compare.
		/// </param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(IMethodInfoCache<TAtt, TArg> x, IMethodInfoCache<TAtt, TArg> y)
#pragma warning restore CS1711 // XML comment has a typeparam tag, but there is no type parameter by that name
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			if (x.MethodName != y.MethodName)
			{
				return false;
			}
			return x.MethodInfo.Equals(y.MethodInfo);
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(IMethodInfoCache<TAtt, TArg> obj)
		{
			return obj.MethodInfo.GetHashCode();
		}
	}
}