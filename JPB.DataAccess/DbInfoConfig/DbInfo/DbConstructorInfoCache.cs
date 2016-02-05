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
		/// <summary>
		/// 
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbConstructorInfoCache()
		{
		}

		internal DbConstructorInfoCache(ConstructorInfo ctorInfo)
			: base(ctorInfo)
		{
		}

		/// <summary>
		///		The class that owns this Property
		/// </summary>
		public DbClassInfoCache DeclaringClass { get; protected internal set; }
	}
}