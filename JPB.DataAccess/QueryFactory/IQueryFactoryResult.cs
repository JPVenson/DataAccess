using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
	/// <summary>
	///     Marker interface for an Query that was created due the invoke of a Factory mehtod
	/// </summary>
	public interface IQueryFactoryResult
	{
		/// <summary>
		///     The SQL Query
		/// </summary>
		string Query { get; }

		/// <summary>
		///     Sql Query Parameter
		/// </summary>
		IEnumerable<IQueryParameter> Parameters { get; }
	}
}