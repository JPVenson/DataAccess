using System.Collections.Generic;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		Provides info for <see cref="IEntityProcessor"/> transformations
	/// </summary>
	public class QueryProcessingEntitiesContext
	{
		internal QueryProcessingEntitiesContext(IEnumerable<object> entities)
		{
			Entities = entities;
		}

		/// <summary>
		///		The result of this query
		/// </summary>
		public IEnumerable<object> Entities { get; private set; }
	}
}