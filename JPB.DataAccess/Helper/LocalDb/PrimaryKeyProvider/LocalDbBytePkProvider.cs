using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.ILocalDbPrimaryKeyConstraint" />
	public class LocalDbBytePkProvider : ILocalDbPrimaryKeyConstraint
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalDbBytePkProvider"/> class.
		/// </summary>
		public LocalDbBytePkProvider()
		{
			_counter = 0;
			Seed = 1;
			Incriment = 1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalDbBytePkProvider"/> class.
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <param name="incriment">The incriment.</param>
		public LocalDbBytePkProvider(byte seed, int incriment)
		{
			_counter = seed;
			Seed = seed;
			Incriment = incriment;
		}

		/// <summary>
		/// The counter
		/// </summary>
		private byte _counter = 1;

		/// <summary>
		/// Type contract what type this generator is for
		/// </summary>
		public Type GeneratingType
		{
			get { return typeof(byte); }
		}

		/// <summary>
		/// Generate a new Uniq primary key that has the type of GeneratingType
		/// </summary>
		/// <returns></returns>
		public object GetNextValue()
		{
			return _counter + 1;
		}

		/// <summary>
		/// Gets the seed.
		/// </summary>
		/// <value>
		/// The seed.
		/// </value>
		public int Seed { get; private set; }
		/// <summary>
		/// Gets the incriment.
		/// </summary>
		/// <value>
		/// The incriment.
		/// </value>
		public int Incriment { get; private set; }

		/// <summary>
		/// Gets the object that indicates an Non Init primary key
		/// </summary>
		/// <returns></returns>
		public object GetUninitilized()
		{
			return 0L;
		}

		/// <summary>
		/// This should return a new Instance of the current ILocalPrimaryKeyValueProvider with resetted internal PK counter
		/// </summary>
		/// <returns></returns>
		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbBytePkProvider();
		}

		/// <summary>
		/// Allows to update the index be faking the number of calles to GetNextNumber by <paramref name="index" />
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="InvalidOperationException">Index must be lower then Byte.MaxValue</exception>
		public void UpdateIndex(long index)
		{
			if(index > Byte.MaxValue)
				throw new InvalidOperationException("Index must be lower then Byte.MaxValue");
			_counter = (byte)index;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="Exception"></exception>
		public new bool Equals(object x, object y)
		{
			if (x is byte && y is byte)
			{
				return (byte)x == (byte)y;
			}
			else if (x is byte? && y != null)
			{
				var nullableX = (byte?)x;
				if (y == null)
				{
					return nullableX.HasValue && y != null;
				}
				return nullableX.Value == (byte)y;
			}
			else if (y is byte?)
			{
				var nullableY = (byte?)y;
				return nullableY.Value == ((byte?)x).Value;
			}

			throw new Exception(string.Format("Type could not be determinated X is '{0}' Y is '{1}' ", x.GetType(), y.GetType()));
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