using System.Collections.Generic;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <seealso cref="ILocalDbConstraint" />
	public class ConstraintComparer : IEqualityComparer<ILocalDbConstraint>
	{
		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <see cref="ILocalDbConstraint" /> to compare.</param>
		/// <param name="y">The second object of type <see cref="ILocalDbConstraint" /> to compare.</param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(ILocalDbConstraint x, ILocalDbConstraint y)
		{
			return x.Name == y.Name;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(ILocalDbConstraint obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}