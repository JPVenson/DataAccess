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

		/// <summary>
		/// Wraps this query type to an new QueryElement
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IQueryBuilder<T> ChangeType<T>() where T : IQueryElement;
	}
}