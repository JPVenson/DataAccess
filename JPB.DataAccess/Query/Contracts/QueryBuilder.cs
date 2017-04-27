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
		IEnumerable<TE> ForResult<TE>();

		/// <summary>
		///     Enumerates the current query for a type <typeparamref name="TE" />
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		Task<IEnumerable<TE>> ForAsyncResult<TE>();

		/// <summary>
		///     Clones all items inside the current Builder into a new One
		/// </summary>
		/// <returns></returns>
		IQueryBuilder Clone();

		/// <summary>
		///     Gets an enumerator for the Current Query
		/// </summary>
		/// <typeparam name="TPoco"></typeparam>
		/// <returns></returns>
		IEnumerator<TPoco> GetEnumerator<TPoco>();
	}
}