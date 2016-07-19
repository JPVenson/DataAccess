using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	public class LocalDbBytePkProvider : ILocalDbPrimaryKeyConstraint
	{
		public LocalDbBytePkProvider()
		{
			_counter = 0;
			Seed = 1;
			Incriment = 1;
		}

		public LocalDbBytePkProvider(byte seed, int incriment)
		{
			_counter = seed;
			Seed = seed;
			Incriment = incriment;
		}

		private byte _counter = 1;

		public Type GeneratingType
		{
			get { return typeof(byte); }
		}

		public object GetNextValue()
		{
			return _counter + 1;
		}

		public int Seed { get; private set; }
		public int Incriment { get; private set; }

		public object GetUninitilized()
		{
			return 0L;
		}

		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbBytePkProvider();
		}

		public void UpdateIndex(long index)
		{
			if(index > Byte.MaxValue)
				throw new InvalidOperationException("Index must be lower then Byte.MaxValue");
			_counter = (byte)index;
		}

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

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}