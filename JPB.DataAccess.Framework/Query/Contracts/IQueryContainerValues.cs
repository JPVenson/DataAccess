using System.Collections.Generic;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Framework.Query.Contracts
{
	internal interface IQueryContainerValues : IQueryContainer
	{
		/// <summary>
		///     Gets the current number of used SQL Arguments
		/// </summary>
		int AutoParameterCounter { get; }

		/// <summary>
		///     Gets the current number of used SQL Columns
		/// </summary>
		int ColumnCounter { get; }

		/// <summary>
		///     Gets the current number of used SQL Arguments
		/// </summary>
		IDictionary<string, QueryIdentifier> TableAlias { get; }

		/// <summary>
		///     Gets the current number of used SQL Arguments
		/// </summary>
		IList<QueryIdentifier> Identifiers { get; }
	}
}