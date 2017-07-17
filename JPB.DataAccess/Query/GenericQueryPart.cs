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
	public class GenericQueryPart
	{
		private readonly IQueryBuilder _builder;

		/// <summary>
		///     Creates a generic query part that can be used for any query
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="parameters"></param>
		/// <param name="builder">The type of building object</param>
		public GenericQueryPart(string prefix, IEnumerable<IQueryParameter> parameters, IQueryBuilder builder)
		{
			_builder = builder;
			Debug.Assert(prefix != null, "prefix != null");
			Prefix = prefix;
			QueryParameters = parameters;
		}

		/// <summary>
		///     Creates a generic query part that can be used for any query
		/// </summary>
		/// <param name="prefix"></param>
		public GenericQueryPart(string prefix)
		{
			Debug.Assert(prefix != null, "prefix != null");
			Prefix = prefix;
			QueryParameters = new IQueryParameter[0];
		}

		/// <summary>
		///     The Partial SQL QueryCommand that is contained inside this part
		/// </summary>
		public string Prefix { get; internal set; }

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
		public object Clone(IQueryBuilder builder)
		{
			return new GenericQueryPart(Prefix, QueryParameters.Select(e => e.Clone()).ToList(), builder);
		}

		/// <summary>
		///     Wrapps the given <paramref name="command" /> into a new QueryPart by storing its QueryCommand statement and
		///     parameter
		/// </summary>
		/// <param name="command"></param>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static GenericQueryPart FromCommand(IDbCommand command, IQueryElement builder)
		{
			return new GenericQueryPart(command.CommandText,
				command.Parameters.Cast<IDataParameter>().Select(s => new QueryParameter(s.ParameterName, s.Value)), builder);
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
				.AppendInterlacedLine("QueryCommand = \"{0}\",", Prefix)
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
	}
}