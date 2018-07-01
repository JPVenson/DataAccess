#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Wrapper for Generic QueryCommand parts
	/// </summary>
	public abstract class QueryPartBase
	{
		private readonly IQueryBuilder _builder;

		/// <summary>
		///     Creates a generic query part that can be used for any query
		/// </summary>
		/// <param name="builder">The type of building object</param>
		protected QueryPartBase(IQueryBuilder builder)
		{
			_builder = builder;
		}

		/// <summary>
		///     If used the Parameters that are used for this Prefix
		/// </summary>
		public IEnumerable<IQueryParameter> QueryParameters { get; set; }

		/// <summary>
		///     Gets the Query builder element.
		/// </summary>
		/// <value>
		///     The builder.
		/// </value>
		public IQueryBuilder Builder
		{
			get { return _builder; }
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public virtual object Clone(IQueryBuilder builder)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     For display
		/// </summary>
		/// <returns></returns>
		public virtual string Render()
		{
			var sb = new ConsoleStringBuilderInterlaced();
			Render(sb);
			return sb.ToString();
		}

		/// <summary>
		///     For display
		/// </summary>
		/// <returns></returns>
		internal virtual void Render(ConsoleStringBuilderInterlaced sb)
		{
			sb
				.AppendInterlacedLine("new GenericQueryPart {")
				.Up()
				.AppendInterlacedLine("Builder = \"{0}\",", Builder != null ? Builder.GetType().Name : "{NULL}")
				.AppendInterlaced("Parameter[{0}] = ", QueryParameters.Count());
			if (QueryParameters.Any())
			{
				sb.AppendInterlacedLine("{")
					.Up();
				foreach (var queryParameter in QueryParameters.Cast<QueryParameter>())
				{
					queryParameter.Render(sb);
					sb.AppendLine(",");
				}
				sb.Down()
					.AppendInterlacedLine("}");
			}
			else
			{
				sb.AppendLine("{}");
			}
			sb.Down()
				.AppendInterlaced("}");
			//return string.Format("{{QueryCommand = {0}, Parameter = [{1}]}}", Prefix, paraString);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Render();
		}

		/// <summary>
		///		Should return an Compiled version of this query 
		/// </summary>
		/// <returns></returns>
		public abstract IDbCommand Parse();
	}

	/// <inheritdoc />
	public class SelectQueryPart : QueryPartBase
	{
		private readonly IDbCommand _command;

		/// <inheritdoc />
		public SelectQueryPart(IQueryBuilder builder, string table, string[] columns) : base(builder)
		{
			_command = builder.ContainerObject.AccessLayer.Database.CreateCommand(
			$"SELECT {columns.Select(e => $"[{e}]").Aggregate((e, f) => e + "," + f)} FROM {table}");
		}

		/// <inheritdoc />
		public SelectQueryPart(IQueryBuilder builder, IDbCommand command) : base(builder)
		{
			_command = command;
		}

		/// <inheritdoc />
		public override IDbCommand Parse()
		{
			return _command;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class UpdateQueryPart : QueryPartBase
	{
		/// <inheritdoc />
		public UpdateQueryPart(IQueryBuilder builder) : base(builder)
		{
		}

		/// <inheritdoc />
		public override IDbCommand Parse()
		{
			
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ExecuteProcedureQueryPart : QueryPartBase
	{
		/// <inheritdoc />
		public ExecuteProcedureQueryPart(IQueryBuilder builder) : base(builder)
		{
		}

		/// <inheritdoc />
		public override IDbCommand Parse()
		{
			throw new NotImplementedException();
		}
	}
}