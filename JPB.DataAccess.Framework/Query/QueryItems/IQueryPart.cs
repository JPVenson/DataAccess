using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Query.Contracts;

namespace JPB.DataAccess.Framework.Query.QueryItems
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
		IQueryFactoryResult Process(IQueryContainer container);
	}
}