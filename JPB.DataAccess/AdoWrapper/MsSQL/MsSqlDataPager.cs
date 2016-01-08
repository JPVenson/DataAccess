using System;
using System.Collections.Generic;

namespace JPB.DataAccess.AdoWrapper.MsSql
{
	public class MsSqlDataPager<T> : MsSqlUntypedDataPager<T>
	{
		private static Type _type;

		static MsSqlDataPager()
		{
			_type = typeof (T);
		}

		public MsSqlDataPager()
		{
			TargetType = _type;
		}

		public new ICollection<T> CurrentPageItems
		{
			get { return base.CurrentPageItems; }
		}
	}
}