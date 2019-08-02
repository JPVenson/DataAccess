#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	public interface IQueryContainer
	{
		/// <summary>
		///		Contains a list of all joins of the current context
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
		///		Should the enumerator schedule the execution async
		/// </summary>
		bool ExecuteAsync { get; set; }

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
		T SearchLast<T>() where T : IQueryPart;

		/// <summary>
		///		Searches in the Parts collection for the nearest occurence of this Query Part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T SearchLast<T>(Func<T, bool> filter) where T : IQueryPart;

		/// <summary>
		///		Searches in the Parts collection for the nearest occurence of this Query Part
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T SearchFirst<T>(Func<T, bool> filter) where T : IQueryPart;

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