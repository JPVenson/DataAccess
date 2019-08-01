#region

using JPB.DataAccess.Framework.MetaApi.Model;

#endregion

namespace JPB.DataAccess.Framework.DbInfoConfig.DbInfo
{
	/// <summary>
	/// </summary>
	public class DbMethodArgument : MethodArgsInfoCache<DbAttributeInfoCache>
	{
		/// <summary>
		///     The class that owns this Property
		/// </summary>
		public DbMethodInfoCache DeclaringMethod { get; protected internal set; }
	}
}