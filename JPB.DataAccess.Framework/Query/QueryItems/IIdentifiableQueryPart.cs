using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	/// <summary>
	///		Defines a query that declares an alias
	/// </summary>
	public interface IIdentifiableQueryPart : IQueryPart
	{
		/// <summary>
		///		The alias
		/// </summary>
		QueryIdentifier Alias { get; }
	}
}