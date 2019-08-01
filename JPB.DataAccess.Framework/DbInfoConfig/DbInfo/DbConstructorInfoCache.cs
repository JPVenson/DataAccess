#region

using System.ComponentModel;
using System.Reflection;
using JPB.DataAccess.Framework.MetaApi.Model;

#endregion

namespace JPB.DataAccess.Framework.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	public class DbConstructorInfoCache : ConstructorInfoCache<DbAttributeInfoCache, DbMethodArgument>
	{
		/// <summary>
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
		///     The class that owns this Property
		/// </summary>
		public DbClassInfoCache DeclaringClass { get; protected internal set; }
	}
}