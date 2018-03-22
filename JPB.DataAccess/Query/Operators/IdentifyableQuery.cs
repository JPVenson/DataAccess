using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
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

		/// <summary>
		///     Initializes a new instance of the <see cref="ElementProducer{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="currentIdentifier">The current identifier.</param>
		public IdentifyableQuery(IQueryBuilder database, string currentIdentifier) : base(database)
		{
			CurrentIdentifier = currentIdentifier;
			SetCache();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ElementProducer{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public IdentifyableQuery(IQueryBuilder database) : base(database)
		{
			SetCache();
		}

		/// <summary>
		///     Gets the current identifier in the query.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

		/// <summary>
		/// Sets the Interal Alias to a new Uniq value
		/// </summary>
		protected void CreateNewIdentifier()
		{
			CurrentIdentifier = string.Format("{0}_{1}", Cache.TableName, ContainerObject.GetNextParameterId());
		}

		/// <summary>
		///     Sets the cache.
		/// </summary>
		private void SetCache()
		{
			Cache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
		}
	}
}