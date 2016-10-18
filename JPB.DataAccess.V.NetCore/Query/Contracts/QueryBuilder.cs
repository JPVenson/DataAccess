using System.Collections.Generic;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///
	/// </summary>
	public interface IQueryBuilder
	{
		/// <summary>
		/// The interal value holder
		/// </summary>
		IQueryContainer ContainerObject { get; }

		/// <summary>
		/// Enumerates the current query for a type <typeparamref name="TE" />
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		IEnumerable<TE> ForResult<TE>();
	}
}