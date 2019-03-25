using System.Data;
using JPB.DataAccess.Query.Contracts;

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
}