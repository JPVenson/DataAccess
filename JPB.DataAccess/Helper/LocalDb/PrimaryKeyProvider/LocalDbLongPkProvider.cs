using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider
{
	/// <summary>
	/// 
	/// </summary>
	public class LocalDbLongPkProvider : ILocalDbPrimaryKeyConstraint
	{
		private long _counter = 1;
		public static string LastMessage = "";

		public Type GeneratingType
		{
			get { return typeof(long); }
		}

		public object GetNextValue()
		{
			return _counter++;
		}

		public object GetUninitilized()
		{
			return 0L;
		}

		public ILocalDbPrimaryKeyConstraint Clone()
		{
			return new LocalDbLongPkProvider();
		}

		public new bool Equals(object x, object y)
		{
			if (x is long && y is long)
			{
				return (long)x == (long)y;
			}
			else if(x is long? && y != null)
			{
				var nullableX = (long?)x;
				if (y == null)
				{
					return nullableX.HasValue && y != null;
				}
				return nullableX.Value == (long)y;
			}
			else if (y is long?)
			{
				var nullableY = (long?)y;
				return nullableY.Value == ((long?)x).Value;
			}

			throw new Exception(string.Format("Type could not be determinated X is '{0}' Y is '{1}' ", x.GetType(), y.GetType()));
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}