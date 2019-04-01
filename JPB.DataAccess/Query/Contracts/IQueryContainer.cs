#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		Allows Modifications on Entities mapping
	/// </summary>
	public interface IEntityProcessor
	{
		/// <summary>
		///		Will be invoked right before execution of the command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		IDbCommand BeforeExecution(IDbCommand command);

		/// <summary>
		///		Transforms an Entity
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		object Transform(object entity, Type entityType, QueryProcessingEntitiesContext context);

		/// <summary>
		///		Transforms an DataReader
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		EagarDataRecord Transform(EagarDataRecord reader, Type entityType, QueryProcessingRecordsContext context);

		/// <summary>
		///		Transforms all DataReaders
		/// </summary>
		/// <param name="readers"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context);
	}

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

	/// <summary>
	/// </summary>
	public interface IQueryContainer
	{
		/// <summary>
		/// 
		/// </summary>
		IList<JoinParseInfo> Joins { get; }
		/// <summary>
		///		A List of processors that will be executed after all entities are loaded.
		///		If any Processor is present the <seealso cref="EnumerationMode"/> will be set to FullOnLoad
		/// </summary>
		List<IEntityProcessor> PostProcessors { get; }

		/// <summary>
		///		A list of Command interceptors
		/// </summary>
		List<IQueryCommandInterceptor> Interceptors { get; }

		/// <summary>
		///		Property bag for Query generation related Infos
		/// </summary>
		IDictionary<string, object> QueryInfos { get; }

		/// <summary>
		///     Declares the Return type that is awaited
		/// </summary>
		Type ForType { get; set; }

		/// <summary>
		///     Defines all elements added by the Add Method
		/// </summary>
		IEnumerable<IQueryPart> Parts { get; }
		
		/// <summary>
		///     If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
		/// </summary>
		bool AllowParamterRenaming { get; set; }

		/// <summary>
		///     Access to the underlying AccessLayer
		/// </summary>
		DbAccessLayer AccessLayer { get; }

		/// <summary>
		///     Will concat all QueryParts into a statement and will check for Spaces
		/// </summary>
		/// <returns></returns>
		IQueryFactoryResult Compile(out IEnumerable<ColumnInfo> columns);

		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		int GetNextParameterId();
		
		/// <summary>
		///     Translates an Identifier object into the corresponding Sql Identifier
		/// </summary>
		/// <returns></returns>
		QueryIdentifier CreateAlias(QueryIdentifier.QueryIdTypes type);
		
		/// <summary>
		///     Translates an Identifier object into the corresponding Sql Identifier
		/// </summary>
		/// <returns></returns>
		QueryIdentifier CreateTableAlias(string path);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		QueryIdentifier SearchTableAlias(string path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		string GetPathOf(QueryIdentifier identifier);

		/// <summary>
		///     Clones this Container
		/// </summary>
		/// <returns></returns>
		IQueryContainer Clone();

		/// <summary>
		///		Searches in the Parts collection for the nearest occurence of this Query Part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Search<T>() where T : IQueryPart;

		/// <summary>
		///		Searches in the Parts collection for the nearest occurence of this Query Part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Search<T>(Func<T, bool> filter) where T : IQueryPart;

		/// <summary>
		///		Searches in the Parts collection for an identifier
		/// </summary>
		/// <returns></returns>
		ISelectableQueryPart Search(QueryIdentifier identifier);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryPart"></param>
		void Add(IQueryPart queryPart);

		/// <summary>
		///		Obtains a new Guaranteed Uniq ID for column name generation
		/// </summary>
		/// <returns></returns>
		int GetNextColumnId();
	}
}