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

namespace JPB.DataAccess.Query
{
	public class InternalContainerContainer : IQueryContainer
	{
		/// <summary>
		/// </summary>
		public DbAccessLayer AccessLayer { get; private set; }

		/// <summary>
		/// </summary>
		public Type ForType { get; set; }

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
		}

		internal InternalContainerContainer(IQueryContainer pre)
		{
			AccessLayer = pre.AccessLayer;
			ForType = pre.ForType;
			AutoParameterCounter = pre.AutoParameterCounter;
			Parts = pre.Parts.ToList();
			EnumerationMode = pre.EnumerationMode;
			AllowParamterRenaming = pre.AllowParamterRenaming;
		}

		public int AutoParameterCounter { get; private set; }
		public List<GenericQueryPart> Parts { get; private set; }
		public EnumerationMode EnumerationMode { get; set; }
		public bool AllowParamterRenaming { get; set; }

		/// <summary>
		///     If enabled the IQueryContainer will insert linebreaks after some Commands
		/// </summary>
		public bool AutoLinebreak { get; set; }

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


		public IDbCommand Compile()
		{
			var query = CompileFlat();
			return AccessLayer.Database.CreateCommandWithParameterValues(query.Item1, query.Item2);
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


		public int GetNextParameterId()
		{
			return ++AutoParameterCounter;
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
			var sb = new StringBuilderInterlaced();
			Render(sb);
			return sb.ToString();
		}

		internal void Render(StringBuilderInterlaced sb)
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

			foreach (GenericQueryPart genericQueryPart in Parts)
			{
				genericQueryPart.Render(sb);
				sb.AppendLine(",");
			}

			sb.Down()
				.AppendInterlacedLine("}")
				.Down()
				.AppendInterlaced("}");
		}

		public override string ToString()
		{
			return Render();
		}

		//public object Clone()
		//{
		//	return new InternalContainerContainer(this);
		//}

		public IQueryContainer Clone()
		{
			return new InternalContainerContainer(this);
		}
	}
}