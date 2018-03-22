#region

using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	public interface IQueryBuilder
	{
		/// <summary>
		///     The interal value holder
		/// </summary>
		IQueryContainer ContainerObject { get; }

		/// <summary>
		///     Enumerates the current query for a type <typeparamref name="TE" />
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		IEnumerable<TE> ForResult<TE>(bool async = true);


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