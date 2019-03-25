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
	public class InternalContainerContainer : IQueryContainer, IQueryContainerValues
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
			TableAlias = new Dictionary<string, string>();
			Identifiers = new List<QueryIdentifier>();
		}

		internal InternalContainerContainer(IQueryContainer pre)
		{
			AccessLayer = pre.AccessLayer;
			ForType = pre.ForType;
			AutoParameterCounter = (pre as IQueryContainerValues)?.AutoParameterCounter ?? 0;
			TableAlias = (pre as IQueryContainerValues)?.TableAlias ?? new Dictionary<string, string>();
			Identifiers = (pre as IQueryContainerValues)?.Identifiers ?? new List<QueryIdentifier>();
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

		/// <summary>
		/// Provides internal formatting infos about the current query
		/// </summary>
		public IDictionary<string, object> QueryInfos { get; private set; }
		
		/// <summary>
		/// </summary>
		public DbAccessLayer AccessLayer { get; private set; }

		/// <summary>
		/// </summary>
		public Type ForType { get; set; }

		/// <inheritdoc />
		public int AutoParameterCounter { get; private set; }

		/// <inheritdoc />
		public IDictionary<string, string> TableAlias { get; }

		/// <inheritdoc />
		public IEnumerable<IQueryPart> Parts
		{
			get { return _parts; }
		}
		
		/// <inheritdoc />
		public bool AllowParamterRenaming { get; set; }

		/// <inheritdoc />
		public IDbCommand Compile(out IEnumerable<ColumnInfo> columns)
		{
			var commands = new List<IDbCommand>();
			columns = new ColumnInfo[0];
			foreach (var queryPart in Parts)
			{
				commands.Add(queryPart.Process(this));
				columns = (queryPart as ISelectableQueryPart)?.Columns ?? columns;
			}

			return DbAccessLayerHelper.ConcatCommands(AccessLayer.Database, true, commands.Where(e => e != null).ToArray());

			//var query = CompileFlat();
			//return AccessLayer.Database.CreateCommandWithParameterValues(query.Item1, query.Item2);
		}

		/// <inheritdoc />
		public IList<QueryIdentifier> Identifiers { get; private set; }
		
		/// <inheritdoc />
		public QueryIdentifier GetAlias(QueryIdentifier.QueryIdTypes table)
		{
			var identifier = new QueryIdentifier();
			identifier.QueryIdType = table;
			identifier.Value =
				$"[{identifier.QueryIdType.ToString()}_{Identifiers.Count(g => g.QueryIdType.Equals(identifier.QueryIdType))}]";
			Identifiers.Add(identifier);
			return identifier;
		}

		/// <inheritdoc />
		public void SetTableAlias(string table, string alias)
		{
			TableAlias[table.Trim('[', ']')] = alias;
		}

		/// <summary>
		///     Compiles the QueryCommand into a String|IEnumerable of Paramameter
		/// </summary>
		/// <returns></returns>
		public Tuple<string, IEnumerable<IQueryParameter>> CompileFlat()
		{
			var sb = new StringBuilder();
			var queryParts = Parts.ToArray();
			
			var param = new List<IQueryParameter>();

			foreach (var queryPart in queryParts)
			{
				var command = queryPart.Process(this);
				if (command == null)
				{
					continue;
				}

				param.AddRange(command.Parameters.AsQueryParameter());
				if (command.CommandText != null)
				{
					if (!command.CommandText.EndsWith(" ", true, CultureInfo.InvariantCulture) || !command.CommandText.StartsWith(" ", true, CultureInfo.InvariantCulture))
					{
						command.CommandText = " " + command.CommandText;
					}
				}

				sb.Append(command.CommandText);
			}

			return new Tuple<string, IEnumerable<IQueryParameter>>(sb.ToString(), param);
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
		public T Search<T>() where T : IQueryPart
		{
			return Parts.OfType<T>().LastOrDefault();
		}

		/// <inheritdoc />
		public T Search<T>(Func<T, bool> filter) where T : IQueryPart
		{
			return Parts.OfType<T>().FirstOrDefault(filter);
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

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ForType == null)
			{
				throw new ArgumentNullException("No type Supplied", new Exception());
			}
			return new QueryEagerEnumerator(this, ForType, true);
		}

		/// <summary>
		///     Executes a query without result parsing
		/// </summary>
		public int Execute()
		{
			return Compile(out var columns).ExecuteGenericCommand(AccessLayer.Database);
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

		/// <summary>
		///     Renders the Current Object
		/// </summary>
		/// <returns></returns>
		public string Render()
		{
			var sb = new ConsoleStringBuilderInterlaced();
			Render(sb);
			return sb.ToString();
		}

		internal void Render(ConsoleStringBuilderInterlaced sb)
		{
			sb.AppendInterlacedLine("new IQueryContainer {")
				.Up()
				.AppendInterlacedLine("AllowParamterRenaming = {0},", AllowParamterRenaming.ToString().ToLower())
				.AppendInterlacedLine("AutoParameterCounter = {0},", AutoParameterCounter)
				.AppendInterlacedLine("QueryDebugger = ")
				.Insert(new QueryDebugger(Compile(out var columns), AccessLayer.Database).Render)
				.AppendInterlacedLine("Parts[{0}] = ", Parts.Count())
				.AppendInterlacedLine("{")
				.Up();

			//foreach (var genericQueryPart in Parts)
			//{
			//	genericQueryPart.Render(sb);
			//	sb.AppendLine(",");
			//}

			sb.Down()
				.AppendInterlacedLine("}")
				.Down()
				.AppendInterlaced("}");
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return Render();
		}
	}
}