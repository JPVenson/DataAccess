#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
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
		IDataRecord Transform(IDataRecord reader, Type entityType, QueryProcessingRecordsContext context);
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
		internal QueryProcessingRecordsContext()
		{
		}
	}

	internal interface IQueryContainerValues
	{
		/// <summary>
		///     Gets the current number of used SQL Arguments
		/// </summary>
		int AutoParameterCounter { get; }

		/// <summary>
		///     Gets the current number of used SQL Arguments
		/// </summary>
		IDictionary<string, string> TableAlias { get; }

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
		///		The list of all Identifiers
		/// </summary>
		IList<QueryIdentifier> Identifiers { get; }

		/// <summary>
		///     Defines the Way how the Data will be loaded
		/// </summary>
		EnumerationMode EnumerationMode { get; set; }

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
		IDbCommand Compile();

		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		int GetNextParameterId();

		/// <summary>
		///     
		/// </summary>
		/// <returns></returns>
		string GetTableAlias(string table);
		
		/// <summary>
		///     Translates an Identifier object into the corresponding Sql Identifier
		/// </summary>
		/// <returns></returns>
		QueryIdentifier GetAlias(QueryIdentifier.QueryIdTypes type);

		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		void SetTableAlias(string table, string alias);

		/// <summary>
		///     Compiles the QueryCommand into a String|IEnumerable of Paramameter
		/// </summary>
		/// <returns></returns>
		Tuple<string, IEnumerable<IQueryParameter>> CompileFlat();

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
		/// 
		/// </summary>
		/// <param name="queryPart"></param>
		void Add(IQueryPart queryPart);
	}
}