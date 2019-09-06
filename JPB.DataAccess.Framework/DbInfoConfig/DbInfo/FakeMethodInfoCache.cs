#region

using System;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal sealed class FakeMethodInfoCache : DbMethodInfoCache
	{
		private readonly Func<object, object[], object> _fakeMehtod;

		public FakeMethodInfoCache(Func<object, object[], object> fakeMehtod, string name)
			: base()
		{
			_fakeMehtod = fakeMehtod;
			Init(fakeMehtod, typeof(FakeMethodInfoCache), name);
		}

		public override object Invoke(object target, params object[] param)
		{
			return _fakeMehtod.Invoke(target, param);
		}
	}
}