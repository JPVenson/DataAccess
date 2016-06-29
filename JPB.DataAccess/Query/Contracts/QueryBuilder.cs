using System.Collections.Generic;

namespace JPB.DataAccess.Query.Contracts
{
	public interface IQueryBuilder
	{
		/// <summary>
		/// The interal value holder
		/// </summary>
		IQueryContainer ContainerObject { get; }

		/// <summary>
		/// Enumerates the current query for a type <typeparamref name="E"/>
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		IEnumerable<E> ForResult<E>();
	}
}