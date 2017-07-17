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
	public class LocalDbLongPkProvider : ILocalDbPrimaryKeyConstraint
	{
		/// <summary>
		///     The counter
		/// </summary>
		private long _counter = 1;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbLongPkProvider" /> class.
		/// </summary>
		public LocalDbLongPkProvider()
		{
			_counter = 0;
			Seed = 1;
			Incriment = 1;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbLongPkProvider" /> class.
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <param name="incriment">The incriment.</param>
		public LocalDbLongPkProvider(int seed, int incriment)
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
			get { return typeof(long); }
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
			return 0L;
		}

		/// <summary>
		///     This should return a new Instance of the current ILocalPrimaryKeyValueProvider with resetted internal PK counter
		/// </summary>
		/// <returns></returns>
		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbLongPkProvider();
		}

		/// <summary>
		///     Allows to update the index be faking the number of calles to GetNextNumber by <paramref name="index" />
		/// </summary>
		/// <param name="index"></param>
		public void UpdateIndex(long index)
		{
			Interlocked.Add(ref _counter, index);
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="Exception"></exception>
		public new bool Equals(object x, object y)
		{
			if (x is long && y is long)
				return (long) x == (long) y;
			if (x is long? && y != null)
			{
				var nullableX = (long?) x;
				if (y == null)
					return nullableX.HasValue && y != null;
				return nullableX.Value == (long) y;
			}
			if (y is long?)
			{
				var nullableY = (long?) y;
				return nullableY.Value == ((long?) x).Value;
			}

			throw new Exception(string.Format("Type could not be determinated X is '{0}' Y is '{1}' ", x.GetType(), y.GetType()));
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