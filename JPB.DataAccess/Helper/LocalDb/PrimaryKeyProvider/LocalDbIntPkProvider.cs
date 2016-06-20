using System;
using System.Threading;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	/// 
	/// </summary>
	public class LocalDbIntPkProvider : ILocalDbPrimaryKeyConstraint
	{
		/// <summary>
		/// Default primary key generation starts with 1 incriments by 1
		/// </summary>
		public LocalDbIntPkProvider()
		{
			_counter = 0;
			Seed = 1;
			Incriment = 1;
		}

		public LocalDbIntPkProvider(int seed, int incriment)
		{
			_counter = seed;
			Seed = seed;
			Incriment = incriment;
		}

		public int Seed { get; private set; }
		public int Incriment { get; private set; }

		private volatile int _counter;

		public Type GeneratingType
		{
			get { return typeof (int); }
		}

		public object GetNextValue()
		{
			return Interlocked.Add(ref _counter, Incriment);
		}

		public object GetUninitilized()
		{
			return 0;
		}

		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbIntPkProvider();
		}

		public void UpdateIndex(long index)
		{
			Interlocked.Add(ref _counter, (int)index);
		}

		public new bool Equals(object x, object y)
		{
			return (int)x == (int)y;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}