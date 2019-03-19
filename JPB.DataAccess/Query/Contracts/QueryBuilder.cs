#region

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.Query.QueryItems;

#endregion

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
		///     Enumerates the current query for a type <typeparamref name="TE" />
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		IEnumerable<TE> ForResult<TE>(bool async = true);

		/// <summary>
		///		Adds an Query part to the Internal collection
		/// </summary>
		/// <param name="queryPart"></param>
		/// <returns></returns>
		IQueryBuilder Add(IQueryPart queryPart);

		/// <summary>
		///     Gets an enumerator for the Current Query
		/// </summary>
		/// <typeparam name="TPoco"></typeparam>
		/// <returns></returns>
		IEnumerator<TPoco> GetEnumerator<TPoco>();

		/// <summary>
		///     Gets an enumerator for the Current Query
		/// </summary>
		/// <typeparam name="TPoco"></typeparam>
		/// <param name="async">enumerates the resultset in background until you first call the enumerator</param>
		/// <returns></returns>
		IEnumerator<TPoco> GetEnumerator<TPoco>(bool async);

		/// <summary>
		/// Internal Usage only
		/// </summary>
		/// <param name="instance"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IQueryBuilder CloneWith<T>(T instance) where T: IQueryBuilder;
	}
}