using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
	/// <summary>
	///     Provides functions that can build SQL Querys
	/// </summary>
	public class QueryBuilder : IEnumerable, ICloneable
	{
		/// <summary>
		/// </summary>
		protected internal readonly IDatabase Database;

		/// <summary>
		/// </summary>
		protected internal readonly Type ForType;

		/// <summary>
		///     Creates a new Instance of an Query Builder that creates Database aware querys
		/// </summary>
		public QueryBuilder(IDatabase database, Type forType)
			: this(database)
		{
			ForType = forType;
		}

		/// <summary>
		///     Creates a new Instance of an Query Builder that creates Database aware querys
		/// </summary>
		public QueryBuilder(IDatabase database)
		{
			Database = database;
			Parts = new List<GenericQueryPart>();
		}

		internal int AutoParameterCounter { get; set; }
		internal List<GenericQueryPart> Parts { get; set; }

		/// <summary>
		///     Defines the Way how the Data will be loaded
		/// </summary>
		public EnumerationMode EnumerationMode { get; set; }

		/// <summary>
		///     If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
		/// </summary>
		public bool AllowParamterRenaming { get; set; }

		/// <summary>
		///     If enabled the QueryBuilder will insert linebreaks after some Commands
		/// </summary>
		public bool AutoLinebreak { get; set; }

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new QueryBuilder(Database)
			{
				EnumerationMode = EnumerationMode,
				Parts = Parts
			};
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator(this, ForType);
			return new QueryLazyEnumerator(this, ForType);
		}

		internal QueryBuilder AutoLinebreakAction()
		{
			if (AutoLinebreak)
				this.LineBreak();
			return this;
		}

		/// <summary>
		///     Will concat all QueryParts into a statement and will check for Spaces
		/// </summary>
		/// <returns></returns>
		public IDbCommand Compile()
		{
			var query = CompileFlat();
			return Database.CreateCommandWithParameterValues(query.Item1, query.Item2);
		}

		/// <summary>
		///     Executes a query without result parsing
		/// </summary>
		public int Execute()
		{
			return Compile().ExecuteGenericCommand(Database);
		}

		/// <summary>
		///     Query like setter for WithEnumerationMode
		/// </summary>
		/// <returns></returns>
		public QueryBuilder WithEnumerationMode(EnumerationMode mode)
		{
			EnumerationMode = mode;
			return this;
		}


		/// <summary>
		///     Query like setter for AllowParamterRenaming [Duplicate]
		/// </summary>
		/// <returns></returns>
		public QueryBuilder WithParamterRenaming(bool mode)
		{
			AllowParamterRenaming = mode;
			return this;
		}

		/// <summary>
		///     Adds a Query part to the Local collection
		/// </summary>
		/// <returns></returns>
		public QueryBuilder Add(GenericQueryPart part)
		{
			if (AllowParamterRenaming)
			{
				foreach (IQueryParameter queryParameter in part.QueryParameters)
				{
					var fod = Parts.SelectMany(s => s.QueryParameters).FirstOrDefault(s => s.Name == queryParameter.Name);

					if (fod == null)
						continue;

					//parameter is existing ... renaming new Parameter to Auto gen and renaming all ref in the Query
					var name = fod.Name;
					var newName = GetParamaterAutoId().ToString().CheckParamter();
					part.Prefix = part.Prefix.Replace(name, newName);
					queryParameter.Name = newName;
				}
			}
			Parts.Add(part);
			return this;
		}

		/// <summary>
		///     Compiles the Query into a String|IEnumerable of Paramameter
		/// </summary>
		/// <returns></returns>
		public Tuple<string, IEnumerable<IQueryParameter>> CompileFlat()
		{
			var sb = new StringBuilder();
			var queryParts = Parts.ToArray();
			string prefRender = null;
			var param = new List<IQueryParameter>();

			foreach (GenericQueryPart queryPart in queryParts)
			{
				//take care of spaces
				//check if the last statement ends with a space or the next will start with one
				var renderCurrent = queryPart.Prefix;
				if (prefRender != null)
				{
					if (!prefRender.EndsWith(" ", true, CultureInfo.InvariantCulture) ||
					    !renderCurrent.StartsWith(" ", true, CultureInfo.InvariantCulture))
					{
						renderCurrent = " " + renderCurrent;
					}
				}
				sb.Append(renderCurrent);
				param.AddRange(queryPart.QueryParameters);
				prefRender = renderCurrent;
			}

			return new Tuple<string, IEnumerable<IQueryParameter>>(sb.ToString(), param);
		}

		/// <summary>
		///     Converts the non Generic QueryBuilder into its Counterpart
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public QueryBuilder<T> ForResult<T>()
		{
			return new QueryBuilder<T>(Database)
			{
				EnumerationMode = EnumerationMode,
				Parts = Parts
			};
		}

		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		public int GetParamaterAutoId()
		{
			return ++AutoParameterCounter;
		}

		/// <summary>
		///     Query like setter for AllowParamterRenaming
		/// </summary>
		/// <returns></returns>
		public QueryBuilder SetAutoRenaming(bool value)
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
			var sb = new StringBuilderIntend();
			Render(sb);
			return sb.ToString();
		}

		internal void Render(StringBuilderIntend sb)
		{
			sb.AppendIntedLine("new QueryBuilder {")
				.Up()
				.AppendIntedLine("AllowParamterRenaming = {0},", AllowParamterRenaming.ToString().ToLower())
				.AppendIntedLine("AutoParameterCounter = {0},", AutoParameterCounter)
				.AppendIntedLine("QueryDebugger = ")
				.Insert(new QueryDebugger(Compile(), Database).Render)
				.AppendIntedLine("Parts[{0}] = ", Parts.Count)
				.AppendIntedLine("{")
				.Up();

			foreach (GenericQueryPart genericQueryPart in Parts)
			{
				genericQueryPart.Render(sb);
				sb.AppendLine(",");
			}

			sb.Down()
				.AppendIntedLine("}")
				.Down()
				.AppendInted("}");
		}

		public override string ToString()
		{
			return Render();
		}
	}


	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class QueryBuilder<T> : QueryBuilder, IEnumerable<T>
	{
		/// <summary>
		///     Creates a new Instance of an Query Builder that creates Database aware querys
		/// </summary>
		public QueryBuilder(IDatabase database)
			: base(database, typeof (T))
		{
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public new IEnumerator<T> GetEnumerator()
		{
			if (ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator<T>(this, ForType);
			return new QueryLazyEnumerator<T>(this, ForType);
		}

		/// <summary>
		///     Adds a Query part to the Local collection
		/// </summary>
		public new QueryBuilder<T> Add(GenericQueryPart part)
		{
			base.Add(part);
			return this;
		}
	}
}