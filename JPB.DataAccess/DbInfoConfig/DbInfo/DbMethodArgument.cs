using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	/// 
	/// </summary>
	public class DbMethodArgument : MethodArgsInfoCache<DbAttributeInfoCache>
	{

		/// <summary>
		///		The class that owns this Property
		/// </summary>
		public DbMethodInfoCache DeclaringMethod { get; protected internal set; }
	}
}