using JPB.DataAccess.Framework.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework.Query.Contracts;

namespace JPB.DataAccess.Framework.Query.Operators
{
	/// <summary>
	/// Defines a Query that can contain and Handle an Alias
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class IdentifyableQuery<TPoco> : QueryBuilderX
	{
		/// <summary>
		///     Easy access to the Cache for TPoco
		/// </summary>
		protected internal DbClassInfoCache Cache;

		/// <inheritdoc />
		public IdentifyableQuery(IQueryBuilder database) : base(database)
		{
			Cache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
		}
	}
}