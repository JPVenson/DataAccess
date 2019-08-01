using JPB.DataAccess.Framework.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Framework.Query.QueryItems
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