using System.Data;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	/// <summary>
	///		The part of the Query
	/// </summary>
	public interface IQueryPart
	{
		/// <summary>
		///		Processes the given information to a new Command
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		IDbCommand Process(IQueryContainer container);
	}

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