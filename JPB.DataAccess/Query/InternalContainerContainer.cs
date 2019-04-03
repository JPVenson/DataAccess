#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Stores the Query data produced by an QueryBuilder element
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryContainer" />
	public class InternalContainerContainer : IQueryContainerValues
	{
		private readonly List<IQueryPart> _parts;

		/// <summary>
		///     Creates a new Instance of an QueryCommand Builder that creates Database aware querys
		/// </summary>
		public InternalContainerContainer(DbAccessLayer database, Type forType)
			: this(database)
		{
			ForType = forType;
		}

		/// <summary>
		///     Creates a new Instance of an QueryText Builder that creates Database aware querys
		/// </summary>
		public InternalContainerContainer(DbAccessLayer database)
		{
			AccessLayer = database;
			_parts = new List<IQueryPart>();
			QueryInfos = new Dictionary<string, object>();
			Interceptors = new List<IQueryCommandInterceptor>();
			PostProcessors = new List<IEntityProcessor>();
			TableAlias = new Dictionary<string, QueryIdentifier>();
			Identifiers = new List<QueryIdentifier>();
			Joins = new List<JoinParseInfo>();
		}

		internal InternalContainerContainer(IQueryContainer pre)
		{
			AccessLayer = pre.AccessLayer;
			ForType = pre.ForType;
			AutoParameterCounter = (pre as IQueryContainerValues)?.AutoParameterCounter ?? 0;
			TableAlias = (pre as IQueryContainerValues)?.TableAlias ?? new Dictionary<string, QueryIdentifier>();
			Identifiers = (pre as IQueryContainerValues)?.Identifiers ?? new List<QueryIdentifier>();
			Joins = (pre as IQueryContainerValues)?.Joins ?? new List<JoinParseInfo>();
			ColumnCounter = (pre as IQueryContainerValues)?.ColumnCounter ?? 0;
			_parts = pre.Parts.ToList();
			AllowParamterRenaming = pre.AllowParamterRenaming;
			QueryInfos = pre.QueryInfos.Select(f => f).ToDictionary(f => f.Key, f => f.Value);
			Interceptors = pre.Interceptors;
			PostProcessors = pre.PostProcessors;
		}
		
		/// <inheritdoc />
		public List<IEntityProcessor> PostProcessors { get; }

		/// <inheritdoc />
		public List<IQueryCommandInterceptor> Interceptors { get; }

		/// <inheritdoc />
		public IDictionary<string, object> QueryInfos { get; private set; }

		/// <inheritdoc />
		public DbAccessLayer AccessLayer { get; private set; }

		/// <inheritdoc />
		public Type ForType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IList<JoinParseInfo> Joins { get; }

		/// <inheritdoc />
		public int AutoParameterCounter { get; private set; }

		/// <inheritdoc />
		public int ColumnCounter { get; private set; }

		/// <inheritdoc />
		public IDictionary<string, QueryIdentifier> TableAlias { get; }

		/// <inheritdoc />
		public IEnumerable<IQueryPart> Parts
		{
			get { return _parts; }
		}
		
		/// <inheritdoc />
		public bool AllowParamterRenaming { get; set; }

		/// <inheritdoc />
		public IQueryFactoryResult Compile(out IEnumerable<ColumnInfo> columns)
		{
			var commands = new List<IQueryFactoryResult>();
			columns = new ColumnInfo[0];
			foreach (var queryPart in Parts)
			{
				commands.Add(queryPart.Process(this));
				var isSelectQuery = (queryPart is ISelectableQueryPart && !(queryPart is JoinTableQueryPart));
				if (isSelectQuery)
				{
					columns = (queryPart as ISelectableQueryPart).Columns;
				}
			}

			return DbAccessLayerHelper.MergeQueryFactoryResult(true, 1, true, null, 
				commands.Where(e => e != null).ToArray());
		}

		/// <inheritdoc />
		public IList<QueryIdentifier> Identifiers { get; private set; }
		
		/// <inheritdoc />
		public QueryIdentifier CreateAlias(QueryIdentifier.QueryIdTypes table)
		{
			var identifier = new QueryIdentifier();
			identifier.QueryIdType = table;
			identifier.Value =
				$"[{identifier.QueryIdType.ToString()}_{Identifiers.Count(g => g.QueryIdType.Equals(identifier.QueryIdType))}]";
			Identifiers.Add(identifier);
			return identifier;
		}

		/// <inheritdoc />
		public QueryIdentifier CreateTableAlias(string path)
		{
			if (TableAlias.ContainsKey(path))
			{
				return TableAlias[path];
			}

			var alias = CreateAlias(QueryIdentifier.QueryIdTypes.Table);
			TableAlias.Add(path, alias);
			return alias;
		}

		/// <inheritdoc />
		public QueryIdentifier SearchTableAlias(string path)
		{
			return TableAlias.FirstOrDefault(e => e.Key.Equals(path)).Value;
		}

		/// <inheritdoc />
		public string GetPathOf(QueryIdentifier identifier)
		{
			return TableAlias.FirstOrDefault(e => e.Value.Equals(identifier)).Key;
		}

		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		public int GetNextParameterId()
		{
			return ++AutoParameterCounter;
		}

		//public object Clone()
		//{
		//	return new InternalContainerContainer(this);
		//}

		/// <summary>
		///     Clones this Container
		/// </summary>
		/// <returns></returns>
		public IQueryContainer Clone()
		{
			return new InternalContainerContainer(this);
		}

		/// <inheritdoc />
		public T SearchLast<T>() where T : IQueryPart
		{
			return Parts.OfType<T>().LastOrDefault();
		}

		/// <inheritdoc />
		public T SearchFirst<T>(Func<T, bool> filter) where T : IQueryPart
		{
			return Parts.OfType<T>().FirstOrDefault(filter);
		}

		/// <inheritdoc />
		public T SearchLast<T>(Func<T, bool> filter) where T : IQueryPart
		{
			return Parts.OfType<T>().LastOrDefault(filter);
		}

		/// <inheritdoc />
		public ISelectableQueryPart Search(QueryIdentifier identifier)
		{
			return Parts.OfType<ISelectableQueryPart>().First(e => e.Alias.Equals(identifier));
		}

		/// <inheritdoc />
		public void Add(IQueryPart queryPart)
		{
			_parts.Add(queryPart);
		}

		/// <inheritdoc />
		public int GetNextColumnId()
		{
			return ++ColumnCounter;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ForType == null)
			{
				throw new ArgumentNullException("No type Supplied", new Exception());
			}
			return new QueryEagerEnumerator(this, ForType, AccessLayer.Async);
		}

		/// <summary>
		///     Executes a query without result parsing
		/// </summary>
		public int Execute()
		{
			var queryContainer = Compile(out var columns);
			return this.AccessLayer.ExecuteGenericCommand(queryContainer.Query, queryContainer.Parameters);
		}
		
		/// <summary>
		///     QueryCommand like setter for AllowParamterRenaming [Duplicate]
		/// </summary>
		/// <returns></returns>
		public IQueryContainer WithParamterRenaming(bool mode)
		{
			AllowParamterRenaming = mode;
			return this;
		}

		/// <summary>
		///     QueryCommand like setter for AllowParamterRenaming
		/// </summary>
		/// <returns></returns>
		public IQueryContainer SetAutoRenaming(bool value)
		{
			AllowParamterRenaming = value;
			return this;
		}
	}
}