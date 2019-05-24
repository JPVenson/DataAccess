#region

using System.Collections.Generic;
using System.Threading.Tasks;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		The base interface for Executing a build Query
	/// </summary>
	public interface IQueryBuilder
	{
		/// <summary>
		///     The internal value holder
		/// </summary>
		IQueryContainer ContainerObject { get; }
		
		/// <summary>
		///		Adds an Query part to the Internal collection
		/// </summary>
		/// <param name="queryPart"></param>
		/// <returns></returns>
		IQueryBuilder Add(IQueryPart queryPart);
	}
}