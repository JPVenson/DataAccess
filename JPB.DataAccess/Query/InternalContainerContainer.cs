#region

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
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Stores the Query data produced by an QueryBuilder element
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryContainer" />
	public class InternalContainerContainer : IQueryContainer
	{
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
			Parts = new List<GenericQueryPart>();
			QueryInfos = new Dictionary<string, object>();
		}

		internal InternalContainerContainer(IQueryContainer pre)
		{
			AccessLayer = pre.AccessLayer;
			ForType = pre.ForType;
			AutoParameterCounter = pre.AutoParameterCounter;
			Parts = pre.Parts.Select(f => f.Clone(null) as GenericQueryPart).ToList();
			EnumerationMode = pre.EnumerationMode;
			AllowParamterRenaming = pre.AllowParamterRenaming;
			QueryInfos = pre.QueryInfos.Select(f => f).ToDictionary(f => f.Key, f => f.Value);
		}

		/// <summary>
		/// Provides internal formatting infos about the current query
		/// </summary>
		public IDictionary<string, object> QueryInfos { get; private set; }

		/// <summary>
		///     If enabled the IQueryContainer will insert linebreaks after some Commands
		/// </summary>
		public bool AutoLinebreak { get; set; }

		/// <summary>
		/// </summary>
		public DbAccessLayer AccessLayer { get; private set; }

		/// <summary>
		/// </summary>
		public Type ForType { get; set; }

		/// <summary>
		///     Gets the current number of used SQL Parameter. This value is used for Autogeneration
		/// </summary>
		public int AutoParameterCounter { get; private set; }

		/// <summary>
		///     Defines all elements added by the Add Method
		/// </summary>
		public List<GenericQueryPart> Parts { get; private set; }

		/// <summary>
		///     Defines the Way how the Data will be loaded
		/// </summary>
		public EnumerationMode EnumerationMode { get; set; }

		/// <summary>
		///     If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
		/// </summary>
		public bool AllowParamterRenaming { get; set; }


		/// <summary>
		///     Will concat all QueryParts into a statement and will check for Spaces
		/// </summary>
		/// <returns></returns>
		public IDbCommand Compile()
		{
			var query = CompileFlat();
			return AccessLayer.Database.CreateCommandWithParameterValues(query.Item1, query.Item2);
		}

		/// <summary>
		///     Compiles the QueryCommand into a String|IEnumerable of Paramameter
		/// </summary>
		/// <returns></returns>
		public Tuple<string, IEnumerable<IQueryParameter>> CompileFlat()
		{
			var sb = new StringBuilder();
			var queryParts = Parts.ToArray();
			string prefRender = null;
			var param = new List<IQueryParameter>();

			foreach (var queryPart in queryParts)
			{
				//take care of spaces
				//check if the last statement ends with a space or the next will start with one
				var renderCurrent = queryPart.Prefix;
				if (prefRender != null)
				{
					if (!prefRender.EndsWith(" ", true, CultureInfo.InvariantCulture) || !renderCurrent.StartsWith(" ", true, CultureInfo.InvariantCulture))
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

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ForType == null)
			{
				throw new ArgumentNullException("No type Supplied", new Exception());
			}

			if (EnumerationMode == EnumerationMode.FullOnLoad)
			{
				return new QueryEagerEnumerator(this, ForType, true);
			}
			return new QueryLazyEnumerator(this, ForType, true);
		}

		/// <summary>
		///     Executes a query without result parsing
		/// </summary>
		public int Execute()
		{
			return Compile().ExecuteGenericCommand(AccessLayer.Database);
		}

		/// <summary>
		///     QueryCommand like setter for WithEnumerationMode
		/// </summary>
		/// <returns></returns>
		public IQueryContainer WithEnumerationMode(EnumerationMode mode)
		{
			EnumerationMode = mode;
			return this;
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
				.Insert(new QueryDebugger(Compile(), AccessLayer.Database).Render)
				.AppendInterlacedLine("Parts[{0}] = ", Parts.Count)
				.AppendInterlacedLine("{")
				.Up();

			foreach (var genericQueryPart in Parts)
			{
				genericQueryPart.Render(sb);
				sb.AppendLine(",");
			}

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