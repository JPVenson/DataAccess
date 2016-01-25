using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	public class DbMethodInfoCache : MethodInfoCache<DbAttributeInfoCache>
	{
		internal DbMethodInfoCache(MethodInfo mehtodInfo) : base(mehtodInfo)
		{
		}

		internal DbMethodInfoCache(Delegate fakeMehtod, string name = null, params DbAttributeInfoCache[] attributes)
			: base(fakeMehtod, name, attributes)
		{
		}
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbMethodInfoCache()
		{
		}
	}
}