#region

using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	/// <summary>
	///     Defines methods for compareing two Property Cache instances on its Name and location
	/// </summary>
	internal class PropertyEquatableComparer<TAtt>
		: IEqualityComparer<IPropertyInfoCache<TAtt>>,
			IComparer<IPropertyInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
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
		public int Compare(IPropertyInfoCache<TAtt> x, IPropertyInfoCache<TAtt> y)
		{
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}

			return x.GetHashCode() - y.GetHashCode();
		}

		/// <summary>
		///     Determines whether the specified cache are equal.
		/// </summary>
		/// <param name="x">The first object of type <typeparamref name="TAtt" />  to compare.</param>
		/// <param name="y">The second object of type <typeparamref name="TAtt" />  to compare.</param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(IPropertyInfoCache<TAtt> x, IPropertyInfoCache<TAtt> y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}

			if (x.PropertyInfo == null && y.PropertyInfo != null || y.PropertyInfo == null && x.PropertyInfo != null)
			{
				return false;
			}
			if (x.PropertyInfo.DeclaringType.FullName != y.PropertyInfo.DeclaringType.FullName)
			{
				return false;
			}
			if (x.PropertyName != y.PropertyName)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		///     Returns a hash code for the given cache.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(IPropertyInfoCache<TAtt> obj)
		{
			return obj.PropertyInfo.GetHashCode();
		}
	}
}