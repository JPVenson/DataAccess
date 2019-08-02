using JetBrains.Annotations;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <seealso cref="QueryBuilderX" />
	public class CountElementsObjectSelector : QueryBuilderX
	{
		/// <inheritdoc />
		public CountElementsObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Gets or sets a value indicating whether the Count should be Distincted.
		/// </summary>
		/// <value>
		///     <c>true</c> if [distinct mode]; otherwise, <c>false</c>.
		/// </value>
		public bool DistinctMode { get; set; }

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public CountElementsObjectSelector Distinct
		{
			get
			{
				return new CountElementsObjectSelector(this)
				       {
					       DistinctMode = true
				       };
			}
		}

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public ElementProducer<int> Table<TPoco>()
		{
			return new SelectQuery<int>(Add(new CountTargetQueryPart(new QueryIdentifier()
			{
				Value = $"[{ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName}]",
				QueryIdType = QueryIdentifier.QueryIdTypes.Table
			}, ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.SubQuery))
			{
				DistinctMode = DistinctMode
			}));
		}
	}
}