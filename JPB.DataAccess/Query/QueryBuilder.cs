/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

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
			Parts = pre.Parts;
			EnumerationMode = pre.EnumerationMode;
			AllowParamterRenaming = pre.AllowParamterRenaming;
		}

		public int AutoParameterCounter { get; set; }
		public List<GenericQueryPart> Parts { get; set; }

		/// <summary>
		///     Defines the Way how the Data will be loaded
		/// </summary>
		public EnumerationMode EnumerationMode { get; set; }

		/// <summary>
		///     If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
		/// </summary>
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
		///     Compiles the QueryCommand into a String|IEnumerable of Paramameter
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
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		public int GetParamaterAutoId()
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

		public object Clone()
		{
			return new InternalContainerContainer(this);
		}
	}

	/// <summary>
	///     Provides functions that can build SQL Querys
	/// </summary>
	public class QueryBuilder<Stack> : IQueryBuilder<Stack>
		where Stack : IQueryElement
	{
		internal QueryBuilder(DbAccessLayer database, Type type)
		{
			this.ContainerObject = new InternalContainerContainer(database, type);
		}

		internal QueryBuilder(IQueryContainer database)
		{
			this.ContainerObject = database;
		}

		internal QueryBuilder(IQueryBuilder<Stack> database)
		{
			this.ContainerObject = database.ContainerObject;
		}

		internal QueryBuilder(IQueryBuilder<Stack> database, Type type)
		{
			this.ContainerObject = database.ContainerObject;
			this.ContainerObject.ForType = type;
		}

		public QueryBuilder(DbAccessLayer database)
		{
			this.ContainerObject = new InternalContainerContainer(database);
		}
		
		public IQueryBuilder<T> ChangeType<T>() where T : IQueryElement
		{
			return new QueryBuilder<T>(ContainerObject);
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new QueryBuilder<Stack>(this.ContainerObject);
		}
		
		public IQueryContainer ContainerObject { get; private set; }

		/// <summary>
		///     Converts the non Generic IQueryContainer into its Counterpart
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<E> ForResult<E>()
		{
			return new QueryEnumerator<Stack, E>(new QueryBuilder<E, Stack>(this));
		}

		public IEnumerable ForResult()
		{
			return new QueryEnumerator<Stack>(this);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ContainerObject.ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator(ContainerObject, ContainerObject.ForType);
			return new QueryLazyEnumerator(ContainerObject, ContainerObject.ForType);
		}
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class QueryBuilder<T, Stack> : QueryBuilder<Stack>
		where Stack : IQueryElement
	{
		/// <summary>
		///     Creates a new Instance of an QueryCommand Builder that creates Database aware querys
		/// </summary>
		public QueryBuilder(DbAccessLayer database)
			: base(database, typeof(T))
		{
		}

		internal QueryBuilder(IQueryBuilder<Stack> source)
			: base(source, typeof(T))
		{

		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public new object Clone()
		{
			return new QueryBuilder<T, Stack>(this);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			if (ContainerObject.ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator<T>(ContainerObject, ContainerObject.ForType);
			return new QueryLazyEnumerator<T>(ContainerObject, ContainerObject.ForType);
		}
	}
}