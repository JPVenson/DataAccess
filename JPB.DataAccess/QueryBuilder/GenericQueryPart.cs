/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
	/// <summary>
	///     Wrapper for Generic Query parts
	/// </summary>
	public class GenericQueryPart : ICloneable
	{
		/// <summary>
		///     Creates a generic query part that can be used for any query
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="parameters"></param>
		public GenericQueryPart(string prefix, IEnumerable<IQueryParameter> parameters)
		{
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
		///     The Partial SQL Query that is contained inside this part
		/// </summary>
		public string Prefix { get; internal set; }

		/// <summary>
		///     If used the Parameters that are used for this Prefix
		/// </summary>
		public IEnumerable<IQueryParameter> QueryParameters { get; set; }

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>
		///     Wrapps the given <paramref name="command" /> into a new QueryPart by storing its Query statement and parameter
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static GenericQueryPart FromCommand(IDbCommand command)
		{
			return new GenericQueryPart(command.CommandText,
				command.Parameters.Cast<IDataParameter>().Select(s => new QueryParameter(s.ParameterName, s.Value)));
		}

		/// <summary>
		///     For display
		/// </summary>
		/// <returns></returns>
		public virtual string Render()
		{
			var sb = new StringBuilderIntend();
			Render(sb);
			return sb.ToString();
		}

		/// <summary>
		///     For display
		/// </summary>
		/// <returns></returns>
		internal virtual void Render(StringBuilderIntend sb)
		{
			sb
				.AppendIntedLine("new GenericQueryPart {")
				.Up()
				.AppendIntedLine("Query = \"{0}\",", Prefix)
				.AppendInted("Parameter[{0}] = ", QueryParameters.Count());
			if (QueryParameters.Any())
			{
				sb.AppendIntedLine("{")
					.Up();
				foreach (QueryParameter queryParameter in QueryParameters.Cast<QueryParameter>())
				{
					queryParameter.Render(sb);
					sb.AppendLine(",");
				}
				sb.Down()
					.AppendIntedLine("}");
			}
			else
			{
				sb.AppendLine("{}");
			}
			sb.Down()
				.AppendInted("}");
			//return string.Format("{{Query = {0}, Parameter = [{1}]}}", Prefix, paraString);
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