#region

using System;
using System.Threading;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.ILocalDbPrimaryKeyConstraint" />
	public class LocalDbIntPkProvider : ILocalDbPrimaryKeyConstraint
	{
		/// <summary>
		///     The counter
		/// </summary>
		private volatile int _counter;

		/// <summary>
		///     Default primary key generation starts with 1 incriments by 1
		/// </summary>
		public LocalDbIntPkProvider()
		{
			_counter = 0;
			Seed = 1;
			Incriment = 1;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbIntPkProvider" /> class.
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <param name="incriment">The incriment.</param>
		public LocalDbIntPkProvider(int seed, int incriment)
		{
			_counter = seed;
			Seed = seed;
			Incriment = incriment;
		}

		/// <summary>
		///     Gets the seed.
		/// </summary>
		/// <value>
		///     The seed.
		/// </value>
		public int Seed { get; private set; }

		/// <summary>
		///     Gets the incriment.
		/// </summary>
		/// <value>
		///     The incriment.
		/// </value>
		public int Incriment { get; private set; }

		/// <summary>
		///     Type contract what type this generator is for
		/// </summary>
		public Type GeneratingType
		{
			get { return typeof(int); }
		}

		/// <summary>
		///     Generate a new Uniq primary key that has the type of GeneratingType
		/// </summary>
		/// <returns></returns>
		public object GetNextValue()
		{
			return Interlocked.Add(ref _counter, Incriment);
		}

		/// <summary>
		///     Gets the object that indicates an Non Init primary key
		/// </summary>
		/// <returns></returns>
		public object GetUninitilized()
		{
			return 0;
		}

		/// <summary>
		///     This should return a new Instance of the current ILocalPrimaryKeyValueProvider with resetted internal PK counter
		/// </summary>
		/// <returns></returns>
		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbIntPkProvider();
		}

		/// <summary>
		///     Allows to update the index be faking the number of calles to GetNextNumber by <paramref name="index" />
		/// </summary>
		/// <param name="index"></param>
		public void UpdateIndex(long index)
		{
			Interlocked.Add(ref _counter, (int) index);
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public new bool Equals(object x, object y)
		{
			return (int) x == (int) y;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}