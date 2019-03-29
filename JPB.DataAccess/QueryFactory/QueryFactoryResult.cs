#region

using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.QueryFactory
{
	/// <summary>
	///     Wraps a query and its Paramters into one single Object.
	///     Can be returned by an Factory on an POCO
	/// </summary>
	public class QueryFactoryResult : IQueryFactoryResult
	{
		/// <summary>
		/// </summary>
		/// <param name="query"></param>
		public QueryFactoryResult(string query) 
		{
			Query = query;
			Parameters = new IQueryParameter[0];
		}

		/// <summary>
		/// </summary>
		/// <param name="query"></param>
		/// <param name="parameters"></param>
		public QueryFactoryResult(string query, params IQueryParameter[] parameters)
			: this(query)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The SQL QueryCommand
		/// </summary>
		public string Query { get; private set; }

		/// <summary>
		///     All used Parameters
		/// </summary>
		public IEnumerable<IQueryParameter> Parameters { get; private set; }
	}
}