using System.Collections.Generic;
using System.Threading.Tasks;

namespace JPB.DataAccess.Framework.Query.Contracts
{
	/// <summary>
	///     This Part of the Query can be executed and returns a set of Pocos
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEnumerableQueryAsync<T> : IEnumerableQuery<T>
	{
		/// <summary>
		///     Creates a new Enumerator that executes async
		/// </summary>
		/// <returns></returns>
		Task<IEnumerator<T>> GetEnumeratorAsync();
	}
}