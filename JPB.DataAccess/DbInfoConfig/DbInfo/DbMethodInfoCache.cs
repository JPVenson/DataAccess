using System;
using System.Reflection;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	public class DbMethodInfoCache : MethodInfoCache, IComparable<MethodInfoCache>
	{
		internal DbMethodInfoCache(MethodInfo mehtodInfo) : base(mehtodInfo)
		{
		}

		internal DbMethodInfoCache(Delegate fakeMehtod, string name = null, params AttributeInfoCache[] attributes)
			: base(fakeMehtod, name, attributes)
		{
		}

		public DbMethodInfoCache()
		{
		}
	}
}