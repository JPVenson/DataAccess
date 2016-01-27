using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	public class DbConstructorInfoCache : ConstructorInfoCache<DbAttributeInfoCache, DbMethodArgument>
	{
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbConstructorInfoCache()
		{
		}

		internal DbConstructorInfoCache(ConstructorInfo ctorInfo)
			: base(ctorInfo)
		{
		}
	}
}