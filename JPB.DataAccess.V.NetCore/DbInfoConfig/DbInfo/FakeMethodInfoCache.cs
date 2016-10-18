using System;
using System.Collections.Generic;
using System.Linq;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal class FakeMethodInfoCache : DbMethodInfoCache
	{
		private readonly Func<object, object[], object> _fakeMehtod;

		public FakeMethodInfoCache(Func<object, object[], object> fakeMehtod, string name)
			: base(fakeMehtod, typeof(FakeMethodInfoCache), name)
		{
			_fakeMehtod = fakeMehtod;
			Console.WriteLine();
		}

		public override object Invoke(object target, params object[] param)
		{
			return _fakeMehtod.Invoke(target, param);
		}
	}
}