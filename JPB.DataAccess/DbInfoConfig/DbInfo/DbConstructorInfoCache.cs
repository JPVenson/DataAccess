using System.Reflection;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	public class DbConstructorInfoCache : ConstructorInfoCache
	{
		public DbConstructorInfoCache()
		{
		}

		internal DbConstructorInfoCache(ConstructorInfo ctorInfo)
			: base(ctorInfo)
		{
		}
	}
}