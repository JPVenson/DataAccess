using System.Collections.Generic;
using JPB.DataAccess.Query.QueryItems;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		Provides info for <see cref="IEntityProcessor"/> transformations
	/// </summary>
	public class QueryProcessingRecordsContext
	{
		internal QueryProcessingRecordsContext(IQueryContainer queryContainer,
			List<IEntityProcessor> queryContainerPostProcessors, IEnumerable<ColumnInfo> columns)
		{
			QueryContainer = queryContainer;
			QueryContainerPostProcessors = queryContainerPostProcessors;
			Columns = columns;
			ColumnMappings = new Dictionary<string, string>();
		}

		/// <summary>
		///		Defines the set of mapped columns where Key is the name of the column that is expected in the result of the query and its value should be the expected value in the POCO
		/// </summary>
		public IDictionary<string, string> ColumnMappings { get; private set; }

		/// <summary>
		///		The executing DbAccessLayer
		/// </summary>
		public IQueryContainer QueryContainer { get; private set; }

		/// <summary>
		///		Post Processors
		/// </summary>
		public List<IEntityProcessor> QueryContainerPostProcessors { get; }

		/// <summary>
		///		The column info of the result query
		/// </summary>
		public IEnumerable<ColumnInfo> Columns { get; set; }

		/// <summary>
		///		Can define a name remapping for columns. Key is the column name from the query where Value is the value recived from the object factory
		/// </summary>
		public IDictionary<string, string> ColumnRemappings { get; private set; }
	}
}