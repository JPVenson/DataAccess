using System.Data;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		Allows for Modifications of Commands
	/// </summary>
	public interface IQueryCommandInterceptor
	{
		/// <summary>
		///		Will be executed right before a query that expects to have a result is executed
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		IDbCommand QueryExecuting(IDbCommand command);

		/// <summary>
		///		Will be executed right before a query that expects to have no result is executed	
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		IDbCommand NonQueryExecuting(IDbCommand command);
	}
}