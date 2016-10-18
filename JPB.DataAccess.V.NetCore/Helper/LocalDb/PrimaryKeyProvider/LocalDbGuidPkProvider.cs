using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.ILocalDbPrimaryKeyConstraint" />
	public class LocalDbGuidPkProvider : ILocalDbPrimaryKeyConstraint
	{
		/// <summary>
		/// Type contract what type this generator is for
		/// </summary>
		public Type GeneratingType
		{
			get { return typeof (Guid); }
		}

		/// <summary>
		/// Generate a new Uniq primary key that has the type of GeneratingType
		/// </summary>
		/// <returns></returns>
		public object GetNextValue()
		{
			return Guid.NewGuid();
		}

		/// <summary>
		/// Gets the object that indicates an Non Init primary key
		/// </summary>
		/// <returns></returns>
		public object GetUninitilized()
		{
			return new Guid();
		}

		/// <summary>
		/// This should return a new Instance of the current ILocalPrimaryKeyValueProvider with resetted internal PK counter
		/// </summary>
		/// <returns></returns>
		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbGuidPkProvider();
		}

		/// <summary>
		/// Allows to update the index be faking the number of calles to GetNextNumber by <paramref name="index" />
		/// </summary>
		/// <param name="index"></param>
		public void UpdateIndex(long index)
		{

		}

		/// <summary>
		/// Updates the index.
		/// </summary>
		/// <param name="index">The index.</param>
		public void UpdateIndex(object index)
		{

		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public new bool Equals(object x, object y)
		{
			return (Guid)x == (Guid)y;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}